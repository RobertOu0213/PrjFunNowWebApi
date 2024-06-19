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

        // 切換房間的RoomStatus狀態
        [HttpPut("rooms/{roomId}/toggleStatus")]
        public async Task<IActionResult> ToggleRoomStatus(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            room.RoomStatus = room.RoomStatus == true ? false : true; // 切換RoomStatus狀態

            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }



        // 更新飯店的isActive狀態
        [HttpPut("{hotelId}/toggleActive")]
        public async Task<IActionResult> ToggleHotelActiveStatus(int hotelId)
        {
            var hotel = await _context.Hotels.FindAsync(hotelId);
            if (hotel == null)
            {
                return NotFound();
            }

            hotel.IsActive = !hotel.IsActive; // 切换isActive状态

            _context.Entry(hotel).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 获取特定酒店的所有房间信息
        [HttpGet("{hotelId}/rooms")]
        public async Task<ActionResult> GetRoomsByHotelId(int hotelId)
        {
            try
            {
                // 获取指定酒店的房间信息
                var rooms = await _context.Rooms
                    .Where(r => r.HotelId == hotelId)
                    .Select(room => new
                    {
                        RoomID = room.RoomId,
                        RoomName = room.RoomName,
                        RoomPrice = room.RoomPrice,
                        Description = room.Description,
                        RoomType = room.RoomType.RoomTypeName,
                        RoomStatus = room.RoomStatus,
                        RoomSize = room.RoomSize,
                        MaximumOccupancy = room.MaximumOccupancy
                    })
                    .ToListAsync();

                if (rooms == null || !rooms.Any())
                {
                    return NotFound("No rooms found for the specified hotel.");
                }

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error while retrieving rooms for hotel ID {hotelId}: {ex.Message}");
            }
        }


        // 此方法根據房間ID獲取飯店信息
        [HttpGet("rooms/{roomId}")]
        public async Task<ActionResult> GetHotelByRoomId(int roomId)
        {
            try
            {
                // 獲取指定房间ID的房間信息
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId);

                // 如果房間不存在，返回404错误
                if (room == null)
                {
                    return NotFound();
                }

                // 獲取與房間關聯的飯店信息
                var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.HotelId == room.HotelId);

                // 如果飯店不存在，返回404错误
                if (hotel == null)
                {
                    return NotFound();
                }

                // 創建一个包含飯店和房間信息的新對象
                var selectedHotel = new
                {
                    HotelId = hotel.HotelId,
                    HotelName = hotel.HotelName,
                    HotelAddress = hotel.HotelAddress,
                    HotelPhone = hotel.HotelPhone,
                    HotelDescription = hotel.HotelDescription,
                    // 只包含指定房間的信息
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
            catch (Exception ex)
            {
                // 日誌記錄錯誤
                Console.WriteLine($"Error retrieving hotel by room ID: {ex.Message}");
                return StatusCode(500, "Internal server error while retrieving hotel by room ID.");
            }
        }

        // 此方法獲取所有飯店的詳細信息
        [HttpGet("details")]
        public async Task<ActionResult> GetHotelDetails([FromQuery] string searchTerm = "")
        {
            try
            {
                var query = _context.Hotels
                    .Include(h => h.Rooms)  // 确保包含关联的房间信息
                    .Select(hotel => new
                    {
                        HotelID = hotel.HotelId,
                        HotelName = hotel.HotelName,
                        HotelAddress = hotel.HotelAddress,
                        HotelPhone = hotel.HotelPhone,
                        HotelIsActive = hotel.IsActive,
                        HotelTypeName = hotel.HotelType.HotelTypeName,  // 加入HotelType的名称
                        Rooms = hotel.Rooms.Select(room => new  // 选择关联的房间信息
                        {
                            RoomID = room.RoomId,
                            RoomName = room.RoomName,
                            RoomPrice = room.RoomPrice,
                            Description = room.Description,
                            RoomType = room.RoomType.RoomTypeName,  // 假设你有RoomType实体且它关联到Room
                            RoomStatus = room.RoomStatus,
                            RoomSize = room.RoomSize,
                            MaximumOccupancy = room.MaximumOccupancy
                        }).ToList()
                    });

                // 如果提供了搜索词，则过滤查询结果
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(hotel =>
                        hotel.HotelName.ToLower().Contains(searchTerm) ||
                        hotel.HotelAddress.ToLower().Contains(searchTerm));
                }

                // 执行查询并将结果转换为列表
                var result = await query.ToListAsync();

                if (result == null || !result.Any())
                {
                    return NotFound("No hotels found.");
                }

                // 返回查询结果
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error while retrieving hotel details.");
            }
        }


        // 根據 HotelTypeID 或 HotelTypeName 查詢飯店
        [HttpGet]
        public async Task<IActionResult> GetHotels([FromQuery] int? hotelTypeId, [FromQuery] string hotelTypeName = null)
        {
            try
            {
                // 確保至少有一個查詢參數
                if (hotelTypeId == null && string.IsNullOrEmpty(hotelTypeName))
                {
                    return BadRequest("You must provide either hotelTypeId or hotelTypeName.");
                }

                // 如果提供了 hotelTypeId 而沒有提供 hotelTypeName，則查找 hotelTypeName
                if (hotelTypeId.HasValue && string.IsNullOrEmpty(hotelTypeName))
                {
                    var hotelType = await _context.HotelTypes.FindAsync(hotelTypeId.Value);
                    if (hotelType == null)
                    {
                        return NotFound("HotelTypeId not found.");
                    }
                    hotelTypeName = hotelType.HotelTypeName;
                }

                // 如果提供了 hotelTypeName 而沒有提供 hotelTypeId，則查找 hotelTypeId
                if (!string.IsNullOrEmpty(hotelTypeName) && !hotelTypeId.HasValue)
                {
                    var hotelType = await _context.HotelTypes.FirstOrDefaultAsync(ht => ht.HotelTypeName == hotelTypeName);
                    if (hotelType == null)
                    {
                        return NotFound("HotelTypeName not found.");
                    }
                    hotelTypeId = hotelType.HotelTypeId;
                }

                // 建立查詢
                var query = _context.Hotels.AsQueryable();

                // 根據 hotelTypeId 和 hotelTypeName 篩選
                query = query.Where(h => h.HotelTypeId == hotelTypeId && h.HotelType.HotelTypeName == hotelTypeName);

                // 選擇所需的結果
                var result = await query.Select(h => new
                {
                    HotelID = h.HotelId,
                    HotelName = h.HotelName
                }).ToListAsync();

                // 返回結果
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error while retrieving hotels.");
            }
        }


        // 更新飯店信息
        // PUT: api/BackSideHotel/5
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

        // 檢查飯店是否存在
        private bool HotelExists(int id)
        {
            return _context.Hotels.Any(e => e.HotelId == id);
        }
    }
}
