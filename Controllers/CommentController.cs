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

        //[HttpGet("GetComment")]
        //public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        //{
        //    var comments = await _context.Comments.ToListAsync();
        //    return Ok(comments);
        //}
        //從資料庫取評論
        [HttpGet("{hotelId}/GetComments")]
        public IActionResult GetComments(int hotelId, int page = 1, int pageSize = 10, string search = null)
        {
            try
            {
                var commentsQuery = _context.Comments
                                            .Where(c => c.HotelId == hotelId)
                                            .Include(c => c.RatingScores)
                                            .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    commentsQuery = commentsQuery.Where(c => c.CommentTitle.Contains(search) || c.CommentText.Contains(search));
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

                var hotelName = _context.Hotels
                                        .Where(h => h.HotelId == hotelId)
                                        .Select(h => h.HotelName)
                                        .FirstOrDefault();

                return Ok(new
                {
                    TotalItems = totalItems,
                    Comments = comments,
                    HotelName = hotelName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }



        //評分過濾
        [HttpGet("commentCounts")]
        public async Task<IActionResult> GetCommentCounts()
        {
            // 计算评分评论数量和详细信息
            var ratingCommentDetails = new Dictionary<int, object>
    {
        { 2, new { Count = await ApplyRatingFilter(_context.Comments.AsQueryable(), 2).CountAsync(), Comments = await GetCommentsByRating(2) } },
        { 3, new { Count = await ApplyRatingFilter(_context.Comments.AsQueryable(), 3).CountAsync(), Comments = await GetCommentsByRating(3) } },
        { 4, new { Count = await ApplyRatingFilter(_context.Comments.AsQueryable(), 4).CountAsync(), Comments = await GetCommentsByRating(4) } },
        { 5, new { Count = await ApplyRatingFilter(_context.Comments.AsQueryable(), 5).CountAsync(), Comments = await GetCommentsByRating(5) } },
        { 6, new { Count = await ApplyRatingFilter(_context.Comments.AsQueryable(), 6).CountAsync(), Comments = await GetCommentsByRating(6) } },
    };

            // 計算月份評論數量和詳細信息
            var dateCommentDetails = new Dictionary<string, object>
    {
        { "1-3", new { Count = await ApplyDateFilter(_context.Comments.AsQueryable(), "1-3").CountAsync(), Comments = await GetCommentsByDateRange("1-3") } },
        { "4-6", new { Count = await ApplyDateFilter(_context.Comments.AsQueryable(), "4-6").CountAsync(), Comments = await GetCommentsByDateRange("4-6") } },
        { "7-9", new { Count = await ApplyDateFilter(_context.Comments.AsQueryable(), "7-9").CountAsync(), Comments = await GetCommentsByDateRange("7-9") } },
        { "10-12", new { Count = await ApplyDateFilter(_context.Comments.AsQueryable(), "10-12").CountAsync(), Comments = await GetCommentsByDateRange("10-12") } },
    };

            var total = await _context.Comments.CountAsync();

            return Ok(new { total, RatingCommentDetails = ratingCommentDetails, DateCommentDetails = dateCommentDetails });
        }

        private async Task<List<CommentResponseDTO.CommentResponse>> GetCommentsByRating(int ratingFilter)
        {
            var comments = await ApplyRatingFilter(_context.Comments.AsQueryable(), ratingFilter)
                               .Select(c => new CommentResponseDTO.CommentResponse
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
                               }).ToListAsync();
            return comments;
        }

        private async Task<List<CommentResponseDTO.CommentResponse>> GetCommentsByDateRange(string dateFilter)
        {
            var comments = await ApplyDateFilter(_context.Comments.AsQueryable(), dateFilter)
                               .Select(c => new CommentResponseDTO.CommentResponse
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
                               }).ToListAsync();
            return comments;
        }

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

        private IQueryable<Comment> ApplyDateFilter(IQueryable<Comment> query, string dateFilter, int? year = null)
        {
            var (startMonth, endMonth) = GetStartAndEndMonths(dateFilter);
            if (startMonth != 0 && endMonth != 0)
            {
                var (startDate, endDate) = GetStartAndEndDateForRange(startMonth, endMonth, year);
                query = query.Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);
            }
            return query;
        }
        private (int startMonth, int endMonth) GetStartAndEndMonths(string dateFilter)
        {
            var parts = dateFilter.Split('-');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int startMonth) && int.TryParse(parts[1], out int endMonth))
                {
                    return (startMonth, endMonth);
                }
            }
            return (0, 0);
        }

        private (DateTime startDate, DateTime endDate) GetStartAndEndDateForRange(int startMonth, int endMonth, int? year = null)
        {
            int currentYear = year ?? DateTime.Now.Year;
            DateTime startDate, endDate;

            if (endMonth < startMonth)
            {
                // 處理跨年的情況
                startDate = new DateTime(currentYear, startMonth, 1);
                endDate = new DateTime(currentYear + 1, endMonth, DateTime.DaysInMonth(currentYear + 1, endMonth));
            }
            else
            {
                // 不跨年的情況
                startDate = new DateTime(currentYear, startMonth, 1);
                endDate = new DateTime(currentYear, endMonth, DateTime.DaysInMonth(currentYear, endMonth));
            }

            return (startDate, endDate);
        }



















        //計算評分平均
        [HttpGet("{hotelId}/AverageScores")]
        public IActionResult GetAverageScores(int hotelId)
        {
            try
            {
                var ratingScores = _context.Comments
                                           .Where(c => c.HotelId == hotelId)
                                           .SelectMany(c => c.RatingScores)
                                           .ToList();

                var averageScores = new
                {
                    ComfortScore = ratingScores.Any() ? ratingScores.Average(r => r.ComfortScore) : 0,
                    CleanlinessScore = ratingScores.Any() ? ratingScores.Average(r => r.CleanlinessScore) : 0,
                    StaffScore = ratingScores.Any() ? ratingScores.Average(r => r.StaffScore) : 0,
                    FacilitiesScore = ratingScores.Any() ? ratingScores.Average(r => r.FacilitiesScore) : 0,
                    ValueScore = ratingScores.Any() ? ratingScores.Average(r => r.ValueScore) : 0,
                    LocationScore = ratingScores.Any() ? ratingScores.Average(r => r.LocationScore) : 0,
                    FreeWifiScore = ratingScores.Any() ? ratingScores.Average(r => r.FreeWifiScore) : 0
                };

                var totalAverageScore = (averageScores.ComfortScore +
                                         averageScores.CleanlinessScore +
                                         averageScores.StaffScore +
                                         averageScores.FacilitiesScore +
                                         averageScores.ValueScore +
                                         averageScores.LocationScore +
                                         averageScores.FreeWifiScore) / 7;

                return Ok(new
                {
                    AverageScore = averageScores,
                    TotalAverageScore = totalAverageScore
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
