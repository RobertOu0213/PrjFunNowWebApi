using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrjFunNowWebApi.Models;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageCategoryController : ControllerBase
    {
        private readonly FunNowContext _context;

        public ImageCategoryController(FunNowContext context)
        {
            _context = context;
        }

        // GET: api/ImageCategory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageCategory>>> GetImageCategories()
        {
            return await _context.ImageCategories.ToListAsync();
        }

        // GET: api/ImageCategory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageCategory>> GetImageCategory(int id)
        {
            var imageCategory = await _context.ImageCategories.FindAsync(id);

            if (imageCategory == null)
            {
                return NotFound();
            }

            return imageCategory;
        }

        
    }
}
