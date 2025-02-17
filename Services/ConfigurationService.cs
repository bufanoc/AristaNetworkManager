using System.IO;
using Microsoft.Extensions.Configuration;
using AristaNetworkManager.Models;

namespace AristaNetworkManager.Services
{
    public class ConfigurationService
    {
        private static ConfigurationService? _instance;
        private readonly IConfiguration _configuration;
        private readonly AppSettings _settings;

        private ConfigurationService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
            _settings = _configuration.Get<AppSettings>() ?? new AppSettings();
        }

        public static ConfigurationService Instance
        {
            get
            {
                _instance ??= new ConfigurationService();
                return _instance;
            }
        }

        public AppSettings Settings => _settings;
    }
}
