using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using System.Globalization;
using TinyPinyin;

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


        //【Get】根據MemberID查詢會員資料
        [HttpGet]
        [Route("searchByID")]
        public async Task<ActionResult<Member>> GetMemberByID(int ID)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == ID);
            if (member == null)
            {
                return NotFound();
            }

            // 假設FirstName為中文時，需要轉換成羅馬拼音
            if (IsChinese(member.FirstName))
            {
                member.FirstName = PinyinHelper.GetPinyin(member.FirstName);
            }

            return member;
        }

        // 判斷字符串是否包含中文字符
        private bool IsChinese(string input)
        {
            foreach (char c in input)
            {
                if (char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter)
                {
                    return true;
                }
            }
            return false;
        }





    }
}
