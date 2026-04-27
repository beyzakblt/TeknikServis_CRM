using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TeknikServis_CRM.Services
{
    public class EmailService
    {
        // Senin MailHelper'ındaki verilerle yapılandırıldı
        private const string FromEmail = "beyzakblt@gmail.com";
        private const string DisplayName = "Teknik CRM Destek";
        private const string FromPassword = "eqvm lqjb ubnm hdok"; // Uygulama şifren
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;

        public async Task<bool> SendEmailAsync(string targetEmail, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress(FromEmail, DisplayName);
                var toAddress = new MailAddress(targetEmail);

                var smtp = new SmtpClient
                {
                    Host = SmtpHost,
                    Port = SmtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, FromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    // Task.Run ile senkron Send metodunu async sarmallıyoruz
                    await Task.Run(() => smtp.Send(message));
                }

                return true;
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapılabilir: ex.Message
                return false;
            }
        }
    }
}