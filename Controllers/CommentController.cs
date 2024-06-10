using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.joannaDTO;
using System.ComponentModel.Design;
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
        [HttpGet("{hotelId}/GetComments")]
        public IActionResult GetComments(int hotelId, int page = 1, int pageSize = 10, string search = null, int? ratingFilter = null, string dateFilter = null, string sortBy = null, string topics = null)
        {
            try
            {
                // 基本查询并包含 RatingScores 表
                IQueryable<Comment> commentsQuery = _context.Comments
                                                             .Where(c => c.HotelId == hotelId)
                                                             .Include(c => c.RatingScores);

                if (!string.IsNullOrEmpty(search))
                {
                    commentsQuery = commentsQuery.Where(c => c.CommentTitle.Contains(search) || c.CommentText.Contains(search));
                }

                if (ratingFilter.HasValue)
                {
                    commentsQuery = ApplyRatingFilter(commentsQuery, ratingFilter.Value);
                }

                if (!string.IsNullOrEmpty(dateFilter))
                {
                    commentsQuery = ApplyDateFilter(commentsQuery, dateFilter);
                }

                if (!string.IsNullOrEmpty(topics))
                {
                    var topicList = topics.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    commentsQuery = commentsQuery.Where(c => topicList.Any(t => c.CommentText.Contains(t)));
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    commentsQuery = sortBy switch
                    {
                        "newest" => commentsQuery.OrderByDescending(c => c.CreatedAt),
                        "oldest" => commentsQuery.OrderBy(c => c.CreatedAt),
                        "highest" => commentsQuery.OrderByDescending(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7)),
                        "lowest" => commentsQuery.OrderBy(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7)),
                        _ => commentsQuery
                    };
                }

                // 计算总数
                var totalItems = commentsQuery.Count();

                // 分页后的评论数据
                var pagedComments = commentsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var commentIds = pagedComments.Select(c => c.CommentId).ToList();

                // 使用分页后的评论ID列表查询 memberInfo
                var memberInfo = _context.CommentWithInfos
                    .Where(ci => commentIds.Contains(ci.CommentId))
                    .Select(ci => new
                    {
                        ci.CommentId,
                        ci.FirstName,
                        ci.TravelerType,
                        ci.RoomTypeName
                    }).ToList();

                var comments = pagedComments
                    .Select(c => new CommentResponse
                    {
                        CommentId = c.CommentId,
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
                

                if (hotelName == null)
                {
                    return NotFound($"Hotel with ID {hotelId} not found.");
                }

                return Ok(new
                {
                    TotalItems = totalItems,
                    Comments = comments,
                    HotelName = hotelName,
                    MemberInfo = memberInfo
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
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

            // 计算月份评论数量和详细信息
            var dateRanges = new[] { "3-5月", "6-8月", "9-11月", "12-2月" };
            var dateCommentDetails = new Dictionary<string, object>();

            foreach (var range in dateRanges)
            {
                dateCommentDetails[range] = new
                {
                    Count = await ApplyDateFilter(_context.Comments.AsQueryable(), range).CountAsync(),
                    Comments = await GetCommentsByDateRange(range)
                };
            }

            var total = await _context.Comments.CountAsync();

            return Ok(new { total, RatingCommentDetails = ratingCommentDetails, DateCommentDetails = dateCommentDetails});
        }

        private async Task<List<CommentResponseDTO.CommentResponse>> GetCommentsByRating(int ratingFilter)
        {
            var comments = await ApplyRatingFilter(_context.Comments.AsQueryable(), ratingFilter)
                               .Select(c => new CommentResponseDTO.CommentResponse
                               {
                                   
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

        
        [HttpPost("filter")]
        public async Task<IActionResult> GetReportReviews()
        {
            var query = _context.ReportReviews
            .Include(r => r.Member) // Include Member details
            .AsQueryable();

           


            var results = await query.Select(r => new
            {
                r.ReportId,
                r.CommentId,
                r.MemberId,
                r.ReportTitleId,
                r.ReportSubtitleId,
                r.ReportedAt,
                r.ReportReason,
                r.ReviewStatus,
                r.ReviewedBy,
                r.ReviewedAt,
                MemberName = r.Member.FirstName,
                MemberEmail = r.Member.Email,
                MemberPhone = r.Member.Phone
            }).ToListAsync();

            return Ok(results);
        }


        [HttpPut("UpdateCommentStatus")]
        public IActionResult UpdateCommentAndReportStatus([FromBody] UpdateCommentAndReportStatusRequest request)
        {
            var comment = _context.Comments.FirstOrDefault(c => c.CommentId == request.CommentId);
            if (comment == null)
            {
                return NotFound();
            }

            comment.CommentStatus = request.Status.ToString();
            comment.UpdatedAt = DateTime.Now;

            var report = _context.ReportReviews.FirstOrDefault(r => r.ReportId == request.ReportId);
            if (report == null)
            {
                return NotFound();
            }

            report.ReviewStatus = request.Status.ToString();
            report.ReviewedAt = DateTime.Now;

            _context.SaveChanges();

            // Reload the updated report to ensure it's the latest state
            var updatedReport = _context.ReportReviews.FirstOrDefault(r => r.ReportId == request.ReportId);

            return Ok(updatedReport);
        }

        public class UpdateCommentAndReportStatusRequest
        {
            public int CommentId { get; set; }
            public int ReportId { get; set; }
            public int Status { get; set; }
        }










    }
}














    





