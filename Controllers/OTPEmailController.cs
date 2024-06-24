using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using System.Net.Mail;
using static System.Net.WebRequestMethods;
using BCrypt.Net;



//【這個API跟忘記密碼要寄送OTPEmail有關】
namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OTPEmailController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly IConfiguration _configuration;


        public OTPEmailController(FunNowContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
             
        }

        //修改密碼
        [HttpPut("{id}")]
        public async Task<IActionResult> EditPassword(int id, EditPwdDTO editPwd)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return BadRequest("一開始資料庫就沒有這個會員");
            }

            // 使用 BCrypt 來雜湊新密碼
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(editPwd.Password);

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

        //【單純寄OTPEmail的功能】
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] EmailQueryDTO request)
        {
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == request.Email);
            if (member == null)
            {
                return NotFound("Email not found");
            }

            string otpCode = GenerateOtp();
            member.Otpcode = otpCode;
            member.Otpexpiry = DateTime.UtcNow.AddSeconds(30); // OTP 有效期 30 秒
            _context.Members.Update(member);
            await _context.SaveChangesAsync();

            SendOtpEmail(request.Email, otpCode);

            return Ok("OTP sent");
        }


        //【驗證OTP碼是不是有效的功能】
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var otpRecord = await _context.Members.FirstOrDefaultAsync(o => o.Email == request.Email && o.Otpcode== request.OtpCode);
            if (otpRecord == null) //輸入的驗證碼不相符
            {
                return BadRequest(new { message = "OTP null" });
            }
            else if (otpRecord.Otpexpiry < DateTime.UtcNow) //驗證碼過期了
            {
                otpRecord.Otpcode = null; //將 OTP code 設為 null
                otpRecord.Otpexpiry = null;
                _context.Members.Update(otpRecord);
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "OTP expired" });
            }
            else //輸入的驗證碼相同
            {
                otpRecord.Otpcode = null; //將 OTP code 設為 null
                otpRecord.Otpexpiry = null;
                _context.Members.Update(otpRecord);
                await _context.SaveChangesAsync();
                return Ok("OTP verified");
            }
        }


        //【這裡只是產生一組隨機驗證碼的功能而已】
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }


        //【這裡只是寄信的功能而已】
        private void SendOtpEmail(string email, string otpCode)
        {

            string htmlBody = $@"
        <!DOCTYPE html>
        <html>
        <head>
        <style> 
         .code{{

             fontsize:50px;
             margin:5px;
             color:#0d6efd;
             font-weight: 600;
            letter-spacing: 0.5em;

          }}
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
            <h4>我們收到您重新設定密碼的要求,</h4>
            <h4>以下為您的一次性驗證碼:</h4>
            <h1 class=""code"">{otpCode}</h1>
            <h4>此驗證碼於30秒內有效。失效後請重新點選驗證</h4>
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
                To = { new MailAddress(email) },
                Subject = "FunNow重設密碼驗證信",
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
    }
}
