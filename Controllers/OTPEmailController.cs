using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using System.Net.Mail;
using static System.Net.WebRequestMethods;



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

        //【寄OTPEmail的功能】
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


        //【驗證OTP碼的功能】
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
                _context.Members.Remove(otpRecord); //刪掉舊的
                await _context.SaveChangesAsync();
                return BadRequest(new { message = "OTP expired" });
            }
            else //輸入的驗證碼相同
            {
                _context.Members.Remove(otpRecord); //只用一次所以要刪掉
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
        </head>
        <body>
            <h2>親愛的會員，您好:</h2>
            <p>我們收到您重新設定密碼的要求,</p>
            <p>您的驗證碼:</p>
            <p>{otpCode}</p>
            <p>此驗證碼於30秒內有效。失效後需重新申請</p>
        </body>
        </html>
    ";

            MailMessage mail = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:From"]), // 添加這行來設置發件人地址
                To = { new MailAddress(email) },
                Subject = "FunNow重設密碼連結",
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
