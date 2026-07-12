using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APMoodle.Pages.BackEnd
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            // Back to the public landing page (the site's default entry point)
            return Redirect("/");
        }
    }
}