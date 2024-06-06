using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.joannaDTO;
using System.Reflection.Metadata.Ecma335;
using static PrjFunNowWebApi.Models.joannaDTO.CommentResponseDTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly FunNowContext _context;

        public CommentController(FunNowContext context) 
        {
            _context = context;
        }

       
        //從資料庫取評論
        [HttpGet("{hotelId}/Getcomments")]
        public IActionResult GetComments(int hotelId, int page = 1, int pageSize = 10, string search = null, int? ratingFilter = null, string dateFilter = null)
        {
            try
            {
                // 取得評論+篩選條件
                var commentsQuery = _context.Comments
                                            .Where(c => c.HotelId == hotelId)
                                            .Include(c => c.RatingScores)
                                            .AsQueryable();

                // 搜索過濾
                if (!string.IsNullOrEmpty(search))
                {
                    commentsQuery = commentsQuery.Where(c => c.CommentTitle.Contains(search) || c.CommentText.Contains(search));
                }

                // 評分篩選
                if (ratingFilter.HasValue && ratingFilter.Value > 0)
                {
                    //commentsQuery = commentsQuery.Where(c => c.RatingScores.Any(r => r.ComfortScore >= ratingFilter.Value));
                    commentsQuery = ApplyRatingFilter(commentsQuery, ratingFilter.Value);
                }

                // 日期篩選r
                if (!string.IsNullOrEmpty(dateFilter))
                    if (!string.IsNullOrEmpty(dateFilter) && DateTime.TryParse(dateFilter, out DateTime parsedDate))
                    {
                        commentsQuery = commentsQuery.Where(c => c.CreatedAt.Date == parsedDate.Date);
                    }

                var totalItems = commentsQuery.Count();

                var comments = commentsQuery
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CommentResponse
                    {
                        CommentId = c.CommentId,
                        HotelId = c.HotelId,
                        CommentTitle = c.CommentTitle,
                        CommentText = c.CommentText,
                        CreatedAt = c.CreatedAt,
                        RatingScores = c.RatingScores.Select(r => new RatingScoreDTO
                        {
                            RatingId = r.RatingId,
                            CommentId = r.CommentId,
                            ComfortScore = r.ComfortScore,
                            CleanlinessScore = r.CleanlinessScore,
                            StaffScore = r.StaffScore,
                            FacilitiesScore = r.FacilitiesScore,
                            ValueScore = r.ValueScore,
                            LocationScore = r.LocationScore,
                            FreeWifiScore = r.FreeWifiScore,
                            TravelerType = r.TravelerType
                        }).ToList()
                    }).ToList();

                var ratingScores = comments.SelectMany(c => c.RatingScores).ToList();

                var averageScores = new
                {
                    ComfortScore = ratingScores.Any() ? ratingScores.Average(r => r.ComfortScore) : 0,
                    CleanlinessScore = ratingScores.Any() ? ratingScores.Average(r => r.CleanlinessScore) : 0,
                    StaffScore = ratingScores.Any() ? ratingScores.Average(r => r.StaffScore) : 0,
                    FacilitiesScore = ratingScores.Any() ? ratingScores.Average(r => r.FacilitiesScore) : 0,
                    ValueScore = ratingScores.Any() ? ratingScores.Average(r => r.ValueScore) : 0,
                    LocationScore = ratingScores.Any() ? ratingScores.Average(r => r.LocationScore) : 0,
                    FreeWifiScore = ratingScores.Any() ? ratingScores.Average(r => r.FreeWifiScore) : 0
                    //travelerType = ratingScores.Any()?ratingScores(r => r.)
                };

                var hotelName = _context.Hotels
                                       .Where(h => h.HotelId == hotelId)
                                       .Select(h => h.HotelName)
                                       .FirstOrDefault();
                var totalAverageScore = (averageScores.ComfortScore +
                                         averageScores.CleanlinessScore +
                                         averageScores.StaffScore +
                                         averageScores.FacilitiesScore +
                                         averageScores.ValueScore +
                                         averageScores.LocationScore +
                                         averageScores.FreeWifiScore) / 7;

               

                return Ok(new
                {
                    TotalItems = totalItems,
                    Comments = comments,
                    AverageScore = averageScores,
                    TotalAverageScore = totalAverageScore,
                    HotelName = hotelName
                });
            }
            catch (Exception ex)
            {
                // 记录错误信息
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("commentCounts")]
        public async Task<IActionResult> GetCommentCounts()
        {     
            var counts = new Dictionary<int, int>
        {
            { 2, await ApplyRatingFilter(_context.Comments.AsQueryable(), 2).CountAsync() },
            { 3, await ApplyRatingFilter(_context.Comments.AsQueryable(), 3).CountAsync() },
            { 4, await ApplyRatingFilter(_context.Comments.AsQueryable(), 4).CountAsync() },
            { 5, await ApplyRatingFilter(_context.Comments.AsQueryable(), 5).CountAsync() },
            { 6, await ApplyRatingFilter(_context.Comments.AsQueryable(), 6).CountAsync() },
        };

            var total = await _context.Comments.CountAsync();

            return Ok(new { total, counts });
        }



        //評論分數篩選器
        private IQueryable<Comment> ApplyRatingFilter(IQueryable<Comment> query, int ratingFilter)
        {
            switch (ratingFilter)
            {
                case 2: // 超讚: 9+
                    query = query.Where(c => c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 9);
                    break;
                case 3: // 很讚: 7-9
                    query = query.Where(c => c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 7 &&
                        c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 9);
                    break;
                case 4: // 很好: 5-7
                    query = query.Where(c => c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 5 &&
                        c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 7);
                    break;
                case 5: // 尚可: 3-5
                    query = query.Where(c => c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 3 &&
                        c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 5);
                    break;
                case 6: // 低於預期: 1-3
                    query = query.Where(c => c.RatingScores.Average(r =>
                        (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 3);
                    break;
            }

            return query;
        }



        [HttpGet("monthCounts")]
        public async Task<IActionResult> GetMonthCounts(string dateFilter = null)
        {
            var query = _context.Comments.AsQueryable();
            if (!string.IsNullOrEmpty(dateFilter))
            {
                query = ApplyDateFilter(query, dateFilter);
            }

            var monthRanges = new[]
            {
        new { Range = "3-5月", StartMonth = 3, EndMonth = 5 },
        new { Range = "6-8月", StartMonth = 6, EndMonth = 8 },
        new { Range = "9-11月", StartMonth = 9, EndMonth = 11 },
        new { Range = "12-2月", StartMonth = 12, EndMonth = 2 }
    };

            var monthCounts = new Dictionary<string, int>();

            foreach (var range in monthRanges)
            {
                var startDate = GetStartDateForRange(range.StartMonth, range.EndMonth);
                var endDate = startDate.AddMonths(3).AddDays(-1);

                var count = await query.CountAsync(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);
                monthCounts.Add(range.Range, count);
            }

            return Ok(monthCounts);
        }

        private DateTime GetStartDateForRange(int startMonth, int endMonth)
        {
            var currentYear = DateTime.Now.Year;
            if (startMonth > endMonth) // For ranges like "12-2月"
            {
                var startDate = new DateTime(currentYear, startMonth, 1);
                if (DateTime.Now.Month <= endMonth) // Current month is within the range
                {
                    startDate = startDate.AddYears(-1);
                }
                return startDate;
            }
            return new DateTime(currentYear, startMonth, 1);
        }

        // 應用日期篩選器
        private IQueryable<Comment> ApplyDateFilter(IQueryable<Comment> query, string dateFilter)
        {
            int monthFilter = GetMonthFilter(dateFilter);
            if (monthFilter != 0)
            {
                var startDate = GetStartDateForRange(monthFilter);
                var endDate = startDate.AddMonths(3).AddDays(-1);
                query = query.Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);
            }
            return query;
        }

        private DateTime GetStartDateForRange(int monthFilter)
        {
            var currentYear = DateTime.Now.Year;
            if (monthFilter == 12 && DateTime.Now.Month <= 2) // 跨年处理
            {
                return new DateTime(currentYear - 1, monthFilter, 1);
            }
            return new DateTime(currentYear, monthFilter, 1);
        }

        private int GetMonthFilter(string dateFilter)
        {
            return dateFilter switch
            {
                "3-5月" => 3,
                "6-8月" => 6,
                "9-11月" => 9,
                "12-2月" => 12,
                _ => 0,
            };
        }

        private IQueryable<Comment> ApplySort(IQueryable<Comment> query, string sortBy)
        {
            return sortBy switch
            {
                "oldest" => query.OrderBy(c => c.CreatedAt),
                "highestScore" => query.OrderByDescending(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                                              r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7)),
                "lowestScore" => query.OrderBy(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                                  r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7)),
                _ => query.OrderByDescending(c => c.CreatedAt) // 預設按最新評論排序
            };
        }




        //------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------

        // GET: api/<CommentController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<CommentController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<CommentController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<CommentController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<CommentController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
