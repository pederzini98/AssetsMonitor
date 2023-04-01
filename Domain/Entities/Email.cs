using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Email
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public DateTime? UpdatedIn { get; set; }
        public SmtpServerConfig? SmtpServerConfig { get; set; }
        public Email(string subject, string body, NameValueCollection appSettings)
        {
            To = appSettings["To"];
            Subject = subject;
            Body = body;
            UpdatedIn = DateTime.Now.ToLocalTime();
            SmtpServerConfig = new(appSettings);
        }
    }
    public class SmtpServerConfig
    {

        public string? SmtpHostName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
        public bool? UseSsl { get; set; }
        public int? Port { get; set; }

        public SmtpServerConfig(NameValueCollection appSettings)
        {
            SmtpHostName = appSettings["hostname"];
            EmailAddress = appSettings["emailaddress"];
            Password = appSettings["password"];
            UseSsl = string.Equals(appSettings["useSsl"], "true");
            Port = int.Parse(appSettings["port"] ?? "485");
        }
    }
}
