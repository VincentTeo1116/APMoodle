using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APMoodle.Security
{
    /// <summary>
    /// Central session-based access control for Razor Pages.
    ///
    /// This app configures NO authentication scheme — access was enforced
    /// ad-hoc inside individual page handlers, which left a large part of the
    /// admin / user-management surface completely open (any anonymous request
    /// could reach it). This one filter patches those holes in a single place
    /// by mapping page paths to the roles allowed to open them, so we don't
    /// have to duplicate (and inevitably miss) guards across ~15 files.
    ///
    /// Pages that ALREADY carry a correct, page-specific guard/redirect are
    /// deliberately NOT listed here so this filter never fights their own
    /// redirect logic — it only closes the gaps.
    /// </summary>
    public class SessionAuthPageFilter : IAsyncPageFilter
    {
        // Page ViewEnginePath  ->  roles permitted to load it.
        private static readonly Dictionary<string, string[]> PageRoles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // ----- Admin-only management surface -----
                ["/FrontEnd/AdminDashboard"] = new[] { "admin" },
                ["/FrontEnd/UserList"] = new[] { "admin" },
                ["/FrontEnd/UserCreate"] = new[] { "admin" },
                ["/FrontEnd/UserEdit"] = new[] { "admin" },
                ["/FrontEnd/UserDetails"] = new[] { "admin" },
                ["/FrontEnd/UserActivate"] = new[] { "admin" },
                ["/FrontEnd/UserDelete"] = new[] { "admin" },
                ["/FrontEnd/UserReactivate"] = new[] { "admin" },
                ["/FrontEnd/UserReject"] = new[] { "admin" },
                ["/FrontEnd/ModuleList"] = new[] { "admin" },
                ["/FrontEnd/AddModule"] = new[] { "admin" },
                ["/FrontEnd/EditModule"] = new[] { "admin" },
                ["/FrontEnd/AnnouncementList"] = new[] { "admin" },
                ["/FrontEnd/AddAnnouncement"] = new[] { "admin" },
                ["/FrontEnd/EditAnnouncement"] = new[] { "admin" },
                ["/FrontEnd/TestDB"] = new[] { "admin" },

                // ----- Lecturer-only authoring -----
                ["/FrontEnd/CreateMaterial"] = new[] { "lecturer" },
                ["/FrontEnd/EditMaterial"] = new[] { "lecturer" },
            };

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(
            PageHandlerExecutingContext context,
            PageHandlerExecutionDelegate next)
        {
            var path = (context.ActionDescriptor as PageActionDescriptor)?.ViewEnginePath;

            if (path != null && PageRoles.TryGetValue(path, out var allowedRoles))
            {
                var session = context.HttpContext.Session;
                var userId = session.GetString("UserID");
                var role = session.GetString("UserRole");

                var loggedIn = !string.IsNullOrEmpty(userId)
                               && !string.IsNullOrEmpty(role)
                               && role != "Guest";

                if (!loggedIn)
                {
                    context.Result = Deny(context, "/FrontEnd/Login");
                    return;
                }

                if (!allowedRoles.Contains(role))
                {
                    // Logged in but wrong role → send home; "/" routes them to
                    // their own dashboard.
                    context.Result = Deny(context, "/");
                    return;
                }
            }

            await next();
        }

        // Redirect normal page loads; return 403 for AJAX / non-GET so client
        // JavaScript sees a clean failure instead of following a redirect.
        private static IActionResult Deny(PageHandlerExecutingContext context, string redirectPath)
        {
            var req = context.HttpContext.Request;
            var isGet = HttpMethods.IsGet(req.Method);
            var wantsJson = req.Headers["X-Requested-With"] == "XMLHttpRequest"
                            || req.Headers["Accept"].ToString().Contains("application/json");

            if (!isGet || wantsJson)
            {
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            return new RedirectResult(redirectPath);
        }
    }
}
