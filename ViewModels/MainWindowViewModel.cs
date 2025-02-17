using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AristaNetworkManager.Models;
using AristaNetworkManager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AristaNetworkManager.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly Dictionary<string, ApiService> _apiServices = new();
        private readonly ConfigurationService _configurationService;
        public ObservableCollection<SwitchViewModel> Switches { get; } = new();
        
        [ObservableProperty]
        private SwitchViewModel? selectedSwitch;

        public MainWindowViewModel()
        {
            _configurationService = ConfigurationService.Instance;
            InitializeApp();
        }

        private async Task InitializeApp()
        {
            var config = _configurationService.Settings.ApiSettings;
            // Add test switch with credentials from configuration
            await AddSwitch("192.168.88.182", "Test Switch", 
                config.DefaultCredentials.Username, 
                config.DefaultCredentials.Password);
        }

        [RelayCommand]
        private async Task AddNewSwitch()
        {
            var dialog = new AddSwitchDialog
            {
                Owner = Application.Current.MainWindow,
                Username = _configurationService.Settings.ApiSettings.DefaultCredentials.Username
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Check if a switch with this IP already exists
                    if (Switches.Any(s => s.Model.IpAddress == dialog.IpAddress))
                    {
                        MessageBox.Show("A switch with this IP address already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    await AddSwitch(dialog.IpAddress, dialog.Hostname, dialog.Username, dialog.Password);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding switch: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task AddSwitch(string ipAddress, string hostname, string username, string password)
        {
            var apiService = new ApiService(username, password);
            _apiServices[ipAddress] = apiService;

            var switchModel = new SwitchModel
            {
                IpAddress = ipAddress,
                Hostname = hostname,
                Username = username,
                Password = password
            };

            var switchViewModel = new SwitchViewModel(switchModel, apiService, _configurationService);
            Switches.Add(switchViewModel);
        }

        [RelayCommand]
        private void RemoveSwitch()
        {
            if (SelectedSwitch == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove {SelectedSwitch.Hostname}?",
                "Confirm Removal",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _apiServices.Remove(SelectedSwitch.IpAddress);
                Switches.Remove(SelectedSwitch);
                SelectedSwitch = null;
            }
        }
    }
}
