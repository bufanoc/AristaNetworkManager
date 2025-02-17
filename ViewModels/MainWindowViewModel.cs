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
        public ObservableCollection<SwitchViewModel> Switches { get; } = new();
        
        [ObservableProperty]
        private SwitchViewModel? selectedSwitch;

        public MainWindowViewModel()
        {
            InitializeApp();
        }

        private async Task InitializeApp()
        {
            var config = ConfigurationService.Instance.Settings.ApiSettings;
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
                Username = ConfigurationService.Instance.Settings.ApiSettings.DefaultCredentials.Username
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Check if a switch with this IP already exists
                    if (Switches.Any(s => s.Model.IpAddress == dialog.IpAddress))
                    {
                        MessageBox.Show(
                            $"A switch with IP address {dialog.IpAddress} already exists.",
                            "Duplicate Switch",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    var apiService = new ApiService(dialog.Username, dialog.Password);
                    _apiServices[dialog.IpAddress] = apiService;

                    var switchModel = new SwitchModel(dialog.IpAddress, dialog.Hostname, dialog.Username, dialog.Password);
                    var switchViewModel = new SwitchViewModel(switchModel, apiService);
                    Switches.Add(switchViewModel);
                    SelectedSwitch = switchViewModel;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error adding switch: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task RemoveSwitch(SwitchViewModel switchToRemove)
        {
            if (MessageBox.Show($"Are you sure you want to remove {switchToRemove.Model.Hostname}?", "Confirm Removal", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _apiServices.Remove(switchToRemove.Model.IpAddress);
                switchToRemove.Dispose();
                Switches.Remove(switchToRemove);
            }
        }

        private async Task AddSwitch(string ipAddress, string hostname, string username, string password)
        {
            try
            {
                // Check if a switch with this IP already exists
                if (Switches.Any(s => s.Model.IpAddress == ipAddress))
                {
                    MessageBox.Show(
                        $"A switch with IP address {ipAddress} already exists.",
                        "Duplicate Switch",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var model = new SwitchModel(ipAddress, hostname ?? ipAddress, username, password);
                var apiService = new ApiService(username, password);
                var viewModel = new SwitchViewModel(model, apiService);
                
                // Add to collections
                Switches.Add(viewModel);
                _apiServices[ipAddress] = apiService;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error adding switch: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RefreshSwitches()
        {
            foreach (var switchVm in Switches)
            {
                await switchVm.FetchConfigurationCommand.ExecuteAsync(null);
            }
        }
    }
}
