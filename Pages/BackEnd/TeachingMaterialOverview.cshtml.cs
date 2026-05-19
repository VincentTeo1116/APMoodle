using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;
using Microsoft.AspNetCore.Antiforgery;

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

        // Request DTO
        public class DeleteMaterialRequest
        {
            public int Id { get; set; }
        }

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

        public async Task<IActionResult> OnPostDeleteMaterialAsync(
            [FromBody] DeleteMaterialRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userId) || userRole != "lecturer")
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Unauthorized"
                    });
                }

                var material = await _materialService.GetMaterialByIdAsync(request.Id);

                if (material == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Material not found"
                    });
                }

                var moduleId = material.ModuleID;
                var success = await _materialService.DeleteMaterialAsync(request.Id);

                if (success)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        moduleId = moduleId
                    });
                }

                return new JsonResult(new
                {
                    success = false,
                    message = "Delete failed"
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}