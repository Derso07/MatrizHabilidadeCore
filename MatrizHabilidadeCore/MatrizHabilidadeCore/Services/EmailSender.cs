using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MatrizHabilidadeDataBaseCore.Services
{
    public class EmailSender
    {
            public static void Send(string senderName, string address, string name, string subject, string body)
            {
                var annimarMail = "novelis.gestao@annimar.com.br";
                var senha = "Tr&R043";

                SmtpClient client = new SmtpClient
                {
                    Host = "mail.annimar.com.br",
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(annimarMail, senha),
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                MailMessage mail = new MailMessage
                {
                    Sender = new MailAddress(annimarMail, senderName),
                    From = new MailAddress(annimarMail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.High,
                };

                mail.To.Add(new MailAddress(address, name));

                client.Send(mail);
            }

            public static async Task SendAsync(string senderName, string address, string name, string subject, string body)
            {
                var annimarMail = "novelis.gestao@annimar.com.br";
                var senha = "Tr&R043";

                SmtpClient client = new SmtpClient
                {
                    Host = "mail.annimar.com.br",
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(annimarMail, senha),
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                MailMessage mail = new MailMessage
                {
                    Sender = new MailAddress(annimarMail, senderName),
                    From = new MailAddress(annimarMail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.High,
                };

                mail.To.Add(new MailAddress(address, name));

                await client.SendMailAsync(mail);
            }
        }
    }
