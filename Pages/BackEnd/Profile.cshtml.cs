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

        // Common properties
        public string UserCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }
        public string? Department { get; set; }
        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");

            await LoadUserData(int.Parse(userId), userRole ?? "");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");
            var removePhoto = Request.Form["RemovePhoto"].ToString() == "true";
            var phoneNumber = Request.Form["PhoneNumber"].ToString();

            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/FrontEnd/Login");
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^0\d{9,10}$"))
            {
                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = "Phone number must be 10-11 digits starting with 0.";
                return RedirectToPage();
            }

            var id = int.Parse(userId);
            bool success = false;

            if (userRole == "student")
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null) return NotFound();

                student.PhoneNumber = phoneNumber;
                if (removePhoto)
                    student.ProfilePic = null;
                else
                {
                    var newPic = await HandleProfilePictureUpload();
                    if (!string.IsNullOrEmpty(newPic))
                        student.ProfilePic = newPic;
                }
                success = await _studentService.UpdateStudentAsync(student);
            }
            else if (userRole == "lecturer")
            {
                var lecturer = await _lecturerService.GetLecturerByIdAsync(id);
                if (lecturer == null) return NotFound();

                lecturer.PhoneNumber = phoneNumber;
                if (removePhoto)
                    lecturer.ProfilePic = null;
                else
                {
                    var newPic = await HandleProfilePictureUpload();
                    if (!string.IsNullOrEmpty(newPic))
                        lecturer.ProfilePic = newPic;
                }
                success = await _lecturerService.UpdateLecturerAsync(lecturer);
            }
            else if (userRole == "admin")
            {
                var admin = await _adminService.GetAdminByIdAsync(id);
                if (admin == null) return NotFound();

                admin.PhoneNumber = phoneNumber;
                if (removePhoto)
                    admin.ProfilePic = null;
                else
                {
                    var newPic = await HandleProfilePictureUpload();
                    if (!string.IsNullOrEmpty(newPic))
                        admin.ProfilePic = newPic;
                }
                success = await _adminService.UpdateAdminAsync(admin);
            }
            else
                return Forbid();

            if (success)
            {
                TempData["ShowSuccessModal"] = true;
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToPage(); 
            }
            else
            {
                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
                return RedirectToPage();
            }
        }

        /// <summary>
        /// Checks the user's CURRENT password safely.
        ///
        /// Not every account stores a BCrypt hash — the seeded/legacy accounts
        /// (e.g. ST00001) still hold a PLAINTEXT password. Calling
        /// BCrypt.Verify() on those throws SaltParseException("Invalid salt
        /// version") and blew up the whole change-password request with a 500.
        /// So: only run BCrypt when the stored value actually looks like a
        /// BCrypt hash ("$2..."), otherwise fall back to a plaintext compare —
        /// exactly what Login.cshtml.cs::VerifyPassword does.
        /// The NEW password is always saved as a BCrypt hash, so changing it
        /// also upgrades a legacy account.
        /// </summary>
        private static bool VerifyCurrentPassword(string input, string? stored)
        {
            if (string.IsNullOrEmpty(stored) || input == null) return false;

            if (stored.StartsWith("$2"))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(input, stored);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    return false; // malformed hash → treat as a wrong password, never a 500
                }
            }

            return input == stored; // legacy plaintext account
        }

        public async Task<IActionResult> OnPostChangePasswordAsync([FromBody] ChangePasswordRequest request)
        {
            var userId = HttpContext.Session.GetString("UserID");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, message = "You must be logged in." });

            if (request.NewPassword != request.ConfirmNewPassword)
                return new JsonResult(new { success = false, message = "New password and confirmation do not match." });

            if (request.NewPassword.Length < 6)
                return new JsonResult(new { success = false, message = "New password must be at least 6 characters long." });

            var id = int.Parse(userId);
            bool passwordUpdated = false;
            string errorMessage = string.Empty;

            if (userRole == "student")
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null) return new JsonResult(new { success = false, message = "Student not found." });

                if (!VerifyCurrentPassword(request.CurrentPassword, student.Password))
                    return new JsonResult(new { success = false, message = "Current password is incorrect." });

                var hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                passwordUpdated = await _studentService.UpdatePasswordAsync(id, hashed);
                if (!passwordUpdated) errorMessage = "Failed to update password.";
            }
            else if (userRole == "lecturer")
            {
                var lecturer = await _lecturerService.GetLecturerByIdAsync(id);
                if (lecturer == null) return new JsonResult(new { success = false, message = "Lecturer not found." });

                if (!VerifyCurrentPassword(request.CurrentPassword, lecturer.Password))
                    return new JsonResult(new { success = false, message = "Current password is incorrect." });

                var hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                passwordUpdated = await _lecturerService.UpdatePasswordAsync(id, hashed);
                if (!passwordUpdated) errorMessage = "Failed to update password.";
            }
            else if (userRole == "admin")
            {
                var admin = await _adminService.GetAdminByIdAsync(id);
                if (admin == null) return new JsonResult(new { success = false, message = "Admin not found." });

                if (!VerifyCurrentPassword(request.CurrentPassword, admin.Password))
                    return new JsonResult(new { success = false, message = "Current password is incorrect." });

                var hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                passwordUpdated = await _adminService.UpdatePasswordAsync(id, hashed);
                if (!passwordUpdated) errorMessage = "Failed to update password.";
            }
            else
                return new JsonResult(new { success = false, message = "Invalid user role." });

            if (passwordUpdated)
                {
                    TempData["ShowSuccessModal"] = true;
                    // TempData["SuccessMessage"] = "Password updated successfully!";
                    return new JsonResult(new { success = true, message = "Password updated successfully!", reload = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = errorMessage ?? "An error occurred." });
                }
        }

        private async Task<string?> HandleProfilePictureUpload()
        {
            var file = Request.Form.Files.GetFile("profilePic");
            if (file == null || file.Length == 0)
                return null;

            // Validate file size (5MB limit)
            if (file.Length > 5 * 1024 * 1024)
                return null;

            // Validate file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

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