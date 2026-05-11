using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class LoginModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly ILecturerService _lecturerService;
        private readonly IAdminService _adminService;

        public LoginModel(IStudentService studentService, ILecturerService lecturerService, IAdminService adminService)
        {
            _studentService = studentService;
            _lecturerService = lecturerService;
            _adminService = adminService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "User ID / Email is required")]
            public string UserId { get; set; } = default!;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = default!;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string userId = Input.UserId.Trim();
            string password = Input.Password;

            // Try to extract role from userId (supports both code and email formats)
            string? role = GetRoleFromInput(userId);
            string? extractedCode = ExtractCodeFromInput(userId);

            if (role != null && extractedCode != null)
            {
                // Login by prefix (User Code or Email)
                switch (role)
                {
                    case "student":
                        var student = await _studentService.GetStudentByCodeAsync(extractedCode);
                        if (student != null && student.Status == "Active" && VerifyPassword(password, student.Password))
                        {
                            SetSession(student.StudentID.ToString(), student.StudentCode, student.Name, "student", student.Email);
                            // return RedirectToPage("/Student/Dashboard");
                            return RedirectToPage("/ModuleOverview");
                        }
                        break;

                    case "lecturer":
                        var lecturer = await _lecturerService.GetLecturerByCodeAsync(extractedCode);
                        if (lecturer != null && lecturer.Status == "Active" && VerifyPassword(password, lecturer.Password))
                        {
                            SetSession(lecturer.LecturerID.ToString(), lecturer.LecturerCode, lecturer.Name, "lecturer", lecturer.Email);
                            // return RedirectToPage("/Lecturer/Dashboard");
                            return RedirectToPage("/ModuleOverview");
                        }
                        break;

                    case "admin":
                        var admin = await _adminService.GetAdminByCodeAsync(extractedCode);
                        if (admin != null && VerifyPassword(password, admin.Password))
                        {
                            SetSession(admin.AdminID.ToString(), admin.AdminCode, admin.Name, "admin", admin.Email);
                            // return RedirectToPage("/Admin/Dashboard");
                            return RedirectToPage("/ModuleOverview");
                        }
                        break;
                }
            }
            else
            {
                // Login by regular email (no prefix) - check all tables
                // Check Student
                var student = await _studentService.GetStudentByEmailAsync(userId);
                if (student != null && student.Status == "Active" && VerifyPassword(password, student.Password))
                {
                    SetSession(student.StudentID.ToString(), student.StudentCode, student.Name, "student", student.Email);
                    return RedirectToPage("/Student/Dashboard");
                }

                // Check Lecturer
                var lecturer = await _lecturerService.GetLecturerByEmailAsync(userId);
                if (lecturer != null && lecturer.Status == "Active" && VerifyPassword(password, lecturer.Password))
                {
                    SetSession(lecturer.LecturerID.ToString(), lecturer.LecturerCode, lecturer.Name, "lecturer", lecturer.Email);
                    return RedirectToPage("/Lecturer/Dashboard");
                }

                // Check Admin
                var admin = await _adminService.GetAdminByEmailAsync(userId);
                if (admin != null && VerifyPassword(password, admin.Password))
                {
                    SetSession(admin.AdminID.ToString(), admin.AdminCode, admin.Name, "admin", admin.Email);
                    return RedirectToPage("/Admin/Dashboard");
                }
            }

            // If no match found
            Message = "Invalid User ID or Password. Please try again.";
            IsSuccess = false;
            return Page();
        }

        private string? GetRoleFromInput(string input)
        {
            string upperInput = input.ToUpper();
            
            if (upperInput.StartsWith("ST"))
                return "student";
            if (upperInput.StartsWith("LT"))
                return "lecturer";
            if (upperInput.StartsWith("AD"))
                return "admin";
            
            return null;
        }

        private string? ExtractCodeFromInput(string input)
        {
            // Remove everything after @ if present (email format)
            string code = input.Contains('@') ? input.Substring(0, input.IndexOf('@')) : input;
            
            // Verify it has the correct format (starts with ST/LT/AD)
            string upperCode = code.ToUpper();
            if (upperCode.StartsWith("ST") || upperCode.StartsWith("LT") || upperCode.StartsWith("AD"))
            {
                return code;
            }
            
            return null;
        }

        private void SetSession(string userId, string userCode, string userName, string role, string email)
        {
            HttpContext.Session.SetString("UserID", userId);
            HttpContext.Session.SetString("UserCode", userCode);
            HttpContext.Session.SetString("UserName", userName);
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("UserEmail", email);
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
                return false;

            if (storedPassword.StartsWith("$2"))
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
            }
            
            return inputPassword == storedPassword;
        }
    }
}