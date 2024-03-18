using Ensuranx.Application.Requests.Mail;

namespace Ensuranx.Infrastructure.Mappers
{
    public class MailMapper
    {
        public MailData SetDataInMailModel(string to,string subject,string body,string from)
        {
            List<string> tos = new List<string>();
            tos.Add(to);
            MailData mailData = new MailData(tos, subject, body, from);

            return mailData;
        }
    }
}
