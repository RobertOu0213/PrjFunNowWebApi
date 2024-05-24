using Microsoft.AspNetCore.Mvc;
using PrjFunNowWebApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class testController : ControllerBase
    {
        private readonly FunNowContext _context;

        public testController(FunNowContext context)
        {          
            _context = context;
        }
        // GET: api/<testController>
        [HttpGet]
        public IEnumerable<Country> Get()
        {
            var x = _context.Countries.ToList();
            return x;
        }

        // GET api/<testController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<testController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<testController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<testController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
