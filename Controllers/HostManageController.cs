using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostManageController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly IConfiguration _configuration;
        private static int _userId;

        public HostManageController(FunNowContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/HostManage
        [HttpGet]
        public async Task<ActionResult> GetHotels(int? userId)
        {

            if (userId == null || userId <= 0)
            {
                return BadRequest("Invalid userId.");
            }
            // 讀取設定
            var imageSavePath = _configuration.GetValue<string>("ImageSavePath");

            var hotels = await (from h in _context.Hotels
                                where h.MemberId == userId
                                select new
                                {
                                    HotelId = h.HotelId,
                                    HotelName = h.HotelName,
                                    CityName = h.City.CityName,
                                    CountryName = h.City.Country.CountryName,
                                    HotelImage = h.HotelImages.Select(hi => hi.HotelImage1).FirstOrDefault(),
                                    isActive = h.IsActive
                                }).ToListAsync();

            var result = hotels.Select(h => new
            {
                h.HotelId,
                h.HotelName,
                h.CityName,
                h.CountryName,
                HotelImage = h.HotelImage != null && (h.HotelImage.StartsWith("http://") || h.HotelImage.StartsWith("https://"))
                             ? h.HotelImage
                             : $"image/{h.HotelImage}",
                h.isActive

            }).ToList();

            return Ok(result);
           

               

        }



    }
}
