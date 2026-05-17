using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class EditQuizModel : PageModel
    {
        private readonly IQuizService _quizService;

        public EditQuizModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [BindProperty]
        public int QuizId { get; set; }

        [BindProperty]
        public int MaterialId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Quiz title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(50)]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Theme is required")]
        [StringLength(50)]
        public string Theme { get; set; } = string.Empty;

        [BindProperty]
        public List<QuestionInput> Questions { get; set; } = new();

        // CSV of QuestionIDs that the lecturer removed during editing.
        // Tracked across postbacks via a hidden field so the final Save knows what to delete.
        // Must be nullable: with nullable reference types enabled, a non-nullable string
        // property is treated as Required by the model binder, and "" (no deletions) would
        // wrongly fail ModelState validation.
        [BindProperty]
        public string? DeletedQuestionIDs { get; set; }

        public string? Message { get; set; }

        public class QuestionInput
        {
            // 0 = newly added during this edit session (no DB row yet)
            public int QuestionID { get; set; }

            [Required(ErrorMessage = "Question text is required")]
            [StringLength(500)]
            public string QuestionText { get; set; } = string.Empty;

            [Required(ErrorMessage = "Option A is required")]
            [StringLength(200)]
            public string Option1 { get; set; } = string.Empty;

            [Required(ErrorMessage = "Option B is required")]
            [StringLength(200)]
            public string Option2 { get; set; } = string.Empty;

            [Required(ErrorMessage = "Option C is required")]
            [StringLength(200)]
            public string Option3 { get; set; } = string.Empty;

            [Required(ErrorMessage = "Option D is required")]
            [StringLength(200)]
            public string Option4 { get; set; } = string.Empty;

            [Required(ErrorMessage = "Pick the correct answer")]
            [RegularExpression("^[A-D]$", ErrorMessage = "Correct answer must be A, B, C, or D")]
            public string CorrectAnswer { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var auth = await GuardAsync(id);
            if (auth != null) return auth;

            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null) return NotFound();

            QuizId = quiz.QuizID;
            MaterialId = quiz.MaterialID;
            Title = quiz.Title;
            Subject = quiz.Subject;
            Theme = quiz.Theme;
            Questions = (quiz.Questions ?? new List<Question>())
                .OrderBy(q => q.QuestionID)
                .Select(q => new QuestionInput
                {
                    QuestionID = q.QuestionID,
                    QuestionText = q.QuestionText,
                    Option1 = q.Option1,
                    Option2 = q.Option2,
                    Option3 = q.Option3,
                    Option4 = q.Option4,
                    CorrectAnswer = q.CorrectAnswer
                })
                .ToList();

            // Always edit with at least one question slot present
            if (Questions.Count == 0)
                Questions.Add(new QuestionInput());

            return Page();
        }

        public async Task<IActionResult> OnPostAddQuestionAsync()
        {
            var auth = await GuardAsync(QuizId);
            if (auth != null) return auth;

            ModelState.Clear();
            Questions.Add(new QuestionInput());
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveQuestionAsync(int index)
        {
            var auth = await GuardAsync(QuizId);
            if (auth != null) return auth;

            ModelState.Clear();
            if (index >= 0 && index < Questions.Count)
            {
                var removed = Questions[index];
                // If it had a real ID, remember it for the eventual Save so we can delete the DB row
                if (removed.QuestionID > 0)
                {
                    var deletedIds = ParseDeletedIds();
                    deletedIds.Add(removed.QuestionID);
                    DeletedQuestionIDs = string.Join(",", deletedIds);
                }
                Questions.RemoveAt(index);
            }

            // Don't let the form go below one slot
            if (Questions.Count == 0)
                Questions.Add(new QuestionInput());

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var auth = await GuardAsync(QuizId);
            if (auth != null) return auth;

            if (!ModelState.IsValid)
            {
                Message = "Please fix the highlighted fields and try again.";
                return Page();
            }

            if (Questions.Count == 0)
            {
                Message = "A quiz must have at least one question.";
                return Page();
            }

            // 1) Update Quiz header
            var quizUpdate = new Quiz
            {
                QuizID = QuizId,
                Title = Title.Trim(),
                Subject = Subject.Trim(),
                Theme = Theme.Trim()
            };
            var headerOk = await _quizService.UpdateQuizAsync(quizUpdate);
            if (!headerOk)
            {
                Message = "Failed to update the quiz header. Please try again.";
                return Page();
            }

            // 2) Delete removed questions (cascade to Results — documented in service)
            foreach (var deletedId in ParseDeletedIds())
            {
                await _quizService.DeleteQuestionAsync(deletedId);
            }

            // 3) Insert/update questions
            foreach (var input in Questions)
            {
                var domainQuestion = new Question
                {
                    QuestionID = input.QuestionID,
                    QuizID = QuizId,
                    QuestionText = input.QuestionText.Trim(),
                    Option1 = input.Option1.Trim(),
                    Option2 = input.Option2.Trim(),
                    Option3 = input.Option3.Trim(),
                    Option4 = input.Option4.Trim(),
                    CorrectAnswer = input.CorrectAnswer.Trim().ToUpper()
                };

                if (input.QuestionID == 0)
                {
                    await _quizService.AddQuestionAsync(domainQuestion);
                }
                else
                {
                    await _quizService.UpdateQuestionAsync(domainQuestion);
                }
            }

            TempData["QuizUpdated"] = $"Quiz '{Title}' was updated.";
            return RedirectToPage("/FrontEnd/ManageQuiz", new { id = QuizId });
        }

        // -------- helpers --------

        private List<int> ParseDeletedIds()
        {
            if (string.IsNullOrWhiteSpace(DeletedQuestionIDs))
                return new List<int>();

            return DeletedQuestionIDs
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                .Where(n => n > 0)
                .Distinct()
                .ToList();
        }

        private async Task<IActionResult?> GuardAsync(int quizId)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            if (userRole != "lecturer")
                return StatusCode(StatusCodes.Status403Forbidden);

            var ownerId = await _quizService.GetLecturerIdForQuizAsync(quizId);
            if (ownerId == null)
                return NotFound();

            if (ownerId.ToString() != userId)
                return StatusCode(StatusCodes.Status403Forbidden);

            return null;
        }
    }
}
