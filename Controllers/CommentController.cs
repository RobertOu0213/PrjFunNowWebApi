using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
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
       
        [HttpGet("GetComment")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            var comments = await _context.Comments.ToListAsync();
            return Ok(comments);
        }
        //從資料庫取評論
        [HttpGet]
        public IActionResult GetComments(int page = 1, int pageSize = 10, string search = null, int ratingFilter = 0, string dateFilter = null)
        {
            //取評論
            var query = _context.Comments.Include(c => c.RatingScores).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.CommentText.Contains(search) || c.CommentTitle.Contains(search));
            }

            if (ratingFilter > 0)
            {
                query = ApplyRatingFilter(query, ratingFilter);
            }

            if (!string.IsNullOrEmpty(dateFilter))
            {
                var month = GetMonthFilter(dateFilter);
                query = query.Where(c => c.CreatedAt.Month == month);
            }

            //分頁
            var totalItems = query.Count();
            var comments = query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            //各項平均
            var averageScores = new
            {
                ComfortScore = _context.RatingScores.Average(r => r.ComfortScore),
                CleanlinessScore = _context.RatingScores.Average(r => r.CleanlinessScore),
                StaffScore = _context.RatingScores.Average(r => r.StaffScore),
                FacilitiesScore = _context.RatingScores.Average(r => r.FacilitiesScore),
                ValueScore = _context.RatingScores.Average(r => r.ValueScore),
                LocationScore = _context.RatingScores.Average(r => r.LocationScore),
                FreeWifiScore = _context.RatingScores.Average(r => r.FreeWifiScore),
            };

            //總平均
            var totalAverageScore = (averageScores.ComfortScore +
                                     averageScores.CleanlinessScore +
                                     averageScores.StaffScore +
                                     averageScores.FacilitiesScore +
                                     averageScores.ValueScore +
                                     averageScores.LocationScore +
                                     averageScores.FreeWifiScore) / 7;

            return Ok(new { TotlaItems = totalItems, Comments = comments, AverageScore = averageScores, TotalAverageScore = totalAverageScore });
        }

        //存入評論
        [HttpPost("PostComment")]
        public IActionResult PostComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Comments.Add(comment);
                _context.SaveChanges();
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false });
        }

        //評論分數篩選器
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
