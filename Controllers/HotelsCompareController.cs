using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class HotelsCompareController : ControllerBase
    {
        private readonly FunNowContext _context;

        public HotelsCompareController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/<HotelsCompareController>
 
      

        // GET api/<HotelsCompareController>/5
        [HttpGet("{hotelId}")]
        public async Task<IActionResult> GetCompareHotels( int hotelId)
        {
            // 获取对应的酒店列表
            var hotels = await _context.Hotels
                        .Where(h =>h.HotelId== hotelId)
                        .Include(h => h.HotelImages)
                        .Select(h => new {
                            HotelId = h.HotelId,
                            HotelName = h.HotelName,
                            City = h.City.CityName,
                            Country = h.City.Country.CountryName,
                            LevelStar = h.LevelStar,
                            MinimumPrice = h.Rooms.Min(r => r.RoomPrice),
                            HotelImage = h.HotelImages.FirstOrDefault().HotelImage1
                        })
                        .ToListAsync();
            // HttpClient 初始化
            HttpClient httpClient = new HttpClient();

            // 为每个酒店添加评分
            var hotelsWithRatings = new List<object>();
            foreach (var hotel in hotels)
            {
                string ratingUrl = $"https://localhost:7103/api/Comment/{hotel.HotelId}/AverageScores";
                var response = await httpClient.GetStringAsync(ratingUrl);
                // 假设response直接返回一个评分值

                hotelsWithRatings.Add(new
                {
                    hotel.HotelId,
                    hotel.HotelName,
                    hotel.City,
                    hotel.LevelStar,
                    hotel.MinimumPrice,
                    hotel.HotelImage,
                    Rating = response  // 将评分添加到输出中
                    //加個設施?
                });
            }

            return Ok(hotelsWithRatings);
        }


    }
}
