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
