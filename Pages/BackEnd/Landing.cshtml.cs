using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APMoodle.Pages.BackEnd
{
    public class LandingModel : PageModel
    {
        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("UserRole");

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role) && role != "Guest")
            {
                return role switch
                {
                    "student" => RedirectToPage("/FrontEnd/StudentDashboard"),
                    "lecturer" => RedirectToPage("/FrontEnd/LecturerDashboard"),
                    "admin" => RedirectToPage("/FrontEnd/AdminDashboard"),
                    _ => Page()
                };
            }

            return Page();
        }
    }
}
