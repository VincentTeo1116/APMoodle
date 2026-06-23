using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using APMoodle.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace APMoodle.Pages.BackEnd
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly ILecturerService _lecturerService;
        private readonly IAdminService _adminService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public ForgotPasswordModel(
            IStudentService studentService,
            ILecturerService lecturerService,
            IAdminService adminService,
            IEmailService emailService,
            IMemoryCache cache,
            IConfiguration config)
        {
            _studentService = studentService;
            _lecturerService = lecturerService;
            _adminService = adminService;
            _emailService = emailService;
            _cache = cache;
            _config = config;
        }

        [BindProperty]
        public ForgotPasswordInput Input { get; set; } = new();

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsOtpSent { get; set; }
        public string? Email { get; set; }

        public class ForgotPasswordInput
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "OTP is required")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
            public string Otp { get; set; } = string.Empty;

            [Required(ErrorMessage = "New password is required")]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please confirm your password")]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // Clear any previous state
            IsOtpSent = false;
            Message = null;

            ViewData["RecaptchaSiteKey"] = _config["Recaptcha:SiteKey"];
        }

        public async Task<IActionResult> OnPostSendOtpAsync()
        {
            // Validate email
            if (string.IsNullOrEmpty(Input.Email) || !IsValidEmail(Input.Email))
            {
                Message = "Please enter a valid email address.";
                return Page();
            }

            // Check if email exists in any user table
            var user = await FindUserByEmailAsync(Input.Email);
            if (user == null)
            {
                // For security, don't reveal if email exists or not
                Message = "If the email exists, an OTP has been sent. Please check your inbox.";
                IsOtpSent = true;
                Email = Input.Email; // Still store email for next step
                return Page();
            }

            // Verify reCAPTCHA
            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            var isValidCaptcha = await VerifyRecaptchaAsync(recaptchaResponse);
            if (!isValidCaptcha)
            {
                Message = "Please complete the CAPTCHA.";
                return Page();
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Store OTP in cache with expiration (5 minutes)
            var cacheKey = $"OTP_{Input.Email}";
            _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            // Send email
            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password for APMoodle.</p>
                <p>Your One-Time Password (OTP) is: <strong>{otp}</strong></p>
                <p>This OTP is valid for 5 minutes.</p>
                <p>If you did not request this, please ignore this email.</p>
            ";

            try
            {
                await _emailService.SendEmailAsync(Input.Email, "APMoodle - Password Reset OTP", emailBody);
                Message = "OTP sent successfully! Please check your email.";
                IsOtpSent = true;
                Email = Input.Email;
            }
            catch (Exception ex)
            {
                Message = "Failed to send OTP. Please try again later.";
                // Log error
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            if (string.IsNullOrEmpty(Input.Otp) || Input.Otp.Length != 6)
            {
                Message = "Please enter a valid 6-digit OTP.";
                return Page();
            }

            var cacheKey = $"OTP_{Input.Email}";
            if (!_cache.TryGetValue(cacheKey, out string? storedOtp))
            {
                Message = "OTP has expired or is invalid. Please request a new one.";
                return Page();
            }

            if (storedOtp != Input.Otp)
            {
                Message = "Invalid OTP. Please try again.";
                return Page();
            }

            var user = await FindUserByEmailAsync(Input.Email);
            if (user == null)
            {
                Message = "User not found. Please try again.";
                return Page();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
            bool updated = false;

            // Deconstruct the tuple
            var (role, userId, _) = user.Value;

            switch (role)
            {
                case "student":
                    updated = await _studentService.UpdatePasswordAsync(userId, hashedPassword);
                    break;
                case "lecturer":
                    updated = await _lecturerService.UpdatePasswordAsync(userId, hashedPassword);
                    break;
                case "admin":
                    updated = await _adminService.UpdatePasswordAsync(userId, hashedPassword);
                    break;
            }

            if (updated)
            {
                _cache.Remove(cacheKey);
                Message = "Password has been reset successfully! You can now login.";
                IsSuccess = true;
            }
            else
            {
                Message = "Failed to reset password. Please try again.";
            }

            return Page();
        }

        // Helper methods
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<(string? Role, int Id, string? PasswordHash)?> FindUserByEmailAsync(string email)
        {
            // Check Students
            var student = await _studentService.GetStudentByEmailAsync(email);
            if (student != null && student.Status == "Active")
                return ("student", student.StudentID, student.Password);

            // Check Lecturers
            var lecturer = await _lecturerService.GetLecturerByEmailAsync(email);
            if (lecturer != null && lecturer.Status == "Active")
                return ("lecturer", lecturer.LecturerID, lecturer.Password);

            // Check Admins
            var admin = await _adminService.GetAdminByEmailAsync(email);
            if (admin != null)
                return ("admin", admin.AdminID, admin.Password);

            return null;
        }

        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            using var client = new HttpClient();
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"secret", _config["Recaptcha:SecretKey"]},
                    {"response", token}
                })
            );
            var json = await response.Content.ReadAsStringAsync();
            dynamic? result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return result?.success == true;
        }
    }
}