using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.louie_dto;
using System.Linq.Expressions;

namespace PrjFunNowWebApi.Controllers.louie_api
{
    [Route("api/[controller]")]
    [ApiController]
    public class pgBackMemberController : ControllerBase
    {
        private readonly FunNowContext _context;

        public pgBackMemberController(FunNowContext context)
        {
            _context = context;
        }
        //限API內部使用--------------------------------------------------------------------------------
        //GetMembersByRole(): 用于获取成员列表并转换为 DTO
        private async Task<List<pgBackMemberDTO>> getMembertoDTO(IQueryable<Member> query)
        {
            var membersWithRoles = await query
            .Select(m => new pgBackMemberDTO
            {
                MemberId = m.MemberId,
                FullName = m.FirstName + " " + m.LastName,
                Email = m.Email,
                Phone = m.Phone,
                RoleName = m.Role.RoleName
            })
            .ToListAsync();

            return membersWithRoles;
        }

        //要被前端呼叫的http方法----------------------------------------------------------------------------
        //狀態篩選紐
        [HttpGet("showAllMember")]
        public async Task<ActionResult<IEnumerable<pgBackMemberDTO>>> showAllMember()//秀全部房客
        {
            IQueryable<Member> query = _context.Members
             .Where(m => m.RoleId == 1 || m.RoleId == 3);

            var membersWithRoles = await getMembertoDTO(query);

            return Ok(membersWithRoles);
        }
        [HttpGet("showNormalMember")]
        public async Task<ActionResult<IEnumerable<pgBackMemberDTO>>> showNormalMember()//秀正常房客
        {
            IQueryable<Member> query = _context.Members
             .Where(m => m.RoleId == 1);

            var membersWithRoles = await getMembertoDTO(query);

            return Ok(membersWithRoles);
        }
        [HttpGet("showBlockedMember")]
        public async Task<ActionResult<IEnumerable<pgBackMemberDTO>>> showBlockedMember()//秀被封鎖房客
        {
            IQueryable<Member> query = _context.Members
             .Where(m => m.RoleId == 3);

            var membersWithRoles = await getMembertoDTO(query);

            return Ok(membersWithRoles);
        }
        //關鍵字搜尋
        [HttpGet("showMemberContainsKeyword")]
        public async Task<ActionResult<IEnumerable<pgBackMemberDTO>>> showMemberContainsKeyword([FromQuery] string keyword = "")
        {
            IQueryable<Member> query = _context.Members;

            if (string.IsNullOrEmpty(keyword))
            {
                query = query.Where(m => m.RoleId == 1 || m.RoleId == 3); // 没有关键字时，获取所有房客
            }
            else
            {
                query = query.Where(m => (m.RoleId == 1 || m.RoleId == 3) &&
                                         (m.FirstName.Contains(keyword) ||
                                          m.LastName.Contains(keyword) ||
                                          m.Email.Contains(keyword) ||
                                          m.Phone.Contains(keyword))); // 有关键字时，根据关键字进行搜索
            }

            var membersWithRoles = await getMembertoDTO(query);

            return Ok(membersWithRoles);
        }
        //更改房客狀態
        [HttpPut("updateMemberRole")]
        public async Task<IActionResult> UpdateMemberRole([FromBody] UpdateMemberRoleDTO dto)
        {
            var member = await _context.Members.Include(m => m.Role).FirstOrDefaultAsync(m => m.MemberId == dto.MemberId);
            if (member == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.NewRole);
            if (role == null)
            {
                return BadRequest("Invalid role.");
            }

            // 更新角色状态
            member.RoleId = role.RoleId; // 设置RoleId，而不是直接设置RoleName

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
