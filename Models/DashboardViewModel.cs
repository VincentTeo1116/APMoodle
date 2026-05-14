using APMoodle.Models;

namespace APMoodle.ViewModels
{
    public class DashboardViewModel
    {
        // Statistics Cards
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalModules { get; set; }
        public int TotalEnrollments { get; set; }
        
        // Module Categories Chart - Since Module has no Category, we'll use ModuleCode prefix or create a default
        public List<ModuleCategoryData> ModuleCategories { get; set; } = new();
        
        // Top Performing Students
        public List<TopStudentData> TopStudents { get; set; } = new();
        
        // Recent Activities
        public List<RecentActivity> RecentActivities { get; set; } = new();
        
        // Enrollment Trend (last 6 months)
        public List<EnrollmentTrendData> EnrollmentTrend { get; set; } = new();
        
        // For Chart.js data serialization
        public ChartData ModuleChartData => new ChartData
        {
            Labels = ModuleCategories.Select(c => c.CategoryName.Length > 15 ? c.CategoryName.Substring(0, 12) + "..." : c.CategoryName).ToList(),
            Values = ModuleCategories.Select(c => c.ModuleCount).ToList()
        };
        
        public ChartData TrendChartData => new ChartData
        {
            Labels = EnrollmentTrend.Select(e => e.Month).ToList(),
            Values = EnrollmentTrend.Select(e => e.EnrollmentCount).ToList()
        };
    }
    
    public class ModuleCategoryData
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ModuleCount { get; set; }
        public double Percentage { get; set; }
    }
    
    public class TopStudentData
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public string? AvatarUrl { get; set; }
    }
    
    public class RecentActivity
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? IconClass { get; set; }
    }
    
    public class EnrollmentTrendData
    {
        public string Month { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
    }
    
    public class ChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Values { get; set; } = new();
    }
}