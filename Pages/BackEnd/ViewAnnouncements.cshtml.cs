using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    // Read-only announcements viewer for STUDENTS and LECTURERS.
    // Admins have the full management console (AnnouncementList); if an admin
    // lands here we send them there instead.
    public class ViewAnnouncementsModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;

        public ViewAnnouncementsModel(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public List<Announcement> Announcements { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role) || role == "Guest")
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            // Admins get the management console instead of the read-only view.
            if (role == "admin")
            {
                return RedirectToPage("/FrontEnd/AnnouncementList");
            }

            // Active announcements, newest first, with the admin creator included.
            Announcements = await _announcementService.GetAllAnnouncementsAsync();

            return Page();
        }
    }
}
