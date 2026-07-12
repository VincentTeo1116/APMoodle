using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class ModuleListModel : PageModel
    {
        private readonly IModuleService _moduleService;

        public ModuleListModel(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public List<ModuleViewModel> Modules { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public List<string> UniqueLecturers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var modules = await _moduleService.GetAllModulesAsync();
            TotalCount = modules.Count;
            ActiveCount = modules.Count(m => m.Status == "Active");

            Modules = modules.Select(m => new ModuleViewModel
            {
                Module = m,
                DisplayStatus = GetDisplayStatus(m)
            }).ToList();

            UniqueLecturers = modules
                .Where(m => m.Lecturer != null)
                .Select(m => m.Lecturer?.Name ?? "Unknown")
                .Distinct()
                .OrderBy(name => name)
                .ToList();
        }

        // View details (AJAX)
        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null) return NotFound();
            var displayStatus = GetDisplayStatus(module);

            return new JsonResult(new
            {
                id = module.ModuleID,
                code = module.ModuleCode,
                name = module.Name,
                description = module.Description ?? "No description",
                lecturer = module.Lecturer?.Name ?? "Unknown",
                status = module.Status,
                displayStatus = displayStatus, 
                invitationCode = module.InvitationCode ?? "N/A",
                startDate = module.StartDate.ToString("MMM dd, yyyy"),
                endDate = module.EndDate.ToString("MMM dd, yyyy")
            });
        }

        private string GetDisplayStatus(Module module)
        {
            if (module.Status != "Active")
                return module.Status;

            var today = DateTime.UtcNow.Date;
            var endDate = module.EndDate.ToUniversalTime().Date; // ensure UTC

            return endDate >= today ? "In Progress" : "Expired";
        }

        public class ModuleViewModel
        {
            public Module Module { get; set; } = null!;
            public string DisplayStatus { get; set; } = string.Empty;
        }

        // Delete 
        public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userId) || userRole != "admin")
                {
                    return new JsonResult(new { success = false, message = "Unauthorized" });
                }

                var result = await _moduleService.DeleteModuleAsync(id);
                if (result)
                    return new JsonResult(new { success = true, message = "Deleted successfully" });
                else
                    return new JsonResult(new { success = false, message = "Module not found" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}