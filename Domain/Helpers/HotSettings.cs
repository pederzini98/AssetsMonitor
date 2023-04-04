using System.Collections.Specialized;
using System.Xml.Linq;

namespace Domain.Helpers
{
    public  class HotSettings
    {
        public static string? Password { get; set; }
        public static string? SmtpHostName { get; set; }
        public static string? EmailAddress { get; set; }
        public static bool? UseSsl { get; set; }
        public static int Port { get; set; }
        public static string? To { get; set; }


        /*   public static string? SmtpHostName { get => _smtpHostName; }
           private static string? _smtpHostName;
           public static string? EmailAddress { get => _emailAddress; }
           private static string? _emailAddress;

           public static string? Password { get => _password; }
           private static string? _password;

           public static bool? UseSsl { get => _useSsl; }
           private static bool? _useSsl;

           public static int Port { get => _port; }
           private static int _port;

           public static List<string>? To { get => _to; }
           private static List<string>? _to;


           public static void StartValue(NameValueCollection appSettings)
           {
               _smtpHostName = appSettings["hostname"];
               _emailAddress = appSettings["emailAddress"];
               _password = appSettings["password"];
               _useSsl = string.Equals(appSettings["useSsl"], "true");
               _port = int.Parse(appSettings["port"] ?? "485");
               _to = new List<string>((appSettings["to"] ?? "").Split(','));
           }
           public override string ToString()
           {
               return $"Configuration File: hostname:{_smtpHostName}\nemailAddress:{_emailAddress}\nhostname:{_smtpHostName}\nhostname:{_smtpHostName}\nhostname:{_smtpHostName}\nhostname:{_smtpHostName}\n";
           }
        */
    }
}
