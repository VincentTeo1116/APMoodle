using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class LecturerDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILecturerService _lecturerService;
        private readonly IAnnouncementService _announcementService;

        public LecturerDashboardModel(
            ApplicationDbContext context,
            ILecturerService lecturerService,
            IAnnouncementService announcementService)
        {
            _context = context;
            _lecturerService = lecturerService;
            _announcementService = announcementService;
        }

        public string LecturerName { get; set; } = "Lecturer";
        public string LecturerCode { get; set; } = string.Empty;

        public int EnrolledModuleCount { get; set; }
        public int StudentsJoinedCount { get; set; } 
        public int QuizzesCreatedCount { get; set; } 

        public List<LecturerModuleRow> Modules { get; set; } = new();
        public List<QuizAverageRow> TopQuizzes { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            if (userRole != "lecturer")
            {
                return RedirectToPage("/FrontEnd/ModuleOverview");
            }

            var lecturerId = int.Parse(userId);

            LecturerName = HttpContext.Session.GetString("UserName") ?? "Lecturer";
            LecturerCode = HttpContext.Session.GetString("UserCode") ?? string.Empty;
            if (string.IsNullOrEmpty(LecturerCode))
            {
                var lecturer = await _lecturerService.GetLecturerByIdAsync(lecturerId);
                if (lecturer != null)
                {
                    LecturerName = lecturer.Name;
                    LecturerCode = lecturer.LecturerCode;
                }
            }

            var modules = await _context.Modules
                .Where(m => m.LecturerID == lecturerId && m.Status == "Active")
                .OrderBy(m => m.Name)
                .ToListAsync();

            var moduleIds = modules.Select(m => m.ModuleID).ToList();
            EnrolledModuleCount = modules.Count;

            var enrollmentCountByModule = new Dictionary<int, int>();
            if (_context.Enrollments != null && moduleIds.Count > 0)
            {
                var activeEnrollments = await _context.Enrollments
                    .Where(e => moduleIds.Contains(e.ModuleID) && e.Status == "Active")
                    .Select(e => new { e.ModuleID, e.StudentID })
                    .ToListAsync();

                StudentsJoinedCount = activeEnrollments
                    .Select(e => e.StudentID)
                    .Distinct()
                    .Count();

                enrollmentCountByModule = activeEnrollments
                    .GroupBy(e => e.ModuleID)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            var lecturerQuizzes = moduleIds.Count == 0
                ? new List<QuizLookup>()
                : await _context.Quizzes
                    .Where(q => q.Material != null && moduleIds.Contains(q.Material.ModuleID)
                                && q.Status == "Active")
                    .Select(q => new QuizLookup
                    {
                        QuizID = q.QuizID,
                        Title = q.Title,
                        Subject = q.Subject,
                        ModuleID = q.Material!.ModuleID,
                        ModuleName = q.Material.Module != null ? q.Material.Module.Name : ""
                    })
                    .ToListAsync();

            QuizzesCreatedCount = lecturerQuizzes.Count;

            var quizCountByModule = lecturerQuizzes
                .GroupBy(q => q.ModuleID)
                .ToDictionary(g => g.Key, g => g.Count());

            Modules = modules.Select(m => new LecturerModuleRow
            {
                ModuleID = m.ModuleID,
                Name = m.Name,
                ModuleCode = m.ModuleCode,
                StudentCount = enrollmentCountByModule.TryGetValue(m.ModuleID, out var sc) ? sc : 0,
                QuizCount = quizCountByModule.TryGetValue(m.ModuleID, out var qc) ? qc : 0
            }).ToList();

            var quizIds = lecturerQuizzes.Select(q => q.QuizID).ToList();
            if (quizIds.Count > 0)
            {
                var averages = await _context.Sessions
                    .Where(s => s.IsCompleted && s.TotalScore != null && quizIds.Contains(s.QuizID))
                    .GroupBy(s => s.QuizID)
                    .Select(g => new
                    {
                        QuizID = g.Key,
                        Average = g.Average(x => (double)x.TotalScore!.Value),
                        Attempts = g.Count()
                    })
                    .ToListAsync();

                var quizById = lecturerQuizzes.ToDictionary(q => q.QuizID);

                TopQuizzes = averages
                    .Where(a => quizById.ContainsKey(a.QuizID))
                    .Select(a => new QuizAverageRow
                    {
                        Title = quizById[a.QuizID].Title,
                        Subject = quizById[a.QuizID].Subject,
                        ModuleName = quizById[a.QuizID].ModuleName,
                        AverageScore = a.Average,
                        AttemptCount = a.Attempts
                    })
                    .OrderByDescending(q => q.AverageScore)
                    .ThenByDescending(q => q.AttemptCount)
                    .Take(5)
                    .ToList();
            }

            Announcements = await _announcementService.GetAllAnnouncementsAsync();

            return Page();
        }

        public class LecturerModuleRow
        {
            public int ModuleID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ModuleCode { get; set; } = string.Empty;
            public int StudentCount { get; set; }
            public int QuizCount { get; set; }
        }

        public class QuizAverageRow
        {
            public string Title { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string ModuleName { get; set; } = string.Empty;
            public double AverageScore { get; set; }
            public int AttemptCount { get; set; }
        }

        private class QuizLookup
        {
            public int QuizID { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public int ModuleID { get; set; }
            public string ModuleName { get; set; } = string.Empty;
        }
    }
}
