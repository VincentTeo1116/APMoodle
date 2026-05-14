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