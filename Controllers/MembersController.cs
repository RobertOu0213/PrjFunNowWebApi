using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
        private readonly IConfiguration _configuration;

        public MembersController(FunNowContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            // 在資料庫中查詢是否存在相符的 Email 記錄以及IsVerified狀態
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == model.Email);

            if (member != null)
            {
                // 檢查IsVerified欄位
                if (member.IsVerified)
                {
                    return Ok(new { message = "YES" });
                }
                else
                {
                    return Ok(new { message = "NoVerify" });
                }
            }
            else
            {
                return NotFound(new { message = "NO" });
            }
        }


        //【Post】新增會員
        [HttpPost]
        public async Task<ActionResult<RegisterMemberDTO>> CreateMember(RegisterMemberDTO registerMember)
        {

            // 儲存會員資訊
            Member members = new Member();
            members.FirstName = registerMember.FirstName;
            members.Email = registerMember.Email;
            members.Password = registerMember.Password;
            members.LastName = registerMember.LastName;
            members.VerificationToken = Guid.NewGuid().ToString();
            members.VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(5);
            members.IsVerified = false;

            _context.Members.Add(members);
            await _context.SaveChangesAsync();

            // 發送驗證信
            try
            {
                SendVerificationEmail(members);
                return CreatedAtAction("CreateMember", new { id = members.MemberId }, members);
            }
            catch (Exception ex)
            {
                return BadRequest($"驗證信寄送失敗! {ex.Message}");
            }
  
        }

        private void SendVerificationEmail(Member members)
        {
            string verificationUrl = $"{_configuration["AppSettings:BaseUrl"]}/api/Members/VerifyEmail?token={members.VerificationToken}";

            MailMessage mail = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:From"]), // 添加這行來設置發件人地址
                To = { new MailAddress(members.Email) },
                Subject = "[系統自動發出]FunNow!點擊連結啟用您的帳號",
                Body = $"請點擊以下連結來啟用您的帳號: {verificationUrl}",
                IsBodyHtml = true
            };

            try
            {
                using (SmtpClient smtpClient = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"])))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mail);
                }
            }
            catch (SmtpException smtpEx)
            {
                // 捕捉 SMTP 相關的異常，並記錄具體錯誤訊息
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // 捕捉所有其他異常
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }

        //【Get】驗證郵件
        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var member = await _context.Members.SingleOrDefaultAsync(m => m.VerificationToken == token && m.VerificationTokenExpiry > DateTime.UtcNow);

            if (member == null)
            {
                return BadRequest("無效的驗證碼或驗證碼已經過期");
            }

            //把資料庫的驗證狀態改為true，再把token刪掉
            member.IsVerified = true;
            member.VerificationToken = null;
            member.VerificationTokenExpiry = null;
            await _context.SaveChangesAsync();

            // 重新導向至MVC專案中的/Member/Login頁面
            string loginPageUrl = _configuration["AppSettings:MVCBaseUrl"] + "/Member/Login";
            return Redirect(loginPageUrl);
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
