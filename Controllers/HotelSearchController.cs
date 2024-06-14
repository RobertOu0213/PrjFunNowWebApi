using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using System.Linq;

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
            try
            {
                if (indexhotelSearchDTO == null)
                {
                    return BadRequest("無效的輸入資料。");
                }

                int totalPeople = (indexhotelSearchDTO.adults ?? 0) + (indexhotelSearchDTO.children ?? 0);

                // 查找在搜尋日期範圍內已有訂單的房間 ID。
                var orders = await _context.OrderDetails
                    .Where(k => !(k.CheckInDate >= indexhotelSearchDTO.CheckOutDate || k.CheckOutDate <= indexhotelSearchDTO.CheckInDate))
                    .Select(k => k.RoomId)
                    .ToListAsync();

                // 查找所有飯店，並包含其房間、城市、國家、設備和圖片等相關信息。
                var hotels = await _context.Hotels
                    .Include(h => h.Rooms)
                        .ThenInclude(r => r.RoomEquipmentReferences)
                            .ThenInclude(re => re.RoomEquipment)
                    .Include(h => h.City).ThenInclude(c => c.Country)
                    .Include(h => h.HotelEquipmentReferences).ThenInclude(r => r.HotelEquipment)
                    .Include(h => h.HotelImages)
                    .ToListAsync();

                // 過濾掉已被訂走的房間，並確保每個飯店有足夠的房間數和容納人數，生成包含飯店和房間的查詢結果。
                var hotelsQuery = hotels
                    .AsEnumerable()
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
                    .Select(x => new
                    {
                        x.Hotel,
                        x.TopRooms,
                        HotelSearchBox = new HotelSearchBox
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
                            HotelEquipmentNames = x.Hotel.HotelEquipmentReferences.Select(e => e.HotelEquipment.HotelEquipmentName).ToList(),
                            HotelImage = x.Hotel.HotelImages.Select(img => img.HotelImage1).FirstOrDefault(),
                            HotelPrice = (int)Math.Round(x.TopRooms.Average(p => p.RoomPrice)),
                            HotelTypeId = x.Hotel.HotelTypeId,
                            RoomEquipmentNames = x.TopRooms
                                .SelectMany(r => r.RoomEquipmentReferences)
                                .Select(re => re.RoomEquipment.RoomEquipmentName)
                                .Distinct()
                                .ToList(),
                            TotalAverageScore = GetAverageScoreForHotel(x.Hotel.HotelId) // 调用方法获取评分
                        }
                    });

                // 处理搜索关键字、价格、酒店类型等筛选条件
                if (!string.IsNullOrEmpty(indexhotelSearchDTO.keyword))
                {
                    hotelsQuery = hotelsQuery.Where(s => s.Hotel.HotelName.Contains(indexhotelSearchDTO.keyword) || s.Hotel.HotelDescription.Contains(indexhotelSearchDTO.keyword));
                }

                if (indexhotelSearchDTO.lowerPrice.HasValue && indexhotelSearchDTO.upperPrice.HasValue)
                {
                    hotelsQuery = hotelsQuery.Where(h => h.HotelSearchBox.HotelPrice >= indexhotelSearchDTO.lowerPrice.Value && h.HotelSearchBox.HotelPrice <= indexhotelSearchDTO.upperPrice.Value);
                }

                if (indexhotelSearchDTO.HotelTypes != null && indexhotelSearchDTO.HotelTypes.Any())
                {
                    hotelsQuery = hotelsQuery.Where(h => indexhotelSearchDTO.HotelTypes.Contains(h.Hotel.HotelTypeId));
                }

                if (indexhotelSearchDTO.HotelEquipments != null && indexhotelSearchDTO.HotelEquipments.Any())
                {
                    hotelsQuery = hotelsQuery.Where(h => h.Hotel.HotelEquipmentReferences.Any(e => indexhotelSearchDTO.HotelEquipments.Contains(e.HotelEquipment.HotelEquipmentId)));
                }

                if (indexhotelSearchDTO.Cities != null && indexhotelSearchDTO.Cities.Any())
                {
                    hotelsQuery = hotelsQuery.Where(h => indexhotelSearchDTO.Cities.Contains(h.Hotel.CityId));
                }

                if (indexhotelSearchDTO.RoomEquipments != null && indexhotelSearchDTO.RoomEquipments.Any())
                {
                    hotelsQuery = hotelsQuery.Where(h => h.TopRooms.Any(r => r.RoomEquipmentReferences.Any(re => indexhotelSearchDTO.RoomEquipments.Contains(re.RoomEquipment.RoomEquipmentId))));
                }

                // 添加对 TotalAverageScore 的排序
                switch (indexhotelSearchDTO.sortBy)
                {
                    case "LevelStar":
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBox.LevelStar) : hotelsQuery.OrderByDescending(s => s.HotelSearchBox.LevelStar);
                        break;
                    case "HotelPrice":
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBox.HotelPrice) : hotelsQuery.OrderByDescending(s => s.HotelSearchBox.HotelPrice);
                        break;
                    case "CityName":
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBox.CityName) : hotelsQuery.OrderByDescending(s => s.HotelSearchBox.CityName);
                        break;
                    case "TotalAverageScore":
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBox.TotalAverageScore) : hotelsQuery.OrderByDescending(s => s.HotelSearchBox.TotalAverageScore);
                        break;
                    default:
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBox.HotelId) : hotelsQuery.OrderByDescending(s => s.HotelSearchBox.HotelId);
                        break;
                }

                var hotelList = hotelsQuery.Select(x => x.HotelSearchBox).ToList();

                // 將字符串轉換為 List<string>
                foreach (var hotel in hotelList)
                {
                    hotel.HotelEquipmentNames = hotel.HotelEquipmentNames.SelectMany(names => names.Split(',')).Select(name => name.Trim()).ToList();
                    hotel.RoomEquipmentNames = hotel.RoomEquipmentNames.SelectMany(names => names.Split(',')).Select(name => name.Trim()).ToList();
                }

                return Ok(hotelList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "發生錯誤：" + ex.Message);
            }
        }

        private decimal? GetAverageScoreForHotel(int hotelId)
        {
            var ratingScores = _context.Comments
                                       .Where(c => c.HotelId == hotelId)
                                       .SelectMany(c => c.RatingScores)
                                       .ToList();

            if (!ratingScores.Any()) return null;

            var averageScores = new
            {
                ComfortScore = ratingScores.Average(r => r.ComfortScore),
                CleanlinessScore = ratingScores.Average(r => r.CleanlinessScore),
                StaffScore = ratingScores.Average(r => r.StaffScore),
                FacilitiesScore = ratingScores.Average(r => r.FacilitiesScore),
                ValueScore = ratingScores.Average(r => r.ValueScore),
                LocationScore = ratingScores.Average(r => r.LocationScore),
                FreeWifiScore = ratingScores.Average(r => r.FreeWifiScore)
            };

            var totalAverageScore = (averageScores.ComfortScore +
                                     averageScores.CleanlinessScore +
                                     averageScores.StaffScore +
                                     averageScores.FacilitiesScore +
                                     averageScores.ValueScore +
                                     averageScores.LocationScore +
                                     averageScores.FreeWifiScore) / 7;

            return totalAverageScore;
        }

        [HttpPost]
        [Route("hotelLike")]
        public IActionResult Post([FromBody] HotelLike hotelLike)
        {
            if (hotelLike == null) //如果傳遞過來的對象為空，返回一個回應，表示錯誤的請求。
            {
                return BadRequest("HotelLike object is null.");
            }

            // 檢查是否已存在該用戶對該飯店的喜歡記錄
            var existingLike = _context.HotelLikes
                .FirstOrDefault(h => h.HotelId == hotelLike.HotelId && h.MemberId == hotelLike.MemberId);

            if (existingLike != null)
            {
                // 更新現有記錄的 LikeStatus
                existingLike.LikeStatus = hotelLike.LikeStatus;
            }
            else
            {
                // 添加新記錄
                _context.HotelLikes.Add(hotelLike);
            }

            _context.SaveChanges();

            return Ok();
        }
    }

}



