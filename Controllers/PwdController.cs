using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PwdController : ControllerBase
    {

        private readonly FunNowContext _context;
        

        public PwdController(FunNowContext context)
        {
            _context = context;
            
        }


        //修改密碼
        [HttpPut("{id}")]
        public async Task<IActionResult> EditPhone(int id, SetNewPwdDTO setNewPwd)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return BadRequest("一開始資料庫就沒有這個會員");
            }

            // 使用 BCrypt 來雜湊新密碼
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(setNewPwd.Password);

            member.Password = hashedPassword;

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

            return Ok("密碼修改成功");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }

        public class SetNewPwdDTO
        {
            
            public string Password { get; set; }
        }
    }
}
