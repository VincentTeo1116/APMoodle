using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class ModuleOverviewModel : PageModel
    {
        private readonly IModuleService _moduleService;

        public ModuleOverviewModel(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public List<ModuleOverviewItem> Modules { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            var modules = await _moduleService.GetModulesByUserRoleAsync(userRole ?? "Guest", int.Parse(userId));

            Modules = modules.Select(m => new ModuleOverviewItem
            {
                Module = m,
                DisplayStatus = GetDisplayStatus(m)
            }).ToList();

            TotalCount = Modules.Count;
            ActiveCount = Modules.Count(m => m.DisplayStatus == "Active");
            CompletedCount = Modules.Count(m => m.DisplayStatus == "Completed");

            return Page();
        }

        private string GetDisplayStatus(Module module)
        {
            if (module.Status != "Active")
                return module.Status;

            var today = DateTime.UtcNow.Date;
            var endDate = module.EndDate.ToUniversalTime().Date;
            return endDate >= today ? "Active" : "Completed";
        }
    }

    public class ModuleOverviewItem
    {
        public Module Module { get; set; } = null!;
        public string DisplayStatus { get; set; } = string.Empty;
    }
}