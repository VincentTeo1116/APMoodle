using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    [AllowAnonymous]
    public class ViewAvailableQuizModel : PageModel
    {
        private readonly IQuizService _quizService;

        public ViewAvailableQuizModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        public List<Quiz> Quizzes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Quizzes = await _quizService.GetPublicQuizzesAsync();
            return Page();
        }
    }
}