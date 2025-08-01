using System.Net.Mail;
using System.Net;

namespace suvarnyug.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _fromAddress;
        private readonly string _fromPassword;
        private readonly string _smtpHost;
        private readonly int _smtpPort;

        public EmailService(string fromAddress, string fromPassword, string smtpHost, int smtpPort)
        {
            _fromAddress = fromAddress;
            _fromPassword = fromPassword;
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
        }

        public bool SendEmail1(string toEmail, string ccEmail, string bccEmail, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress(_fromAddress, "Suvarnyug Support");
                var toAddress = new MailAddress(toEmail);

                var smtp = new SmtpClient
                {
                    Host = _smtpHost,
                    Port = _smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, _fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    if (!string.IsNullOrEmpty(ccEmail))
                    {
                        message.CC.Add(ccEmail);
                    }

                    if (!string.IsNullOrEmpty(bccEmail))
                    {
                        message.Bcc.Add(bccEmail);
                    }

                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
