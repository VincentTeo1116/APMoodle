using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class QuizHistoryModel : PageModel
    {
        private readonly ISessionService _sessionService;

        public QuizHistoryModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public List<Session> Sessions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // Only students have a personal quiz history
            if (userRole != "student")
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            Sessions = await _sessionService.GetSessionsByStudentAsync(int.Parse(userId));
            return Page();
        }
    }
}
