using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MembersController : ControllerBase
    {
        private readonly FunNowContext _context;

        public MembersController(FunNowContext context)
        {
            _context = context;
        }

        //【Get】查詢所有會員
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        //【Get】根據關鍵字查詢會員
        [HttpGet]
        [Route("search")]
        public async Task<ActionResult<IEnumerable<Member>>> GetMemberByKeyword(string keyword)
        {
            return await _context.Members.Where(m => m.FirstName.Contains(keyword) || m.Phone.Contains(keyword) || m.Email.Contains(keyword)).ToListAsync();
        }

        //查有無相符Email
        [HttpPost("query")]
        public async Task<IActionResult> QueryEmail([FromBody] EmailQueryDTO model)
        {
            // 在資料庫中查詢是否存在相符的 Email 記錄
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == model.Email);

            if (member != null)
            {
                return Ok(new { message = "YES" });
            }
            else
            {
                return NotFound(new { message ="NO" });
            }
        }


        //【Post】新增會員
        [HttpPost]
        public async Task<ActionResult<RegisterMemberDTO>> CreateMember(RegisterMemberDTO registerMember)
        {
            Member members = new Member();
            members.FirstName = registerMember.FirstName;
            members.Email = registerMember.Email;
            members.Password = registerMember.Password;
            members.LastName = registerMember.LastName;

            _context.Members.Add(members);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateMember", new { id = members.MemberId }, members);
        }


        //【Put】修改會員資料
        [HttpPut("{id}")]
        public async Task<IActionResult> EditMember(int id, EditMemberDTO editMember)
        {
    
            var member = await _context.Members.FindAsync(id); //使用FindAsync方法從資料庫中找有沒有符合輸入id的MemberID
            if (member == null) //如果沒有符合的會員
            {
                return BadRequest("一開始資料庫就沒有這個會員");
            }

            //如果有找到符合id的會員
            // 把接受到的editMember資料更新到Member屬性中
            member.FirstName = editMember.FirstName;
            member.LastName = editMember.LastName;
            member.Password = editMember.Password;
            member.Phone = editMember.Phone;
            member.Birthday = editMember.Birthday;
            member.Image = editMember.Image;

            try
            {
                await _context.SaveChangesAsync();
            }

            //如果在資料庫儲存過程中發生衝突（ex.在你尋找會員和儲存更新資料的同時，會員可能被其他人刪掉了），
            //這時候就會引發DbUpdateConcurrencyException異常。    
            catch (DbUpdateConcurrencyException)
            {
             
                
                if (!MemberExists(id)) //所以這邊才又再檢查一次，是不是真的有這個會員id
                {
                    return BadRequest("你在把更新資料存進資料庫時找不到這個會員了");
                }
                else
                {
                    throw; //如果會員存在，則重新拋出異常，此時，異常可能是其他原因造成的
                }
            }

            return Content("更新成功~~");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }
    
        
        // 【Delete】刪除會員
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return BadRequest("沒有這個會員喔~");
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return Content("刪除成功!!!");
        }

        
    }
}
