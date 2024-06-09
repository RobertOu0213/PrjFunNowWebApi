using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using PrjFunNowWebApi.Models.louie_dto;

namespace PrjFunNowWebApi.Controllers.louie_api
{
    [Route("api/[controller]")]
    [ApiController]
    public class pgHotelController : ControllerBase
    {
        private readonly FunNowContext _context;

        public pgHotelController(FunNowContext context)
        {
            _context = context;
        }
        //限API內部使用--------------------------------------------------------------------------------

        //要被前端呼叫的http方法----------------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<pgHotel_HotelDetailDTO>> GetHotelDetail(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.City)
                .ThenInclude(c => c.Country)
                .Include(h => h.HotelEquipmentReferences)
                .ThenInclude(r => r.HotelEquipment)
                .Include(h => h.HotelImages)
                .ThenInclude(img => img.ImageCategoryReferences)
                .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomEquipmentReferences)
                .ThenInclude(re => re.RoomEquipment)
                .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomImages)
                .ThenInclude(ri => ri.ImageCategoryReferences)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
            {
                return NotFound();
            }

            var hotelDetail = new pgHotel_HotelDetailDTO
            {
                HotelName = hotel.HotelName,
                HotelAddress = hotel.HotelAddress,
                HotelDescription = hotel.HotelDescription,
                CityName = hotel.City.CityName,
                CountryName = hotel.City.Country.CountryName,
                LevelStar = hotel.LevelStar,
                Latitude = hotel.Latitude,
                Longitude = hotel.Longitude,
                IsActive = (bool)hotel.IsActive,
                MemberID = hotel.MemberId,
                HotelEquipments = hotel.HotelEquipmentReferences.Select(e => e.HotelEquipment.HotelEquipmentName).ToList(),
                HotelImages = hotel.HotelImages.Select(i => new pgHotel_ImageDTO
                {
                    ImageUrl = i.HotelImage1,
                    ImageCategoryID = i.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault()
                }).ToList(),
                Rooms = hotel.Rooms.Select(r => new pgHotel_RoomDTO
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    RoomPrice = r.RoomPrice,
                    MaximumOccupancy = r.MaximumOccupancy,
                    MemberID = r.MemberId,
                    RoomEquipments = r.RoomEquipmentReferences.Select(e => e.RoomEquipment.RoomEquipmentName).ToList(),
                    RoomImages = r.RoomImages.Select(i => new pgHotel_ImageDTO
                    {
                        ImageUrl = i.RoomImage1,
                        ImageCategoryID = i.ImageCategoryReferences.Select(ic => ic.ImageCategoryId).FirstOrDefault()
                    }).ToList()

                }).ToList()
            };

            return Ok(hotelDetail);
        }
    }
}

