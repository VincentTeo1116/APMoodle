using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using APMoodle.Services.Interfaces;

namespace APMoodle.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Generic email send method
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Get email settings with null checks and defaults
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortStr = _configuration["EmailSettings:SmtpPort"];

                // Check if email is configured
                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpServer))
                {
                    _logger.LogWarning("Email not configured. Would send to: {ToEmail}, Subject: {Subject}", toEmail, subject);
                    return true; // Return true to not block registration
                }

                // Parse port with safe default
                int smtpPort = 587;
                if (!string.IsNullOrEmpty(smtpPortStr))
                {
                    int.TryParse(smtpPortStr, out smtpPort);
                }

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(smtpUser));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                
                // Only authenticate if password is provided
                if (!string.IsNullOrEmpty(smtpPassword))
                {
                    await smtp.AuthenticateAsync(smtpUser, smtpPassword);
                }
                
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                return false;
            }
        }

        // Email: Registration pending approval
        public async Task<bool> SendRegistrationPendingEmailAsync(string toEmail, string userName)
        {
            string subject = "Registration Received - Pending Approval";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #ffc107; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Registration Received</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {userName},</p>
                            <p>Thank you for registering with <strong>APMoodle</strong>!</p>
                            <p>Your registration has been received and is currently <strong>pending approval</strong> by our admin team.</p>
                            <p>You will receive another email once your account has been approved.</p>
                            <br/>
                            <p>Thank you for your patience.</p>
                            <p>Best regards,<br/>APMoodle Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 APMoodle. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }

        // Email: Account approved
        public async Task<bool> SendAccountApprovedEmailAsync(string toEmail, string userName, string password)
        {
            string subject = "Account Approved - Welcome to APMoodle";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #28a745; padding: 10px; text-align: center; color: white; }}
                        .content {{ padding: 20px; }}
                        .credentials {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Account Approved!</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {userName},</p>
                            <p>Great news! Your account has been <strong>approved</strong> by the admin.</p>
                            <div class='credentials'>
                                <p><strong>Your Login Credentials:</strong></p>
                                <p>📧 Email: {toEmail}</p>
                                <p>🔑 Password: {password}</p>
                            </div>
                            <p><strong>Please change your password after your first login.</strong></p>
                            <a href='https://localhost:5001/Login' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Login Now</a>
                            <br/><br/>
                            <p>Best regards,<br/>APMoodle Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 APMoodle. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }

        // Email: Account rejected
        public async Task<bool> SendAccountRejectedEmailAsync(string toEmail, string userName, string reason)
        {
            string subject = "Account Rejected - APMoodle";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #dc3545; padding: 10px; text-align: center; color: white; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Account Rejected</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {userName},</p>
                            <p>We regret to inform you that your account request has been <strong>rejected</strong>.</p>
                            <p><strong>Reason:</strong> {reason}</p>
                            <p>Please contact the admin for further assistance.</p>
                            <p>Best regards,<br/>APMoodle Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 APMoodle. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }

        // Email: Send credentials (when admin creates account)
        public async Task<bool> SendCredentialsEmailAsync(string toEmail, string userName, string password, string role)
        {
            string subject = $"Welcome to APMoodle - Your {role} Account";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; padding: 10px; text-align: center; color: white; }}
                        .content {{ padding: 20px; }}
                        .credentials {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Welcome to APMoodle!</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {userName},</p>
                            <p>An account has been created for you on <strong>APMoodle</strong> as a <strong>{role}</strong>.</p>
                            <div class='credentials'>
                                <p><strong>Your Login Credentials:</strong></p>
                                <p>📧 Email: {toEmail}</p>
                                <p>🔑 Password: {password}</p>
                            </div>
                            <p><strong>Please change your password after you login.</strong></p>
                            <a href='https://localhost:5001/Login' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Login Now</a>
                            <br/><br/>
                            <p>Best regards,<br/>APMoodle Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 APMoodle. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }

        // Email: Password reset
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            string subject = "Password Reset Request - APMoodle";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #dc3545; padding: 10px; text-align: center; color: white; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Password Reset Request</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {userName},</p>
                            <p>We received a request to reset your password for your APMoodle account.</p>
                            <p>Click the button below to reset your password:</p>
                            <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a>
                            <p>This link will expire in 1 hour.</p>
                            <p>If you did not request this, please ignore this email.</p>
                            <p>Best regards,<br/>APMoodle Team</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 APMoodle. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}