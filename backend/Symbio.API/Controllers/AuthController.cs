using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Symbio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public record RegisterRequest(string Email, string Password, string Role);

        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            // For local testing this endpoint accepts registration payloads.
            return Accepted(new { message = "Registration received" });
        }

        [HttpGet("verify-sme")]
        [Authorize(Policy = "RequireSmeRole")]
        public IActionResult VerifySme()
        {
            return Ok(new { message = "SME role verified" });
        }
    }
}
