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
        public async Task<ActionResult<pgHotel_HotelDetailDTO>> GetHotelDetail(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.City)
                .ThenInclude(c => c.Country)
                .Include(h => h.HotelEquipmentReferences)
                .ThenInclude(r => r.HotelEquipment)
                .Include(h => h.HotelImages)
                .ThenInclude(img => img.ImageCategoryReferences)
                .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomEquipmentReferences)
                .ThenInclude(re => re.RoomEquipment)
                .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomImages)
                .ThenInclude(ri => ri.ImageCategoryReferences)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
            {
                return NotFound();
            }

            // 获取特定日期范围内的预订记录
            var checkInDate = DateTime.Parse("2024-06-12"); // 示例开始日期
            var checkOutDate = DateTime.Parse("2024-06-13"); // 示例结束日期

            var orders = await _context.OrderDetails
                .Where(k => !(k.CheckInDate >= checkOutDate || k.CheckOutDate <= checkInDate))
                .Select(k => k.RoomId)
                .ToListAsync();

            // 过滤掉已预订的房间
            var availableRooms = hotel.Rooms
                .Where(r => !orders.Contains(r.RoomId))
                .Select(r => new pgHotel_RoomDTO
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
                        ImageCategoryID = i.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault()
                    }).ToList()
                })
                .ToList();

            var similarHotels = await _context.Hotels
                .Where(h => h.City.CityId == hotel.City.CityId && h.HotelId != id) // 根据 CityId 进行比对
                .Take(9)
                .Select(h => new pgHotel_SimilarHotelsDTO
                {
                    HotelId = h.HotelId,
                    HotelName = h.HotelName,
                    HotelAddress = h.HotelAddress,
                    LevelStar = (int)h.LevelStar,
                    AverageRoomPrice = h.Rooms.Average(r => r.RoomPrice),
                    AvailableRooms = h.Rooms.Count(),
                    HotelImage = h.HotelImages.Select(img => new pgHotel_ImageDTO
                    {
                        ImageUrl = img.HotelImage1,
                        ImageCategoryID = img.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault()
                    }).FirstOrDefault() // 获取第一张图片
                })
                .ToListAsync();

            // 调用另一个 API 获取评分信息并更新 similarHotels
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
                CityId = hotel.City.CityId, // 设置 CityId 属性=> 用來推薦你可能喜歡
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
                    ImageCategoryID = i.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault()
                }).ToList(),
                Rooms = availableRooms,
                AverageRoomPrice = hotel.Rooms.Average(r => r.RoomPrice),
                SimilarHotels = similarHotels.ToList() // 添加相似酒店
            };

            return Ok(hotelDetail);
        }
    }
}

