using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.louie_dto;
using System.Linq.Expressions;
using PrjFunNowWebApi.Models.louie_class;

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
        private async Task<List<pgBackMemberDTO>> getMembertoDTOAsync(IQueryable<Member> pagedQuery)
        {
            var membersWithRoles = await pagedQuery
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
        //getPagingAsync():分頁
        private async Task<CPaging<pgBackMemberDTO>> getPagingAsync(IQueryable<Member> query, int pageNumber, int pageSize)
        {
            int totalRecords = await query.CountAsync();

            var pagedQuery = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var membersWithRoles = await getMembertoDTOAsync(pagedQuery);

            return new CPaging<pgBackMemberDTO>(membersWithRoles, totalRecords, pageNumber, pageSize);
        }

        //要被前端呼叫的http方法----------------------------------------------------------------------------
        //狀態篩選紐
        [HttpPost("showAllMember")]
        public async Task<ActionResult<CPaging<pgBackMemberDTO>>> showAllMember([FromBody] SearchParametersDTO searchParameters)//秀全部房客
        {
            IQueryable<Member> query = _context.Members
             .Where(m => m.RoleId == 1 || m.RoleId == 3);

            var pagedResult = await getPagingAsync(query, searchParameters.PageNumber, searchParameters.PageSize);

            return Ok(pagedResult);
        }
        [HttpPost("showNormalMember")]
        public async Task<ActionResult<CPaging<pgBackMemberDTO>>> showNormalMember([FromBody] SearchParametersDTO searchParameters)//秀正常房客
        {
            IQueryable<Member> query = _context.Members
             .Where(m => m.RoleId == 1);

            var pagedResult = await getPagingAsync(query, searchParameters.PageNumber, searchParameters.PageSize);

            return Ok(pagedResult);
        }
        [HttpPost("showBlockedMember")]
        public async Task<ActionResult<CPaging<pgBackMemberDTO>>> showBlockedMember([FromBody] SearchParametersDTO searchParameters)//秀被封鎖房客
        {
            IQueryable<Member> query = _context.Members
             .Where(m => m.RoleId == 3);

            var pagedResult = await getPagingAsync(query, searchParameters.PageNumber, searchParameters.PageSize);

            return Ok(pagedResult);
        }
        //關鍵字搜尋
        [HttpPost("showMemberContainsKeyword")]
        public async Task<ActionResult<CPaging<pgBackMemberDTO>>> showMemberContainsKeyword([FromBody] SearchParametersDTO searchParameters)
        {
            IQueryable<Member> query = _context.Members;

            if (string.IsNullOrEmpty(searchParameters.Keyword))
            {
                query = query.Where(m => m.RoleId == 1 || m.RoleId == 3); // 没有关键字时，获取所有房客
            }
            else
            {
                query = query.Where(m => (m.RoleId == 1 || m.RoleId == 3) &&
                                         (m.FirstName.Contains(searchParameters.Keyword) ||
                                          m.LastName.Contains(searchParameters.Keyword) ||
                                          m.Email.Contains(searchParameters.Keyword) ||
                                          m.Phone.Contains(searchParameters.Keyword))); // 有关键字时，根据关键字进行搜索
            }

            var pagedResult = await getPagingAsync(query, searchParameters.PageNumber, searchParameters.PageSize);

            return Ok(pagedResult);
        }
        //更改房客狀態
        [HttpPut("updateMemberRole")]
        public async Task<IActionResult> updateMemberRole([FromBody] UpdateMemberRoleDTO dto)
        {
            // 1. 查找指定的成员
            var member = await _context.Members.Include(m => m.Role).FirstOrDefaultAsync(m => m.MemberId == dto.MemberId);
            if (member == null)
            {
                return NotFound();
            }

            // 2. 验证新的角色是否存在
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.NewRoleName);
            if (role == null)
            {
                return BadRequest("Invalid role.");
            }

            // 3. 更新角色状态
            member.RoleId = role.RoleId; // 设置RoleId，而不是直接设置RoleName
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
