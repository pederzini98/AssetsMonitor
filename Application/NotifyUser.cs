using Domain.Entities;
using Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class NotifyUser
    {
        public Dictionary<string, DateTime> UserThrottle = new();
        public DateTime UserLastEmailSent(string email)
        {
            return UserThrottle.ContainsKey(email) ? UserThrottle[email] : DateTime.UtcNow;
        }
        public static void SendNotificationEmail(Email email)
        {
            try
            {

                MailMessage message = new();

                message.From = new MailAddress(email.SmtpServerConfig?.EmailAddress, HotDefault.SenderName, Encoding.UTF8);
                foreach (var mailAddress in email.To)
                {
                    message.To.Add(new MailAddress(mailAddress ?? ""));
                }
                message.Subject = email.Subject;
                message.Body = email.Body;
                message.IsBodyHtml = true;



                using SmtpClient smtp = new(email.SmtpServerConfig?.SmtpHostName, email.SmtpServerConfig?.Port ?? 587);
                smtp.Credentials = new NetworkCredential(email.SmtpServerConfig?.EmailAddress, email.SmtpServerConfig?.Password);
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            catch (Exception e)
            {

                string messasag = e.Message;
            }

        }
    }
}
