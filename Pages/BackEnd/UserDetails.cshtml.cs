using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class UserDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id, string type)
        {
            try
            {
                UserDetailDto? userDetail = null;

                switch (type?.ToLower())
                {
                    case "student":
                        var student = await _context.Students
                            .Where(s => s.StudentID == id)
                            .Select(s => new
                            {
                                s.StudentID,
                                s.Name,
                                s.Email,
                                s.StudentCode,
                                s.Status,
                                s.ProfilePic,
                                s.RegisteredDate,
                                s.PhoneNumber,
                                s.Gender
                            })
                            .FirstOrDefaultAsync();

                        if (student != null)
                        {
                            userDetail = new UserDetailDto
                            {
                                Id = student.StudentID,
                                Name = student.Name,
                                Email = student.Email,
                                UserCode = student.StudentCode,
                                Role = "Student",
                                Status = student.Status,
                                ProfilePic = student.ProfilePic,
                                RegisteredDate = student.RegisteredDate,
                                PhoneNumber = student.PhoneNumber,
                                Gender = student.Gender,
                                Department = null,
                                Initials = GetInitials(student.Name)
                            };
                        }
                        break;

                    case "lecturer":
                        var lecturer = await _context.Lecturers
                            .Where(l => l.LecturerID == id)
                            .Select(l => new
                            {
                                l.LecturerID,
                                l.Name,
                                l.Email,
                                l.LecturerCode,
                                l.Status,
                                l.ProfilePic,
                                l.RegisteredDate,
                                l.PhoneNumber,
                                l.Gender,
                                l.Department
                            })
                            .FirstOrDefaultAsync();

                        if (lecturer != null)
                        {
                            userDetail = new UserDetailDto
                            {
                                Id = lecturer.LecturerID,
                                Name = lecturer.Name,
                                Email = lecturer.Email,
                                UserCode = lecturer.LecturerCode,
                                Role = "Lecturer",
                                Status = lecturer.Status,
                                ProfilePic = lecturer.ProfilePic,
                                RegisteredDate = lecturer.RegisteredDate,
                                PhoneNumber = lecturer.PhoneNumber,
                                Gender = lecturer.Gender,
                                Department = lecturer.Department,
                                Initials = GetInitials(lecturer.Name)
                            };
                        }
                        break;

                    case "admin":
                        var admin = await _context.Admins
                            .Where(a => a.AdminID == id)
                            .Select(a => new
                            {
                                a.AdminID,
                                a.Name,
                                a.Email,
                                a.AdminCode,
                                a.Status,
                                a.ProfilePic,
                                a.PhoneNumber,
                                a.Gender
                            })
                            .FirstOrDefaultAsync();

                        if (admin != null)
                        {
                            userDetail = new UserDetailDto
                            {
                                Id = admin.AdminID,
                                Name = admin.Name,
                                Email = admin.Email,
                                UserCode = admin.AdminCode,
                                Role = "Admin",
                                Status = admin.Status,
                                ProfilePic = admin.ProfilePic,
                                RegisteredDate = null,
                                RegisteredDateDisplay = "System User",  
                                PhoneNumber = admin.PhoneNumber,
                                Gender = admin.Gender,
                                Department = null,
                                Initials = GetInitials(admin.Name)
                            };
                        }
                        break;

                    default:
                        return new JsonResult(new { success = false, message = "Invalid user type" });
                }

                if (userDetail == null)
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }

                return new JsonResult(new { success = true, user = userDetail });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?";
            var parts = name.Trim().Split(' ');
            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
            return name.Substring(0, Math.Min(2, name.Length)).ToUpper();
        }
    }

    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }
        public DateTime? RegisteredDate { get; set; } 
        public string? RegisteredDateDisplay { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Department { get; set; }
        public string Initials { get; set; } = string.Empty;
    }
}