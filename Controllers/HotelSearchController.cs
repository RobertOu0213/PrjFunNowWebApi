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
                if (indexhotelSearchDTO == null)
                {
                    return BadRequest("無效的輸入資料。");
                }

                int totalPeople = (indexhotelSearchDTO.adults ?? 0) + (indexhotelSearchDTO.children ?? 0);

                var orders = await _context.OrderDetails
                    .Where(k => !(k.CheckInDate >= indexhotelSearchDTO.CheckOutDate || k.CheckOutDate <= indexhotelSearchDTO.CheckInDate))
                    .Select(k => k.RoomId)
                    .ToListAsync();

                var hotels = await _context.Hotels
                    .Include(h => h.Rooms)
                        .ThenInclude(r => r.RoomEquipmentReferences)
                            .ThenInclude(re => re.RoomEquipment)
                    .Include(h => h.City).ThenInclude(c => c.Country)
                    .Include(h => h.HotelEquipmentReferences).ThenInclude(r => r.HotelEquipment)
                    .Include(h => h.HotelImages)
                    .ToListAsync();

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
                                .ToList()
                        }
                    });

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


    }

}



