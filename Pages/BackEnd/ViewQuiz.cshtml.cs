using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;
using System.Text.Json;

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

            if (userRole != "student")
            {
                IsGuest = true;
                return Page();
            }

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

            if (userRole != "student")
            {
                var correctCount = submissions.Count(s =>
                    !string.IsNullOrEmpty(s.Answer) &&
                    questions.First(q => q.QuestionID == s.QuestionID).CorrectAnswer == s.Answer
                );
                var total = questions.Count;
                var score = (int)Math.Round((double)correctCount / total * 100);

                var guestResult = new GuestQuizResult
                {
                    QuizTitle = CurrentQuiz.Title,
                    QuizSubject = CurrentQuiz.Subject,
                    CorrectCount = correctCount,
                    TotalQuestions = total,
                    Score = score,
                    Reviews = questions.Select(q =>
                    {
                        var sub = submissions.FirstOrDefault(s => s.QuestionID == q.QuestionID);
                        var given = sub?.Answer ?? "-";
                        var isCorrect = !string.IsNullOrEmpty(given) && given == q.CorrectAnswer;
                        return new GuestQuestionReview
                        {
                            QuestionText = q.QuestionText,
                            Option1 = q.Option1,
                            Option2 = q.Option2,
                            Option3 = q.Option3,
                            Option4 = q.Option4,
                            CorrectAnswer = q.CorrectAnswer,
                            GivenAnswer = given,
                            IsCorrect = isCorrect,
                            TimeUsed = sub?.TimeUsed ?? 0
                        };
                    }).ToList()
                };

                TempData["GuestResultData"] = JsonSerializer.Serialize(guestResult);

                return RedirectToPage("/FrontEnd/QuizResult", new { id = 0, guest = 1 });
            }

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