using Microsoft.AspNetCore.Mvc;
using PrjFunNowWebApi.Services;
using System;

namespace PrjFunNowWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILoggerService _logger;

        public ChatController(ILoggerService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("Test endpoint called.");
            return Ok("Test successful.");
        }

        // Other API endpoints for chat functionality
    }
}
