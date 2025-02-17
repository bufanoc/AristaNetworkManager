using System.Collections.Generic;

namespace AristaNetworkManager.Models
{
    public class AppSettings
    {
        public ApiSettings ApiSettings { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }

    public class ApiSettings
    {
        public DefaultCredentials DefaultCredentials { get; set; } = new();
        public int ConnectionTimeout { get; set; } = 30;
        public int PollingInterval { get; set; } = 10;
        public bool IgnoreSslErrors { get; set; } = true;
    }

    public class DefaultCredentials
    {
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = string.Empty;
    }

    public class LoggingSettings
    {
        public Dictionary<string, string> LogLevel { get; set; } = new();
    }
}
