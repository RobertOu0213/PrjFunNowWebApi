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
    public class CartController : ControllerBase
    {
        private readonly FunNowContext _context;

        public CartController(FunNowContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
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
                    .Where(od => od.MemberId == userId && od.IsOrdered == false)
                    .Include(od => od.Room)
                        .ThenInclude(r => r.Hotel)
                            .ThenInclude(h => h.City)
                    .Include(od => od.Room)
                        .ThenInclude(r => r.RoomType)
                    .Include(od => od.Room)
                        .ThenInclude(r => r.Hotel)
                            .ThenInclude(h => h.Comments)
                    .Include(od => od.Room)
                        .ThenInclude(r => r.RoomImages)
                    .ToListAsync();

                var orderDetailDtos = orderDetails.Select(od => new cartItemsDTO
                {
                    OrderDetailID = od.OrderDetailId,
                    HotelName = od.Room?.Hotel?.HotelName,
                    RoomType = od.Room?.RoomType?.RoomTypeName,
                    RoomName = od.Room?.RoomName,
                    RoomPrice = od.Room?.RoomPrice ?? 0,
                    CityName = od.Room?.Hotel?.City?.CityName,
                    AllCommentsCount = od.Room?.Hotel?.Comments?.Count() ?? 0,
                    LevelStar = od.Room?.Hotel?.LevelStar ?? 0,
                    CheckInDate = od.CheckInDate,
                    CheckOutDate = od.CheckOutDate,
                    RoomID = od.RoomId,
                    MaximumOccupancy = od.Room?.MaximumOccupancy ?? 0,
                    AllOrderDetailsCount = allOrderDetailsCount,
                    RoomImage = od.Room?.RoomImages?.FirstOrDefault()?.RoomImage1,
                    HotelID = od.Room?.Hotel?.HotelId ?? 0,
                    GuestNumber = od.GuestNumber
                }).ToList();

                return Ok(new { success=true, orderDetailDtos});
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }

        }

     
        [HttpPost]
        public async Task<ActionResult<OrderDetail>> PostOrderDetail(OrderDetail orderDetail)
        {
            try
            {
                orderDetail.CreatedAt = DateTime.Now;
                orderDetail.IsOrdered = false;

                _context.OrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();

                var room = await _context.Rooms
                .Include(r => r.RoomImages) 
                .FirstOrDefaultAsync(r => r.RoomId == orderDetail.RoomId);

                var roomName = room != null ? room.RoomName : null;
                var roomPrice = room != null ? room.RoomPrice : 0; 
                var roomImage = room != null && room.RoomImages.Any() ? room.RoomImages.FirstOrDefault().RoomImage1 : null;

                var filterData = new
                {
                    success = true,
                    CheckInDate = orderDetail.CheckInDate,
                    CheckOutDate = orderDetail.CheckOutDate,
                    RoomName = roomName,
                    RoomPrice = roomPrice,
                    RoomImage = roomImage
                };
                return Ok(filterData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        // DELETE: api/CartController/{orderDetailID}
        [HttpDelete("{orderDetailID}")]
        public async Task<ActionResult> DeleteOrderDetail(int orderDetailID)
        {
            try
            {
                var orderDetail = await _context.OrderDetails.FirstOrDefaultAsync(or => or.OrderDetailId == orderDetailID);
                if (orderDetail == null)
                {
                    return NotFound("OrderDetail not found");
                }

                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }


    }
}
