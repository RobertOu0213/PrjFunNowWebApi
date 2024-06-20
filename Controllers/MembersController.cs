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
            return member;
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


        //根據Email傳回memberID
        [HttpPost("returnID")]
        public async Task<IActionResult> QueryEmailReturnID([FromBody] EmailQueryDTO model)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == model.Email);
            if (member != null)
            {
                return Ok(new { memberId = member.MemberId });

            }
            else
            {
                return NotFound(new { message = "NO" });
            }
        }

        //根據memberID傳回roleID
        [HttpPost("returnRoleID")]
        public async Task<IActionResult> QueryMemberIDReturnRoleID([FromBody] MemberIDQueryDTO model)
        {
            if (model == null || string.IsNullOrEmpty(model.MemberId.ToString()))
            {
                return BadRequest("Invalid request data");
            }

            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == model.MemberId);
            if (member != null)
            {
                return Ok(new { roleID = member.RoleId });
            }
            else
            {
                return NotFound(new { message = "NO" });
            }
        }


        //修改姓名
        [HttpPut("{id}")]
        public async Task<IActionResult> EditPassword(int id, EditNameDTO editName)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return BadRequest("一開始資料庫就沒有這個會員");
            }

            member.FirstName = editName.FirstName;
            member.LastName = editName.LastName;

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

            return Ok("姓名修改成功");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
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
            members.VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(10);
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

        //【Post】重寄驗證信
        [HttpPost("reSent")]
        public async Task<ActionResult<EmailQueryDTO>> reSent(EmailQueryDTO resentMember)
        {

            // 在資料庫中查詢是否存在相符的 Email 記錄以及IsVerified狀態
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == resentMember.Email);

            if (member != null)
            {
                // 檢查IsVerified欄位
                if (member.IsVerified) //這個帳號早就驗證過了，來亂的(但基本上前端就會被擋掉)
                {
                    return Ok(new { message = "Done" });
                }
                else //這個帳號註冊過，但真的還沒驗證過
                {
                    // 更新會員的 VerificationToken 和 VerificationTokenExpiry
                    member.VerificationToken = Guid.NewGuid().ToString();
                    member.VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(10);

                    // 保存更改到資料庫
                    _context.Members.Update(member);
                    await _context.SaveChangesAsync();


                    // 發送驗證信
                    try
                    {
                        SendVerificationEmail(member);
                        return Ok(new { message = "Success" });
                    }
                    catch (Exception ex)
                    {
                        return BadRequest($"驗證信寄送失敗! {ex.Message}");
                    }
                    
                }
            }
            else //這個帳號根本就沒註冊過，來亂的(但基本上前端就會被擋掉)
            {
                return NotFound(new { message = "NOMember" }); 
            }

        }

        
        //驗證信內容
        private void SendVerificationEmail(Member members)
        {
            string verificationUrl = $"{_configuration["AppSettings:BaseUrl"]}/api/Members/VerifyEmail?token={members.VerificationToken}";
            string htmlBody = $@"
        <!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>FunNow樂遊網</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
            color: #333;
            padding: 20px;
            margin: 0;
        }}

        .message-container {{
            background-color: #fff;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            max-width: 600px;
            margin: 0 auto;
        }}

        .header {{
            background-color: #fff;
            padding: 10px;
            text-align: center;
            height: 50px;
            width: 60px;
        }}

            .header img {{
                max-width: 100%;
                height: auto;
                cursor: pointer;
            }}

        .sender {{
            background-color: #6d4dff;
            color: #fff;
            padding: 10px;
            font-weight: bold;
        }}

        .message-content {{
            padding: 20px;
            text-align: center;
        }}

            .message-content img {{
                max-width: 100%;
                height: auto;
            }}

        .links {{
            background-color: #f0f0f0;
            padding: 10px;
            text-align: center;
        }}

            .links a {{
                color: #6d4dff;
                text-decoration: none;
                margin: 0 10px;
            }}

        .social-icons {{
            background-color: #f0f0f0;
            padding: 10px;
            text-align: center;
        }}

            .social-icons img {{
                max-width: 30px;
                height: auto;
                margin: 0 5px;
                cursor: pointer;
            }}

        hr {{
            border: none;
            border-top: 1px solid #ccc;
            margin: 20px 0;
        }}

        .footer
        {{
            background-color: #f0f0f0;
            padding: 10px;
            border-radius: 0 0 4px 4px;
            text-align: center;
            font-size: 14px;
            color: #999;
        }}


            .footer img {{
                max-width: 100px;
                height: auto;
                cursor: pointer;
            }}
    </style>
