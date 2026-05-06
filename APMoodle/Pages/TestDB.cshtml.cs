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
        
        // Counts for display
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
                // Test if database can be connected
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    ErrorMessage = "Cannot connect to database. Please check your connection string and make sure Supabase is running.";
                    ConnectionSuccessful = false;
                    return;
                }

                ConnectionSuccessful = true;

                // Get counts from each table (use try-catch for tables that might not exist yet)
                try { StudentCount = await _context.Students.CountAsync(); } catch { StudentCount = 0; }
                try { LecturerCount = await _context.Lecturers.CountAsync(); } catch { LecturerCount = 0; }
                try { AdminCount = await _context.Admins.CountAsync(); } catch { AdminCount = 0; }
                try { ModuleCount = await _context.Modules.CountAsync(); } catch { ModuleCount = 0; }
                try { MaterialCount = await _context.Materials.CountAsync(); } catch { MaterialCount = 0; }
                try { QuizCount = await _context.Quizzes.CountAsync(); } catch { QuizCount = 0; }
                try { QuestionCount = await _context.Questions.CountAsync(); } catch { QuestionCount = 0; }
                try { AnnouncementCount = await _context.Announcements.CountAsync(); } catch { AnnouncementCount = 0; }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ConnectionSuccessful = false;
            }
        }
    }
}