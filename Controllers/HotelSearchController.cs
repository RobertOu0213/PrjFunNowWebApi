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



        //[HttpPost]
        //[Route("indexsearch")]
        //public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetHotelsByIndexSearch([FromBody] IndexHotelSearchDTO indexhotelSearchDTO)
        //{
        //    try
        //    {
        //        // 数据验证
        //        if (indexhotelSearchDTO == null)
        //        {
        //            return BadRequest("無效的輸入資料。");
        //        }

        //        // 計算總人數
        //        int totalPeople = (indexhotelSearchDTO.adults ?? 0) + (indexhotelSearchDTO.children ?? 0);

        //        // 查找已預訂的房間
        //        var orders = await _context.OrderDetails
        //            .Where(k => !(k.CheckInDate >= indexhotelSearchDTO.CheckOutDate || k.CheckOutDate <= indexhotelSearchDTO.CheckInDate))
        //            .Select(k => k.RoomId)
        //            .ToListAsync();

        //        // 查詢所有旅館
        //        var hotels = await _context.Hotels
        //            .Include(h => h.Rooms)
        //            .Include(h => h.City)
        //                .ThenInclude(c => c.Country)
        //            .Include(h => h.HotelEquipmentReferences)
        //                .ThenInclude(r => r.HotelEquipment)
        //            .Include(h => h.HotelImages)
        //            .ToListAsync();

        //        // 篩選符合條件的旅館
        //        var hotelsQuery = hotels
        //            .AsEnumerable() // 強制在客戶端進行計算
        //            .Select(h => new
        //            {
        //                Hotel = h,
        //                TopRooms = h.Rooms
        //                    .Where(r => !orders.Contains(r.RoomId))
        //                    .GroupBy(r => r.HotelId)
        //                    .Where(g => g.Count() >= indexhotelSearchDTO.roomnum)
        //                    .SelectMany(g => g.OrderByDescending(r => r.MaximumOccupancy).Take(indexhotelSearchDTO.roomnum))
        //                    .ToList()
        //            })
        //            .Where(x => x.TopRooms.Count == indexhotelSearchDTO.roomnum && x.TopRooms.Sum(r => r.MaximumOccupancy) >= totalPeople)
        //            .Select(x => new HotelSearchBox
        //            {
        //                HotelId = x.Hotel.HotelId,
        //                HotelName = x.Hotel.HotelName,
        //                HotelAddress = x.Hotel.HotelAddress,
        //                HotelPhone = x.Hotel.HotelPhone,
        //                HotelDescription = x.Hotel.HotelDescription,
        //                LevelStar = x.Hotel.LevelStar,
        //                Latitude = x.Hotel.Latitude,
        //                Longitude = x.Hotel.Longitude,
        //                IsActive = x.Hotel.IsActive,
        //                MemberId = x.Hotel.MemberId,
        //                CityName = x.Hotel.City.CityName,
        //                CountryName = x.Hotel.City.Country.CountryName,
        //                HotelEquipmentName = x.Hotel.HotelEquipmentReferences.Select(e => e.HotelEquipment.HotelEquipmentName).FirstOrDefault(),
        //                HotelImage = x.Hotel.HotelImages.Select(img => img.HotelImage1).FirstOrDefault(),
        //                HotelPrice = (int)Math.Round(x.Hotel.Rooms.Average(p => p.RoomPrice)), // 將平均值轉換為整數
        //            });

        //        // 根據關鍵字篩選旅館
        //        if (!string.IsNullOrEmpty(indexhotelSearchDTO.keyword))
        //        {
        //            hotelsQuery = hotelsQuery.Where(s => s.HotelName.Contains(indexhotelSearchDTO.keyword) || s.HotelDescription.Contains(indexhotelSearchDTO.keyword));
        //        }

        //        // 根據價格範圍篩選旅館
        //        if (indexhotelSearchDTO.lowerPrice.HasValue && indexhotelSearchDTO.upperPrice.HasValue)
        //        {
        //            hotelsQuery = hotelsQuery.Where(h => h.HotelPrice >= indexhotelSearchDTO.lowerPrice.Value && h.HotelPrice <= indexhotelSearchDTO.upperPrice.Value);
        //        }

        //        // 根據選中的住宿類型篩選旅館
        //        if (indexhotelSearchDTO.HotelTypes != null && indexhotelSearchDTO.HotelTypes.Any())
        //        {
        //            hotelsQuery = hotelsQuery.Where(h => indexhotelSearchDTO.HotelTypes.Contains(h.HotelTypeId));
        //        }

        //        // 排序
        //        switch (indexhotelSearchDTO.sortBy)
        //        {
        //            case "LevelStar":
        //                hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.LevelStar) : hotelsQuery.OrderByDescending(s => s.LevelStar);
        //                break;
        //            case "HotelPrice":
        //                hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelPrice) : hotelsQuery.OrderByDescending(s => s.HotelPrice);
        //                break;
        //            case "CityName":
        //                hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.CityName) : hotelsQuery.OrderByDescending(s => s.CityName);
        //                break;
        //            default:
        //                hotelsQuery = indexhotelSearchDTO.sortType == "asc" ? hotelsQuery.OrderBy(s => s.HotelId) : hotelsQuery.OrderByDescending(s => s.HotelId);
        //                break;
        //        }

        //        // 執行查詢並將結果轉換為 HotelSearchBox
        //        var hotelList = hotelsQuery.ToList();
        //        return Ok(hotelList);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 錯誤處理
        //        return StatusCode(500, "發生錯誤：" + ex.Message);
        //    }
        //}

        [HttpPost]
        [Route("indexsearch")]
        public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetHotelsByIndexSearch([FromBody] IndexHotelSearchDTO indexhotelSearchDTO)
        {
            try
            {
                // 数据验证
                if (indexhotelSearchDTO == null)
                {
                    return BadRequest("無效的輸入資料。");
                }

                // 計算總人數
                int totalPeople = (indexhotelSearchDTO.adults ?? 0) + (indexhotelSearchDTO.children ?? 0);

                // 查找已預訂的房間
                var orders = await _context.OrderDetails
                    .Where(k => !(k.CheckInDate >= indexhotelSearchDTO.CheckOutDate || k.CheckOutDate <= indexhotelSearchDTO.CheckInDate))
                    .Select(k => k.RoomId)
                    .ToListAsync();

                // 查詢所有旅館
                var hotels = await _context.Hotels
                    .Include(h => h.Rooms)
                    .Include(h => h.City)
                        .ThenInclude(c => c.Country)
                    .Include(h => h.HotelEquipmentReferences)
                        .ThenInclude(r => r.HotelEquipment)
                    .Include(h => h.HotelImages)
                    .ToListAsync();

                // 篩選符合條件的旅館
                var hotelsQuery = hotels
                    .AsEnumerable() // 強制在客戶端進行計算
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
                        HotelTypeId = x.Hotel.HotelTypeId // 加入 HotelTypeId
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

                // 根據選中的住宿類型篩選旅館
                if (indexhotelSearchDTO.HotelTypes != null && indexhotelSearchDTO.HotelTypes.Any())
                {
                    hotelsQuery = hotelsQuery.Where(h => indexhotelSearchDTO.HotelTypes.Contains(h.HotelTypeId));
                }

                // 排序
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
            catch (Exception ex)
            {
                // 錯誤處理
                return StatusCode(500, "發生錯誤：" + ex.Message);
            }
        }


    }

}



