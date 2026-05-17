using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class QuizResultModel : PageModel
    {
        private readonly ISessionService _sessionService;

        public QuizResultModel(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public Session? CurrentSession { get; set; }
        public Quiz? CurrentQuiz { get; set; }
        public List<QuestionReview> Reviews { get; set; } = new();
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalTimeUsed { get; set; } // seconds

        public class QuestionReview
        {
            public Question Question { get; set; } = null!;
            public string GivenAnswer { get; set; } = "-";
            public bool IsCorrect { get; set; }
            public int TimeUsed { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // Student-only flow: only the owner of the session can view their result
            if (userRole != "student")
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (!await _sessionService.IsSessionOwnedByStudentAsync(id, int.Parse(userId)))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            CurrentSession = await _sessionService.GetSessionWithDetailsAsync(id);
            if (CurrentSession == null || !CurrentSession.IsCompleted)
            {
                return NotFound();
            }

            CurrentQuiz = CurrentSession.Quiz;
            var questions = (CurrentQuiz?.Questions ?? new List<Question>())
                .OrderBy(q => q.QuestionID)
                .ToList();
            var resultsByQuestionId = (CurrentSession.Results ?? new List<Result>())
                .ToDictionary(r => r.QuestionID, r => r);

            foreach (var q in questions)
            {
                resultsByQuestionId.TryGetValue(q.QuestionID, out var result);
                Reviews.Add(new QuestionReview
                {
                    Question = q,
                    GivenAnswer = result?.Answer ?? "-",
                    IsCorrect = result?.IsCorrect ?? false,
                    TimeUsed = result?.TimeUsed ?? 0
                });
            }

            CorrectCount = Reviews.Count(r => r.IsCorrect);
            TotalQuestions = Reviews.Count;
            TotalTimeUsed = Reviews.Sum(r => r.TimeUsed);

            return Page();
        }
    }
}
