using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackSideHotelController : ControllerBase
    {
        private readonly FunNowContext _context;

        public BackSideHotelController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/BackSIdeHotel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels()
        {

            //你寫





            return await _context.Hotels.ToListAsync();
        }

    


        // PUT: api/BackSIdeHotel/5
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

       
        private bool HotelExists(int id)
        {
            return _context.Hotels.Any(e => e.HotelId == id);
        }
    }
}
