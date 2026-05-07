using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;

namespace APMoodle.Pages
{
    public class TestDBModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TestDBModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string? ErrorMessage { get; set; }
        public bool ConnectionSuccessful { get; set; }
        
        public int StudentCount { get; set; }
        public int LecturerCount { get; set; }
        public int AdminCount { get; set; }
        public int ModuleCount { get; set; }
        public int MaterialCount { get; set; }
        public int QuizCount { get; set; }
        public int QuestionCount { get; set; }
        public int AnnouncementCount { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    ErrorMessage = "Cannot connect to database.";
                    ConnectionSuccessful = false;
                    return;
                }

                ConnectionSuccessful = true;

                // Safe null checks with null-forgiving operator
                StudentCount = await _context.Students!.CountAsync();
                LecturerCount = await _context.Lecturers!.CountAsync();
                AdminCount = await _context.Admins!.CountAsync();
                ModuleCount = await _context.Modules!.CountAsync();
                MaterialCount = await _context.Materials!.CountAsync();
                QuizCount = await _context.Quizzes!.CountAsync();
                QuestionCount = await _context.Questions!.CountAsync();
                AnnouncementCount = await _context.Announcements!.CountAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ConnectionSuccessful = false;
            }
        }
    }
}