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
    public class RoomTypeController : ControllerBase
    {
        private readonly FunNowContext _context;

        public RoomTypeController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/RoomType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomType>>> GetRoomTypes()
        {
            var result = await _context.RoomTypes.Select(rt => new
            {
                rt.RoomTypeId,
                rt.RoomTypeName,
                rt.Description
            })
                .ToListAsync();
            return Ok(result);
        }

    }
}
