using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;
using System.Text.Json;

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
        public int MaterialId { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalTimeUsed { get; set; }
        public bool IsGuest { get; set; }

        public class QuestionReview
        {
            public Question Question { get; set; } = null!;
            public string GivenAnswer { get; set; } = "-";
            public bool IsCorrect { get; set; }
            public int TimeUsed { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id, [FromQuery] int guest = 0)
        {
            if (id == 0 && guest == 1)
            {
                return await LoadGuestResultAsync();
            }

            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            if (userRole == "student")
            {
                if (!await _sessionService.IsSessionOwnedByStudentAsync(id, int.Parse(userId)))
                    return StatusCode(StatusCodes.Status403Forbidden);
            }
            else if (userRole != "lecturer" && userRole != "admin")
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            CurrentSession = await _sessionService.GetSessionWithDetailsAsync(id);
            if (CurrentSession == null || !CurrentSession.IsCompleted)
                return NotFound();

            CurrentQuiz = CurrentSession.Quiz;
            MaterialId = CurrentQuiz?.MaterialID ?? 0;
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

        private async Task<IActionResult> LoadGuestResultAsync()
        {
            var json = TempData["GuestResultData"] as string;
            if (string.IsNullOrEmpty(json))
                return RedirectToPage("/FrontEnd/ViewAvailableQuiz");

            var guestData = JsonSerializer.Deserialize<GuestQuizResult>(json);
            if (guestData == null)
                return RedirectToPage("/FrontEnd/ViewAvailableQuiz");

            IsGuest = true;

            CurrentSession = new Session
            {
                SessionID = 0,
                Timestamp = DateTime.UtcNow,
                TotalScore = guestData.Score,
                Student = new Student { Name = "Guest", StudentCode = "N/A" }
            };

            CurrentQuiz = new Quiz
            {
                Title = guestData.QuizTitle,
                Subject = guestData.QuizSubject
            };

            Reviews = guestData.Reviews.Select(r => new QuestionReview
            {
                Question = new Question
                {
                    QuestionText = r.QuestionText,
                    Option1 = r.Option1,
                    Option2 = r.Option2,
                    Option3 = r.Option3,
                    Option4 = r.Option4,
                    CorrectAnswer = r.CorrectAnswer
                },
                GivenAnswer = r.GivenAnswer,
                IsCorrect = r.IsCorrect,
                TimeUsed = r.TimeUsed
            }).ToList();

            CorrectCount = guestData.CorrectCount;
            TotalQuestions = guestData.TotalQuestions;
            TotalTimeUsed = Reviews.Sum(r => r.TimeUsed);

            return Page();
        }
    }
}