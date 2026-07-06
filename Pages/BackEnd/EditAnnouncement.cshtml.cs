using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace APMoodle.Pages.BackEnd
{
    public class EditAnnouncementModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IAdminService _adminService;

        public EditAnnouncementModel(IAnnouncementService announcementService, IAdminService adminService)
        {
            _announcementService = announcementService;
            _adminService = adminService;
        }

        [BindProperty]
        public Announcement Announcement { get; set; } = new();

        public string CreatorName { get; set; } = string.Empty;
        public string FormattedCreatedAt { get; set; } = string.Empty;
        public string LastModifierName { get; set; } = string.Empty;
        public string FormattedLastModifiedAt { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "Invalid announcement ID.";
                    return RedirectToPage("/FrontEnd/AnnouncementList");
                }

                var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
                
                if (announcement == null)
                {
                    TempData["ErrorMessage"] = "Announcement not found.";
                    return RedirectToPage("/FrontEnd/AnnouncementList");
                }

                if (announcement.Status == "Removed")
                {
                    TempData["ErrorMessage"] = "This announcement has been deleted.";
                    return RedirectToPage("/FrontEnd/AnnouncementList");
                }

                Announcement = announcement;

                // Get creator information
                if (announcement.CreatedBy > 0)
                {
                    try
                    {
                        var creator = await _adminService.GetAdminByIdAsync(announcement.CreatedBy);
                        CreatorName = creator?.Name ?? "Unknown Admin";
                    }
                    catch
                    {
                        CreatorName = "Unknown Admin";
                    }
                }

                // Get last modifier information
                if (announcement.LastModifiedBy.HasValue && announcement.LastModifiedBy.Value > 0)
                {
                    try
                    {
                        var lastModifier = await _adminService.GetAdminByIdAsync(announcement.LastModifiedBy.Value);
                        LastModifierName = lastModifier?.Name ?? "Unknown Admin";
                    }
                    catch
                    {
                        LastModifierName = "Unknown Admin";
                    }
                }

                // Format dates
                FormattedCreatedAt = announcement.CreatedAt.ToString("MMMM dd, yyyy 'at' hh:mm tt");
                FormattedLastModifiedAt = announcement.LastModifiedAt?.ToString("MMMM dd, yyyy 'at' hh:mm tt") ?? "Not modified yet";
                IsDeleted = announcement.Status == "Removed";

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading announcement: {ex.Message}";
                return RedirectToPage("/FrontEnd/AnnouncementList");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload data for the page
                    await LoadAnnouncementData(id);
                    TempData["ErrorMessage"] = "Please correct the errors below.";
                    return Page();
                }

                // Get current admin ID
                var adminId = await GetCurrentAdminId();
                if (!adminId.HasValue)
                {
                    TempData["ErrorMessage"] = "Unable to identify current admin.";
                    await LoadAnnouncementData(id);
                    return Page();
                }

                // Update the announcement
                var updatedAnnouncement = await _announcementService.UpdateAnnouncementAsync(id, Announcement, adminId.Value);

                TempData["SuccessMessage"] = "Announcement updated successfully!";
                return RedirectToPage("/FrontEnd/AnnouncementList");
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Announcement not found.";
                return RedirectToPage("/FrontEnd/AnnouncementList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating announcement: {ex.Message}";
                await LoadAnnouncementData(id);
                return Page();
            }
        }

        // public async Task<IActionResult> OnPostDeleteAsync(int id)
        // {
        //     try
        //     {
        //         var adminId = await GetCurrentAdminId();
                
        //         if (!adminId.HasValue)
        //         {
        //             TempData["ErrorMessage"] = "Unable to identify current admin.";
        //             return RedirectToPage("/FrontEnd/Announcements");
        //         }

        //         var result = await _announcementService.DeleteAnnouncementAsync(id, adminId.Value);
                
        //         if (result)
        //         {
        //             TempData["SuccessMessage"] = "Announcement deleted successfully.";
        //         }
        //         else
        //         {
        //             TempData["ErrorMessage"] = "Failed to delete announcement.";
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         TempData["ErrorMessage"] = $"Error deleting announcement: {ex.Message}";
        //     }

        //     return RedirectToPage("/FrontEnd/Announcements");
        // }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
{
    try
    {
        var adminId = await GetCurrentAdminId();
        
        if (!adminId.HasValue)
        {
            return new JsonResult(new { success = false, message = "Unable to identify current admin." });
        }

        var result = await _announcementService.DeleteAnnouncementAsync(id, adminId.Value);
        
        if (result)
        {
            return new JsonResult(new { success = true, message = "Announcement deleted successfully." });
        }
        else
        {
            return new JsonResult(new { success = false, message = "Failed to delete announcement." });
        }
    }
    catch (Exception ex)
    {
        return new JsonResult(new { success = false, message = ex.Message });
    }
}
        private async Task LoadAnnouncementData(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement != null)
            {
                Announcement = announcement;
                
                // Get creator information
                if (announcement.CreatedBy > 0)
                {
                    var creator = await _adminService.GetAdminByIdAsync(announcement.CreatedBy);
                    CreatorName = creator?.Name ?? "Unknown Admin";
                }

                // Get last modifier information
                if (announcement.LastModifiedBy.HasValue && announcement.LastModifiedBy.Value > 0)
                {
                    var lastModifier = await _adminService.GetAdminByIdAsync(announcement.LastModifiedBy.Value);
                    LastModifierName = lastModifier?.Name ?? "Unknown Admin";
                }

                FormattedCreatedAt = announcement.CreatedAt.ToString("MMMM dd, yyyy 'at' hh:mm tt");
                FormattedLastModifiedAt = announcement.LastModifiedAt?.ToString("MMMM dd, yyyy 'at' hh:mm tt") ?? "Not modified yet";
            }
        }

        private async Task<int?> GetCurrentAdminId()
        {
            // Try to get from session first
            var userId = HttpContext.Session.GetString("UserID");
            
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int adminId))
            {
                return adminId;
            }

            // Try to get from claims
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(adminIdClaim) && int.TryParse(adminIdClaim, out int adminIdFromClaim))
            {
                return adminIdFromClaim;
            }

            // Try to get admin by email
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(adminEmail))
            {
                try
                {
                    var admin = await _adminService.GetAdminByEmailAsync(adminEmail);
                    if (admin != null)
                    {
                        return admin.AdminID;
                    }
                }
                catch
                {
                    // Admin service error
                }
            }

            return null;
        }
    }
}