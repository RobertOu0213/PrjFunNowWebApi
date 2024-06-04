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
    public class HotelEquipmentController : ControllerBase
    {
        private readonly FunNowContext _context;

        public HotelEquipmentController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/HotelEquipment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelEquipment>>> GetHotelEquipments()
        {
            return await _context.HotelEquipments.ToListAsync();
        }

    }
}
