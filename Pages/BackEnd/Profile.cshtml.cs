using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Services.Interfaces;
using APMoodle.Models;

namespace APMoodle.Pages.BackEnd
{
    public class ProfileModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly ILecturerService _lecturerService;
        private readonly IAdminService _adminService;
        private readonly IWebHostEnvironment _environment;

        public ProfileModel(
            IStudentService studentService,
            ILecturerService lecturerService,
            IAdminService adminService,
            IWebHostEnvironment environment)
        {
            _studentService = studentService;
            _lecturerService = lecturerService;
            _adminService = adminService;
            _environment = environment;
        }

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        // Common properties
        public string UserCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }
        
        // Lecturer specific
        public string? Department { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            await LoadUserData(int.Parse(userId), userRole ?? "");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/FrontEnd/Login");
            }

            var id = int.Parse(userId);
            var name = Request.Form["Name"].ToString();
            var phoneNumber = Request.Form["PhoneNumber"].ToString();
            var dob = DateTime.TryParse(Request.Form["DOB"], out var dobResult) ? dobResult : (DateTime?)null;
            var gender = Request.Form["Gender"].ToString();
            
            // Handle profile picture upload
            var profilePicUrl = await HandleProfilePictureUpload();

            bool success = false;

            if (userRole == "student")
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student != null)
                {
                    student.Name = name;
                    student.PhoneNumber = phoneNumber;
                    if (dob.HasValue) student.DOB = DateOnly.FromDateTime(dob.Value);
                    student.Gender = gender;
                    if (!string.IsNullOrEmpty(profilePicUrl)) student.ProfilePic = profilePicUrl;
                    success = await _studentService.UpdateStudentAsync(student);
                }
            }
            else if (userRole == "lecturer")
            {
                var department = Request.Form["Department"].ToString();
                var lecturer = await _lecturerService.GetLecturerByIdAsync(id);
                if (lecturer != null)
                {
                    lecturer.Name = name;
                    lecturer.PhoneNumber = phoneNumber;
                    if (dob.HasValue) lecturer.DOB = DateOnly.FromDateTime(dob.Value);
                    lecturer.Gender = gender;
                    lecturer.Department = department;
                    if (!string.IsNullOrEmpty(profilePicUrl)) lecturer.ProfilePic = profilePicUrl;
                    success = await _lecturerService.UpdateLecturerAsync(lecturer);
                }
            }
            else if (userRole == "admin")
            {
                var admin = await _adminService.GetAdminByIdAsync(id);
                if (admin != null)
                {
                    admin.Name = name;
                    admin.PhoneNumber = phoneNumber;
                    if (dob.HasValue) admin.DOB = DateOnly.FromDateTime(dob.Value);
                    admin.Gender = gender;
                    if (!string.IsNullOrEmpty(profilePicUrl)) admin.ProfilePic = profilePicUrl;
                    success = await _adminService.UpdateAdminAsync(admin);
                }
            }

            if (success)
            {
                Message = "Profile updated successfully!";
                IsSuccess = true;
                // Refresh session name
                HttpContext.Session.SetString("UserName", name);
            }
            else
            {
                Message = "Failed to update profile. Please try again.";
                IsSuccess = false;
            }

            await LoadUserData(id, userRole ?? "");
            return Page();
        }

        private async Task<string?> HandleProfilePictureUpload()
        {
            var file = Request.Form.Files.GetFile("profilePic");
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/profiles/{uniqueFileName}";
        }

        private async Task LoadUserData(int userId, string userRole)
        {
            if (userRole == "student")
            {
                var student = await _studentService.GetStudentByIdAsync(userId);
                if (student != null)
                {
                    UserCode = student.StudentCode;
                    Name = student.Name;
                    Email = student.Email;
                    PhoneNumber = student.PhoneNumber;
                    DOB = student.DOB.ToDateTime(TimeOnly.MinValue);
                    Gender = student.Gender;
                    ProfilePic = student.ProfilePic;
                }
            }
            else if (userRole == "lecturer")
            {
                var lecturer = await _lecturerService.GetLecturerByIdAsync(userId);
                if (lecturer != null)
                {
                    UserCode = lecturer.LecturerCode;
                    Name = lecturer.Name;
                    Email = lecturer.Email;
                    PhoneNumber = lecturer.PhoneNumber;
                    DOB = lecturer.DOB.ToDateTime(TimeOnly.MinValue);
                    Gender = lecturer.Gender;
                    ProfilePic = lecturer.ProfilePic;
                    Department = lecturer.Department;
                }
            }
            else if (userRole == "admin")
            {
                var admin = await _adminService.GetAdminByIdAsync(userId);
                if (admin != null)
                {
                    UserCode = admin.AdminCode;
                    Name = admin.Name;
                    Email = admin.Email;
                    PhoneNumber = admin.PhoneNumber;
                    DOB = admin.DOB.ToDateTime(TimeOnly.MinValue);
                    Gender = admin.Gender;
                    ProfilePic = admin.ProfilePic;
                }
            }
        }
    }
}