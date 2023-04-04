using Domain.Helpers;

namespace Domain.Entities
{
    public class Email
    {
        public List<string>? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public SmtpServerConfig? SmtpServerConfig { get; set; }
        public Email(string subject, string body)
        {
            To = HotSettings.To;
            Subject = subject;
            Body = body;
            SmtpServerConfig = new();
        }
    }
    public class SmtpServerConfig
    {

        public string? SmtpHostName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
        public bool? UseSsl { get; set; }
        public int? Port { get; set; }

        public SmtpServerConfig()
        {
            SmtpHostName = HotSettings.SmtpHostName;
            EmailAddress = HotSettings.EmailAddress;
            Password = HotSettings.Password;
            UseSsl = HotSettings.UseSsl ?? true;
            Port = HotSettings.Port == 0 ? 485 : HotSettings.Port;
        }
    }
}
