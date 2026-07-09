using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
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

            if (role == "admin")
            {
                return RedirectToPage("/FrontEnd/AnnouncementList");
            }

            Announcements = await _announcementService.GetAllAnnouncementsAsync();

            return Page();
        }
    }
}
