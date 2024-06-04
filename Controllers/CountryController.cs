using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly FunNowContext _context;

        public CountryController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/Countries
        [HttpGet("countriesAndCities")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            var countries = await _context.Countries
                                .Include(c => c.Cities)
                                .Select(c => new
                                {
                                    c.CountryId,
                                    c.CountryName,
                                    Cities = c.Cities.Select(city => new
                                    {
                                        city.CityId,
                                        city.CityName
                                    })
                                })
                                .ToListAsync();

            return Ok(countries);


        }


    }
}
