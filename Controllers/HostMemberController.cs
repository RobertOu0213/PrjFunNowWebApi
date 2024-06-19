using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostMemberController : ControllerBase
    {
        private readonly FunNowContext _context;
        

        public HostMemberController(FunNowContext context)
        {
            _context = context;
            
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await _context.Countries
                                          .Select(c => new
                                          {
                                              c.CountryId,
                                              c.CountryName
                                          })
                                          .ToListAsync();

            return Ok(countries);
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities([FromQuery] int countryId)
        {
            var cities = await _context.Cities
                                       .Where(c => c.CountryId == countryId)
                                       .Select(c => new
                                       {
                                           c.CityId,
                                           c.CityName
                                       })
                                       .ToListAsync();

            if (cities == null || !cities.Any())
            {
                return NotFound("Country not found or no cities available");
            }

            return Ok(cities);
        }
    }
}
