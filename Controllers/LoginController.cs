using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PrjFunNowWebApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace PrjFunNowWebApi.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly FunNowContext _context;
      
        public LoginController(IConfiguration config, FunNowContext context)
        {
            _config = config; //注入appsettings.json
            _context = context;
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody]LoginRequestt loginRequest)
        {
            //呼叫 Authenticate 方法進行帳號密碼驗證
            bool isAuthenticated = Authenticate(loginRequest).Result;

            if (isAuthenticated)
            {
                //拿到jwt Token
                var token = GenerateToken(loginRequest.Email);

                //拿到Member ID
                var memberInfo = GetMemberInfo(loginRequest);

                return Ok(new { token, memberInfo });
            }

            return BadRequest("登入失敗");
        }

        //用來進行帳號密碼比對的方法
        private async Task<bool> Authenticate(LoginRequestt loginRequest)
        {
            // 從 FunNowContext 中取得 Member 資料表，比對資料
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == loginRequest.Email);

            // 如果找到符合的會員資料
            if (member != null)
            {
                // 比對密碼是否正確
                if (member.Password == loginRequest.Password)
                {
                    return true; // 驗證成功
                }
            }
            return false; // 驗證失敗
        }


        //用來產生jwt Token的方法
        private string GenerateToken(string MemberEmail)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); //把key用 HmacSha256的方式加密

            // 根據Email從資料庫中獲取對應的Member
            var member = _context.Members.FirstOrDefault(m => m.Email == MemberEmail);
            if (member == null)
            {
                throw new ArgumentException("Invalid member email");
            }

            // 創建包含Member ID的聲明
            var claims = new[]
            {
               new Claim("MemberID", member.MemberId.ToString())
            };

            // 生成JWT token
            var token = new JwtSecurityToken (
                _config.GetSection("Jwt:Issuer").Value,
                _config.GetSection("Jwt:Audience").Value,
                claims,
                expires:DateTime.Now.AddMinutes(60),
                signingCredentials:credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private MemberInfo GetMemberInfo(LoginRequestt loginRequest)
        {
            // 根據Email從資料庫中拿到MemberID 
            var member = _context.Members.FirstOrDefault(m => m.Email == loginRequest.Email);
            if (member != null)
            {
                return new MemberInfo
                {
                    MemberId = member.MemberId,
                    FirstName = member.FirstName,
                    Email = member.Email,
                    Password = member.Password,
                    Phone = member.Phone,
                    Birthday = member.Birthday,
                    RoleId = member.RoleId,
                    Image = member.Image,
                    LastName = member.LastName,
                };
            }
            return null;
        }


        //這個給Angular用的
        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Token成功驗證通過!");
        }



    }
}
