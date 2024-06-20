using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using Microsoft.Extensions.Logging;  // 添加這一行

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebsocketController : ControllerBase
    {
        private readonly FunNowContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<WebsocketController> _logger;  // 添加這一行

        public WebsocketController(FunNowContext context, IHubContext<ChatHub> hubContext, ILogger<WebsocketController> logger) // 修改這一行
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;  // 修改這一行
        }



        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage(int senderId, int receiverId, [FromBody] string message)
        {
            // 驗證 senderId 和 receiverId 是否有效
            if (senderId <= 0 || receiverId <= 0)
            {
                return BadRequest("Invalid sender or receiver ID.");
            }

            // 獲取發送者和接收者
            var sender = await _context.Members.FindAsync(senderId);
            var receiver = await _context.Members.FindAsync(receiverId);

            if (sender == null || receiver == null)
            {
                return NotFound("Sender or receiver not found.");
            }

            try
            {
                var sendTime = DateTime.UtcNow; // 記錄傳送時間

                // 儲存訊息到 Dialog 表
                await SaveMessageAsync(senderId, receiverId, message, sendTime);

                // 使用 SignalR 發送訊息給接收者
                await _hubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId.ToString(), message);

                return Ok();
            }
            catch (Exception ex)
            {
                // 記錄異常
                _logger.LogError(ex, "Error sending message.");
                return StatusCode(500, "Internal server error.");
            }
        }

        private async Task SaveMessageAsync(int senderId, int receiverId, string message, DateTime sendTime)
        {
            var dialog = new Dialog
            {
                MemberId = senderId,
                CalltoMemberId = receiverId,  // 設置接收者的 ID
                Detail = message,
                CreateAt = DateTime.Now // 使用傳送時間
            };

            _context.Dialogs.Add(dialog);
            await _context.SaveChangesAsync();
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }
    }
}
