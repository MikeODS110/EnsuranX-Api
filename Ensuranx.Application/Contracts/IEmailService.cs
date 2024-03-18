using Ensuranx.Application.Requests.Mail;

namespace Ensuranx.Application.Contracts
{
    public interface IEmailService
    {
        Task<bool> SendAsync(MailData mailData, CancellationToken ct);
        //public  Task SendPasswordResetEmail(string recipientEmail, string resetLink);
    }
}
