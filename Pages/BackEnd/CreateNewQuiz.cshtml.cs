using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class CreateNewQuizModel : PageModel
    {
        private readonly IMaterialService _materialService;
        private readonly IModuleService _moduleService;
        private readonly IQuizService _quizService;

        public CreateNewQuizModel(
            IMaterialService materialService,
            IModuleService moduleService,
            IQuizService quizService)
        {
            _materialService = materialService;
            _moduleService = moduleService;
            _quizService = quizService;
        }

        // Hidden so the form posts back the same context every time
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
        [BindProperty]
        public bool IsPublic { get; set; }

        public Material? CurrentMaterial { get; set; }
        public string? Message { get; set; }

        public class QuestionInput
        {
            // 0 = new question (Create flow only ever has new ones)
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

        public async Task<IActionResult> OnGetAsync(int materialId)
        {
            var auth = await GuardAndLoadMaterial(materialId);
            if (auth != null) return auth;

            MaterialId = materialId;
            // Start with one empty question slot — lecturer can hit "Add Question" for more
            Questions = new List<QuestionInput> { new QuestionInput() };
            return Page();
        }

        public async Task<IActionResult> OnPostAddQuestionAsync()
        {
            var auth = await GuardAndLoadMaterial(MaterialId);
            if (auth != null) return auth;

            ModelState.Clear(); // user just clicked +Add; don't surface validation errors yet
            Questions.Add(new QuestionInput());
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveQuestionAsync(int index)
        {
            var auth = await GuardAndLoadMaterial(MaterialId);
            if (auth != null) return auth;

            ModelState.Clear();
            if (index >= 0 && index < Questions.Count && Questions.Count > 1)
            {
                Questions.RemoveAt(index);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var auth = await GuardAndLoadMaterial(MaterialId);
            if (auth != null) return auth;

            if (!ModelState.IsValid)
            {
                Message = "Please fix the highlighted fields and try again.";
                return Page();
            }

            if (Questions == null || Questions.Count == 0)
            {
                Message = "A quiz needs at least one question.";
                return Page();
            }

            // Build domain objects from the input model
            var quiz = new Quiz
            {
                Title = Title.Trim(),
                Subject = Subject.Trim(),
                Theme = Theme.Trim(),
                MaterialID = MaterialId,
                IsPublic = IsPublic
            };

            var domainQuestions = Questions.Select(q => new Question
            {
                QuestionText = q.QuestionText.Trim(),
                Option1 = q.Option1.Trim(),
                Option2 = q.Option2.Trim(),
                Option3 = q.Option3.Trim(),
                Option4 = q.Option4.Trim(),
                CorrectAnswer = q.CorrectAnswer.Trim().ToUpper()
            }).ToList();

            var newQuizId = await _quizService.CreateQuizWithQuestionsAsync(quiz, domainQuestions);
            if (newQuizId <= 0)
            {
                Message = "Failed to save the quiz. Please try again.";
                return Page();
            }

            TempData["QuizCreated"] = $"Quiz '{quiz.Title}' created with {domainQuestions.Count} question(s).";
            return RedirectToPage("/FrontEnd/TeachingMaterialOverview", new { id = MaterialId });
        }

        // -------- helpers --------

        // Centralised session + ownership guard. Returns null when the lecturer may proceed,
        // otherwise returns the IActionResult the page should respond with (redirect / 403 / 404).
        private async Task<IActionResult?> GuardAndLoadMaterial(int materialId)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            if (userRole != "lecturer")
                return StatusCode(StatusCodes.Status403Forbidden);

            CurrentMaterial = await _materialService.GetMaterialByIdAsync(materialId);
            if (CurrentMaterial == null)
                return NotFound();

            var module = await _moduleService.GetModuleByIdAsync(CurrentMaterial.ModuleID);
            if (module == null)
                return NotFound();

            if (module.LecturerID.ToString() != userId)
                return StatusCode(StatusCodes.Status403Forbidden);

            return null;
        }
    }
}
