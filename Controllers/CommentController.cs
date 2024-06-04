﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.joannaDTO;
using System.Reflection.Metadata.Ecma335;

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
        [HttpGet("GetComment")]
        public IActionResult GetComments(int hotelId, int page = 1, int pageSize = 10, string search = null, int? ratingFilter = null, string dateFilter = null)
        {
            try
            {
                // 获取评论
                var commentsQuery = _context.Comments
                                            .Where(c => c.HotelId == hotelId)
                                            .Include(c => c.RatingScores)
                                            .AsQueryable();

                // 过滤搜索条件
                if (!string.IsNullOrEmpty(search))
                {
                    commentsQuery = commentsQuery.Where(c => c.CommentTitle.Contains(search) || c.CommentText.Contains(search));
                }

                // 过滤评分条件
                if (ratingFilter.HasValue && ratingFilter.Value > 0)
                {
                    commentsQuery = commentsQuery.Where(c => c.RatingScores.Any(r => r.ComfortScore >= ratingFilter.Value));
                }

                // 过滤日期条件
                if (!string.IsNullOrEmpty(dateFilter))
                {
                    if (DateTime.TryParse(dateFilter, out DateTime parsedDate))
                    {
                        commentsQuery = commentsQuery.Where(c => c.CreatedAt.Date == parsedDate.Date);
                    }
                }

                var totalItems = commentsQuery.Count();

                var comments = commentsQuery
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CommentDTO
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
                            FreeWifiScore = r.FreeWifiScore
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
                };

                var totalAverageScore = (averageScores.ComfortScore +
                                         averageScores.CleanlinessScore +
                                         averageScores.StaffScore +
                                         averageScores.FacilitiesScore +
                                         averageScores.ValueScore +
                                         averageScores.LocationScore +
                                         averageScores.FreeWifiScore) / 7;

                var hotelName = _context.Hotels
                                        .Where(h => h.HotelId == hotelId)
                                        .Select(h => h.HotelName)
                                        .FirstOrDefault();

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


        //評論分數篩選器
        [HttpGet("ApplyRatingFilter")]
        private IQueryable<Comment> ApplyRatingFilter(IQueryable<Comment> query, int ratingFilter)
        {
            switch (ratingFilter)
            {
                case 2: // 超讚: 9+
                    query = query.Where(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 9);
                    break;
                case 3: // 很讚: 7-9
                    query = query.Where(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 7 &&
                                             c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 9);
                    break;
                case 4: // 很好: 5-7
                    query = query.Where(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 5 &&
                                             c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 7);
                    break;
                case 5: // 尚可: 3-5
                    query = query.Where(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) >= 3 &&
                                             c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 5);
                    break;
                case 6: // 低於預期: 1-3
                    query = query.Where(c => c.RatingScores.Average(r => (r.ComfortScore + r.CleanlinessScore + r.StaffScore +
                                                                            r.FacilitiesScore + r.ValueScore + r.LocationScore + r.FreeWifiScore) / 7) < 3);
                    break;
            }

            return query;
        }

        //評論月份篩選器
        [HttpGet("GetMonthFilter")]
        private int GetMonthFilter(string dateFilter)
        {
            switch (dateFilter)
            {
                case "3-5月": return 3;
                case "6-8月": return 6;
                case "9-11月": return 9;
                case "12-2月": return 12;
                default: return 0;
            }

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
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CommentController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CommentController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CommentController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
