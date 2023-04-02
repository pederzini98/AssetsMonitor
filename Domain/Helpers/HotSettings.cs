using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helpers
{
    public static class HotSettings
    {

     
        public static string? SmtpHostName { get => _smtpHostName; }
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
            _to = new List<string>(appSettings["to"].Split(','));
        }

    }
}
