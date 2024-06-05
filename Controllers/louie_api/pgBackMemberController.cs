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
        private async Task<List<pgBackMemberDTO>> getMembertoDTO(Expression<Func<Member, bool>> predicate)
        {
            var query = _context.Members.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

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
        [HttpGet("showAllMember")]
        public async Task<ActionResult<IEnumerable<pgBackMemberDTO>>> showAllMember()//秀全部房客
        {
            var membersWithRoles = await getMembertoDTO(m => m.RoleId == 1 || m.RoleId == 3);
            return Ok(membersWithRoles);
        }
        [HttpGet("showNormalMember")]
        public async Task<ActionResult<IEnumerable<pgBackMemberDTO>>> showNormalMember()//秀正常房客
        {
            var membersWithRoles = await getMembertoDTO(m => m.RoleId == 1);
            return Ok(membersWithRoles);
        }
        [HttpGet("showBlockedMember")]
        public async Task<ActionResult<IEnumerable<pgBackMemberDTO>>> showBlockedMember()//秀被封鎖房客
        {
            var membersWithRoles = await getMembertoDTO(m => m.RoleId == 3);
            return Ok(membersWithRoles);
        }
    }
}
