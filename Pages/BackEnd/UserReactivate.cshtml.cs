using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class UserReactivateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserReactivateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostReactivateAsync([FromForm] int id, [FromForm] string type)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(type))
                {
                    return new JsonResult(new { success = false, message = "Invalid request" });
                }

                bool reactivated = false;
                string userName = string.Empty;

                switch (type?.ToLower())
                {
                    case "student":
                        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == id);
                        if (student != null)
                        {
                            student.Status = "Active";
                            _context.Students.Update(student);
                            await _context.SaveChangesAsync();
                            reactivated = true;
                            userName = student.Name;
                        }
                        break;

                    case "lecturer":
                        var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == id);
                        if (lecturer != null)
                        {
                            lecturer.Status = "Active";
                            _context.Lecturers.Update(lecturer);
                            await _context.SaveChangesAsync();
                            reactivated = true;
                            userName = lecturer.Name;
                        }
                        break;

                    case "admin":
                        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.AdminID == id);
                        if (admin != null)
                        {
                            admin.Status = "Active";
                            _context.Admins.Update(admin);
                            await _context.SaveChangesAsync();
                            reactivated = true;
                            userName = admin.Name;
                        }
                        break;

                    default:
                        return new JsonResult(new { success = false, message = "Invalid user type" });
                }

                if (reactivated)
                {
                    return new JsonResult(new 
                    { 
                        success = true, 
                        message = $"User '{userName}' has been reactivated successfully.",
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
    }
}