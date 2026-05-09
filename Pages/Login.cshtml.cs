using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages
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

            // Try to login as Student
            var student = await _studentService.GetStudentByCodeOrEmailAsync(Input.UserId);
            if (student != null && student.Status == "Active")
            {
                if (VerifyPassword(Input.Password, student.Password))
                {
                    // Set session variables
                    HttpContext.Session.SetString("UserID", student.StudentID.ToString());
                    HttpContext.Session.SetString("UserCode", student.StudentCode);
                    HttpContext.Session.SetString("UserName", student.Name);
                    HttpContext.Session.SetString("UserRole", "student");
                    HttpContext.Session.SetString("UserEmail", student.Email);

                    return RedirectToPage("/Student/Dashboard");
                }
            }

            // Try to login as Lecturer
            var lecturer = await _lecturerService.GetLecturerByCodeOrEmailAsync(Input.UserId);
            if (lecturer != null && lecturer.Status == "Active")
            {
                if (VerifyPassword(Input.Password, lecturer.Password))
                {
                    HttpContext.Session.SetString("UserID", lecturer.LecturerID.ToString());
                    HttpContext.Session.SetString("UserCode", lecturer.LecturerCode);
                    HttpContext.Session.SetString("UserName", lecturer.Name);
                    HttpContext.Session.SetString("UserRole", "lecturer");
                    HttpContext.Session.SetString("UserEmail", lecturer.Email);

                    return RedirectToPage("/Lecturer/Dashboard");
                }
            }

            // Try to login as Admin
            var admin = await _adminService.GetAdminByCodeOrEmailAsync(Input.UserId);
            if (admin != null)
            {
                if (VerifyPassword(Input.Password, admin.Password))
                {
                    HttpContext.Session.SetString("UserID", admin.AdminID.ToString());
                    HttpContext.Session.SetString("UserCode", admin.AdminCode);
                    HttpContext.Session.SetString("UserName", admin.Name);
                    HttpContext.Session.SetString("UserRole", "admin");
                    HttpContext.Session.SetString("UserEmail", admin.Email);

                    return RedirectToPage("/Admin/Dashboard");
                }
            }

            // If no match found
            Message = "Invalid User ID or Password. Please try again.";
            IsSuccess = false;
            return Page();
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            // If password is stored as BCrypt hash
            if (storedPassword.StartsWith("$2"))
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
            }
            
            // Plain text comparison (for development)
            return inputPassword == storedPassword;
        }
    }
}