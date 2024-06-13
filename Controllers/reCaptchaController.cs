using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class reCaptchaController : ControllerBase
    {
        private const string ReCaptchaSecret = "6Ld5o_cpAAAAAOi9Rr9zOWyRfgpXjHD5sf1mo-RM";

        [HttpPost("VerifyCaptcha")]
        public async Task<IActionResult> VerifyCaptcha([FromBody] CaptchaRequest captchaRequest)
        {
            if (string.IsNullOrEmpty(captchaRequest.Response))
            {
                return BadRequest(new { success = false, message = "Captcha response is missing" });
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
                        return BadRequest(new { success = false, message = "Captcha validation failed" });
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new { success = false, message = "Captcha validation request failed" });
                }
            }
        }

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
