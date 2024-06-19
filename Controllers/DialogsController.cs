using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;
using PrjFunNowWebApi.Models.DTO;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DialogsController : ControllerBase
    {
        private readonly FunNowContext _context;

        public DialogsController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/Dialogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dialog>>> GetDialogs()
        {
            return await _context.Dialogs.ToListAsync();
        }

        // GET: api/Dialogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Dialog>> GetDialog(int id)
        {
            var dialog = await _context.Dialogs.FindAsync(id);

            if (dialog == null)
            {
                return NotFound();
            }

            return dialog;
        }

        // GET: api/Dialogs/byMember/5
        [HttpGet("byMember/{memberId}")]
        public async Task<ActionResult<IEnumerable<DialogWithHotelInfoDto>>> GetDialogsByMemberId(int memberId)
        {
            var dialogs = await _context.Dialogs
                .Where(d => d.MemberId == memberId || d.CalltoMemberId == memberId)
                .ToListAsync();

            var result = new List<DialogWithHotelInfoDto>();

            foreach (var dialog in dialogs)
            {
                var relatedMemberId = dialog.MemberId == memberId ? dialog.CalltoMemberId : dialog.MemberId;
                var relatedMember = await _context.Members
                    .Include(m => m.Hotels)
                    .ThenInclude(h => h.HotelImages)
                    .FirstOrDefaultAsync(m => m.MemberId == relatedMemberId);

                var relatedHotel = relatedMember?.Hotels.FirstOrDefault();

                result.Add(new DialogWithHotelInfoDto
                {
                    DialogId = dialog.DialogId,
                    MemberId = dialog.MemberId,
                    Detail = dialog.Detail,
                    CalltoMemberId = dialog.CalltoMemberId,
                    CreateAt = dialog.CreateAt,
                    HotelName = relatedHotel?.HotelName,
                    HotelAddress = relatedHotel?.HotelAddress,
                    HotelImage = relatedHotel?.HotelImages.FirstOrDefault()?.HotelImage1,
                    MemberImage = relatedMember?.Image,
                    FirstName = relatedMember?.FirstName,
                    LastName = relatedMember?.LastName
                });
            }

            if (result.Count == 0)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // PUT: api/Dialogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDialog(int id, Dialog dialog)
        {
            if (id != dialog.DialogId)
            {
                return BadRequest();
            }

            _context.Entry(dialog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DialogExists(id))
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

        // POST: api/Dialogs
        [HttpPost]
        public async Task<ActionResult<Dialog>> PostDialog(Dialog dialog)
        {
            _context.Dialogs.Add(dialog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDialog", new { id = dialog.DialogId }, dialog);
        }

        // DELETE: api/Dialogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDialog(int id)
        {
            var dialog = await _context.Dialogs.FindAsync(id);
            if (dialog == null)
            {
                return NotFound();
            }

            _context.Dialogs.Remove(dialog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DialogExists(int id)
        {
            return _context.Dialogs.Any(e => e.DialogId == id);
        }
    }
}
