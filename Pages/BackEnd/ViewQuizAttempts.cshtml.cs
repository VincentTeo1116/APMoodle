using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.FrontEnd
{
    public class ViewQuizAttemptsModel : PageModel
    {
        private readonly ISessionService _sessionService;
        private readonly IQuizService _quizService;

        public ViewQuizAttemptsModel(ISessionService sessionService, IQuizService quizService)
        {
            _sessionService = sessionService;
            _quizService = quizService;
        }

        [BindProperty(SupportsGet = true)]
        public int QuizId { get; set; }

        public string? QuizTitle { get; set; }
        public List<Session> Sessions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            if (QuizId == 0)
                return RedirectToPage("/FrontEnd/ModuleOverview");

            var quiz = await _quizService.GetQuizByIdAsync(QuizId);
            if (quiz == null)
                return NotFound();
            QuizTitle = quiz.Title;

            int studentId = int.Parse(userId);
            if (userRole == "student")
            {
                Sessions = await _sessionService.GetSessionsByQuizAsync(QuizId, studentId);
            }
            else if (userRole == "lecturer" || userRole == "admin")
            {
                Sessions = await _sessionService.GetSessionsByQuizAsync(QuizId);
            }
            else
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            return Page();
        }
    }
}