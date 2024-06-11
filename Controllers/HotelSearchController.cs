using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelSearchController : ControllerBase
    {

        private readonly FunNowContext _context;

        public HotelSearchController(FunNowContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("indexsearch")]
        public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetHotelsByIndexSearch([FromBody] IndexHotelSearchDTO indexhotelSearchDTO)
        {


            // 計算總人數
            int totalPeople = (indexhotelSearchDTO.adults ?? 0) + (indexhotelSearchDTO.children ?? 0);

            // 查詢已被訂房的房間
            var orders = _context.OrderDetails
                .Where(k => !(k.CheckInDate >= indexhotelSearchDTO.CheckOutDate || k.CheckOutDate <= indexhotelSearchDTO.CheckInDate))
                .Select(k => k.RoomId)
                .ToList();

            // 查詢所有旅館
            var hotels = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.City)
                .ThenInclude(c => c.Country)
                .Include(h => h.HotelEquipmentReferences)
                .ThenInclude(r => r.HotelEquipment)
                .Include(h => h.HotelImages)
                .ToList(); // 先在用戶端載入所有旅館資料

            // 篩選符合條件的旅館
            var hotelsQuery = hotels
                .AsEnumerable() // 強制在用戶端進行計算
                .Select(h => new
                {
                    Hotel = h,
                    TopRooms = h.Rooms
                        .Where(r => !orders.Contains(r.RoomId))
                        .GroupBy(r => r.HotelId)
                        .Where(g => g.Count() >= indexhotelSearchDTO.roomnum)
                        .SelectMany(g => g.OrderByDescending(r => r.MaximumOccupancy).Take(indexhotelSearchDTO.roomnum))
                        .ToList()
                })
                .Where(x => x.TopRooms.Count == indexhotelSearchDTO.roomnum && x.TopRooms.Sum(r => r.MaximumOccupancy) >= totalPeople)
                .Select(x => new HotelSearchBox
                {
                    HotelId = x.Hotel.HotelId,
                    HotelName = x.Hotel.HotelName,
                    HotelAddress = x.Hotel.HotelAddress,
                    HotelPhone = x.Hotel.HotelPhone,
                    HotelDescription = x.Hotel.HotelDescription,
                    LevelStar = x.Hotel.LevelStar,
                    Latitude = x.Hotel.Latitude,
                    Longitude = x.Hotel.Longitude,
                    IsActive = x.Hotel.IsActive,
                    MemberId = x.Hotel.MemberId,
                    CityName = x.Hotel.City.CityName,
                    CountryName = x.Hotel.City.Country.CountryName,
                    HotelEquipmentName = x.Hotel.HotelEquipmentReferences.Select(e => e.HotelEquipment.HotelEquipmentName).FirstOrDefault(),
                    HotelImage = x.Hotel.HotelImages.Select(img => img.HotelImage1).FirstOrDefault(),
                    HotelPrice = (int)Math.Round(x.Hotel.Rooms.Average(p => p.RoomPrice)), // 將平均值轉換為整數
                });

            // 根據關鍵字篩選旅館
            if (!string.IsNullOrEmpty(indexhotelSearchDTO.keyword))
            {
                hotelsQuery = hotelsQuery.Where(s => s.HotelName.Contains(indexhotelSearchDTO.keyword) || s.HotelDescription.Contains(indexhotelSearchDTO.keyword));
            }

            // 根據價格範圍篩選旅館
            if (indexhotelSearchDTO.lowerPrice.HasValue && indexhotelSearchDTO.upperPrice.HasValue)
            {
                hotelsQuery = hotelsQuery.Where(h => h.HotelPrice >= indexhotelSearchDTO.lowerPrice.Value && h.HotelPrice <= indexhotelSearchDTO.upperPrice.Value);
            }


            //排序
            switch (indexhotelSearchDTO.sortBy)
            {
                case "LevelStar":
                    hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.LevelStar) : hotelsQuery.OrderByDescending(s => s.LevelStar);
                    break;
                case "HotelPrice":
                    hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelPrice) : hotelsQuery.OrderByDescending(s => s.HotelPrice);
                    break;
                case "CityName":
                    hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.CityName) : hotelsQuery.OrderByDescending(s => s.CityName);
                    break;
                default:
                    hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelId) : hotelsQuery.OrderByDescending(s => s.HotelId);
                    break;
            }


            // 執行查詢並將結果轉換為 HotelSearchBox
            var hotelList = hotelsQuery.ToList();
            return Ok(hotelList);

        }


        //checkbox新方法
        [HttpPost("advancedsearch")]
        public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetHotelsByAdvancedSearch([FromBody] IndexHotelSearchDTO request)
        {
            // 添加日誌來檢查接收到的請求資料
            Console.WriteLine("Received request: " + JsonConvert.SerializeObject(request));

            var hotelsQuery = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.City)
                    .ThenInclude(c => c.Country)
                .Include(h => h.HotelEquipmentReferences)
                    .ThenInclude(r => r.HotelEquipment)
                .Include(h => h.HotelImages)
                .AsQueryable();

            // 根據新篩選條件篩選
            if (!string.IsNullOrEmpty(request.keyword))
            {
                hotelsQuery = hotelsQuery.Where(h => h.HotelName.Contains(request.keyword) || h.HotelDescription.Contains(request.keyword));
            }

            if (request.lowerPrice.HasValue)
            {
                hotelsQuery = hotelsQuery.Where(h => h.Rooms.Any(r => r.RoomPrice >= request.lowerPrice));
            }

            if (request.upperPrice.HasValue)
            {
                hotelsQuery = hotelsQuery.Where(h => h.Rooms.Any(r => r.RoomPrice <= request.upperPrice));
            }

            if (request.HotelTypes != null && request.HotelTypes.Any())
            {
                hotelsQuery = hotelsQuery.Where(h => request.HotelTypes.Contains(h.HotelTypeId));
            }

            if (request.HotelEquipments != null && request.HotelEquipments.Any())
            {
                hotelsQuery = hotelsQuery.Where(h => h.HotelEquipmentReferences.Any(e => request.HotelEquipments.Contains(e.HotelEquipmentId)));
            }

            if (request.Cities != null && request.Cities.Any())
            {
                hotelsQuery = hotelsQuery.Where(h => request.Cities.Contains(h.City.CityId));
            }

            //if (request.RoomEquipments != null && request.RoomEquipments.Any())
            //{
            //    hotelsQuery = hotelsQuery.Where(h => h.Rooms.Any(r => request.RoomEquipments.Contains(r.RoomEquipmentId)));
            //}

            // 排序邏輯
            if (!string.IsNullOrEmpty(request.sortBy))
            {
                switch (request.sortBy.ToLower())
                {
                    case "hotelid":
                        hotelsQuery = request.sortType.ToLower() == "asc" ? hotelsQuery.OrderBy(h => h.HotelId) : hotelsQuery.OrderByDescending(h => h.HotelId);
                        break;
                    case "levelstar":
                        hotelsQuery = request.sortType.ToLower() == "asc" ? hotelsQuery.OrderBy(h => h.LevelStar) : hotelsQuery.OrderByDescending(h => h.LevelStar);
                        break;
                    case "hotelprice":
                        hotelsQuery = request.sortType.ToLower() == "asc" ? hotelsQuery.OrderBy(h => h.Rooms.Average(r => r.RoomPrice)) : hotelsQuery.OrderByDescending(h => h.Rooms.Average(r => r.RoomPrice));
                        break;
                    case "cityname":
                        hotelsQuery = request.sortType.ToLower() == "asc" ? hotelsQuery.OrderBy(h => h.City.CityName) : hotelsQuery.OrderByDescending(h => h.City.CityName);
                        break;
                    default:
                        break;
                }
            }

            // 執行查詢並將結果轉換為 HotelSearchBox
            var result = await hotelsQuery.Select(h => new HotelSearchBox
            {
                HotelId = h.HotelId,
                HotelName = h.HotelName,
                HotelAddress = h.HotelAddress,
                HotelPhone = h.HotelPhone,
                HotelDescription = h.HotelDescription,
                LevelStar = h.LevelStar,
                Latitude = h.Latitude,
                Longitude = h.Longitude,
                IsActive = h.IsActive,
                MemberId = h.MemberId,
                CityName = h.City.CityName,
                CountryName = h.City.Country.CountryName,
                HotelEquipmentName = h.HotelEquipmentReferences.Select(e => e.HotelEquipment.HotelEquipmentName).FirstOrDefault(),
                HotelImage = h.HotelImages.Select(img => img.HotelImage1).FirstOrDefault(),
                HotelPrice = (int)Math.Round(h.Rooms.Average(p => p.RoomPrice)), // 將平均值轉換為整數
            }).ToListAsync();

            // 添加日誌來檢查返回的酒店數據
            Console.WriteLine("Returned hotels: " + JsonConvert.SerializeObject(result));

            return Ok(result);
        }

    }
}
