using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

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

        //根據 cityID 查詢國家
        [HttpGet("cityDetails")]
        public async Task<IActionResult> GetCityDetails(int cityId)
        {
            var city = await _context.Cities
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.CityId == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var cityDetails = new
            {
                CityId = city.CityId,
                CityName = city.CityName,
                CountryId = city.Country.CountryId,
                CountryName = city.Country.CountryName,
            };

            return Ok(cityDetails);
        }


        ////修改會員所有資料
        //[HttpPut("{id}")]
        //public async Task<IActionResult> HostMemberEdit(int id, HostMemberEditDTO edit)
        //{
        //    var member = await _context.Members.FindAsync(id);
        //    if (member == null)
        //    {
        //        return BadRequest("一開始資料庫就沒有這個會員");
        //    }

        //    member.FirstName = edit.FirstName;
        //    member.LastName = edit.LastName;
        //    member.Phone = edit.Phone;
        //    member.Birthday = edit.Birthday;
        //    member.CityId = edit.CityId;
        //    member.MemberAddress = edit.MemberAddress;
        //    member.Introduction = edit.Introduction;


        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!MemberExists(id))
        //        {
        //            return BadRequest("你在把更新資料存進資料庫時找不到這個會員了");
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return Ok("會員資料修改成功");
        //}

        //private bool MemberExists(int id)
        //{
        //    return _context.Members.Any(e => e.MemberId == id);
        //}


        //public class HostMemberEditDTO
        //{

        //    public string FirstName { get; set; }

        //    public string LastName { get; set; }

        //    public string Phone { get; set; }

        //    public DateTime? Birthday { get; set; }

        //    public int? CityId { get; set; }

        //    public string MemberAddress { get; set; }

        //    public string Introduction { get; set; }


        //}



       

    }
}
