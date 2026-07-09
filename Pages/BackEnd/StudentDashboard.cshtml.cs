using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class StudentDashboardModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ISessionService _sessionService;
        private readonly IStudentService _studentService;
        private readonly IAnnouncementService _announcementService;

        public StudentDashboardModel(
            IEnrollmentService enrollmentService,
            ISessionService sessionService,
            IStudentService studentService,
            IAnnouncementService announcementService)
        {
            _enrollmentService = enrollmentService;
            _sessionService = sessionService;
            _studentService = studentService;
            _announcementService = announcementService;
        }

        public string StudentName { get; set; } = "Student";
        public string StudentCode { get; set; } = string.Empty;

        // Top-line stats
        public int EnrolledModuleCount { get; set; }
        public int CompletedQuizCount { get; set; }
        public int AverageScore { get; set; } 
        public int BestScore { get; set; }

        // Listings
        public List<Module> EnrolledModules { get; set; } = new();
        public List<Session> RecentAttempts { get; set; } = new(); 
        public List<Session> TrendAttempts { get; set; } = new(); 
        public List<Announcement> Announcements { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // Only students have a student dashboard; other roles get bounced to ModuleOverview
            // (which itself redirects them based on their own role).
            if (userRole != "student")
            {
                return RedirectToPage("/FrontEnd/ModuleOverview");
            }

            var studentId = int.Parse(userId);

            // Display name + code from session if present, otherwise fall back to the DB
            StudentName = HttpContext.Session.GetString("UserName") ?? "Student";
            StudentCode = HttpContext.Session.GetString("UserCode") ?? string.Empty;
            if (string.IsNullOrEmpty(StudentCode))
            {
                var student = await _studentService.GetStudentByIdAsync(studentId);
                if (student != null)
                {
                    StudentName = student.Name;
                    StudentCode = student.StudentCode;
                }
            }

            // Enrolled modules (Active only — matches EnrollmentService.GetEnrolledModulesAsync)
            EnrolledModules = await _enrollmentService.GetEnrolledModulesAsync(studentId);
            EnrolledModuleCount = EnrolledModules.Count;

            // Completed quiz sessions (newest first)
            var sessions = await _sessionService.GetSessionsByStudentAsync(studentId);
            CompletedQuizCount = sessions.Count;

            if (sessions.Count > 0)
            {
                var scoredSessions = sessions.Where(s => s.TotalScore.HasValue).ToList();
                if (scoredSessions.Count > 0)
                {
                    AverageScore = (int)Math.Round(scoredSessions.Average(s => s.TotalScore!.Value));
                    BestScore = scoredSessions.Max(s => s.TotalScore!.Value);
                }
            }

            // Last 5 attempts for the activity feed
            RecentAttempts = sessions.Take(5).ToList();

            // Wider window (last 20 attempts) for the quiz-score trend chart so the
            // line spans more than just the few most recent attempts.
            TrendAttempts = sessions.Take(20).ToList();

            // Active announcements (newest first) — managed from the Announcement List page
            Announcements = await _announcementService.GetAllAnnouncementsAsync();

            return Page();
        }
    }
}
