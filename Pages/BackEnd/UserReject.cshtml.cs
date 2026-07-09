using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class UserRejectModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserRejectModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostRejectAsync([FromForm] int id, [FromForm] string type)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(type))
                {
                    return new JsonResult(new { success = false, message = "Invalid request" });
                }

                bool rejected = false;
                string userName = string.Empty;

                switch (type?.ToLower())
                {
                    case "student":
                        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == id);
                        if (student != null)
                        {
                            student.Status = "Inactive";
                            _context.Students.Update(student);
                            await _context.SaveChangesAsync();
                            rejected = true;
                            userName = student.Name;
                        }
                        break;

                    case "lecturer":
                        var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == id);
                        if (lecturer != null)
                        {
                            lecturer.Status = "Inactive";
                            _context.Lecturers.Update(lecturer);
                            await _context.SaveChangesAsync();
                            rejected = true;
                            userName = lecturer.Name;
                        }
                        break;

                    case "admin":
                        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.AdminID == id);
                        if (admin != null)
                        {
                            admin.Status = "Inactive";
                            _context.Admins.Update(admin);
                            await _context.SaveChangesAsync();
                            rejected = true;
                            userName = admin.Name;
                        }
                        break;

                    default:
                        return new JsonResult(new { success = false, message = "Invalid user type" });
                }

                if (rejected)
                {
                    return new JsonResult(new 
                    { 
                        success = true, 
                        message = $"User '{userName}' has been rejected successfully.",
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