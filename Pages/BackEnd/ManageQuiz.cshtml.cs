using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class ManageQuizModel : PageModel
    {
        private readonly IQuizService _quizService;

        public ManageQuizModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        public Quiz? CurrentQuiz { get; set; }
        public List<Question> Questions { get; set; } = new();
        public int MaterialId { get; set; }
        public string? Message { get; set; }
        public Module? CurrentModule { get; set; }
        public string? MaterialTitle  { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var auth = await GuardAndLoadAsync(id);
            if (auth != null) return auth;
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var auth = await GuardAndLoadAsync(id);
            if (auth != null) return auth;

            var materialId = CurrentQuiz!.MaterialID;
            var title = CurrentQuiz.Title;

            var success = await _quizService.DeleteQuizAsync(id);
            if (!success)
            {
                Message = "Failed to delete this quiz. Please try again.";
                return Page();
            }

            TempData["QuizDeleted"] = $"Quiz '{title}' was deleted.";
            return RedirectToPage("/FrontEnd/TeachingMaterialOverview", new { id = materialId });
        }

        private async Task<IActionResult?> GuardAndLoadAsync(int quizId)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            if (userRole != "lecturer" && userRole != "admin")
                return StatusCode(StatusCodes.Status403Forbidden);

            CurrentQuiz = await _quizService.GetQuizByIdAsync(quizId);
            if (CurrentQuiz == null)
                return NotFound();

            CurrentModule = CurrentQuiz.Material?.Module;
            MaterialTitle = CurrentQuiz.Material?.Title;

            if (userRole == "admin")
            {
                Questions = (CurrentQuiz.Questions ?? new List<Question>())
                    .Where(q => q.Status == "Active")
                    .OrderBy(q => q.QuestionID)
                    .ToList();
                MaterialId = CurrentQuiz.MaterialID;
                return null;
            }

            var ownerId = await _quizService.GetLecturerIdForQuizAsync(quizId);
            if (ownerId == null || ownerId.ToString() != userId)
                return StatusCode(StatusCodes.Status403Forbidden);

            Questions = (CurrentQuiz.Questions ?? new List<Question>())
                .Where(q => q.Status == "Active")
                .OrderBy(q => q.QuestionID)
                .ToList();
            MaterialId = CurrentQuiz.MaterialID;
            return null;
        }
    }
}
