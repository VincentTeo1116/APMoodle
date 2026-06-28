using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class UserDeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserDeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id, [FromForm] string type)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(type))
                {
                    return new JsonResult(new { success = false, message = "Invalid request" });
                }

                bool deactivated = false;
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
                            deactivated = true;
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
                            deactivated = true;
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
                            deactivated = true;
                            userName = admin.Name;
                        }
                        break;

                    default:
                        return new JsonResult(new { success = false, message = "Invalid user type" });
                }

                if (deactivated)
                {
                    return new JsonResult(new 
                    { 
                        success = true, 
                        message = $"User '{userName}' has been deactivated successfully.",
                        userName = userName
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }
            }
            catch (DbUpdateException)
            {
                return new JsonResult(new { success = false, message = "Cannot deactivate user because they have related records." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}