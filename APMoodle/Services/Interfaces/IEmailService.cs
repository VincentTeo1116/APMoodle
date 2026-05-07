namespace APMoodle.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendRegistrationPendingEmailAsync(string toEmail, string userName);
        Task<bool> SendAccountApprovedEmailAsync(string toEmail, string userName, string password);
        Task<bool> SendCredentialsEmailAsync(string toEmail, string userName, string password, string role);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
    }
}