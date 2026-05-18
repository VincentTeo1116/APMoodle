using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class TeachingMaterialModel : PageModel
    {
        private readonly IModuleService _moduleService;
        private readonly IMaterialService _materialService;

        public TeachingMaterialModel(IModuleService moduleService, IMaterialService materialService)
        {
            _moduleService = moduleService;
            _materialService = materialService;
        }

        public Module? CurrentModule { get; set; }
        public List<Material> Materials { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            CurrentModule = await _moduleService.GetModuleByIdAsync(id);
            if (CurrentModule == null)
            {
                return NotFound();
            }

            Materials = await _materialService.GetMaterialsByModuleIdAsync(id);
            return Page();
        }

        // POST handler for regenerating invitation code
        public async Task<IActionResult> OnPostRegenerateCodeAsync([FromForm] int moduleId)
        {
            try
            {
                var module = await _moduleService.GetModuleByIdAsync(moduleId);
                if (module == null)
                {
                    return new JsonResult(new { success = false, message = "Module not found" });
                }

                var newCode = GenerateUniqueCode();

                while (await _moduleService.IsInvitationCodeExistsAsync(newCode))
                {
                    newCode = GenerateUniqueCode();
                }

                module.InvitationCode = newCode;
                await _moduleService.UpdateModuleAsync(module);

                return new JsonResult(new { success = true, code = newCode });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Add Microsoft.AspNetCore.Mvc.HttpPost attribute
        [HttpPost]
        public async Task<JsonResult> OnPostDeleteMaterialAsync([FromQuery] int materialId)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userRole = HttpContext.Session.GetString("UserRole");

                // Add debug logging
                Console.WriteLine($"Delete called for materialId: {materialId}");
                Console.WriteLine($"User: {userId}, Role: {userRole}");

                if (string.IsNullOrEmpty(userId) || userRole != "lecturer")
                {
                    return new JsonResult(new { success = false, message = "Unauthorized" });
                }

                var success = await _materialService.DeleteMaterialAsync(materialId);
                Console.WriteLine($"Delete success: {success}");

                return new JsonResult(new { success = success });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}