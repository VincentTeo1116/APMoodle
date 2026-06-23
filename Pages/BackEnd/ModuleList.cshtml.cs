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

        public List<Module> Modules { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public List<string> UniqueLecturers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Modules = await _moduleService.GetAllModulesAsync();
            TotalCount = Modules.Count;
            ActiveCount = Modules.Count(m => m.Status == "Active");

            UniqueLecturers = Modules
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

            return new JsonResult(new
            {
                id = module.ModuleID,
                code = module.ModuleCode,
                name = module.Name,
                description = module.Description ?? "No description",
                lecturer = module.Lecturer?.Name ?? "Unknown",
                status = module.Status,
                invitationCode = module.InvitationCode ?? "N/A",
                startDate = module.StartDate.ToString("MMM dd, yyyy"),
                endDate = module.EndDate.ToString("MMM dd, yyyy")
            });
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