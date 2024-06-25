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
            var groupedResult = await query
                  .GroupBy(h => new { h.City.CityName, h.City.Country.CountryName })
                  .Select(g => new {
                      City = g.Key.CityName,
                      Country = g.Key.CountryName,
                      HotelCount = g.Count(),
                      HotelImage = g.FirstOrDefault().HotelImages.FirstOrDefault().HotelImage1 // 假設每個酒店至少有一張圖片// 使用空條件運算符和預設圖片
                  })
                                 .ToListAsync();
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
                            HotelPrice = (int)Math.Round(h.Rooms.Average(p => p.RoomPrice)),
                            HotelImage = h.HotelImages.FirstOrDefault().HotelImage1
                        })
                        .ToListAsync();

            // HttpClient 初始化
            HttpClient httpClient = new HttpClient();

            // 为每个酒店添加评分
            var hotelsWithRatings = new List<object>();
            foreach (var hotel in hotels)
            {
                // 第一個 API 請求：獲取平均評分
                string ratingUrl = $"https://localhost:7103/api/Comment/{hotel.HotelId}/AverageScores";
                var response = await httpClient.GetStringAsync(ratingUrl);                       // 假设response直接返回一个评分值


                // 第二個 API 請求：獲取評論總數
                string commentCountUrl = $"https://localhost:7103/api/Comment/commentCounts";
                var commentCountResponse = await httpClient.GetStringAsync(commentCountUrl);

                hotelsWithRatings.Add(new
                {
                    hotel.HotelId,
                    hotel.HotelName,
                    hotel.City,
                    hotel.Country,
                    hotel.LevelStar,
                    hotel.HotelPrice,
                    hotel.HotelImage,
                    Rating = response,  // 将评分添加到输出中
                    TotalComments = commentCountResponse  // 新添加的評論總數
                });
            }

            return Ok(hotelsWithRatings);
        }



        [HttpPut("{memberId}/{hotelId}")]   //不要用這個
        public async Task<IActionResult> UpdateLikeStatus(int memberId, int hotelId, [FromBody] bool likeStatus)
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



        [HttpGet("like/{memberId}/{hotelId}")]   //不用POST原因是因為沒有機密數據,所以用GET
        public async Task<IActionResult> UpdateLike(int memberId, int hotelId)
        {
         //   Console.WriteLine($"Received request to update like status for memberId: {memberId}, hotelId: {hotelId}, likeStatus: {likeStatus}");
            var hotelLike = await _context.HotelLikes.FirstOrDefaultAsync(hl => hl.MemberId == memberId && hl.HotelId == hotelId);

            //如果沒有hotelLike就新增
            if (hotelLike == null)
            {
                hotelLike = new HotelLike
                {
                    MemberId = memberId,
                    HotelId = hotelId,
                    LikeStatus = true
                };
                _context.HotelLikes.Add(hotelLike);
            }
            else
            {
                //如果有hotelike就改變likestatus的狀態
                hotelLike.LikeStatus = !hotelLike.LikeStatus;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet("hotelLikes/{memberId}")]
        public async Task<IActionResult> GetHotelLikes(int memberId)
        {
            var hotelLikes = await _context.HotelLikes
                .Where(hl => hl.MemberId == memberId)
                .Select(hl => new
                {
                    HotelId = hl.HotelId,
                    LikeStatus = hl.LikeStatus
                })
                .ToListAsync();

            return Ok(hotelLikes);
        }

    }





}

