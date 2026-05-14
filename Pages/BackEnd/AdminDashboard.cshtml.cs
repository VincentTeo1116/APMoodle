using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.ViewModels;

namespace APMoodle.Pages.BackEnd
{
    public class AdminDashboardModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public AdminDashboardModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public DashboardViewModel Dashboard { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Dashboard = await _dashboardService.GetDashboardDataAsync();
            return Page();
        }
    }
}