using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class JoinModuleModel : PageModel
    {
        private readonly IModuleService _moduleService;
        private readonly IEnrollmentService _enrollmentService;

        public JoinModuleModel(IModuleService moduleService, IEnrollmentService enrollmentService)
        {
            _moduleService = moduleService;
            _enrollmentService = enrollmentService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Invitation code is required")]
            [Display(Name = "Invitation Code")]
            public string InvitationCode { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || userRole != "student")
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || userRole != "student")
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var code = Input.InvitationCode.Trim().ToUpper();
            var module = await _moduleService.GetModuleByInvitationCodeAsync(code);

            if (module == null)
            {
                Message = "Invalid invitation code. Please check with your lecturer.";
                IsSuccess = false;
                return Page();
            }

            var studentId = int.Parse(userId);

            if (await _enrollmentService.IsEnrolledAsync(studentId, module.ModuleID))
            {
                Message = $"You are already enrolled in '{module.Name}'.";
                IsSuccess = false;
                return Page();
            }

            var success = await _enrollmentService.EnrollStudentAsync(studentId, module.ModuleID);

            if (success)
            {
                TempData["JoinSuccess"] = $"Successfully joined '{module.Name}' ({module.ModuleCode}).";
                return RedirectToPage("/FrontEnd/ModuleOverview");
            }

            Message = "Failed to join the module. Please try again later.";
            IsSuccess = false;
            return Page();
        }
    }
}
