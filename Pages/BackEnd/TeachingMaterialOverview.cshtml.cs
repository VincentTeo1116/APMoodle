using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class TeachingMaterialOverviewModel : PageModel
    {
        private readonly IModuleService _moduleService;
        private readonly IMaterialService _materialService;
        private readonly IQuizService _quizService;

        public TeachingMaterialOverviewModel(
            IModuleService moduleService,
            IMaterialService materialService,
            IQuizService quizService)
        {
            _moduleService = moduleService;
            _materialService = materialService;
            _quizService = quizService;
        }

        public Material? CurrentMaterial { get; set; }
        public Module? CurrentModule { get; set; }
        public List<Quiz> Quizzes { get; set; } = new();
        public string UserRole { get; set; } = "Guest";
        public string UserName { get; set; } = "User";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            UserRole = userRole ?? "Guest";
            UserName = HttpContext.Session.GetString("UserName") ?? "User";

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            CurrentMaterial = await _materialService.GetMaterialByIdAsync(id);
            if (CurrentMaterial == null)
            {
                return NotFound();
            }

            if (CurrentMaterial.ModuleID > 0)
            {
                CurrentModule = await _moduleService.GetModuleByIdAsync(CurrentMaterial.ModuleID);
            }

            Quizzes = await _quizService.GetQuizzesByMaterialIdAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteMaterialAsync([FromForm] int id)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userId) || userRole != "lecturer")
                {
                    TempData["ErrorMessage"] = "Unauthorized";
                    return RedirectToPage("/FrontEnd/Login");
                }

                // Get the material to find its module ID
                var material = await _materialService.GetMaterialByIdAsync(id);
                if (material == null)
                {
                    TempData["ErrorMessage"] = "Material not found";
                    return RedirectToPage("/FrontEnd/TeachingMaterial", new { id = 0 });
                }

                var moduleId = material.ModuleID;
                
                // Call service which sets Status = "Removed"
                var success = await _materialService.DeleteMaterialAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Material deleted successfully!";
                    return RedirectToPage("/FrontEnd/TeachingMaterial", new { id = moduleId });
                }

                TempData["ErrorMessage"] = "Delete failed";
                return RedirectToPage("/FrontEnd/TeachingMaterial", new { id = moduleId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/FrontEnd/TeachingMaterial", new { id = 0 });
            }
        }
    }
}