</head>
<body>
    <div class=""message-container"">
        <div class=""header"">
            <a href=""https://localhost:7284/home/index"">
                <img src=""https://imgur.com/uOBWVvi.jpg"" alt=""FunNow樂遊網"">
            </a>
        </div>
                    <h2>親愛的會員，您好:</h2>
                    <p>我們收到您註冊帳號的要求，</p>
                    <p>請點擊下方按鈕完成註冊。</p>
                    <a href=""{verificationUrl}"" style=""display: inline-block; padding: 10px 20px; background-color: #007bff; color: #fff; text-decoration: none; border-radius: 5px;"">完成註冊驗證</a>
                    <p>此連結於 24 小時內有效。失效後需重新註冊</p>
                    <p>系統將再次寄送驗證信。</p>
                    <p>如非本人操作，請忽略或關閉此信件。</p>
              <div class=""message-content"">
            <img src=""https://imgur.com/7b5OMro.png"" alt=""回覆顧客問題就是這麼簡單！"">
            <p>FunNow 體貼你，只要直接回覆這封郵件，你的答覆就會直接發送給客服專員，不用再費心編輯或輸入電子信箱，客服專員將會協助您處理。</p>
        </div>
        <div class=""links"">
            <a href=""https://localhost:7284/home/index"">24小時客服中心</a>
            <a href=""https://localhost:7284/home/index"">關於FunNow</a>
            <a href=""https://localhost:7284/home/index"">隱私權政策</a>
        </div>
        <div class=""social-icons"">
            <a href=""https://www.instagram.com/agoda"">
                <img src=""https://imgur.com/8LPZSM2.png"" alt=""Instagram"">
            </a>
            <a href=""https://www.facebook.com/agodataiwan/?brand_redir=159632516873"">
                <img src=""https://imgur.com/twosdWQ.png"" alt=""Facebook"">
            </a>
            <a href=""https://www.pinterest.com/agodadotcom/"">
                <img src=""https://imgur.com/DFhfwxH.png"" alt=""Twitter"">
            </a>
        </div>
        <hr>
        <div class=""footer"">
            FunNow Company Pte. Ltd（以下簡稱「FunNow 」）提供住宿預訂和其他支援服務，FunNow 僅作為住宿供應商和FunNow 用戶之間的聯絡平台，此處所有資訊交換都是介於住宿供應商和FunNow 用戶之間，因此不包含或代表FunNow 的觀點、意見或建議。此外，不論住宿供應商是否有向FunNow 用戶提供住宿或服務，FunNow 不會為住宿供應商和FunNow 用戶之間所交換的任何資訊負責。
        </div>
        <hr>
        <div class=""footer"">
            本電子郵件由FunNow Company Pte. Ltd.發送（地址：30 Cecil Street, Prudential Tower #19-08, Singapore, 049712）。
            <br>
            <a href=""https://localhost:7284/home/index"">
                <img src=""https://imgur.com/uOBWVvi.jpg"" alt=""FunNow樂遊網"">
            </a>
        </div>
    </div>
   
             </body>
           </html>
           ";

            MailMessage mail = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:From"]), // 添加這行來設置發件人地址
                To = { new MailAddress(members.Email) },
                Subject = "FunNow歡迎你的加入!快點擊連結啟用您的帳號",
                Body = htmlBody,
                IsBodyHtml = true,
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
                return BadRequest("無效的驗證碼或驗證碼已經過期，請至登入頁面輸入Email，以取得重新寄發驗證信的連結");
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
