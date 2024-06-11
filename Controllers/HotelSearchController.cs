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
            ////計算每間房人數
            //int peopleperroom = (totalPeople / indexhotelSearchDTO.roomnum)+1; //無條件進位
            // 查詢已被訂房的房間
            var orders = from k in _context.OrderDetails
                         where !(k.CheckInDate >= indexhotelSearchDTO.CheckOutDate || k.CheckOutDate <= indexhotelSearchDTO.CheckInDate)
                         select k.RoomId;

            // 查詢尚未被訂房的房間
            var rooms = from r in _context.Rooms
                        where !orders.Contains(r.RoomId)
                        select r.HotelId;

            // 查詢尚有空房的旅館並且總人數不超過旅館最大容納人數
            var hotelsQuery = from h in _context.Hotels
                              where rooms.Contains(h.HotelId) && totalPeople <= h.Rooms.Sum(s => s.MaximumOccupancy)
                              select new HotelSearchBox
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
                                  HotelEquipmentName = h.HotelEquipmentReferences.Select(e => e.HotelEquipment.HotelEquipmentName).FirstOrDefault(), // 假设 HotelEquipmentReferences 是一个包含设备名称的集合
                                  HotelImage = h.HotelImages.Select(img => img.HotelImage1).FirstOrDefault(), // 假设 HotelImages 是一个包含图片URL的集合
                                  HotelPrice = h.Rooms.Average(p => p.RoomPrice),
                                  HotelMaximumOccupancy = h.Rooms.Sum(s => s.MaximumOccupancy)
                              };

            // 根據關鍵字篩選旅館
            if (!string.IsNullOrEmpty(indexhotelSearchDTO.keyword))
            {
                hotelsQuery = hotelsQuery.Where(s => s.HotelName.Contains(indexhotelSearchDTO.keyword) || s.HotelDescription.Contains(indexhotelSearchDTO.keyword));
            }

            // 執行查詢並將結果轉換為 HotelSearchBox
            var hotelList = await hotelsQuery.ToListAsync();
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
