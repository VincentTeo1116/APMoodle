using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using APMoodle.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace APMoodle.Pages.BackEnd
{
    public class UserActivateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserActivateModel> _logger;

        public UserActivateModel(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<UserActivateModel> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostActivateAsync([FromForm] int id, [FromForm] string type)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(type))
                {
                    return new JsonResult(new { success = false, message = "Invalid request" });
                }

                bool activated = false;
                string userName = string.Empty;
                string userEmail = string.Empty;

                switch (type?.ToLower())
                {
                    case "student":
                        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == id);
                        if (student != null)
                        {
                            userEmail = student.Email;
                            userName = student.Name;
                            student.Status = "Active";
                            _context.Students.Update(student);
                            await _context.SaveChangesAsync();
                            activated = true;
                        }
                        break;

                    case "lecturer":
                        var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == id);
                        if (lecturer != null)
                        {
                            userEmail = lecturer.Email;
                            userName = lecturer.Name;
                            lecturer.Status = "Active";
                            _context.Lecturers.Update(lecturer);
                            await _context.SaveChangesAsync();
                            activated = true;
                        }
                        break;

                    case "admin":
                        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.AdminID == id);
                        if (admin != null)
                        {
                            userEmail = admin.Email;
                            userName = admin.Name;
                            admin.Status = "Active";
                            _context.Admins.Update(admin);
                            await _context.SaveChangesAsync();
                            activated = true;
                        }
                        break;

                    default:
                        return new JsonResult(new { success = false, message = "Invalid user type" });
                }

                if (activated)
                {
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        try
                        {
                            await SendActivationEmailAsync(userEmail, userName);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Failed to send activation email to {Email}", userEmail);
                        }
                    }

                    return new JsonResult(new
                    {
                        success = true,
                        message = $"User '{userName}' has been activated successfully.",
                        userName = userName
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private async Task SendActivationEmailAsync(string email, string name)
        {
            string subject = "Your APMoodle Account is Now Active!";
            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7fc; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 16px; box-shadow: 0 8px 30px rgba(0,0,0,0.08); overflow: hidden; border: 1px solid #e9edf2; }}
                        .header {{ background: linear-gradient(135deg, #10b981, #34d399); padding: 28px 20px; text-align: center; color: white; }}
                        .header h1 {{ margin: 0; font-weight: 700; font-size: 26px; letter-spacing: -0.3px; }}
                        .header p {{ margin: 6px 0 0; opacity: 0.9; font-size: 15px; }}
                        .content {{ padding: 30px 28px; }}
                        .content h2 {{ color: #1e293b; font-size: 22px; font-weight: 600; margin-top: 0; }}
                        .content p {{ color: #475569; line-height: 1.6; font-size: 15px; margin: 0 0 12px; }}
                        .btn {{ display: inline-block; background: linear-gradient(135deg, #10b981, #059669); color: white; padding: 12px 28px; border-radius: 40px; text-decoration: none; font-weight: 600; font-size: 15px; margin-top: 8px; transition: 0.2s; }}
                        .btn:hover {{ transform: translateY(-2px); box-shadow: 0 4px 14px rgba(16,185,129,0.35); }}
                        .footer {{ background: #f8fafc; padding: 16px 20px; text-align: center; font-size: 13px; color: #94a3b8; border-top: 1px solid #e9edf2; }}
                        .success-icon {{ font-size: 48px; color: #10b981; text-align: center; margin: 10px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Account Activated</h1>
                            <p>You're all set to start learning!</p>
                        </div>
                        <div class='content'>
                            <div class='success-icon'>🎉</div>
                            <h2>Hello {name},</h2>
                            <p>Your <strong>APMoodle</strong> account has been <strong>activated</strong> by the administrator.</p>
                            <p>You can now log in using the credentials you received during registration.</p>
                            <div style='text-align: center; margin: 24px 0 16px;'>
                                <a href='https://apmoodle.onrender.com/FrontEnd/Login' class='btn'>🔑 Log in Now</a>
                            </div>
                            <p style='font-size: 14px; color: #64748b;'>
                                If you have any questions, please contact support.
                            </p>
                        </div>
                        <div class='footer'>
                            &copy; {DateTime.Now.Year} APMoodle - Built with <span style='color:#ef4444;'>❤</span> for education.
                        </div>
                    </div>
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(email, subject, body);
        }
    }
}