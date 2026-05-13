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
            // Get session values
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Check if logged in
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // Get module details
            CurrentModule = await _moduleService.GetModuleByIdAsync(id);
            if (CurrentModule == null)
            {
                return NotFound();
            }

            // Get teaching materials for this module
            Materials = await _materialService.GetMaterialsByModuleIdAsync(id);

            return Page();
        }
    }
}