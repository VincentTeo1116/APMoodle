using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using APMoodle.Services.Interfaces;

namespace APMoodle.Pages.BackEnd
{
    public class UserCreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailService _emailService;

        public UserCreateModel(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IEmailService emailService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService; 
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
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Phone number must be 10 - 11 digits starting with 0")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Date of Birth is required")]
        public DateOnly? DOB { get; set; }

        [BindProperty]
        public string? Department { get; set; }
        public bool ShowSuccessModal { get; set; }

        // [BindProperty]
        // [Required(ErrorMessage = "Password is required")]
        // [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        // public string Password { get; set; } = string.Empty;

        // [BindProperty]
        // [Required(ErrorMessage = "Please confirm your password")]
        // [Compare("Password", ErrorMessage = "Passwords do not match")]
        // public string ConfirmPassword { get; set; } = string.Empty;

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
                if (!System.Text.RegularExpressions.Regex.IsMatch(PhoneNumber, @"^0\d{9,10}$"))
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

                // string hashedPassword = HashPassword(Password);

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
                string generatedPassword = string.Empty;

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
                            Status = "Active",
                            // Password = hashedPassword,
                            RegisteredDate = DateTime.UtcNow,
                            ProfilePic = imageUrl
                        };

                        student.StudentCode = await GenerateUniqueStudentCodeAsync();

                        created = true;
                        userCode = student.StudentCode;
                        userName = student.Name;
                        generatedPassword = GeneratePassword(userCode, student.DOB);
                        student.Password = HashPassword(generatedPassword);
                        _context.Students.Add(student);
                        
                        await _context.SaveChangesAsync();
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
                            Status = "Active",
                            // Password = hashedPassword,
                            RegisteredDate = DateTime.UtcNow,
                            Department = Department,
                            ProfilePic = imageUrl
                        };

                        lecturer.LecturerCode = await GenerateUniqueLecturerCodeAsync();

                        created = true;
                        userCode = lecturer.LecturerCode;
                        userName = lecturer.Name;
                        generatedPassword = GeneratePassword(userCode, lecturer.DOB);
                        lecturer.Password = HashPassword(generatedPassword);
                        _context.Lecturers.Add(lecturer);
                        
                        await _context.SaveChangesAsync();
                        break;

                    default:
                        TempData["ShowErrorModal"] = true;
                        TempData["ErrorMessage"] = "Invalid user type selected.";
                        return Page();
                }

                if (created)
                {
                    await SendWelcomeEmailAsync(Email, userName, userCode, generatedPassword, UserType, DOB ?? DateOnly.FromDateTime(DateTime.Now), PhoneNumber, Gender, Department);
                    
                    ShowSuccessModal = true;  
                    TempData["ShowSuccessModal"] = true;
                    TempData["GeneratedPassword"] = generatedPassword;
                    TempData["SuccessMessage"] = $"User '{userName}' has been created successfully as a {UserType} with code: {userCode}\n Password: {generatedPassword}";
                    
                    // Clear the form
                    ModelState.Clear();
                    UserType = string.Empty;
                    Name = string.Empty;
                    Email = string.Empty;
                    PhoneNumber = string.Empty;
                    Gender = string.Empty;
                    DOB = null;
                    Department = null;
                    // Password = string.Empty;
                    // ConfirmPassword = string.Empty;
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

        private string GeneratePassword(string userCode, DateOnly dob)
        {
            return $"{userCode}_{dob:ddMMyyyy}@";
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private async Task SendWelcomeEmailAsync(string email, string name, string code, string password, string role, DateOnly dob, string phone, string gender, string? department = null)
        {
            try
            {
                string subject = $"Welcome to APMoodle - Your {role} Account Created";
                string body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7fc; margin: 0; padding: 20px; }}
                            .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 16px; box-shadow: 0 8px 30px rgba(0,0,0,0.08); overflow: hidden; border: 1px solid #e9edf2; }}
                            .header {{ background: linear-gradient(135deg, #4f46e5, #6366f1); padding: 28px 20px; text-align: center; color: white; }}
                            .header h1 {{ margin: 0; font-weight: 700; font-size: 26px; letter-spacing: -0.3px; }}
                            .header p {{ margin: 6px 0 0; opacity: 0.9; font-size: 15px; }}
                            .content {{ padding: 30px 28px; }}
                            .content h2 {{ color: #1e293b; font-size: 22px; font-weight: 600; margin-top: 0; }}
                            .content p {{ color: #475569; line-height: 1.6; font-size: 15px; margin: 0 0 12px; }}
                            .details {{ background: #f8fafc; border-radius: 12px; padding: 16px 20px; margin: 16px 0; border: 1px solid #e9edf2; }}
                            .details-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e2e8f0; font-size: 14px; }}
                            .details-row:last-child {{ border-bottom: none; }}
                            .details-label {{ color: #64748b; font-weight: 500; }}
                            .details-value {{ color: #1e293b; font-weight: 600; }}
                            .credentials {{ background: #fef9e7; border-left: 4px solid #f59e0b; padding: 16px 20px; border-radius: 8px; margin: 20px 0; }}
                            .credentials p {{ margin: 4px 0; font-size: 14px; }}
                            .credentials strong {{ color: #1e293b; }}
                            .password-box {{ background: #ffffff; border: 1px dashed #94a3b8; padding: 8px 8px; border-radius: 8px; font-family: monospace; font-size: 14px; letter-spacing: 0.5px; margin: 4px 0; display: inline-block; color: #1e293b; }}
                            .footer {{ background: #f8fafc; padding: 16px 20px; text-align: center; font-size: 13px; color: #94a3b8; border-top: 1px solid #e9edf2; }}
                            .btn {{ display: inline-block; background: linear-gradient(135deg, #ceceff, #b0b0ff); color: #474783; padding: 12px 28px; border-radius: 40px; text-decoration: none; font-weight: 600; font-size: 15px; margin-top: 8px; transition: 0.2s; }}
                            .btn:hover {{ transform: translateY(-2px); box-shadow: 0 4px 14px rgba(79,70,229,0.35); }}
                            .warning {{ font-size: 13px; color: #ef4444; margin-top: 12px; padding: 12px; background: #fef2f2; border-radius: 8px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎓 APMoodle</h1>
                                <p>Your account has been created</p>
                            </div>
                            <div class='content'>
                                <h2>Hello {name} 👋</h2>
                                <p>An account has been created for you on <strong>APMoodle</strong> as a <strong>{role}</strong>.</p>
                                <div class='details'>
                                    <div class='details-row'>
                                        <span class='details-label'>👤 User ID: </span>
                                        <span class='details-value'> {code}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>✉️ Email: </span>
                                        <span class='details-value'> {email}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>📞 Phone: </span>
                                        <span class='details-value'> {phone}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>🎂 Date of Birth: </span>
                                        <span class='details-value'> {dob:dd MMM yyyy}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>♂️ Gender: </span>
                                        <span class='details-value'> {gender}</span>
                                    </div>
                                    {(role == "Lecturer" ? $"<div class='details-row'><span class='details-label'>🏬 Department: </span><span class='details-value'> {Department}</span></div>" : "")}
                                </div>
                                <div class='credentials'>
                                    <p><strong>🔐 Your login credentials</strong></p>
                                    <p>Email: <strong>{email}</strong></p>
                                    <p>Password: <span class='password-box'>{password}</span></p>
                                    <p style='margin-top: 10px; font-size: 13px; color: #8b6464;'>Please change your password after your first login.</p>
                                </div>
                                <div style='text-align: center; margin: 20px 0 8px;'>
                                    <a href='https://apmoodle.onrender.com/FrontEnd/Login' class='btn'>🗝️ Login Now</a>
                                </div>
                                <div class='warning'>
                                    ⚠️ This password is auto-generated. We recommend changing it immediately after logging in.
                                </div>
                            </div>
                            <div class='footer'>
                                &copy; {DateTime.Now.Year} APMoodle - Built with <span style='color:#ef4444;'>❤</span> for education.
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                await _emailService.SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
            }
        }
    }
}