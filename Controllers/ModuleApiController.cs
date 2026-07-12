using Microsoft.AspNetCore.Mvc;
using APMoodle.Services.Interfaces;

namespace APMoodle.Controllers
{
    [ApiController]
    [Route("api/module")]
    public class ModuleApiController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModuleApiController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpPost("{id}/regenerate-code")]
        public async Task<IActionResult> RegenerateCode(int id)
        {
            // This endpoint had NO auth — anyone could rotate any module's
            // invitation code (invalidating the one the lecturer shared and
            // handing themselves a working join code). Require a lecturer who
            // owns the module, or an admin.
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || (role != "lecturer" && role != "admin"))
            {
                return Unauthorized(new { success = false, message = "Not authorized." });
            }

            if (role == "lecturer")
            {
                if (!int.TryParse(userId, out var lecturerId) ||
                    !await _moduleService.IsModuleOwnerAsync(id, lecturerId))
                {
                    return StatusCode(403, new { success = false, message = "You do not own this module." });
                }
            }

            try
            {
                var newCode = await _moduleService.RegenerateInvitationCodeAsync(id);
                return Ok(new { success = true, code = newCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}