using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly IConfiguration _configuration;

        public EditController(FunNowContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        //修改電話
        [HttpPut("{id}")]
        public async Task<IActionResult> EditPhone(int id, EditPhoneDTO editPhone)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return BadRequest("一開始資料庫就沒有這個會員");
            }
            member.Phone = editPhone.Phone;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return BadRequest("你在把更新資料存進資料庫時找不到這個會員了");
                }
                else
                {
                    throw;
                }
            }

            return Ok("電話修改成功");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }


        //根據MemberID檢查密碼是否正確
        [HttpPost("CheckPassword")]
        public async Task<IActionResult> CheckPassword(CheckPasswordDTO checkPasswordDto)
        {
            var member = await _context.Members.FindAsync(checkPasswordDto.MemberID);
            if (member == null)
            {
                return BadRequest("會員不存在");
            }

            
            if (member.Password == checkPasswordDto.Password)
            {
                return Ok("密碼正確");
            }
            else
            {
                return BadRequest("密碼不正確");
            }
        }

        public class CheckPasswordDTO
        {
            public int MemberID { get; set; }
            public string Password { get; set; }
        }

    }
}
