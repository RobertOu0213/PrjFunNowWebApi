using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Hubs;
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

        // GET: api/Websocket
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        // GET: api/Websocket/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Websocket/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.MemberId)
            {
                return BadRequest();
            }

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Websocket
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.MemberId }, member);
        }

        // DELETE: api/Websocket/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
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
                // 儲存訊息到 Dialog 表
                await SaveMessageAsync(senderId, receiverId, message);

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

        private async Task SaveMessageAsync(int senderId, int receiverId, string message)
        {
            var dialog = new Dialog
            {
                MemberId = senderId,
                CalltoMemberId = receiverId,  // 設置接收者的 ID
                Detail = message,
                CreateAt = DateTime.UtcNow
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
