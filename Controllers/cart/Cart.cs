using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers.cart
{
    [Route("api/[controller]")]
    [ApiController]
    public class Cart : ControllerBase
    {
        private readonly FunNowContext _context;

        public Cart(FunNowContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllCarts(int? userId)
        {
            if (userId == null)
            {
                return BadRequest("UserID is required");
            }
            try
            {
    
                var allOrderDetailsCount = await _context.OrderDetails
                    .Where(od => od.MemberId == userId)
                    .CountAsync();

                var orderDetails = await _context.OrderDetails
                    .Where(od => od.MemberId == userId)
                .ToListAsync();

                var orderDetailDtos = orderDetails.Select(od => new cartItemsDTO
                {
                    HotelName = od.Room.Hotel.HotelName,
                    RoomType = od.Room.RoomType.RoomTypeName,  
                    RoomName = od.Room.RoomName,
                    RoomPrice = od.Room.RoomPrice,
                    CityName = od.Room.Hotel.City.CityName,
                    AllCommentsCount = od.Room.Hotel.Comments.Count,
                    LevelStar = (int)od.Room.Hotel.LevelStar,
                    CheckInDate = od.CheckInDate,
                    CheckOutDate = od.CheckOutDate,
                    RoomID = od.RoomId,
                    MaximumOccupancy = od.Room.MaximumOccupancy,
                    AllOrderDetailsCount = allOrderDetailsCount
                }).ToList();

                return Ok(orderDetailDtos);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }

        }

        // GET: api/OrderDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(int? id )
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            return orderDetail;
        }


        // POST: api/OrderDetails
        [HttpPost]
        public async Task<ActionResult<OrderDetail>> PostOrderDetail(OrderDetail orderDetail)
        {
            try
            {
                orderDetail.CreatedAt = DateTime.Now;
                orderDetail.IsOrdered = false;
                _context.OrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == orderDetail.RoomId);
                var roomName = room != null ? room.RoomName : null;

                var filterData = new
                {
                    success = true,
                    CheckInDate = orderDetail.CheckInDate,
                    CheckOutDate = orderDetail.CheckOutDate,
                    RoomName = roomName
                };
                return Ok(filterData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }




    }
}
