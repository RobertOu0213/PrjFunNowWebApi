using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelSearchController : ControllerBase
    {
        private readonly FunNowContext _context;

        public HotelSearchController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/HotelSearch
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetAllHotels()
        {
            return await _context.HotelSearchBoxes.ToListAsync();
        }

        // GET: api/HotelSearch/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hotel>> GetHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
            {
                return NotFound();
            }

            return hotel;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelSearchBox>>> GetHotelsByKey(string keyword)
        {
            return await _context.HotelSearchBoxes.Where(s => s.HotelName.Contains(keyword)).ToListAsync();
        }

        public async Task<ActionResult<HotelSearchDTO>> GetSpotsBySearch(SearchDTO searchDTO)
        {



            ////根據分類編號搜尋景點資料
            //var spots = searchDTO.categoryId == 0 ? _context.SpotImagesSpots : _context.SpotImagesSpots.Where(s => s.CategoryId == searchDTO.categoryId);

            ////根據關鍵字搜尋景點資料(title、desc) 
            //if (!string.IsNullOrEmpty(searchDTO.keyword))
            //{
            //    spots = spots.Where(s => s.SpotTitle.Contains(searchDTO.keyword) || s.SpotDescription.Contains(searchDTO.keyword));
            //}

            ////排序
            //switch (searchDTO.sortBy)
            //{
            //    case "spotTitle":
            //        spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
            //        break;
            //    case "categoryId":
            //        spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
            //        break;
            //    default:
            //        spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
            //        break;
            //}

            ////總共有多少筆資料
            //int totalCount = spots.Count();
            ////每頁要顯示幾筆資料
            //int pageSize = searchDTO.pageSize ?? 9;   //searchDTO.pageSize ?? 9;
            ////目前第幾頁
            //int page = searchDTO.page ?? 1;

            ////計算總共有幾頁
            //int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);

            ////分頁
            //spots = spots.Skip((page - 1) * pageSize).Take(pageSize);


            ////包裝要傳給client端的資料
            //SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
            //spotsPaging.TotalCount = totalCount;
            //spotsPaging.TotalPages = totalPages;
            //spotsPaging.SpotsResult = await spots.ToListAsync();


            //return spotsPaging;
        }


        // PUT: api/HotelSearch/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, Hotel hotel)
        {
            if (id != hotel.HotelId)
            {
                return BadRequest();
            }

            _context.Entry(hotel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HotelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/HotelSearch
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Hotel>> PostHotel(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHotel", new { id = hotel.HotelId }, hotel);
        }

        // DELETE: api/HotelSearch/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HotelExists(int id)
        {
            return _context.Hotels.Any(e => e.HotelId == id);
        }
    }
}
