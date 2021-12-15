using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using System;
using System.Net.Mail;

namespace Services
{
    public class MailService
    {
        private readonly DataBaseContext _db;
        public MailService(DataBaseContext db)
        {
            _db = db;
        }
        public async void SendEmailAsync(string email, string userName, string mensagem, string subject)
        {
            try
            {
                string host = System.Configuration.ConfigurationManager.AppSettings["Host"];
                string address = System.Configuration.ConfigurationManager.AppSettings["Address"];
                string password = System.Configuration.ConfigurationManager.AppSettings["Password"];
                int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Port"]);
                string displayName = System.Configuration.ConfigurationManager.AppSettings["DisplayName"];

                using (var client = new SmtpClient())
                {
                    client.Host = host;
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(address, password);
                    client.Port = port;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    using (var mail = new MailMessage())
                    {
                        mail.Sender = new MailAddress(address, displayName);
                        mail.From = new MailAddress(address, displayName);
                        mail.Subject = subject;
                        mail.Body = mensagem;
                        mail.IsBodyHtml = true;
                        mail.Priority = MailPriority.High;

                        mail.To.Add(new MailAddress(email, userName));

                        await client.SendMailAsync(mail);
                    }
                }
            }
            catch (Exception e)
            {
                    _db.Erros.Add(new Error(e, "MailService"));
                    await _db.SaveChangesAsync();
            }
        }
    }
}