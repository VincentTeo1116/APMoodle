using APMoodle.ViewModels;

namespace APMoodle.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<List<ModuleCategoryData>> GetModuleCategoriesAsync();
        Task<List<TopStudentData>> GetTopStudentsAsync(int topCount = 5);
        Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 5);
        Task<List<EnrollmentTrendData>> GetEnrollmentTrendAsync();
    }
}