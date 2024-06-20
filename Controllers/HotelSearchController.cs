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


        [HttpGet]
        [Route("suggestions")]
        public async Task<ActionResult<IEnumerable<HotelSearchBoxDTO>>> GetHotelSuggestions([FromQuery] string keyword)
        {
            // 檢查關鍵字是否為空或 null
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest("Keyword is required."); // 如果關鍵字為空，返回 400 錯誤請求
            }

            // 從資料庫中查詢酒店建議
            var suggestions = await _context.Hotels
                .Where(h => h.HotelName.Contains(keyword) || h.City.CityName.Contains(keyword)) // 篩選酒店名稱或城市名稱包含關鍵字的酒店
                .Select(h => new HotelSearchBoxDTO
                {
                    HotelName = h.HotelName, // 酒店名稱
                    CityName = h.City.CityName + ", " + h.City.Country.CountryName, // 城市名稱及國家名稱
                })
                .Distinct() // 去除重複的建議
                .Take(10) // 限制返回的建議數量為 10 個
                .ToListAsync(); // 轉換為列表並執行異步操作

            return Ok(suggestions); // 返回 200 狀態碼和建議列表
        }




        [HttpPost]
        [Route("indexsearch")]
        public async Task<ActionResult<IEnumerable<HotelSearchBoxDTO>>> GetHotelsByIndexSearch([FromBody] IndexHotelSearchDTO indexhotelSearchDTO)
        {
            try
            {
                // 檢查輸入的搜尋條件是否為 null
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
                    // 包含飯店的房間信息
                    .Where(h => h.IsActive == true)
                    .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomEquipmentReferences)
                    .ThenInclude(re => re.RoomEquipment)
                    .Include(h => h.City).ThenInclude(c => c.Country)
                    .Include(h => h.HotelEquipmentReferences).ThenInclude(r => r.HotelEquipment)
                    .Include(h => h.HotelImages)
                    .ToListAsync();

                // 過濾掉已被訂走的房間，並確保每個飯店有足夠的房間數和容納人數，生成包含飯店和房間的查詢結果。
                var hotelsQuery = hotels
                    .AsEnumerable() // 將資料庫查詢結果轉換為本地集合，以便在記憶體中進行操作 可+可不+
                    .Select(h => new
                    {
                        Hotel = h,
                        // 過濾掉已被訂走的房間，並確保每個飯店有足夠的房間數和容納人數
                        TopRooms = h.Rooms
                            .Where(r => !orders.Contains(r.RoomId)) // 過濾掉已被訂走的房間
                            .GroupBy(r => r.HotelId) // 按照飯店 ID 進行分組
                            .Where(g => g.Count() >= indexhotelSearchDTO.roomnum) // 確保每個飯店有足夠的房間數
                            .SelectMany(g => g.OrderByDescending(r => r.MaximumOccupancy).Take(indexhotelSearchDTO.roomnum)) // 選擇最多容納人數的房間
                            .ToList(), // 將結果轉換為列表
                        AvailableRooms = h.Rooms.Count(r => !orders.Contains(r.RoomId))  // 計算空房間數
                    })
                    // 確保每個飯店的房間數符合要求，並且總容納人數足夠
                    .Where(x => x.TopRooms.Count == indexhotelSearchDTO.roomnum && x.TopRooms.Sum(r => r.MaximumOccupancy) >= totalPeople)
                    .Select(x => new
                    {
                        x.Hotel,
                        x.TopRooms, //容納得下的房間
                                    // 建立包含飯店和房間的查詢結果
                        HotelSearchBoxDTO = new HotelSearchBoxDTO
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
                            HotelEquipmentNames = x.Hotel.HotelEquipmentReferences
                            .Select(e => e.HotelEquipment.HotelEquipmentName).ToList(),// 獲取飯店的所有設備名稱並轉換為列表
                            HotelImages = x.Hotel.HotelImages.Select(img => img.HotelImage1).ToList(), //獲取飯店的所有圖片 URL 並轉換為列表
                            HotelPrice = (int)Math.Round(x.TopRooms.Average(p => p.RoomPrice)),// 計算選擇房間的平均價格並四捨五入為整數
                            HotelTypeId = x.Hotel.HotelTypeId,// 獲取飯店的類型 ID
                            RoomEquipmentNames = x.TopRooms
                                .SelectMany(r => r.RoomEquipmentReferences)// 從選中的房間中選取所有的設備參考
                                .Select(re => re.RoomEquipment.RoomEquipmentName)
                                .Distinct()// 去除重複的設備名稱
                                .ToList(),// 獲取所有選擇房間的設備名稱，去重覆後轉換為列表(代表旅館內所有房間的設備)
                            TotalAverageScore = GetAverageScoreForHotel(x.Hotel.HotelId), // 调用方法获取评分
                            AvailableRooms = x.AvailableRooms  // 設置空房間數
                        }
                    });

                // 处理其他筛选条件和排序逻辑
                if (!string.IsNullOrEmpty(indexhotelSearchDTO.keyword))
                {
                    hotelsQuery = hotelsQuery.Where(s => s.Hotel.HotelName.Contains(indexhotelSearchDTO.keyword) ||
                    s.Hotel.HotelDescription.Contains(indexhotelSearchDTO.keyword) ||
                    s.Hotel.City.CityName.Contains(indexhotelSearchDTO.keyword));
                }

                if (indexhotelSearchDTO.lowerPrice.HasValue && indexhotelSearchDTO.upperPrice.HasValue)
                {
                    hotelsQuery = hotelsQuery.Where(h => h.HotelSearchBoxDTO.HotelPrice >= indexhotelSearchDTO.lowerPrice.Value &&
                    h.HotelSearchBoxDTO.HotelPrice <= indexhotelSearchDTO.upperPrice.Value);
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

                // 根據用戶選擇的排序條件對飯店進行排序
                switch (indexhotelSearchDTO.sortBy)
                {
                    case "LevelStar": // 根據飯店星級進行排序，升序或降序
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBoxDTO.LevelStar) : hotelsQuery.OrderByDescending(s => s.HotelSearchBoxDTO.LevelStar);
                        break;
                    case "HotelPrice":
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBoxDTO.HotelPrice) : hotelsQuery.OrderByDescending(s => s.HotelSearchBoxDTO.HotelPrice);
                        break;
                    case "CityName":
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBoxDTO.CityName) : hotelsQuery.OrderByDescending(s => s.HotelSearchBoxDTO.CityName);
                        break;
                    case "TotalAverageScore": // 根據飯店總平均評分進行排序，升序或降序
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBoxDTO.TotalAverageScore) : hotelsQuery.OrderByDescending(s => s.HotelSearchBoxDTO.TotalAverageScore);
                        break;
                    default: // 根據飯店 ID 進行排序（默認排序）
                        hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelSearchBoxDTO.HotelId) : hotelsQuery.OrderByDescending(s => s.HotelSearchBoxDTO.HotelId);
                        break;
                }

                // 將字符串轉換為 List<string>
                var hotelList = hotelsQuery.Select(x => x.HotelSearchBoxDTO).ToList();

                foreach (var hotel in hotelList)
                {
                    // 將飯店設備名稱的字符串按逗號分隔，轉換為列表，並去除空白
                    hotel.HotelEquipmentNames = hotel.HotelEquipmentNames
                        .SelectMany(names => names.Split(',')) // 按逗號分隔設備名稱
                        .Select(name => name.Trim()).ToList(); // 去除每個設備名稱前後的空白，將結果轉換為列表
                    hotel.RoomEquipmentNames = hotel.RoomEquipmentNames
                        .SelectMany(names => names.Split(',')) // 按逗號分隔設備名稱
                        .Select(name => name.Trim()).ToList(); // 去除每個設備名稱前後的空白，將結果轉換為列表
                }

                // 返回處理過的飯店列表，並以 200 OK 狀態碼作為回應
                return Ok(hotelList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "發生錯誤：" + ex.Message);
            }
        }




        private decimal? GetAverageScoreForHotel(int hotelId)
        {   // 從評論中獲取所有評分
            var ratingScores = _context.Comments
                                       .Where(c => c.HotelId == hotelId)// 篩選出特定飯店的評論
                                       .SelectMany(c => c.RatingScores)// 獲取每條評論中的所有評分
                                       .ToList();// 將結果轉換為列表
            
            if (!ratingScores.Any()) return null;// 如果沒有評分，返回 null

            var averageScores = new // 計算每個評分項目的平均分
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

            return totalAverageScore;// 返回總平均分
        }

        [HttpPost]
        [Route("hotelLike")]
        public IActionResult Post([FromBody] HotelLike hotelLike)
        {
            if (hotelLike == null) //如果傳遞過來的對象為空，返回一個回應，表示錯誤的請求。
            {
                return BadRequest("HotelLike object is null.");
            }

            // 檢查是否已存在該用戶對該飯店的喜歡記錄//在資料庫中查找是否已經存在該用戶對該飯店的喜歡記錄
            var existingLike = _context.HotelLikes
                .FirstOrDefault(h => h.HotelId == hotelLike.HotelId && h.MemberId == hotelLike.MemberId);

            if (existingLike != null)
            {
                // 如果存在，更新現有記錄的 LikeStatus
                existingLike.LikeStatus = hotelLike.LikeStatus;
            }
            else
            {
                // 如果不存在，添加新記錄
                _context.HotelLikes.Add(hotelLike);
            }

            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("likedHotels/{memberId}")]
        public async Task<ActionResult<IEnumerable<int>>> GetLikedHotels(int memberId)
        {
            var likedHotelIds = await _context.HotelLikes
                .Where(like => like.MemberId == memberId && like.LikeStatus)
                .Select(like => like.HotelId)
                .ToListAsync();

            return Ok(likedHotelIds);
        }
    }

}



