using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using PrjFunNowWebApi.Models.louie_dto;
using System.Net.Http;
using System.Text.Json;

namespace PrjFunNowWebApi.Controllers.louie_api
{
    [Route("api/[controller]")]
    [ApiController]
    public class pgHotelController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly HttpClient _client;

        public pgHotelController(FunNowContext context, HttpClient client)
        {
            _context = context;
            _client = client;
        }
        //限API內部使用--------------------------------------------------------------------------------
        // 从另一个 API 获取平均评分的方法
        private async Task<double?> GetHotelAverageScore(int hotelId)
        {
            var response = await _client.GetAsync($"https://localhost:7103/api/Comment/{hotelId}/AverageScores");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using (var document = JsonDocument.Parse(jsonString))
                {
                    if (document.RootElement.TryGetProperty("totalAverageScore", out JsonElement totalAverageScoreElement))
                    {
                        var score = totalAverageScoreElement.GetDouble();
                        return Math.Round(score, 1); // 四舍五入到小数点后 1 位
                    }
                }
            }
            return null;
        }


        //要被前端呼叫的http方法----------------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<pgHotel_HotelDetailDTO>> GetHotelDetail(int id, [FromQuery] string checkInDate, [FromQuery] string checkOutDate)
        {
            if (string.IsNullOrEmpty(checkInDate) || string.IsNullOrEmpty(checkOutDate))
            {
                return BadRequest("Check-in and check-out dates are required.");
            }

            DateTime parsedCheckInDate;
            DateTime parsedCheckOutDate;

            if (!DateTime.TryParse(checkInDate, out parsedCheckInDate) || !DateTime.TryParse(checkOutDate, out parsedCheckOutDate))
            {
                return BadRequest("Invalid date format.");
            }

            var hotel = await _context.Hotels
                .Include(h => h.City)
                    .ThenInclude(c => c.Country)
                .Include(h => h.HotelEquipmentReferences)
                    .ThenInclude(r => r.HotelEquipment)
                .Include(h => h.HotelImages)
                    .ThenInclude(img => img.ImageCategoryReferences)
                    .ThenInclude(ic => ic.ImageCategory)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomEquipmentReferences)
                    .ThenInclude(re => re.RoomEquipment)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomImages)
                    .ThenInclude(ri => ri.ImageCategoryReferences)
                    .ThenInclude(ic => ic.ImageCategory)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
            {
                return NotFound();
            }

            var orders = await _context.OrderDetails
                .Where(k => !(k.CheckInDate >= parsedCheckOutDate || k.CheckOutDate <= parsedCheckInDate))
                .Select(k => k.RoomId)
                .ToListAsync();

            var rooms = hotel.Rooms.Select(r => new pgHotel_RoomDTO
            {
                RoomId = r.RoomId,
                RoomName = r.RoomName,
                RoomPrice = r.RoomPrice,
                MaximumOccupancy = r.MaximumOccupancy,
                MemberID = r.MemberId,
                RoomEquipments = r.RoomEquipmentReferences.Select(e => e.RoomEquipment.RoomEquipmentName).ToList(),
                RoomImages = r.RoomImages.Select(i => new pgHotel_ImageDTO
                {
                    ImageUrl = i.RoomImage1,
                    ImageCategoryID = i.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault(),
                    ImageCategoryName = i.ImageCategoryReferences.Select(ic => ic.ImageCategory.ImageCategoryName).FirstOrDefault()
                }).ToList(),
                IsBooked = orders.Contains(r.RoomId)
            }).ToList();

            // 添加房間檢查，避免計算平均價格時出錯
            decimal averageRoomPrice = rooms.Any(r => !r.IsBooked) ? rooms.Where(r => !r.IsBooked).Average(r => r.RoomPrice) : 0;

            var similarHotels = await _context.Hotels
                .Where(h => h.City.CityId == hotel.City.CityId && h.HotelId != id)
                .Take(9)
                .Select(h => new pgHotel_SimilarHotelsDTO
                {
                    HotelId = h.HotelId,
                    HotelName = h.HotelName,
                    HotelAddress = h.HotelAddress,
                    LevelStar = (int)h.LevelStar,
                    AverageRoomPrice = h.Rooms.Any() ? h.Rooms.Average(r => r.RoomPrice) : 0, // 添加房間檢查，避免計算平均價格時出錯
                    AvailableRooms = h.Rooms.Count(),
                    HotelImage = h.HotelImages.Select(img => new pgHotel_ImageDTO
                    {
                        ImageUrl = img.HotelImage1,
                        ImageCategoryID = img.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault(),
                        ImageCategoryName = img.ImageCategoryReferences.Select(ic => ic.ImageCategory.ImageCategoryName).FirstOrDefault()
                    }).FirstOrDefault(),
                    Latitude = h.Latitude,
                    Longitude = h.Longitude
                })
                .ToListAsync();

            var tasks = similarHotels.Select(async similarHotel =>
            {
                var averageScore = await GetHotelAverageScore(similarHotel.HotelId);
                if (averageScore.HasValue)
                {
                    similarHotel.AverageRatingScores = averageScore.Value;
                }
                return similarHotel;
            });

            similarHotels = (await Task.WhenAll(tasks)).ToList();

            var hotelDetail = new pgHotel_HotelDetailDTO
            {
                HotelName = hotel.HotelName,
                HotelAddress = hotel.HotelAddress,
                HotelDescription = hotel.HotelDescription,
                CityName = hotel.City.CityName,
                CityId = hotel.City.CityId,
                CountryName = hotel.City.Country.CountryName,
                LevelStar = hotel.LevelStar,
                Latitude = hotel.Latitude,
                Longitude = hotel.Longitude,
                IsActive = (bool)hotel.IsActive,
                MemberID = hotel.MemberId,
                HotelEquipments = hotel.HotelEquipmentReferences.Select(e => e.HotelEquipment.HotelEquipmentName).ToList(),
                HotelImages = hotel.HotelImages.Select(i => new pgHotel_ImageDTO
                {
                    ImageUrl = i.HotelImage1,
                    ImageCategoryID = i.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault(),
                    ImageCategoryName = i.ImageCategoryReferences.Select(ic => ic.ImageCategory.ImageCategoryName).FirstOrDefault()
                }).ToList(),
                Rooms = rooms,
                AverageRoomPrice = hotel.Rooms.Average(r => r.RoomPrice),
                SimilarHotels = similarHotels.ToList()
            };

            return Ok(hotelDetail);
        }

    }
}

