using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class LecturerDashboardModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ISessionService _sessionService;
        private readonly ILecturerService _lecturerService;
        private readonly IAnnouncementService _announcementService;

        public LecturerDashboardModel(
            IEnrollmentService enrollmentService,
            ISessionService sessionService,
            ILecturerService lecturerService,
            IAnnouncementService announcementService)
        {
            _enrollmentService = enrollmentService;
            _sessionService = sessionService;
            _lecturerService = lecturerService;
            _announcementService = announcementService;
        }

        public string LecturerName { get; set; } = "Lecturer";
        public string LecturerCode { get; set; } = string.Empty;

        // Top-line stats
        public int EnrolledModuleCount { get; set; }
        public int AverageScore { get; set; } // rounded percentage 0..100
        public int BestScore { get; set; } // 0..100

        // Listings
        public List<Module> EnrolledModules { get; set; } = new();
        public List<Session> RecentAttempts { get; set; } = new(); // 5 newest, for the activity feed
        public List<Session> TrendAttempts { get; set; } = new();  // wider window, for the score-trend chart
        public List<Announcement> Announcements { get; set; } = new();
        public void OnGet()
        {
        }
    }
}
