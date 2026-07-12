using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class AnnouncementListModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementListModel(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public List<Announcement> Announcements { get; set; } = new List<Announcement>();

        public int TotalCount { get; set; }
        public int PublishedCount { get; set; }
        public int DraftCount { get; set; }
        public int ArchivedCount { get; set; }

        public List<string> UniqueCreators { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            Announcements = await _announcementService.GetAllAnnouncementsAsync();

            // Get stats
            TotalCount = await _announcementService.GetTotalCountAsync();

            // Get unique creators for filter dropdown
            UniqueCreators = Announcements
                .Select(a => a.Creator?.Name ?? "Unknown")
                .Distinct()
                .OrderBy(name => name)
                .ToList();
        }
    
        // Updated Delete handler - uses form data instead of JSON
        public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userId) || (userRole != "admin" && userRole != "lecturer"))
                {
                    return new JsonResult(new { success = false, message = "Unauthorized" });
                }

                var result = await _announcementService.DeleteAnnouncementAsync(id, int.Parse(userId));
                
                if (result)
                {
                    return new JsonResult(new { success = true, message = "Deleted successfully" });
                }
                
                return new JsonResult(new { success = false, message = "Announcement not found" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);

            if (announcement == null)
            {
                return NotFound();
            }

            return new JsonResult(new
            {
                id = announcement.AnnouncementID,
                title = announcement.Title,
                message = announcement.Message,
                createdBy = announcement.Creator?.Name ?? "Unknown",
                createdAt = announcement.CreatedAt.ToString("MMMM dd, yyyy HH:mm")
            });
        }
    }
}