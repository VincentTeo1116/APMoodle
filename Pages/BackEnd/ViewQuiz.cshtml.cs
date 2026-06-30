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
        public bool IsGuest { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole") ?? "Guest";

            CurrentQuiz = await _quizService.GetQuizByIdAsync(id);
            if (CurrentQuiz == null) return NotFound();

            // If quiz is not public, only students can access
            if (!CurrentQuiz.IsPublic && userRole != "student")
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            Questions = (CurrentQuiz.Questions ?? new List<Question>())
                .OrderBy(q => q.QuestionID)
                .ToList();

            if (Questions.Count == 0)
            {
                Message = "This quiz has no questions yet.";
                return Page();
            }

            // If user is not a student (i.e., guest), show the quiz without creating a session
            if (userRole != "student")
            {
                IsGuest = true;
                return Page();
            }

            // Student flow: create session
            SessionId = await _sessionService.StartSessionAsync(int.Parse(userId), id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole") ?? "Guest";

            // Reload quiz
            CurrentQuiz = await _quizService.GetQuizByIdAsync(id);
            if (CurrentQuiz == null) return NotFound();

            var questions = (CurrentQuiz.Questions ?? new List<Question>())
                .OrderBy(q => q.QuestionID)
                .ToList();

            // Collect submitted answers
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

            // Handle guest submission – no session, just compute score and return
            if (userRole != "student")
            {
                // Compute score and correct answers
                var correctCount = submissions.Count(s =>
                    !string.IsNullOrEmpty(s.Answer) &&
                    questions.First(q => q.QuestionID == s.QuestionID).CorrectAnswer == s.Answer
                );
                var total = questions.Count;
                var score = (int)Math.Round((double)correctCount / total * 100);

                // Store in TempData to display on the same page
                TempData["GuestResult"] = $"{correctCount} out of {total} correct ({score}%)";
                TempData["GuestDetails"] = submissions; // optional

                // Reload questions for display
                Questions = questions;
                IsGuest = true;
                Message = $"You scored {correctCount}/{total} ({score}%).";
                return Page();
            }

            // Student flow: existing logic
            if (!int.TryParse(Request.Form["SessionId"], out var sessionId) || sessionId <= 0)
            {
                Message = "Quiz session is invalid. Please reload the quiz and try again.";
                Questions = questions;
                return Page();
            }

            if (!await _sessionService.IsSessionOwnedByStudentAsync(sessionId, int.Parse(userId)))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var success = await _sessionService.SubmitSessionAsync(sessionId, submissions);
            if (!success)
            {
                Message = "Failed to submit the quiz. Please try again.";
                Questions = questions;
                SessionId = sessionId;
                return Page();
            }

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
