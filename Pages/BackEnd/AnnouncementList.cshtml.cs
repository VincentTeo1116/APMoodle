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
    
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _announcementService.DeleteAnnouncementAsync(id);
            
            if (result)
            {
                return new JsonResult(new { success = true, message = "Deleted successfully" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Failed to delete announcement" }) 
                { 
                    StatusCode = 400 
                };
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