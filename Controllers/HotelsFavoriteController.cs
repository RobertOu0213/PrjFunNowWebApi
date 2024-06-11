using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsFavoriteController : ControllerBase
    {
        private readonly FunNowContext _context;

        public HotelsFavoriteController(FunNowContext context)
        {
            _context = context;
        }
        // GET: api/Members/{memberId}/favorite-hotels
        [HttpGet("{memberId}/FavoriteHotels")]
        public async Task<ActionResult<IEnumerable<Hotel>>> GetFavoriteHotels(int memberId)
        {
            var memberLikes = await _context.HotelLikes
                         .Where(hl => hl.MemberId == memberId && hl.LikeStatus == true)
                         .Select(hl => hl.HotelId)
                         .ToListAsync();

            IQueryable<Hotel> query = _context.Hotels
                                               .Where(h => memberLikes.Contains(h.HotelId))
                                              .Include(h => h.HotelImages);  // 確保加載相關的圖片數據
            var groupedResult = await query
                                 .GroupBy(h => new { h.City.CityName, h.City.Country.CountryName/*,h.HotelImages */})
                                 .Select(g => new {
                                     //g.Key.HotelImages,
                                     City = g.Key.CityName,
                                     Country = g.Key.CountryName,
                                     HotelCount = g.Count(),
                                     ImageUrl = g.FirstOrDefault().HotelImages.FirstOrDefault().HotelImage1 // 假設每個酒店至少有一張圖片// 使用空條件運算符和預設圖片
                                 })
                                 .ToListAsync();
            return Ok(groupedResult);


        }

        [HttpGet("{memberId}/FavoriteHotels/{cityName}")]
        public async Task<ActionResult<IEnumerable<Hotel>>> GetFavoriteHotelsForCity(int memberId, string cityName)
        {
            var memberLikes = await _context.HotelLikes
                                .Where(hl => hl.MemberId == memberId && hl.LikeStatus == true)
                                .Select(hl => hl.HotelId)
                                .ToListAsync();

            var hotels = await _context.Hotels
                            .Where(h => memberLikes.Contains(h.HotelId) && h.City.CityName == cityName)
                             .Select(h => new {
                                 //HotelImage = h.HotelImages,
                                 HotelName = h.HotelName,
                                 City = h.City.CityName,
                                 Country = h.City.Country.CountryName,
                                 LevelStar = h.LevelStar,
                                 //hotel.rating,
                                 //hotel.reviewCount,
                                 MinimumPrice = h.Rooms.Min(r => r.RoomPrice),
                                 HotelImage = h.HotelImages.FirstOrDefault().HotelImage1 // 假设每个酒店至少有一张图片
                             })
                             .ToListAsync();

            return Ok(hotels);
        }

    }



     
       
    }

