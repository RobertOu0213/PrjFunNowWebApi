using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace PrjFunNowWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly FunNowContext _context;

        public AuthController(IConfiguration config, FunNowContext context)
        {
            _config = config;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody]LoginMemberDTO login)
        {
            Member m = new Member();
      

            if (login.Email =="ruby"  && login.Password == "1234")
            {
                var token = GenerateToken(login.Email);
                return Ok(token);
            }

            return BadRequest("登入失敗");

        }

        private string GenerateToken(string UserName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); //把key用 HmacSha256的方式加密

            var claims = new[]
            {
               new Claim(ClaimTypes.NameIdentifier,UserName)
            };

            var token = new JwtSecurityToken (
                _config.GetSection("Jwt:Issuer").Value,_config.GetSection("Jwt:Audience").Value,
                claims,
                expires:DateTime.Now.AddMinutes(15),
                signingCredentials:credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }


        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Token成功驗證通過!");
        }



    }
}
