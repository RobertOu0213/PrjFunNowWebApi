using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.julia_class;


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

               // 首先，执行基本查询并将结果加载到内存中
            var hotelData = await query
                .GroupBy(h => new { h.City.CityName, h.City.Country.CountryName })
                .Select(g => new {
                    City = g.Key.CityName,
                    Country = g.Key.CountryName,
                    Hotels = g.ToList()  // 将组内的酒店列表一起取出
                })
                .ToListAsync();
            // 然后，在内存中处理 null 值和提取图片 URL
            var groupedResult = hotelData.Select(g => new {
                City = g.City,
                Country = g.Country,
                HotelCount = g.Hotels.Count,
                ImageUrl = string.IsNullOrEmpty(g.Hotels
                    .SelectMany(h => h.HotelImages)
                    .Select(img => img.HotelImage1)
                    .FirstOrDefault())? "https://stickershop.line-scdn.net/stickershop/v1/sticker/548880784/IOS/sticker.png"
                                               : g.Hotels
                    .SelectMany(h => h.HotelImages)
                    .Select(img => img.HotelImage1)
                    .FirstOrDefault()
                      }).ToList();
                 return Ok(groupedResult);
        }

        [HttpGet("{memberId}/FavoriteHotels/{cityName}")]
        public async Task<ActionResult<IEnumerable<object>>> GetFavoriteHotelsForCity(int memberId, string cityName)
        {
            // 获取会员喜欢的酒店ID列表
            var memberLikes = await _context.HotelLikes
                        .Where(hl => hl.MemberId == memberId && hl.LikeStatus == true)
                        .Select(hl => hl.HotelId)
                        .ToListAsync();

            // 获取对应城市的酒店列表
            var hotels = await _context.Hotels
                        .Where(h => memberLikes.Contains(h.HotelId) && h.City.CityName == cityName)
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
                    hotel.Country,
                    hotel.LevelStar,
                    hotel.MinimumPrice,
                    hotel.HotelImage,
                    Rating = response  // 将评分添加到输出中
                });
            }

            return Ok(hotelsWithRatings);
        }



        [HttpPut("{memberId}/{hotelId}")]
        public async Task<IActionResult> UpdateLikeStatus(int memberId, int hotelId,  bool likeStatus)
        {
            Console.WriteLine($"Received request to update like status for memberId: {memberId}, hotelId: {hotelId}, likeStatus: {likeStatus}");
            var hotelLike = await _context.HotelLikes.FirstOrDefaultAsync(hl => hl.MemberId == memberId && hl.HotelId == hotelId);

            if (hotelLike == null)
            {
                hotelLike = new HotelLike
                {
                    MemberId = memberId,
                    HotelId = hotelId,
                    LikeStatus = likeStatus
                };
                _context.HotelLikes.Add(hotelLike);
            }
            else
            {
                hotelLike.LikeStatus = likeStatus;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }






    }



     
       
    }

