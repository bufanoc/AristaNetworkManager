using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AristaNetworkManager.Models
{
    public partial class SwitchModel : ObservableObject
    {
        [ObservableProperty]
        private string _ipAddress;

        [ObservableProperty]
        private string _hostname;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _status;

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private string? _currentConfig;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private string _model;

        [ObservableProperty]
        private string _serialNumber;

        [ObservableProperty]
        private string _systemMacAddress;

        [ObservableProperty]
        private string _uptime;

        [ObservableProperty]
        private List<VirtualNetwork> _virtualNetworks = new();

        public Dictionary<string, string> Properties { get; set; } = new();

        public SwitchModel(string ipAddress, string hostname, string username, string password)
        {
            IpAddress = ipAddress;
            Hostname = hostname;
            Username = username;
            Password = password;
            Status = "Not Connected";
            IsConnected = false;
        }
    }
}
