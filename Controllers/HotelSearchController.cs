using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetSpots()
        {
            return await _context.HotelSearchBoxes.ToListAsync();
        }
        [HttpGet]
        [Route("search")]
        public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetSpots1(string keyword)
        {
            return await _context.HotelSearchBoxes.Where(s => s.HotelName.Contains(keyword)).ToListAsync();
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

            // 執行查詢並將結果轉換為 HotelSearchBox
            var hotelList = hotelsQuery.ToList();
            return Ok(hotelList);

        }



        [HttpPost]
        public async Task<ActionResult<HotelsPagingDTO>> GetHotelsBySearch(HotelSearchDTO hotelSearchDTO)
        {

            //根據Hotel分類編號搜尋Hotel分類資料 //未修正
            var Hotels = hotelSearchDTO.HotelId == 0 ? _context.HotelSearchBoxes : _context.HotelSearchBoxes.Where(s => s.HotelId == hotelSearchDTO.HotelId);

            //根據關鍵字搜尋景點資料(HotelName、desc) 
            if (!string.IsNullOrEmpty(hotelSearchDTO.keyword))
            {
                Hotels = Hotels.Where(s => s.HotelName.Contains(hotelSearchDTO.keyword) || s.HotelDescription.Contains(hotelSearchDTO.keyword));
            }

            ////排序
            //switch (hotelSearchDTO.sortBy)
            //{
            //    case "HotelName":
            //        Hotels = hotelSearchDTO.sortType == "asc" ? Hotels.OrderBy(s => s.HotelName) : Hotels.OrderByDescending(s => s.HotelName);
            //        break;
            //    case "HotelId":
            //        Hotels = hotelSearchDTO.sortType == "asc" ? Hotels.OrderBy(s => s.HotelId) : Hotels.OrderByDescending(s => s.HotelId);
            //        break;
            //    default:
            //        Hotels = hotelSearchDTO.sortType == "asc" ? Hotels.OrderBy(s => s.LevelStar) : Hotels.OrderByDescending(s => s.LevelStar);
            //        break;
            //}

            //總共有多少筆資料
            int totalCount = Hotels.Count();
            ////每頁要顯示幾筆資料
            //int pageSize = hotelSearchDTO.pageSize ?? 9;   //searchDTO.pageSize ?? 9;
            ////目前第幾頁
            //int page = hotelSearchDTO.page ?? 1;

            ////計算總共有幾頁
            //int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);

            ////分頁
            //Hotels = Hotels.Skip((page - 1) * pageSize).Take(pageSize);


            //包裝要傳給client端的資料
            HotelsPagingDTO hotelsPaging = new HotelsPagingDTO();
            hotelsPaging.TotalCount = totalCount;
            //hotelsPaging.TotalPages = totalPages;
            hotelsPaging.HotelsResult = await Hotels.ToListAsync();


            return hotelsPaging;
        }



    }
}
