using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using System.Security.Claims;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class AddAnnouncementModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;

        public AddAnnouncementModel(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [BindProperty]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        // Flag to show success popup
        public bool ShowSuccessPopup { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if we need to show success popup
            if (TempData["ShowSuccessPopup"] != null && (bool)TempData["ShowSuccessPopup"])
            {
                ShowSuccessPopup = true;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Title))
            {
                TempData["ErrorMessage"] = "Title is required.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                TempData["ErrorMessage"] = "Message is required.";
                return RedirectToPage();
            }

            if (Title.Length > 200)
            {
                TempData["ErrorMessage"] = "Title cannot exceed 200 characters.";
                return RedirectToPage();
            }

            if (Message.Length > 2000)
            {
                TempData["ErrorMessage"] = "Message cannot exceed 2000 characters.";
                return RedirectToPage();
            }

            // Get current user ID
            int currentUserId = GetCurrentUserId();

            // Create new announcement
            var announcement = new Announcement
            {
                Title = Title.Trim(),
                Message = Message.Trim(),
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            try
            {
                // Pass the currentUserId as the createdByAdminId parameter
                await _announcementService.CreateAnnouncementAsync(announcement, currentUserId);
                
                // Set flag to show popup on GET request
                TempData["ShowSuccessPopup"] = true;
                
                // Redirect to GET version of AddAnnouncement to show popup
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating announcement: {ex.Message}";
                return RedirectToPage();
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            var sessionUserId = HttpContext.Session.GetString("UserID");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int sid))
            {
                return sid;
            }

            return 1;
        }
    }
}