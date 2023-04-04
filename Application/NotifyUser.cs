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
        public static bool SendNotificationEmail(Email email)
        {
            try
            {
                MailMessage message = new()
                {
                    From = new MailAddress(email?.SmtpServerConfig?.EmailAddress, HotDefault.SenderName, Encoding.UTF8)
                };
                foreach (var mailAddress in email.To ?? new List<string>())
                {
                    try
                    {
                        message.To.Add(new MailAddress(mailAddress ?? ""));
                    }
                    catch (Exception ex)
                    {

                        if (ex.Message.Contains("is not in the form required for an e-mail address"))
                        {
                            Console.WriteLine($"Unable to add recipient.This email is not in the right format and will not receive the email: ['{mailAddress}']");
                            continue;
                        };
                    }
                }
                message.Subject = email.Subject;
                message.Body = email.Body;
                message.IsBodyHtml = true;

                Console.WriteLine($"Trying to connect to smtp server and send message...");

                using SmtpClient smtp = new(email.SmtpServerConfig?.SmtpHostName, email.SmtpServerConfig?.Port ?? 587);
                smtp.Credentials = new NetworkCredential(email.SmtpServerConfig?.EmailAddress, email.SmtpServerConfig?.Password);
                smtp.EnableSsl = email.SmtpServerConfig?.UseSsl ?? true;
                smtp.Send(message);
                Console.WriteLine($"Email sent to {string.Join(",", message.To)}. If we receive any other change, your next email will be sent at {DateTime.Now.AddMinutes(HotDefault.SendEmailTimeout)}");

                return true;
            }
            catch (SmtpException smtpException)
            {
                Console.WriteLine($"Unable to notify user about the change in assets.\n Please check your smtp server configs. If everithing is fine please wait until we try again or Contact our suport {HotDefault.SupportLink}. \nError: {smtpException.Message}");
                return false;
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("is not in the form required for an e-mail address"))
                {
                    Console.WriteLine($"Unable to notify user about the changes.\nEmails are not in the right format:\nUserEmail: '{email?.SmtpServerConfig?.EmailAddress}'\n Email recipient '{string.Join("", email?.To)}'.");
                    return false;
                }
                Console.WriteLine($"Unable to notify user about the change in assets.\n Please check your smtp server configs and the emails. If everithing is fine please wait until we try again or Contact our suport {HotDefault.SupportLink}. \nError: {exception.Message} -- {exception.StackTrace}");
                return true;
            }


        }
    }
}
