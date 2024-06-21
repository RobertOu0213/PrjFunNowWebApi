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
    public class HotelDailyController : ControllerBase
    {
        private readonly FunNowContext _context;

        public HotelDailyController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/Orders1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders1/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // 新增這個方法來查詢特定 hotelId 的訂單詳情
        [HttpGet("details/{hotelId}")]
        public async Task<ActionResult<IEnumerable<OrderDailyDto>>> GetOrderDetailsByHotelId(int hotelId)
        {
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Room)
                .Where(od => od.Room.HotelId == hotelId)
                .Select(od => new OrderDailyDto
                {
                    RoomId = od.RoomId,
                    CheckInDate = od.CheckInDate,
                    CheckOutDate = od.CheckOutDate,
                    GuestNumber = od.GuestNumber,
                    RoomName = od.Room.RoomName,
                    RoomPrice = od.Room.RoomPrice
                })
                .ToListAsync();

            if (orderDetails == null || !orderDetails.Any())
            {
                return NotFound();
            }

            return Ok(orderDetails);
        }

        // 新增資料的 API
        [HttpPost("addDetail")]
        public async Task<ActionResult<OrderDailyDto>> AddOrderDetail(int hotelId, DateTime date)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.HotelId == hotelId);
            if (room == null)
            {
                return NotFound();
            }

            var newDetail = new OrderDetail
            {
                RoomId = room.RoomId,
                MemberId = 1,
                CheckInDate = date,
                CheckOutDate = date,
                GuestNumber = 0,
                IsOrdered = true,
                OrderId = 7,
                CreatedAt = DateTime.Now
            };

            _context.OrderDetails.Add(newDetail);
            await _context.SaveChangesAsync();

            var newDetailDto = new OrderDailyDto
            {
                RoomId = newDetail.RoomId,
                CheckInDate = newDetail.CheckInDate,
                CheckOutDate = newDetail.CheckOutDate,
                GuestNumber = newDetail.GuestNumber,
                RoomName = room.RoomName,
                RoomPrice = room.RoomPrice
            };

            return CreatedAtAction(nameof(GetOrder), new { id = newDetail.OrderDetailId }, newDetailDto);
        }

        // 刪除資料的 API
        [HttpDelete("deleteDetail")]
        public async Task<IActionResult> DeleteOrderDetail(int hotelId, DateTime date)
        {
            var detail = await _context.OrderDetails
                .Include(od => od.Room)
                .FirstOrDefaultAsync(od => od.Room.HotelId == hotelId && od.CheckInDate == date && od.CheckOutDate == date && od.GuestNumber == 0);

            if (detail == null)
            {
                return NotFound();
            }

            _context.OrderDetails.Remove(detail);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
