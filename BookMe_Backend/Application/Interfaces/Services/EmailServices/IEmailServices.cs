namespace Application.Interfaces.Services.EmailServices
{
    public interface IEmailServices
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendVerificationEmailAsync(string toEmail, string verificationCode, int expirationTime);
    }
}
 