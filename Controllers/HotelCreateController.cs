using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
    public class HotelCreateController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly ILogger<HotelCreateController> _logger;

        public HotelCreateController(FunNowContext context, ILogger<HotelCreateController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/HotelCreate
        [HttpPost]
        public async Task<ActionResult<Hotel>> PostHotel([FromForm] string hotelData, [FromForm] string hotelEquipmentData)
        {

            try
            {
                var hotel = JsonSerializer.Deserialize<HotelDTO>(hotelData);
                var hotelEquipments = JsonSerializer.Deserialize<List<int>>(hotelEquipmentData);

                if (hotel != null)
                {
                    Hotel newHotel = new Hotel
                    {
                        HotelName = hotel.HotelName,
                        HotelAddress = "123",
                        HotelPhone = hotel.HotelPhone,
                        LevelStar = int.Parse(hotel.LevelStar),
                        HotelDescription = hotel.HotelDescription,
                        HotelTypeId = Convert.ToInt32(hotel.TypeID),
                        Latitude = null,
                        Longitude = null,
                        CityId = 10,
                        MemberId = 1,
                    };
                    _context.Hotels.Add(newHotel);
                    //await _context.SaveChangesAsync();

                    foreach (var equipmentId in hotelEquipments)
                    {
                        _context.HotelEquipmentReferences.Add(new HotelEquipmentReference
                        {
                            //HotelId = newHotel.HotelId,
                            HotelEquipmentId = equipmentId,
                        });
                    }

                    await _context.SaveChangesAsync();
                    return Ok(new { success = true });
                }

                return BadRequest(new { success = false, message = "Invalid hotel data." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating hotel.");
                return StatusCode(500, new { success = false, message = "Internal server error." });
            }
        }

       
    }
}
