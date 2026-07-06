using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using System.ComponentModel.DataAnnotations;

namespace APMoodle.Pages.BackEnd
{
    public class UserEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserEditModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public int UserId { get; set; }

        [BindProperty]
        public string UserType { get; set; } = string.Empty;

        [BindProperty]
        public string UserCode { get; set; } = string.Empty;

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [BindProperty]
        public string Gender { get; set; } = string.Empty;

        [BindProperty]
        public DateOnly? DOB { get; set; }

        [BindProperty]
        public string Status { get; set; } = string.Empty;

        [BindProperty]
        public string? Department { get; set; }

        [BindProperty]
        public string? ProfilePic { get; set; }

        [BindProperty]
        public string? CurrentProfilePic { get; set; }

        [BindProperty]
        public IFormFile? ProfileImage { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string AvatarColor { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id, string type)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(type))
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = "Invalid user ID or type.";
                    return RedirectToPage("/FrontEnd/UserList");
                }

                UserId = id;
                UserType = type;

                switch (type?.ToLower())
                {
                    case "student":
                        var student = await _context.Students
                            .FirstOrDefaultAsync(s => s.StudentID == id);
                        
                        if (student == null)
                        {
                            TempData["ShowErrorModal"] = true;
                            TempData["ErrorMessage"] = "Student not found.";
                            return RedirectToPage("/FrontEnd/UserList");
                        }

                        UserCode = student.StudentCode;
                        Name = student.Name;
                        Email = student.Email;
                        PhoneNumber = student.PhoneNumber;
                        Gender = student.Gender;
                        DOB = student.DOB;
                        Status = student.Status;
                        ProfilePic = student.ProfilePic;
                        CurrentProfilePic = student.ProfilePic;
                        UserName = student.Name;
                        Department = null;
                        break;

                    case "lecturer":
                        var lecturer = await _context.Lecturers
                            .FirstOrDefaultAsync(l => l.LecturerID == id);
                        
                        if (lecturer == null)
                        {
                            TempData["ShowErrorModal"] = true;
                            TempData["ErrorMessage"] = "Lecturer not found.";
                            return RedirectToPage("/FrontEnd/UserList");
                        }

                        UserCode = lecturer.LecturerCode;
                        Name = lecturer.Name;
                        Email = lecturer.Email;
                        PhoneNumber = lecturer.PhoneNumber;
                        Gender = lecturer.Gender;
                        DOB = lecturer.DOB;
                        Status = lecturer.Status;
                        ProfilePic = lecturer.ProfilePic;
                        CurrentProfilePic = lecturer.ProfilePic;
                        UserName = lecturer.Name;
                        Department = lecturer.Department;
                        break;

                    case "admin":
                        var admin = await _context.Admins
                            .FirstOrDefaultAsync(a => a.AdminID == id);
                        
                        if (admin == null)
                        {
                            TempData["ShowErrorModal"] = true;
                            TempData["ErrorMessage"] = "Admin not found.";
                            return RedirectToPage("/FrontEnd/UserList");
                        }

                        UserCode = admin.AdminCode;
                        Name = admin.Name;
                        Email = admin.Email;
                        PhoneNumber = admin.PhoneNumber;
                        Gender = admin.Gender;
                        DOB = admin.DOB;
                        Status = admin.Status;
                        ProfilePic = admin.ProfilePic;
                        CurrentProfilePic = admin.ProfilePic;
                        UserName = admin.Name;
                        Department = null;
                        break;

                    default:
                        TempData["ShowErrorModal"] = true;
                        TempData["ErrorMessage"] = "Invalid user type.";
                        return RedirectToPage("/FrontEnd/UserList");
                }

                AvatarColor = GetAvatarColor(type);
                Initials = GetInitials(Name);

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = $"Error loading user: {ex.Message}";
                return RedirectToPage("/FrontEnd/UserList");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = "Please fix the validation errors.";
                    return Page();
                }

                // Handle image upload
                string imageUrl = CurrentProfilePic;

                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
                    var extension = Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ShowErrorModal"] = true;
                        TempData["ErrorMessage"] = "Invalid file format. Please upload PNG, JPG, JPEG, GIF, or WEBP images only.";
                        return Page();
                    }

                    if (ProfileImage.Length > 5 * 1024 * 1024)
                    {
                        TempData["ShowErrorModal"] = true;
                        TempData["ErrorMessage"] = "File size must be less than 5MB.";
                        return Page();
                    }

                    string fileName = $"{Guid.NewGuid()}{extension}";
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(fileStream);
                    }

                    if (!string.IsNullOrEmpty(CurrentProfilePic) && CurrentProfilePic.Contains("/uploads/profiles/"))
                    {
                        var oldFileName = CurrentProfilePic.Split('/').Last();
                        var oldFilePath = Path.Combine(uploadFolder, oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    imageUrl = $"/uploads/profiles/{fileName}";
                }
                else if (string.IsNullOrEmpty(CurrentProfilePic) && string.IsNullOrEmpty(ProfilePic))
                {
                    imageUrl = null;
                }
                else if (!string.IsNullOrEmpty(ProfilePic) && ProfilePic != CurrentProfilePic)
                {
                    imageUrl = ProfilePic;
                }

                bool updated = false;
                string userName = string.Empty;

                switch (UserType?.ToLower())
                {
                    case "student":
                        var student = await _context.Students
                            .FirstOrDefaultAsync(s => s.StudentID == UserId);
                        
                        if (student == null)
                        {
                            TempData["ShowErrorModal"] = true;
                            TempData["ErrorMessage"] = "Student not found.";
                            return Page();
                        }

                        student.Name = Name;
                        student.Email = Email;
                        student.PhoneNumber = PhoneNumber;
                        student.Gender = Gender;
                        student.DOB = DOB ?? student.DOB;
                        student.Status = Status;
                        student.ProfilePic = imageUrl;

                        _context.Students.Update(student);
                        await _context.SaveChangesAsync();
                        updated = true;
                        userName = student.Name;
                        break;

                    case "lecturer":
                        var lecturer = await _context.Lecturers
                            .FirstOrDefaultAsync(l => l.LecturerID == UserId);
                        
                        if (lecturer == null)
                        {
                            TempData["ShowErrorModal"] = true;
                            TempData["ErrorMessage"] = "Lecturer not found.";
                            return Page();
                        }

                        lecturer.Name = Name;
                        lecturer.Email = Email;
                        lecturer.PhoneNumber = PhoneNumber;
                        lecturer.Gender = Gender;
                        lecturer.DOB = DOB ?? lecturer.DOB;
                        lecturer.Status = Status;
                        lecturer.ProfilePic = imageUrl;
                        lecturer.Department = Department ?? lecturer.Department;

                        _context.Lecturers.Update(lecturer);
                        await _context.SaveChangesAsync();
                        updated = true;
                        userName = lecturer.Name;
                        break;

                    case "admin":
                        var admin = await _context.Admins
                            .FirstOrDefaultAsync(a => a.AdminID == UserId);
                        
                        if (admin == null)
                        {
                            TempData["ShowErrorModal"] = true;
                            TempData["ErrorMessage"] = "Admin not found.";
                            return Page();
                        }

                        admin.Name = Name;
                        admin.Email = Email;
                        admin.PhoneNumber = PhoneNumber;
                        admin.Gender = Gender;
                        admin.DOB = DOB ?? admin.DOB;
                        admin.Status = Status;
                        admin.ProfilePic = imageUrl;

                        _context.Admins.Update(admin);
                        await _context.SaveChangesAsync();
                        updated = true;
                        userName = admin.Name;
                        break;

                    default:
                        TempData["ShowErrorModal"] = true;
                        TempData["ErrorMessage"] = "Invalid user type.";
                        return Page();
                }

                if (updated)
                {
                    TempData["ShowSuccessModal"] = true;
                    TempData["SuccessMessage"] = $"User '{userName}' has been updated successfully.";
                    return Page();
                }

                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = "No changes were made.";
                return Page();
            }
            catch (DbUpdateException ex)
            {
                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = $"Database error: {ex.InnerException?.Message ?? ex.Message}";
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
                return Page();
            }
        }

        private string GetAvatarColor(string role)
        {
            return role?.ToLower() switch
            {
                "student" => "#3b82f6",
                "lecturer" => "#f59e0b",
                "admin" => "#ef4444",
                _ => "#64748b"
            };
        }

        private string GetInitials(string name)
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
}