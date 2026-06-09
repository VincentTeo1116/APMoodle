using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class ViewQuizModel : PageModel
    {
        private readonly IQuizService _quizService;
        private readonly ISessionService _sessionService;

        public ViewQuizModel(IQuizService quizService, ISessionService sessionService)
        {
            _quizService = quizService;
            _sessionService = sessionService;
        }

        public Quiz? CurrentQuiz { get; set; }
        public List<Question> Questions { get; set; } = new();
        public int SessionId { get; set; }
        public string? Message { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // Only students attempt quizzes; lecturers/admins viewing the quiz read-only is a separate concern
            if (userRole != "student")
            {
                Message = "Only students can attempt quizzes.";
                return Page();
            }

            CurrentQuiz = await _quizService.GetQuizByIdAsync(id);
            if (CurrentQuiz == null)
            {
                return NotFound();
            }

            Questions = (CurrentQuiz.Questions ?? new List<Question>())
                .OrderBy(q => q.QuestionID)
                .ToList();

            if (Questions.Count == 0)
            {
                Message = "This quiz has no questions yet.";
                return Page();
            }

            // Create a new attempt session as soon as the student opens the quiz
            SessionId = await _sessionService.StartSessionAsync(int.Parse(userId), id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || userRole != "student")
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // The hidden field "SessionId" from the form ties answers to the session created in OnGet
            if (!int.TryParse(Request.Form["SessionId"], out var sessionId) || sessionId <= 0)
            {
                Message = "Quiz session is invalid. Please reload the quiz and try again.";
                await ReloadQuestionsForRender(id);
                return Page();
            }

            // Authorization: make sure this session belongs to the logged-in student
            if (!await _sessionService.IsSessionOwnedByStudentAsync(sessionId, int.Parse(userId)))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            CurrentQuiz = await _quizService.GetQuizByIdAsync(id);
            if (CurrentQuiz == null)
            {
                return NotFound();
            }

            var questions = (CurrentQuiz.Questions ?? new List<Question>())
                .OrderBy(q => q.QuestionID)
                .ToList();

            // Collect submissions: one Answer + TimeUsed field per question, named by QuestionID
            var submissions = new List<AnswerSubmission>();
            foreach (var question in questions)
            {
                var answer = Request.Form[$"Answer_{question.QuestionID}"].ToString();
                _ = int.TryParse(Request.Form[$"TimeUsed_{question.QuestionID}"], out var timeUsed);

                submissions.Add(new AnswerSubmission
                {
                    QuestionID = question.QuestionID,
                    Answer = answer,
                    TimeUsed = timeUsed
                });
            }

            var success = await _sessionService.SubmitSessionAsync(sessionId, submissions);
            if (!success)
            {
                Message = "Failed to submit the quiz. Please try again.";
                Questions = questions;
                SessionId = sessionId;
                return Page();
            }

            // celebrate=1 triggers the one-time congrats animation on the result page
            // (it won't fire when the same result is opened later from Quiz History).
            return RedirectToPage("/FrontEnd/QuizResult", new { id = sessionId, celebrate = 1 });
        }

        private async Task ReloadQuestionsForRender(int quizId)
        {
            CurrentQuiz = await _quizService.GetQuizByIdAsync(quizId);
            Questions = (CurrentQuiz?.Questions ?? new List<Question>())
                .OrderBy(q => q.QuestionID)
                .ToList();
        }
    }
}
