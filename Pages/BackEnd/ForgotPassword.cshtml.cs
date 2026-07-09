using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using APMoodle.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            IStudentService studentService,
            ILecturerService lecturerService,
            IAdminService adminService,
            IEmailService emailService,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<ForgotPasswordModel> logger)
        {
            _studentService = studentService;
            _lecturerService = lecturerService;
            _adminService = adminService;
            _emailService = emailService;
            _cache = cache;
            _config = config;
            _logger = logger;
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
            IsOtpSent = false;
            Message = null;
            IsSuccess = false;
            ViewData["RecaptchaSiteKey"] = _config["Recaptcha:SiteKey"];
        }

        public async Task<IActionResult> OnPostSendOtpAsync()
        {
            if (string.IsNullOrEmpty(Input.Email) || !IsValidEmail(Input.Email))
            {
                Message = "Please enter a valid email address.";
                return Page();
            }

            var user = await FindUserByEmailAsync(Input.Email);
            if (user == null)
            {
                Message = "If the email exists, an OTP has been sent. Please check your inbox.";
                IsOtpSent = true;
                Email = Input.Email;
                return Page();
            }

            if (Request.Host.Host != "localhost" && Request.Host.Host != "127.0.0.1")
            {
                var recaptchaResponse = Request.Form["g-recaptcha-response"];
                if (string.IsNullOrEmpty(recaptchaResponse) || !await VerifyRecaptchaAsync(recaptchaResponse))
                {
                    Message = "Please complete the CAPTCHA.";
                    return Page();
                }
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"OTP_{Input.Email}";
            _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password for APMoodle.</p>
                <p>Your One-Time Password (OTP) is: <strong>{otp}</strong></p>
                <p>This OTP is valid for 5 minutes.</p>
                <p>If you did not request this, please ignore this email.</p>
            ";

            try
            {
                var sent = await _emailService.SendEmailAsync(Input.Email, "APMoodle - Password Reset OTP", emailBody);
                if (!sent)
                {
                    Message = "Failed to send OTP. Please check your email configuration.";
                    return Page();
                }
                Message = "OTP sent successfully! Please check your email.";
                IsOtpSent = true;
                Email = Input.Email;
            }
            catch (Exception ex)
            {
                Message = "Failed to send OTP. Please try again later.";
                _logger.LogError(ex, "Error sending OTP");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostVerifyOtpAsync([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
                return new JsonResult(new { success = false, message = "Invalid request." });

            var cacheKey = $"OTP_{request.Email}";
            if (!_cache.TryGetValue(cacheKey, out string? storedOtp) || storedOtp != request.Otp)
                return new JsonResult(new { success = false, message = "Invalid or expired OTP." });

            _cache.Set(cacheKey, storedOtp, TimeSpan.FromMinutes(10));
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            // Validate OTP again
            var cacheKey = $"OTP_{Input.Email}";
            if (!_cache.TryGetValue(cacheKey, out string? storedOtp) || storedOtp != Input.Otp)
            {
                Message = "OTP has expired or is invalid. Please request a new one.";
                return Page();
            }

            var user = await FindUserByEmailAsync(Input.Email);
            if (user == null)
            {
                Message = "User not found.";
                return Page();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
            bool updated = false;
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
                Message = "Password has been reset successfully! You will be redirected to login.";
                IsSuccess = true;

                // Send notification email
                await _emailService.SendEmailAsync(
                    Input.Email,
                    "Password Reset Successful",
                    $@"
                        <h2>Password Reset Notification</h2>
                        <p>Your APMoodle password has been reset successfully.</p>
                        <p>If you did not perform this action, please contact support immediately.</p>
                    "
                );
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
            if (admin != null && admin.Status == "Active")
                return ("admin", admin.AdminID, admin.Password);

            return null;
        }

        private async Task<bool> VerifyRecaptchaAsync(string token)
        {
            var secretKey = _config["Recaptcha:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("reCAPTCHA secret key is not configured.");
                return false;
            }

            using var client = new HttpClient();
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"secret", secretKey},
                    {"response", token}
                })
            );
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("reCAPTCHA response: {Response}", json);
            
            dynamic? result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return result?.success == true;
        }
            public class VerifyOtpRequest
            {
                public string Email { get; set; } = "";
                public string Otp { get; set; } = "";
            }
    }
}