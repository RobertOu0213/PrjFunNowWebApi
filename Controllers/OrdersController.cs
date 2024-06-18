using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly FunNowContext _context;

        public OrdersController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/Orders/ByMemberAndStatus/{memberId}/{orderStatusId}
        [HttpGet("ByMemberAndStatus/{memberId}/{orderStatusId}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByMemberAndStatus(int memberId, int orderStatusId)
        {
            var orders = await _context.Orders
                                       .Where(o => o.MemberId == memberId && o.OrderStatusId == orderStatusId)
                                       .ToListAsync();

            if (!orders.Any())
            {
                return NotFound();
            }

            var orderIds = orders.Select(o => o.OrderId).ToList();
            var orderDetails = await _context.OrderDetails
                                             .Where(od => orderIds.Contains(od.OrderId ?? 0))
                                             .Include(od => od.Room)
                                             .ThenInclude(r => r.Hotel)
                                             .ThenInclude(h => h.HotelImages) // 包含 HotelImages
                                             .ToListAsync();

            var result = orders.Select(o => new OrderDTO
            {
                OrderId = o.OrderId,
                OrderStatusId = o.OrderStatusId,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                GuestLastName = o.GuestLastName,
                GuestFirstName = o.GuestFirstName,
                GuestEmail = o.GuestEmail,
                OrderDetails = orderDetails.Where(od => od.OrderId == o.OrderId).Select(od => new OrderDetailDTO
                {
                    OrderDetailId = od.OrderDetailId,
                    RoomID = od.RoomId,
                    CheckInDate = od.CheckInDate,
                    CheckOutDate = od.CheckOutDate,
                    HotelID = od.Room.Hotel.HotelId,
                    HotelName = od.Room.Hotel.HotelName,
                    HotelImages = od.Room.Hotel.HotelImages.Select(hi => hi.HotelImage1).ToList() // 映射圖片
                }).ToList()
            });

            return Ok(result);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
        // GET: api/Orders/ByMemberAndStatus/{memberId}/{orderStatusId}/{pageNumber}/{pageSize}
        [HttpGet("ByMemberAndStatus/{memberId}/{orderStatusId}/{pageNumber}/{pageSize}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByMemberAndStatus(int memberId, int orderStatusId, int pageNumber, int pageSize)
        {
            var ordersQuery = _context.Orders
                                      .Where(o => o.MemberId == memberId && o.OrderStatusId == orderStatusId);

            var totalOrders = await ordersQuery.CountAsync();

            var orders = await ordersQuery
                                  .Skip((pageNumber - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            if (!orders.Any())
            {
                return NotFound();
            }

            var orderIds = orders.Select(o => o.OrderId).ToList();
            var orderDetails = await _context.OrderDetails
                                             .Where(od => orderIds.Contains(od.OrderId ?? 0))
                                             .Include(od => od.Room)
                                             .ThenInclude(r => r.Hotel)
                                             .ThenInclude(h => h.HotelImages)
                                             .ToListAsync();

            var result = orders.Select(o => new OrderDTO
            {
                OrderId = o.OrderId,
                OrderStatusId = o.OrderStatusId,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                GuestLastName = o.GuestLastName,
                GuestFirstName = o.GuestFirstName,
                GuestEmail = o.GuestEmail,
                OrderDetails = orderDetails.Where(od => od.OrderId == o.OrderId).Select(od => new OrderDetailDTO
                {
                    OrderDetailId = od.OrderDetailId,
                    RoomID = od.RoomId,
                    CheckInDate = od.CheckInDate,
                    CheckOutDate = od.CheckOutDate,
                    HotelID = od.Room.Hotel.HotelId,
                    HotelName = od.Room.Hotel.HotelName,
                    HotelImages = od.Room.Hotel.HotelImages.Select(hi => hi.HotelImage1).ToList()
                }).ToList()
            });

            return Ok(new
            {
                TotalOrders = totalOrders,
                Orders = result
            });
        }

    }
}
