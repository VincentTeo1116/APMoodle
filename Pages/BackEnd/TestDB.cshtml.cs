using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;

namespace APMoodle.Pages.BackEnd
{
    public class TestDBModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TestDBModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string? ErrorMessage { get; set; }
        public bool ConnectionSuccessful { get; set; }
        public int StudentCount { get; set; }
        public int LecturerCount { get; set; }
        public int AdminCount { get; set; }
        public int ModuleCount { get; set; }
        public int MaterialCount { get; set; }
        public int QuizCount { get; set; }
        public int QuestionCount { get; set; }
        public int AnnouncementCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            try
            {
                if (!await _context.Database.CanConnectAsync())
                {
                    ErrorMessage = "Cannot connect to database.";
                    ConnectionSuccessful = false;
                    return Page();
                }

                ConnectionSuccessful = true;

                StudentCount = await _context.Students!.CountAsync();
                LecturerCount = await _context.Lecturers!.CountAsync();
                AdminCount = await _context.Admins!.CountAsync();
                ModuleCount = await _context.Modules!.CountAsync();
                MaterialCount = await _context.Materials!.CountAsync();
                QuizCount = await _context.Quizzes!.CountAsync();
                QuestionCount = await _context.Questions!.CountAsync();
                AnnouncementCount = await _context.Announcements!.CountAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ConnectionSuccessful = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDataAsync(string table)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                object? data = table switch
                {
                    "Students" => await _context.Students
                        .OrderBy(s => s.StudentID)
                        .Select(s => new { s.StudentID, s.StudentCode, s.Name, s.Email, s.PhoneNumber, s.DOB, s.Gender, s.Status, s.RegisteredDate, s.ProfilePic })
                        .ToListAsync(),

                    "Lecturers" => await _context.Lecturers
                        .OrderBy(l => l.LecturerID)
                        .Select(l => new { l.LecturerID, l.LecturerCode, l.Name, l.Email, l.PhoneNumber, l.DOB, l.Gender, l.Department, l.Status, l.RegisteredDate, l.ProfilePic })
                        .ToListAsync(),

                    "Admins" => await _context.Admins
                        .OrderBy(a => a.AdminID)
                        .Select(a => new { a.AdminID, a.AdminCode, a.Name, a.Email, a.PhoneNumber, a.DOB, a.Gender, a.Status, a.ProfilePic })
                        .ToListAsync(),

                    "Modules" => await _context.Modules
                        .OrderBy(m => m.ModuleID)
                        .Include(m => m.Lecturer)
                        .Select(m => new { m.ModuleID, m.ModuleCode, m.Name, m.Description, m.StartDate, m.EndDate, m.Status, m.InvitationCode, LecturerName = m.Lecturer != null ? m.Lecturer.Name : null })
                        .ToListAsync(),

                    "Materials" => await _context.Materials
                        .OrderBy(m => m.MaterialID)
                        .Include(m => m.Module)
                        .Select(m => new { m.MaterialID, m.Title, m.Description, m.ContentType, m.FileURL, m.CreatedAt, m.Status, ModuleName = m.Module != null ? m.Module.Name : null })
                        .ToListAsync(),

                    "Quizzes" => await _context.Quizzes
                        .OrderBy(q => q.QuizID)
                        .Include(q => q.Material)
                        .Select(q => new { q.QuizID, q.Title, q.Subject, q.Theme, q.Status, MaterialTitle = q.Material != null ? q.Material.Title : null })
                        .ToListAsync(),

                    "Questions" => await _context.Questions
                        .OrderBy(q => q.QuestionID)
                        .Select(q => new { q.QuestionID, q.QuestionText, q.Option1, q.Option2, q.Option3, q.Option4, q.CorrectAnswer, q.Status, q.QuizID })
                        .ToListAsync(),

                    "Announcements" => await _context.Announcements
                        .OrderBy(a => a.AnnouncementID)
                        .Include(a => a.Creator)
                        .Select(a => new { a.AnnouncementID, a.Title, a.Message, a.CreatedAt, a.Status, CreatedByName = a.Creator != null ? a.Creator.Name : null })
                        .ToListAsync(),

                    _ => null
                };

                if (data == null)
                    return new JsonResult(new { success = false, message = "Invalid table name" });

                return new JsonResult(new { success = true, data });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}