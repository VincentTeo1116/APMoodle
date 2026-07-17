using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;
using APMoodle.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace APMoodle.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var dashboard = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(s => s.Status == "Active"),
                TotalLecturers = await _context.Lecturers.CountAsync(l => l.Status == "Active"),
                TotalModules = await _context.Modules.CountAsync(m => m.Status == "Active"),
                TotalEnrollments = await _context.Enrollments.CountAsync(e => e.Status == "Active"),
                ModuleCategories = await GetModuleCategoriesAsync(),
                TopStudents = await GetTopStudentsAsync(5),
                RecentActivities = await GetRecentActivitiesAsync(5),
                EnrollmentTrend = await GetEnrollmentTrendAsync()
            };
            return dashboard;
        }

        public async Task<List<ModuleCategoryData>> GetModuleCategoriesAsync()
        {
            var modulesWithEnrollments = await _context.Modules
                .Where(m => m.Status == "Active")
                .Select(m => new
                {
                    m.ModuleID,
                    m.ModuleCode,
                    EnrollmentCount = _context.Enrollments.Count(e => e.ModuleID == m.ModuleID && e.Status == "Active")
                })
                .ToListAsync();

            var grouped = modulesWithEnrollments
                .GroupBy(m => GetModuleCategory(m.ModuleCode))
                .Select(g => new ModuleCategoryData
                {
                    CategoryName = g.Key,
                    ModuleCount = g.Sum(x => x.EnrollmentCount)  
                })
                .OrderByDescending(x => x.ModuleCount)
                .ToList();

            // Calculate percentages
            int totalEnrollments = grouped.Sum(x => x.ModuleCount);
            if (totalEnrollments > 0)
            {
                foreach (var item in grouped)
                {
                    item.Percentage = Math.Round((double)item.ModuleCount / totalEnrollments * 100, 1);
                }
            }

            return grouped;
        }

        private string GetModuleCategory(string moduleCode)
        {
            if (string.IsNullOrWhiteSpace(moduleCode))
                return "Other";

            var letters = new string(moduleCode.TakeWhile(char.IsLetter).ToArray());
            if (letters.Length >= 2)
                return letters[..2].ToUpperInvariant();
            
            if (moduleCode.Length >= 2 && char.IsLetter(moduleCode[0]) && char.IsLetter(moduleCode[1]))
                return moduleCode[..2].ToUpperInvariant();

            return "Other";
        }

        public async Task<List<TopStudentData>> GetTopStudentsAsync(int topCount = 5)
        {
            var students = await _context.Students
                .Where(s => s.Status == "Active")
                .Select(s => new
                {
                    s.StudentID,
                    s.Name,
                    s.StudentCode,
                    s.ProfilePic,
                    Sessions = _context.Sessions
                        .Where(se => se.StudentID == s.StudentID && se.IsCompleted)
                        .Select(se => new
                        {
                            se.SessionID,
                            CorrectCount = _context.Results
                                .Where(r => r.SessionID == se.SessionID && r.IsCorrect)
                                .Count(),
                            TotalQuestions = _context.Results
                                .Where(r => r.SessionID == se.SessionID)
                                .Count()
                        })
                        .ToList()
                })
                .ToListAsync();

            var topStudents = students
                .Select(s => new TopStudentData
                {
                    StudentId = s.StudentID,
                    StudentName = s.Name,
                    StudentCode = s.StudentCode,
                    AvatarUrl = s.ProfilePic,
                    AverageScore = ComputeAverageScore(s.Sessions)
                })
                .Where(s => s.AverageScore > 0)
                .OrderByDescending(s => s.AverageScore)
                .Take(topCount)
                .ToList();

            return topStudents;
        }

        private double ComputeAverageScore(IEnumerable<dynamic> sessions)
        {
            if (sessions == null || !sessions.Any())
                return 0;

            double totalCorrect = 0;
            double totalQuestions = 0;

            foreach (var session in sessions)
            {
                totalCorrect += session.CorrectCount;
                totalQuestions += session.TotalQuestions;
            }

            if (totalQuestions == 0)
                return 0;

            return Math.Round((totalCorrect / totalQuestions) * 100, 1);
        }

        public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 5)
        {
            var activities = new List<RecentActivity>();

            var studentRegs = await _context.Students
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.RegisteredDate)
                .Take(count)
                .Select(s => new RecentActivity
                {
                    ActivityType = "Student",
                    Description = $"New student registered: {s.Name}",
                    Timestamp = s.RegisteredDate,
                    UserName = s.Name,
                    IconClass = "fa-user-plus"
                })
                .ToListAsync();

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Module)
                .Where(e => e.Status == "Active")
                .OrderByDescending(e => e.EnrolledDate)
                .Take(count)
                .Select(e => new RecentActivity
                {
                    ActivityType = "Enrollment",
                    Description = $"{e.Student!.Name} enrolled in {e.Module!.Name}",
                    Timestamp = e.EnrolledDate,
                    UserName = e.Student.Name,
                    IconClass = "fa-graduation-cap"
                })
                .ToListAsync();

            var quizCompletions = await _context.Sessions
                .Include(s => s.Student)
                .Include(s => s.Quiz)
                .Where(s => s.IsCompleted)
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .Select(s => new RecentActivity
                {
                    ActivityType = "Quiz",
                    Description = $"{s.Student!.Name} completed quiz: {s.Quiz!.Title}",
                    Timestamp = s.Timestamp,
                    UserName = s.Student.Name,
                    IconClass = "fa-check-circle"
                })
                .ToListAsync();

            // Combine and sort by timestamp desc
            activities.AddRange(studentRegs);
            activities.AddRange(enrollments);
            activities.AddRange(quizCompletions);

            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public async Task<List<EnrollmentTrendData>> GetEnrollmentTrendAsync()
        {
            var trendData = new List<EnrollmentTrendData>();
            var now = DateTime.UtcNow;

            var enrollmentsByMonth = await _context.Enrollments
                .Where(e => e.Status == "Active" && e.EnrolledDate >= now.AddMonths(-5))
                .GroupBy(e => new { e.EnrolledDate.Year, e.EnrolledDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var entry = enrollmentsByMonth.FirstOrDefault(e => e.Year == date.Year && e.Month == date.Month);
                trendData.Add(new EnrollmentTrendData
                {
                    Month = date.ToString("MMM yyyy"),
                    EnrollmentCount = entry?.Count ?? 0
                });
            }

            return trendData;
        }
    }
}