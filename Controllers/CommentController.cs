using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using PrjFunNowWebApi.Models.joannaDTO;
using System.ComponentModel.Design;
using System.Globalization;
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


        [HttpGet("{hotelId}/ForHotelComment")]
        public IActionResult GetHotelComment(int hotelId) {
            
            // 获取特定酒店的最新12条评论
            var latestComments = _context.Comments
                
                .Where(c => c.HotelId == hotelId)
                .Include(c => c.Member)
                .OrderByDescending(c => c.CreatedAt)
                .Take(6)
                .Select(c => new {
                    c.CommentText,
                    c.CreatedAt,
                    c.Member.FirstName

                })
                .ToList();

            // 获取评论总数
            var totalComments = _context.Comments.Count(c => c.HotelId == hotelId);
            var result = new 
            {
                TotalComments = totalComments,
                TopComments = latestComments
            };

            return Ok(result);
        }

        //從資料庫取評論
        [HttpGet("{hotelId}/GetComments")]
        public IActionResult GetComments(int hotelId, int page = 1, int pageSize = 10, string search = null, int? ratingFilter = null, string dateFilter = null, string sortBy = null, string topics = null)
        {
            try
            {
                IQueryable<Comment> commentsQuery = from c in _context.Comments
                                                    where c.HotelId == hotelId
                                                    join r in _context.RatingScores on c.CommentId equals r.CommentId into ratingGroup
                                                    join m in _context.Members on c.MemberId equals m.MemberId
                                                    from rg in ratingGroup.DefaultIfEmpty()
                                                    select new Comment
                                                    {
                                                        CommentId = c.CommentId,
                                                        HotelId = c.HotelId,
                                                        CommentTitle = c.CommentTitle,
                                                        CommentText = c.CommentText,
                                                        CreatedAt = c.CreatedAt,
                                                        RatingScores = ratingGroup.ToList(),
                                                    };

                //commentsQuery = commentsQuery.AsNoTracking(); // 禁用跟踪
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
                            CommentId = r.CommentId ??0,
                            ComfortScore = r.ComfortScore ?? 0,
                            CleanlinessScore = r.CleanlinessScore ?? 0,
                            StaffScore = r.StaffScore ?? 0,
                            FacilitiesScore = r.FacilitiesScore ?? 0,
                            ValueScore = r.ValueScore ?? 0,
                            LocationScore = r.LocationScore ?? 0,
                            FreeWifiScore = r.FreeWifiScore ?? 0,
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

                                   RatingScores = c.RatingScores.Select(r => new RatingScoreDTO
                                   {
                                       RatingId = r.RatingId,
                                       CommentId = r.CommentId ?? 0,
                                       ComfortScore = r.ComfortScore ?? 0,
                                       CleanlinessScore = r.CleanlinessScore ?? 0,
                                       StaffScore = r.StaffScore ?? 0,
                                       FacilitiesScore = r.FacilitiesScore ?? 0,
                                       ValueScore = r.ValueScore    ?? 0,
                                       LocationScore = r.LocationScore ?? 0,
                                       FreeWifiScore = r.FreeWifiScore ?? 0,
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
                                       CommentId = r.CommentId ??0,
                                       ComfortScore = r.ComfortScore ?? 0,
                                       CleanlinessScore = r.CleanlinessScore ?? 0,
                                       StaffScore = r.StaffScore ?? 0,
                                       FacilitiesScore = r.FacilitiesScore ?? 0,
                                       ValueScore = r.ValueScore ?? 0,
                                       LocationScore = r.LocationScore ?? 0,
                                       FreeWifiScore = r.FreeWifiScore ?? 0,
                                       TravelerType = r.TravelerType
                                   }).ToList()
                               }).ToListAsync();
            return comments;
        }

        private IQueryable<Comment> ApplyRatingFilter(IQueryable<Comment> query, int ratingFilter)
        {
            var ratingText = RatingTxtgforA(query);

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


        [HttpGet("AverageRatingText/{hotelId}")]
        public async Task<IActionResult> GetAverageRatingText(int hotelId)
        {
            var averageScore = await _context.Comments
                .Where(c => c.HotelId == hotelId)
                .Select(c => c.RatingScores.Average(r =>
                    ((r.ComfortScore ?? 0) + (r.CleanlinessScore ?? 0) + (r.StaffScore ?? 0) + (r.FacilitiesScore ?? 0) + (r.ValueScore ?? 0) + (r.LocationScore ?? 0) + (r.FreeWifiScore ?? 0)) / 7))
                .FirstOrDefaultAsync();

            if (averageScore == null)
            {
                return NotFound("未找到评分数据");
            }

            var ratingText = GetRatingText(averageScore);
            var counts = await _context.Comments
                .Where( c => c.HotelId == hotelId).CountAsync();

            return Ok(new { HotelId = hotelId, AverageScore = averageScore, RatingText = ratingText ,Counts = counts });
        }

        private string GetRatingText(decimal averageScore)
        {
            if (averageScore >= 9)
            {
                return "超讚！";
            }
            else if (averageScore >= 7)
            {
                return "很讚";
            }
            else if (averageScore >= 5)
            {
                return "很好";
            }
            else if (averageScore >= 3)
            {
                return "尚可";
            }
            else
            {
                return "低於預期";
            }
        }


        private string RatingTxtgforA(IQueryable<Comment> query)
        {
            var averageScore = query.Select(c => c.RatingScores.Average(r =>
                (r.ComfortScore + r.CleanlinessScore + r.StaffScore + r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7))
                .FirstOrDefault();

            if (averageScore >= 9)
            {
                return "超讚";
            }
            else if (averageScore >= 7)
            {
                return "很讚";
            }
            else if (averageScore >= 5)
            {
                return "很好";
            }
            else if (averageScore >= 3)
            {
                return "尚可";
            }
            else
            {
                return "低於預期";
            }
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



        [HttpPost("ReportedComment")]
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
                r.Member.FirstName,
                r.Member.Email,
                r.Member.Phone,
                r.ReportTitleId,
                r.ReportSubtitleId,
                r.ReportedAt,
                r.ReportReason,
                r.ReviewStatus,
                r.ReviewedBy,
                r.ReviewedAt,
                r.Comment.CommentTitle,
                r.Comment.CommentText,
          
            }).ToListAsync();

            return Ok(results);
        }


        [HttpPut("UpdateCommentStatus")]
        public async Task<IActionResult> UpdateCommentAndReportStatus([FromBody] UpdateCommentAndReportStatusRequest request)
        {
            try
            {
                if (request == null || request.CommentId <= 0 || request.ReportId <= 0 || request.Status <= 0)
                {
                    return BadRequest("Invalid request data");
                }

                var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == request.CommentId);
                if (comment == null)
                {
                    return NotFound($"Comment with ID {request.CommentId} not found");
                }

                comment.CommentStatus = request.Status.ToString(); // 确保类型转换为字符串
                comment.UpdatedAt = DateTime.Now;

                var report = await _context.ReportReviews.FirstOrDefaultAsync(r => r.ReportId == request.ReportId);
                if (report == null)
                {
                    return NotFound($"Report with ID {request.ReportId} not found");
                }

                report.ReviewStatus = request.Status.ToString(); // 确保类型转换为字符串
                report.ReviewedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // 重新加载更新后的报告以确保是最新状态
                var updatedReport = await _context.ReportReviews.FirstOrDefaultAsync(r => r.ReportId == request.ReportId);

                // 返回更新后的报告
                return Ok(updatedReport);
            }
            catch (Exception ex)
            {
               
                // 记录详细的异常信息
                return StatusCode(500, "Internal server error");
            }
        }

        public class UpdateCommentAndReportStatusRequest
        {
            public int CommentId { get; set; }
            public int ReportId { get; set; }
            public int Status { get; set; }
        }





        [HttpPost("SubmitReportReview")]
        public async Task<IActionResult> SubmitReportReview([FromBody] ReportReviewDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request body");
            }

            // 解析日期时间字符串
            if (!DateTime.TryParseExact(dto.ReportedAt, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var reportedAt))
            {
                return BadRequest("Invalid ReportedAt value");
            }

            var reportReview = new ReportReview
            {
                CommentId = dto.CommentID,
                MemberId = dto.MemberID,
                ReportTitleId = dto.ReportTitleID,
                ReportSubtitleId = dto.ReportSubtitleID,
                ReportedAt = reportedAt,
                ReportReason = dto.ReportReason,
                ReviewStatus = dto.ReviewStatus,
                ReviewedBy = null, 
                ReviewedAt = null 
            };

            _context.ReportReviews.Add(reportReview);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Report submitted successfully" });
        }




        public class ReportReviewDto
        {
            public int CommentID { get; set; }
            public int MemberID { get; set; }
            public int ReportTitleID { get; set; }
            public int ReportSubtitleID { get; set; }
            public string ReportedAt { get; set; } // 字符串类型，用于接收前端传来的日期字符串
            public string ReportReason { get; set; }
            public string ReviewStatus { get; set; }
        }


        //根據會員選取未評論、尚未完成、已評論
        [HttpGet("GetCommentsByStatus/{memberId}")]

        public async Task<IActionResult> GetCommentsByStatus(int memberId)
        {
               var comments = await _context.Comments
            .Where(c => c.MemberId == memberId &&
                        (c.CommentStatus == "5" || c.CommentStatus == "6" || c.CommentStatus == "7"))
            .Select(c => new
            {   
                c.CommentId,
                c.MemberId,
                c.OrderId,
                c.Hotel.HotelName,
                c.Member.FirstName,
                c.Hotel.HotelAddress,
                c.Member.Email,
                c.CommentStatus,
                c.CommentText,
                c.CommentTitle,
                c.RatingScores,
                c.RoomId,
                c.HotelId,           

                MemberRatingScores = c.RatingScores.Select(r => new RatingScoreDTO
                {
                    RatingId = r.RatingId,
                    CommentId = r.CommentId ??0,
                    ComfortScore = r.ComfortScore ?? 0,
                    CleanlinessScore = r.CleanlinessScore ?? 0,
                    StaffScore = r.StaffScore ?? 0,
                    FacilitiesScore = r.FacilitiesScore ?? 0,
                    ValueScore = r.ValueScore ?? 0,
                    LocationScore = r.LocationScore ?? 0,
                    FreeWifiScore = r.FreeWifiScore ?? 0,
                    TravelerType = r.TravelerType
                }).ToList()
                })
                .ToListAsync();

            var commentIds = comments.Select(c => c.CommentId).ToList();
            // 获取 CommentWithInfos 数据
            var commentWithInfos = await _context.CommentWithInfos
       .Where(ci => commentIds.Contains(ci.CommentId))
       .Select(ci => new
       {
           ci.CommentId,
           ci.FirstName,
           ci.TravelerType,
           ci.RoomTypeName
       })
       .ToListAsync();

            var orders = await _context.OrderDetails
         .Where(o => comments.Select(c => c.OrderId).Contains(o.OrderId))
         .ToListAsync();

            var hotelIds = comments.Select(c => c.HotelId).Distinct().ToList();
            var hotelImages = await _context.HotelImages
       .Where(h => hotelIds.Contains(h.HotelId))
       .Select(h => new { h.HotelId,h.HotelImage1 })
       .ToListAsync();


            return Ok(new 
            { commentinfo = commentWithInfos,
                comments = comments,
                orders = orders,
                hotelImage = hotelImages,
            });
           }


        //填寫評論表單、新增到DB
        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest newCommentRequest)
        {
            if (newCommentRequest == null)
            {
                return BadRequest();
            }


            var newRatingScore = new RatingScore
            {
                CommentId = newCommentRequest.CommentID,
                RoomId = newCommentRequest.RoomID,
                ComfortScore = newCommentRequest.ComfortScore,
                CleanlinessScore = newCommentRequest.CleanlinessScore,
                StaffScore = newCommentRequest.StaffScore,
                FacilitiesScore = newCommentRequest.FacilitiesScore,
                ValueScore = newCommentRequest.ValueScore,
                LocationScore = newCommentRequest.LocationScore,
                FreeWifiScore = newCommentRequest.FreeWifiScore,
                TravelerType = newCommentRequest.TravelerType
            };

            _context.RatingScores.Add(newRatingScore);
            await _context.SaveChangesAsync();
            return Ok(newRatingScore);
        }


        [HttpPut("UpdateCommentStatus/{CmmentID}")]
        public async Task<IActionResult> UpdateCommentStatus(int CmmentID, [FromBody] CommentUpdateRequest updateRequest)
        {
            var existingComment = await _context.Comments.FindAsync(CmmentID);
           
            existingComment.CommentTitle = updateRequest.CommentTitle;
            existingComment.CommentText = updateRequest.CommentText;
            existingComment.CommentStatus = updateRequest.CommentStatus;
            existingComment.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok(existingComment);
        }

        public class CommentUpdateRequest
        {
            public string CommentStatus { get; set; }
            public string CommentTitle { get; set; }
            public string CommentText { get; set; }
            
        }

        public class CommentRequest
        {
            public int CommentID { get; set; }
            public int RoomID { get; set; }
            public int ComfortScore { get; set; }
            public int CleanlinessScore { get; set; }
            public int StaffScore { get; set; }
            public int FacilitiesScore { get; set; }
            public int ValueScore { get; set; }
            public int LocationScore { get; set; }
            public int FreeWifiScore { get; set; }
            public string TravelerType { get; set; }


        }
    }
}

























