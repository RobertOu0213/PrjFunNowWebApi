using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using static System.Collections.Specialized.BitVector32;

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
      
        [HttpGet]
        public async Task<ActionResult> GetHotel()
        {
            // 建立查詢，將Hotel和Room表進行連接，並選擇所需的字段
            var query = from hotel in _context.Hotels
                        select new
                        {
                            HotelID = hotel.HotelId,
                            HotelName = hotel.HotelName,
                            HotelAddress = hotel.HotelAddress,
                            HotelPhone = hotel.HotelPhone,
                            HotelIsActive = hotel.IsActive  // 判斷isActive狀態
                        };

            // 執行查詢並將結果轉換為列表
            var result = await query.ToListAsync();

            // 返回查詢結果
            return Ok(result);

        }

        // GET: api/BackSIdeHotel    
        [HttpGet("rooms/{roomId}")]
        public async Task<ActionResult> GetHotelByRoomId(int roomId)
        {
            // 獲取指定房间ID的房间信息
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            // 如果房间不存在，返回404错誤
            if (room == null)
            {
                return NotFound();
            }

            // 獲取與房間關聯的飯店信息
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == room.HotelId);

            // 如果飯店不存在，返回404错误
            if (hotel == null)
            {
                return NotFound();
            }

            // 創建一个包含飯店和房间信息的新對象
            var selectedHotel = new
            {
                HotelId = hotel.HotelId,
                HotelName = hotel.HotelName,
                HotelAddress = hotel.HotelAddress,
                HotelPhone = hotel.HotelPhone,
                HotelDescription = hotel.HotelDescription,

                // 只包含指定房间的信息
                Rooms = new[] {
            new {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                RoomPrice = room.RoomPrice,
                Description = room.Description,
                RoomTypeId = room.RoomTypeId,
                RoomStatus = room.RoomStatus,
                RoomSize = room.RoomSize,
                MaximumOccupancy = room.MaximumOccupancy,
                MemberId = room.MemberId
            }
        }
            };

            return Ok(selectedHotel);
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

