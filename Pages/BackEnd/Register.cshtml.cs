using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using APMoodle.Models;
using APMoodle.Services.Interfaces;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace APMoodle.Pages.BackEnd
{
    public class RegisterModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IEmailService _emailService;
        private readonly ILecturerService _lecturerService;
        private readonly IAdminService _adminService;

        public RegisterModel(
            IStudentService studentService,
            IEmailService emailService,
            ILecturerService lecturerService,
            IAdminService adminService)
        {
            _studentService = studentService;
            _emailService = emailService;
            _lecturerService = lecturerService;
            _adminService = adminService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Date of birth is required")]
        public DateOnly? DOB { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Phone number must be 10-11 digits starting with 0")]
        public string Contact { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public bool ShowPendingMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                Message = "Please fix the validation errors.";
                IsSuccess = false;
                return Page();
            }

            // Phone format (extra check)
            if (!Regex.IsMatch(Contact, @"^0\d{9,10}$"))
            {
                Message = "Phone number must be 10-11 digits starting with 0.";
                IsSuccess = false;
                return Page();
            }

            // DOB range
            if (DOB.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - DOB.Value.Year;
                if (DOB.Value > today.AddYears(-age)) age--;
                if (age < 15 || age > 80)
                {
                    Message = "Date of birth must be between 15 and 80 years old.";
                    IsSuccess = false;
                    return Page();
                }
            }

            // Email uniqueness
            if (await EmailExistsAsync(Email))
            {
                Message = "Email already registered. Please use a different email or login.";
                IsSuccess = false;
                return Page();
            }

            // Generate student code
            var studentCode = await _studentService.GenerateNextStudentCodeAsync();
            if (string.IsNullOrEmpty(studentCode))
            {
                Message = "Unable to generate a unique student code. Please try again.";
                IsSuccess = false;
                return Page();
            }

            // Generate & hash password
            var password = $"{studentCode}_{DOB:ddMMyyyy}@";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Create student
            var student = new Student
            {
                Name = FullName,
                Email = Email,
                PhoneNumber = Contact,
                Gender = Gender,
                DOB = DOB ?? DateOnly.FromDateTime(DateTime.Today),
                StudentCode = studentCode,
                Password = hashedPassword,
                Status = "Pending",
                RegisteredDate = DateTime.UtcNow
            };

            var success = await _studentService.CreateStudentAsync(student);
            if (!success)
            {
                Message = "Registration failed. Please try again later.";
                IsSuccess = false;
                return Page();
            }

            // Send welcome email
            await SendWelcomeEmailAsync(student.Email, student.Name, student.StudentCode, password, student.DOB, student.PhoneNumber, student.Gender);

            ShowPendingMessage = true;
            Message = "Registration successful! Your account is pending approval. Login credentials have been sent to your email.";
            IsSuccess = true;

            // Clear form
            ModelState.Clear();
            FullName = string.Empty;
            Email = string.Empty;
            Contact = string.Empty;
            Gender = string.Empty;
            DOB = null;

            return Page();
        }

        private async Task<bool> EmailExistsAsync(string email)
        {
            if (await _studentService.StudentExistsAsync(email)) return true;
            if (await _lecturerService.LecturerExistsAsync(email)) return true;
            var admin = await _adminService.GetAdminByEmailAsync(email);
            return admin != null;
        }

        private async Task SendWelcomeEmailAsync(string email, string name, string code, string password, DateOnly dob, string phone, string gender)
        {
            try
            {
                string subject = "Welcome to APMoodle – Registration Received (Pending Approval)";
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
                            .password-box {{ background: #ffffff; border: 1px dashed #94a3b8; padding: 8px 8px; border-radius: 8px; font-family: monospace; font-size: 14px; letter-spacing: 0.5px; margin: 4px 0; display: inline-block; color: #1e293b; }}
                            .footer {{ background: #f8fafc; padding: 16px 20px; text-align: center; font-size: 13px; color: #94a3b8; border-top: 1px solid #e9edf2; }}
                            .btn {{ display: inline-block; background: linear-gradient(135deg, #ceceff, #b0b0ff); color: #474783; padding: 12px 28px; border-radius: 40px; text-decoration: none; font-weight: 600; font-size: 15px; margin-top: 8px; transition: 0.2s; }}
                            .btn:hover {{ transform: translateY(-2px); box-shadow: 0 4px 14px rgba(79,70,229,0.35); }}
                            .warning {{ font-size: 13px; color: #ef4444; margin-top: 12px; padding: 12px; background: #fef2f2; border-radius: 8px; }}
                            .pending-box {{ background: #fef3c7; border-radius: 12px; padding: 16px; text-align: center; margin-top: 20px; }}
                            .pending-box i {{ font-size: 24px; color: #f59e0b; }}
                            .pending-box span {{ font-weight: 600; color: #92400e; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎓 APMoodle</h1>
                                <p>Registration Received</p>
                            </div>
                            <div class='content'>
                                <h2>Hello {name} 👋</h2>
                                <p>Thank you for registering with <strong>APMoodle</strong>.</p>
                                <div class='details'>
                                    <div class='details-row'>
                                        <span class='details-label'>👤 Student Code</span>
                                        <span class='details-value'>{code}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>✉️ Email</span>
                                        <span class='details-value'>{email}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>📞 Phone</span>
                                        <span class='details-value'>{phone}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>🎂 Date of Birth</span>
                                        <span class='details-value'>{dob:dd MMM yyyy}</span>
                                    </div>
                                    <div class='details-row'>
                                        <span class='details-label'>⚥ Gender</span>
                                        <span class='details-value'>{gender}</span>
                                    </div>
                                </div>
                                <div class='credentials'>
                                    <p><strong>🔐 Your login credentials</strong></p>
                                    <p>Email: <strong>{email}</strong></p>
                                    <p>Password: <span class='password-box'>{password}</span></p>
                                    <p style='margin-top: 10px; font-size: 13px; color: #8b6464;'>Please change your password after your first login.</p>
                                </div>
                                <div class='pending-box'>
                                    <i class='bi bi-clock-history'></i>
                                    <span>Your account is <strong>pending approval</strong>.</span>
                                    <p style='margin: 8px 0 0; font-size: 14px; color: #92400e;'>
                                        You will receive a confirmation email once your account is activated by the admin.
                                    </p>
                                </div>
                                <div style='text-align: center; margin: 20px 0 8px;'>
                                    <a href='https://apmoodle.onrender.com/FrontEnd/Login' class='btn'>🗝️ Login Now</a>
                                </div>
                                <div class='warning'>
                                    ⚠️ If you did not request this registration, please ignore this email.
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
                // Log but don't block registration
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
            }
        }
    }
}