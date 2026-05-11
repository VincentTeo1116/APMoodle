using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;
using APMoodle.Pages.BackEnd;

namespace APMoodle.Pages.BackEnd
{
    public class ModuleOverviewModel : PageModel
    {
        private readonly IModuleService _moduleService;

        public ModuleOverviewModel(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public List<Module> Modules { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current logged-in user from session
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            // If not logged in, redirect to login page
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // Load modules using the service
            Modules = await _moduleService.GetModulesByUserRoleAsync(userRole ?? "Guest", int.Parse(userId));

            return Page();
        }
    }
}