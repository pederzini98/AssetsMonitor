using Domain.Entities;
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

        public void SendNotificationEmail(Email email)
        {

            MailMessage message = new();
            SmtpClient smtp = new();

            message.From = new MailAddress("From", email.SmtpServerConfig?.EmailAddress, Encoding.UTF8);
            message.To.Add(new MailAddress(email.To ?? ""));
            message.Subject = email.Subject;
            message.Body = email.Body;
            message.IsBodyHtml = true;

            smtp.Host = email.SmtpServerConfig?.SmtpHostName ?? "";
            smtp.EnableSsl = email.SmtpServerConfig?.UseSsl ?? true;
            smtp.Port = email.SmtpServerConfig?.Port ?? 587; //Gmail port for e-mail 465 or 587

            NetworkCredential NetworkCred = new()
            {
                UserName = email.SmtpServerConfig?.EmailAddress,//gmail user name
                Password = email.SmtpServerConfig?.Password// password
            };
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;

            smtp.Send(message);
        }
    }
}
