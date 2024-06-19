using Microsoft.AspNetCore.Mvc;
using PrjFunNowWebApi.Services;
using System;
using System.Threading.Tasks;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Body))
            {
                return BadRequest("Request data is invalid.");
            }

            try
            {
                // 添加郵件樣式
                string styledBody = $@"";
                // 發送郵件，使用 styledBody 作為郵件內容
                await _emailService.SendEmailAsync(request.To, request.Subject, styledBody);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while sending email: {ex.Message}");
            }
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Body))
            {
                return BadRequest("Request data is invalid.");
            }

            try
            {
                // 添加訊息樣式
                string styledBody = $@"
        <!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>FunNow樂遊網</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3"" crossorigin=""anonymous"">
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
        <div class=""sender"">來自 FunNow樂遊網 的訊息</div>
                    <p>{request.Body}</p>
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
  <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p"" crossorigin=""anonymous""></script>
</body>
</html>";
                // 發送郵件，使用 styledBody 作為郵件內容
                await _emailService.SendEmailAsync(request.To, request.Subject, styledBody);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while sending message: {ex.Message}");
            }
        }
    }

    public class SendEmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class SendMessageRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
