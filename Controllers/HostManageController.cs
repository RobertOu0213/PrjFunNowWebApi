using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostManageController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly IConfiguration _configuration;

        public HostManageController(FunNowContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/HostManage
        [HttpGet]
        public async Task<ActionResult> GetHotels()
        {
            // 讀取設定
            var imageSavePath = _configuration.GetValue<string>("ImageSavePath");

            var hotels = await (from h in _context.Hotels
                                where h.MemberId == 1
                                select new
                                {
                                    HotelId = h.HotelId,
                                    HotelName = h.HotelName,
                                    CityName = h.City.CityName,
                                    CountryName = h.City.Country.CountryName,
                                    HotelImage = h.HotelImages.Select(hi => hi.HotelImage1).FirstOrDefault(),
                                    isActive = h.IsActive
                                }).ToListAsync();

            var result = hotels.Select(h => new
            {
                h.HotelId,
                h.HotelName,
                h.CityName,
                h.CountryName,
                HotelImage = h.HotelImage != null && (h.HotelImage.StartsWith("http://") || h.HotelImage.StartsWith("https://"))
                             ? h.HotelImage
                             : $"image/{h.HotelImage}",      //https://localhost:7284/(前端)   image/圖片檔名(後端)
                h.isActive

            }).ToList();

            return Ok(result);
           

               

        }











        // GET: api/HostManage/5
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

        // PUT: api/HostManage/5
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

        // POST: api/HostManage
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Hotel>> PostHotel(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHotel", new { id = hotel.HotelId }, hotel);
        }



        private bool HotelExists(int id)
        {
            return _context.Hotels.Any(e => e.HotelId == id);
        }
    }
}
