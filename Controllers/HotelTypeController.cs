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
    public class HotelTypeController : ControllerBase
    {
        private readonly FunNowContext _context;

        public HotelTypeController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/HotelType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelType>>> GetHotelTypes()
        {
            return await _context.HotelTypes.ToListAsync();
        }

    }
}
