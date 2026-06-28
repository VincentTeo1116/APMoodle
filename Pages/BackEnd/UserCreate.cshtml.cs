using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using System.ComponentModel.DataAnnotations;
// using System.Security.Cryptography;  // COMMENTED OUT - for hashing
// using System.Text;  // COMMENTED OUT - for hashing

namespace APMoodle.Pages.BackEnd
{
    public class UserCreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserCreateModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public string UserType { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must be 10 digits starting with 0")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Date of Birth is required")]
        public DateOnly? DOB { get; set; }

        [BindProperty]
        public string? Department { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile? ProfileImage { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public string InfoMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            InfoMessage = "Select a user type to begin creating a new user.";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Validate user type
                if (string.IsNullOrEmpty(UserType))
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = "Please select a user type.";
                    return Page();
                }

                if (UserType.ToLower() == "admin")
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = "Admin accounts cannot be created through this form.";
                    return Page();
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = "Please fix the validation errors.";
                    return Page();
                }

                // Validate phone number format
                if (!System.Text.RegularExpressions.Regex.IsMatch(PhoneNumber, @"^0\d{9}$"))
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = "Phone number must be 10 digits starting with 0.";
                    return Page();
                }

                // Validate age
                if (DOB.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var age = today.Year - DOB.Value.Year;
                    if (DOB.Value > today.AddYears(-age)) age--;
                    
                    if (age < 5 || age > 120)
                    {
                        TempData["ShowErrorModal"] = true;
                        TempData["ErrorMessage"] = "Date of birth must be between 5 and 120 years old.";
                        return Page();
                    }
                }

                // Check if email already exists
                var emailExists = await CheckEmailExistsAsync(Email);
                if (emailExists)
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = $"Email '{Email}' is already registered in the system.";
                    return Page();
                }

                // COMMENTED OUT - Password hashing disabled
                // string hashedPassword = HashPassword(Password);
                // Using plain text password instead
                string plainPassword = Password;

                // Handle image upload
                string imageUrl = null;
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

                    imageUrl = $"/uploads/profiles/{fileName}";
                }

                bool created = false;
                string userCode = string.Empty;
                string userName = string.Empty;

                switch (UserType.ToLower())
                {
                    case "student":
                        var student = new Student
                        {
                            Name = Name,
                            Email = Email,
                            PhoneNumber = PhoneNumber,
                            Gender = Gender,
                            DOB = DOB ?? DateOnly.FromDateTime(DateTime.Now.AddYears(-18)),
                            Status = "Pending",
                            Password = plainPassword,
                            RegisteredDate = DateTime.UtcNow,
                            ProfilePic = imageUrl
                        };

                        student.StudentCode = await GenerateUniqueStudentCodeAsync();

                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();
                        created = true;
                        userCode = student.StudentCode;
                        userName = student.Name;
                        break;

                    case "lecturer":
                        if (string.IsNullOrEmpty(Department))
                        {
                            TempData["ShowErrorModal"] = true;
                            TempData["ErrorMessage"] = "Department is required for lecturers.";
                            return Page();
                        }

                        var lecturer = new Lecturer
                        {
                            Name = Name,
                            Email = Email,
                            PhoneNumber = PhoneNumber,
                            Gender = Gender,
                            DOB = DOB ?? DateOnly.FromDateTime(DateTime.Now.AddYears(-18)),
                            Status = "Pending",
                            Password = plainPassword,
                            RegisteredDate = DateTime.UtcNow,
                            Department = Department,
                            ProfilePic = imageUrl
                        };

                        lecturer.LecturerCode = await GenerateUniqueLecturerCodeAsync();

                        _context.Lecturers.Add(lecturer);
                        await _context.SaveChangesAsync();
                        created = true;
                        userCode = lecturer.LecturerCode;
                        userName = lecturer.Name;
                        break;

                    default:
                        TempData["ShowErrorModal"] = true;
                        TempData["ErrorMessage"] = "Invalid user type selected.";
                        return Page();
                }

                if (created)
                {
                    TempData["ShowSuccessModal"] = true;
                    TempData["SuccessMessage"] = $"User '{userName}' has been created successfully as a {UserType} with code: {userCode}";
                    
                    // Clear the form
                    ModelState.Clear();
                    UserType = string.Empty;
                    Name = string.Empty;
                    Email = string.Empty;
                    PhoneNumber = string.Empty;
                    Gender = string.Empty;
                    DOB = null;
                    Department = null;
                    Password = string.Empty;
                    ConfirmPassword = string.Empty;
                    ProfileImage = null;
                    InfoMessage = "User created successfully! You can create another one below.";
                    
                    return Page();
                }

                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = "Failed to create user. Please try again.";
                return Page();
            }
            catch (DbUpdateException ex)
            {
                // Check if it's a duplicate key error (PostgreSQL error code 23505)
                if (ex.InnerException?.Message?.Contains("23505") == true || 
                    ex.Message?.Contains("23505") == true ||
                    ex.InnerException?.Message?.Contains("duplicate key") == true)
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = "A user with this code already exists. Please try again.";
                }
                else
                {
                    TempData["ShowErrorModal"] = true;
                    TempData["ErrorMessage"] = $"Database error: {ex.InnerException?.Message ?? ex.Message}";
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ShowErrorModal"] = true;
                TempData["ErrorMessage"] = $"Error creating user: {ex.Message}";
                return Page();
            }
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Email == email);
            var lecturerExists = await _context.Lecturers.AnyAsync(l => l.Email == email);
            var adminExists = await _context.Admins.AnyAsync(a => a.Email == email);

            return studentExists || lecturerExists || adminExists;
        }

        private async Task<string> GenerateUniqueStudentCodeAsync()
        {
            // Start from 1
            int nextNumber = 1;
            string code = $"ST{nextNumber:D5}";  // Format: ST00001
            bool exists = true;
            
            // Keep checking until we find a unique code
            while (exists)
            {
                // Check if this code already exists
                exists = await _context.Students.AnyAsync(s => s.StudentCode == code);
                
                if (exists)
                {
                    nextNumber++;
                    code = $"ST{nextNumber:D5}";
                }
            }
            
            return code;
        }

        private async Task<string> GenerateUniqueLecturerCodeAsync()
        {
            // Start from 1
            int nextNumber = 1;
            string code = $"LT{nextNumber:D5}";  // Format: LT00001
            bool exists = true;
            
            // Keep checking until we find a unique code
            while (exists)
            {
                // Check if this code already exists
                exists = await _context.Lecturers.AnyAsync(l => l.LecturerCode == code);
                
                if (exists)
                {
                    nextNumber++;
                    code = $"LT{nextNumber:D5}";
                }
            }
            
            return code;
        }

        // COMMENTED OUT - Password hashing method disabled
        /*
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        */
    }
}