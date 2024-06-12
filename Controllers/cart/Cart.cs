using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;

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
        //public async Task<ActionResult<OrderDetail>> GetAllCarts()
        //{

        //}

        // GET: api/OrderDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(int id=5 )
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
