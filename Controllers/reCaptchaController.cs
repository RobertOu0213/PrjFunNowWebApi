using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace PrjFunNowWebApi.Controllers
{
    //reCaptcha我不是機器人的參考網址:https://reurl.cc/Vzle26

    [Route("api/[controller]")]
    [ApiController]
    public class reCaptchaController : ControllerBase
    {
        //放從GoogleDep取得的密鑰
        private const string ReCaptchaSecret = "6Ld5o_cpAAAAAOi9Rr9zOWyRfgpXjHD5sf1mo-RM";

        [HttpPost("VerifyCaptcha")]
        public async Task<IActionResult> VerifyCaptcha([FromBody] CaptchaRequest captchaRequest)
        {
            if (string.IsNullOrEmpty(captchaRequest.Response))
            {
                return BadRequest(new { success = false, message = "沒拿到Captcha response" });
            }

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={ReCaptchaSecret}&response={captchaRequest.Response}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(await response.Content.ReadAsStringAsync());
                    if (captchaResponse.Success)
                    {
                        return Ok(new { success = true });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "Captcha 驗證失敗" });
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new { success = false, message = "Captcha 驗證失敗" });
                }
            }
        }

        //這些Model應該要放在DTO比較好，但我好懶得一直新增，就放這裡吧~
        public class CaptchaRequest
        {
            public string Response { get; set; }
        }

        public class CaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("challenge_ts")]
            public string ChallengeTs { get; set; }
            [JsonProperty("hostname")]
            public string Hostname { get; set; }
            [JsonProperty("error-codes")]
            public string[] ErrorCodes { get; set; }
        }

    }
}
