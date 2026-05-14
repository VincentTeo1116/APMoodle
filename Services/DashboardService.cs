using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;
using APMoodle.ViewModels;

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
            var dashboard = new DashboardViewModel();

            try
            {
                // Get total counts - handle null DbSets
                dashboard.TotalStudents = _context.Students != null ? await _context.Students.CountAsync() : 0;
                dashboard.TotalLecturers = _context.Lecturers != null ? await _context.Lecturers.CountAsync() : 0;
                dashboard.TotalModules = _context.Modules != null ? await _context.Modules.CountAsync() : 0;
                dashboard.TotalEnrollments = _context.Enrollments != null 
                    ? await _context.Enrollments.CountAsync(e => e.Status == "Active") 
                    : 0;

                // Get module categories - grouping by ModuleCode prefix since no Category property
                dashboard.ModuleCategories = await GetModuleCategoriesAsync();

                // Get top students
                dashboard.TopStudents = await GetTopStudentsAsync(5);

                // Get recent activities
                dashboard.RecentActivities = await GetRecentActivitiesAsync(5);

                // Get enrollment trend
                dashboard.EnrollmentTrend = await GetEnrollmentTrendAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting dashboard data: {ex.Message}");
            }

            return dashboard;
        }

        public async Task<List<ModuleCategoryData>> GetModuleCategoriesAsync()
        {
            try
            {
                if (_context.Modules == null) return new List<ModuleCategoryData>();
                
                var modules = await _context.Modules.ToListAsync();
                var totalModules = modules.Count;

                if (totalModules == 0)
                    return new List<ModuleCategoryData>();

                // Group by ModuleCode prefix (first 2-3 characters) as category
                var categories = modules
                    .GroupBy(m => GetModuleCategoryFromCode(m.ModuleCode))
                    .Select(g => new ModuleCategoryData
                    {
                        CategoryName = g.Key,
                        ModuleCount = g.Count(),
                        Percentage = Math.Round((double)g.Count() / totalModules * 100, 1)
                    })
                    .OrderByDescending(c => c.ModuleCount)
                    .ToList();

                return categories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting module categories: {ex.Message}");
                return new List<ModuleCategoryData>();
            }
        }

        private string GetModuleCategoryFromCode(string moduleCode)
        {
            if (string.IsNullOrEmpty(moduleCode)) return "Uncategorized";
            
            // Try to extract prefix (e.g., "CS" from "CS101", "MATH" from "MATH201")
            var match = System.Text.RegularExpressions.Regex.Match(moduleCode, @"^[A-Za-z]+");
            if (match.Success && match.Value.Length >= 2)
            {
                return match.Value.ToUpper();
            }
            
            return "Other";
        }

        public async Task<List<TopStudentData>> GetTopStudentsAsync(int topCount = 5)
        {
            try
            {
                if (_context.Enrollments == null || _context.Students == null)
                    return new List<TopStudentData>();

                // Calculate average score per student across all enrolled modules
                var topStudents = await _context.Enrollments
                    .Where(e => e.Status == "Active")
                    .GroupBy(e => new { e.StudentID, StudentCode = e.Student != null ? e.Student.StudentCode : "", Name = e.Student != null ? e.Student.Name : "" })
                    .Select(g => new TopStudentData
                    {
                        StudentId = g.Key.StudentID,
                        StudentName = string.IsNullOrEmpty(g.Key.Name) ? "Unknown" : g.Key.Name,
                        StudentCode = string.IsNullOrEmpty(g.Key.StudentCode) ? "N/A" : g.Key.StudentCode,
                        AverageScore = g.Average(e => e.Progress ?? 0)
                    })
                    .OrderByDescending(s => s.AverageScore)
                    .Take(topCount)
                    .ToListAsync();

                return topStudents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting top students: {ex.Message}");
                return new List<TopStudentData>();
            }
        }

        public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 5)
        {
            var activities = new List<RecentActivity>();

            try
            {
                // Get recent enrollments
                if (_context.Enrollments != null && _context.Students != null && _context.Modules != null)
                {
                    var recentEnrollments = await _context.Enrollments
                        .Where(e => e.EnrolledDate != null && e.Status == "Active")
                        .OrderByDescending(e => e.EnrolledDate)
                        .Take(count)
                        .Include(e => e.Student)
                        .Include(e => e.Module)
                        .ToListAsync();

                    foreach (var enrollment in recentEnrollments)
                    {
                        activities.Add(new RecentActivity
                        {
                            ActivityType = "Enrollment",
                            Description = $"{enrollment.Student?.Name ?? "Student"} enrolled in {enrollment.Module?.Name ?? "Module"}",
                            Timestamp = enrollment.EnrolledDate,
                            UserName = enrollment.Student?.Name ?? "Unknown",
                            IconClass = "fa-user-graduate"
                        });
                    }
                }

                // Get recent module creations (using StartDate as proxy for creation)
                if (_context.Modules != null && _context.Lecturers != null)
                {
                    var recentModules = await _context.Modules
                        .OrderByDescending(m => m.StartDate)
                        .Take(count)
                        .Include(m => m.Lecturer)
                        .ToListAsync();

                    foreach (var module in recentModules)
                    {
                        activities.Add(new RecentActivity
                        {
                            ActivityType = "Module",
                            Description = $"New module '{module.Name}' created by {module.Lecturer?.Name ?? "Admin"}",
                            Timestamp = module.StartDate,
                            UserName = module.Lecturer?.Name ?? "Admin",
                            IconClass = "fa-book-open"
                        });
                    }
                }

                // Get recent student registrations
                if (_context.Students != null)
                {
                    var recentStudents = await _context.Students
                        .OrderByDescending(s => s.RegisteredDate)
                        .Take(count)
                        .ToListAsync();

                    foreach (var student in recentStudents)
                    {
                        activities.Add(new RecentActivity
                        {
                            ActivityType = "Registration",
                            Description = $"New student registration: {student.Name} ({student.StudentCode})",
                            Timestamp = student.RegisteredDate,
                            UserName = student.Name ?? "Unknown",
                            IconClass = "fa-user-plus"
                        });
                    }
                }

                // Sort all activities by timestamp and take top count
                activities = activities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent activities: {ex.Message}");
            }

            return activities;
        }

        public async Task<List<EnrollmentTrendData>> GetEnrollmentTrendAsync()
        {
            var trend = new List<EnrollmentTrendData>();

            try
            {
                if (_context.Enrollments == null)
                {
                    return GetEmptyTrendData();
                }

                var sixMonthsAgo = DateTime.Now.AddMonths(-6);
                
                var enrollmentsByMonth = await _context.Enrollments
                    .Where(e => e.EnrolledDate >= sixMonthsAgo && e.Status == "Active")
                    .GroupBy(e => new { e.EnrolledDate.Year, e.EnrolledDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                // Fill in missing months
                for (int i = 5; i >= 0; i--)
                {
                    var date = DateTime.Now.AddMonths(-i);
                    var monthName = date.ToString("MMM yyyy");
                    
                    var existingData = enrollmentsByMonth
                        .FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month);
                    
                    trend.Add(new EnrollmentTrendData
                    {
                        Month = monthName,
                        EnrollmentCount = existingData?.Count ?? 0
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting enrollment trend: {ex.Message}");
                return GetEmptyTrendData();
            }

            return trend;
        }

        private List<EnrollmentTrendData> GetEmptyTrendData()
        {
            var trend = new List<EnrollmentTrendData>();
            for (int i = 5; i >= 0; i--)
            {
                trend.Add(new EnrollmentTrendData
                {
                    Month = DateTime.Now.AddMonths(-i).ToString("MMM yyyy"),
                    EnrollmentCount = 0
                });
            }
            return trend;
        }
    }
}