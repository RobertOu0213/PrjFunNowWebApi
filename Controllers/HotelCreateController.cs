using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelCreateController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly ILogger<HotelCreateController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public HotelCreateController(FunNowContext context, ILogger<HotelCreateController> logger, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        // POST: api/HotelCreate
        [HttpPost]
        public async Task<ActionResult> PostHotel([FromForm] string hotelData, [FromForm] string roomData, [FromForm] int MemberId)
        {

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hotel = JsonSerializer.Deserialize<HotelDTO>(hotelData);
                var rooms = JsonSerializer.Deserialize<List<RoomDTO>>(roomData);

                if (hotel != null)
                {
                    Hotel newHotel = new Hotel
                    {
                        HotelName = hotel.HotelName,
                        HotelAddress = hotel.HotelAddress,
                        HotelPhone = hotel.HotelPhone,
                        HotelDescription = hotel.HotelDescription,
                        CityId = Convert.ToInt32(hotel.CityId),
                        HotelTypeId = Convert.ToInt32(hotel.TypeID),
                        LevelStar = Convert.ToInt32(hotel.LevelStar),
                        Latitude = hotel.Latitude,
                        Longitude = hotel.Longitude,
                        IsActive = false,
                        MemberId = MemberId
                    };
                    _context.Hotels.Add(newHotel);
                    await _context.SaveChangesAsync();

                    foreach (var equipmentId in hotel.HotelEquipmentID)
                    {
                        _context.HotelEquipmentReferences.Add(new HotelEquipmentReference
                        {
                            HotelId = newHotel.HotelId,
                            HotelEquipmentId = equipmentId,
                        });
                    }
                    await _context.SaveChangesAsync();

                    foreach (var image in hotel.HotelImage)
                    {
                        // 分割 base64 字串以獲取圖片的格式和數據
                        var parts = image.HotelImage.Split(new[] { ',' }, 2);
                        var header = parts[0];
                        var data = parts[1];

                        // 從 header 中提取出圖片的格式
                        var format = header.Split(new[] { ';' }, 2)[0].Split(new[] { '/' }, 2)[1];

                        // 將 base64 數據轉換為 byte 陣列
                        var imageBytes = Convert.FromBase64String(data);

                        // 產生一個 UUID 作為檔案名稱
                        var fileName = Guid.NewGuid().ToString() + "." + format;

                        // 讀取設定
                        var imageSavePath = _configuration.GetValue<string>("ImageSavePath");
                        if (imageSavePath == null)
                        {
                            return BadRequest(new { success = false, message = "Image save path is not configured." });
                        }

                        // 建立檔案的完整路徑
                        var filePath = Path.Combine(imageSavePath, "image", fileName);

                        // 將 byte 陣列寫入檔案
                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                        // 將檔案名稱儲存到資料庫
                        var hotelImage = new HotelImage
                        {
                            HotelId = newHotel.HotelId,
                            HotelImage1 = fileName
                        };
                        _context.HotelImages.Add(hotelImage);
                        await _context.SaveChangesAsync();

                        // 儲存 ImageCategoryID 到 ImageCategoryReference 資料表
                        if (!string.IsNullOrEmpty(image.ImageCategoryID))
                        {
                            _context.ImageCategoryReferences.Add(new ImageCategoryReference
                            {
                                HotelImageId = hotelImage.HotelImageId,
                                ImageCategoryId = Convert.ToInt32(image.ImageCategoryID)
                            });
                        }

                    }
                    await _context.SaveChangesAsync();

                    if (rooms == null)
                    {
                        // 處理 null 值，例如返回一個錯誤或設定一個預設值
                        return BadRequest(new { success = false, message = "rooms are null" });
                    }

                    foreach (var room in rooms)
                    {
                  
                        Room newRoom = new Room
                        {
                            HotelId = newHotel.HotelId,
                            RoomName = room.RoomName,
                            RoomSize = Convert.ToDecimal(room.RoomSize),
                            MaximumOccupancy = Convert.ToInt32(room.MaximumOccupancy),
                            RoomPrice = Convert.ToDecimal(room.RoomPrice),
                            RoomTypeId = Convert.ToInt32(room.RoomTypeID),
                            Description = room.Description,
                            MemberId = MemberId,
                            RoomStatus = true

                        };
                        _context.Rooms.Add(newRoom);
                        await _context.SaveChangesAsync();

                        foreach (var equipmentId in room.RoomEquipmentID)
                        {
                            _context.RoomEquipmentReferences.Add(new RoomEquipmentReference
                            {
                                RoomId = newRoom.RoomId,
                                RoomEquipmentId = equipmentId,
                            });
                        }
                        await _context.SaveChangesAsync();

                        foreach (var image in room.RoomImages)
                        {
                            // 分割 base64 字串以獲取圖片的格式和數據
                            var parts = image.RoomImage.Split(new[] { ',' }, 2);
                            var header = parts[0];
                            var data = parts[1];
                            var format = header.Split(new[] { ';' }, 2)[0].Split(new[] { '/' }, 2)[1];
                            var imageBytes = Convert.FromBase64String(data);
                            var fileName = Guid.NewGuid().ToString() + "." + format;
                            var imageSavePath = _configuration.GetValue<string>("ImageSavePath");
                            if (imageSavePath == null)
                            {
                                return BadRequest(new { success = false, message = "Image save path is not configured." });
                            }


                            var filePath = Path.Combine(imageSavePath, "image", fileName);
                            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    

                            // 將檔案名稱儲存到資料庫
                            var roomImage = new RoomImage
                            {
                                RoomId = newRoom.RoomId,
                                RoomImage1 = fileName
                            };
                            _context.RoomImages.Add(roomImage);
                            await _context.SaveChangesAsync();

                            // 儲存 ImageCategoryID 到 ImageCategoryReference 資料表
                            if (!string.IsNullOrEmpty(image.ImageCategoryID))
                            {
                                _context.ImageCategoryReferences.Add(new ImageCategoryReference
                                {
                                    RoomImageId = roomImage.RoomImageId,
                                    ImageCategoryId = Convert.ToInt32(image.ImageCategoryID)
                                });
                            }
                        }
                        await _context.SaveChangesAsync();

                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { success = true });
                }

                return BadRequest(new { success = false, message = "Invalid hotel data." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }
    }
}
