using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models.DTO;
using PrjFunNowWebApi.Models;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeleteMemberController : ControllerBase
    {

        private readonly FunNowContext _context;
        private readonly IConfiguration _configuration;

        public DeleteMemberController(FunNowContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        //修改會員角色(其實是軟刪除，把會員角色改為4)
        [HttpPut("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return BadRequest("一開始資料庫就沒有這個會員");
            }
            member.RoleId = 4;

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

            return Ok("角色修改成功");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }



    }
}
