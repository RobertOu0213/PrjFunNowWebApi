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
    public class RoomEquipmentController : ControllerBase
    {
        private readonly FunNowContext _context;

        public RoomEquipmentController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/RoomEquipment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomEquipment>>> GetRoomEquipments()
        {
            return await _context.RoomEquipments.ToListAsync();
        }


    }
}
