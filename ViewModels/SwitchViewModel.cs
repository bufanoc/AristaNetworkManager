using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using AristaNetworkManager.Models;
using AristaNetworkManager.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AristaNetworkManager.ViewModels
{
    public partial class SwitchViewModel : ObservableObject, IDisposable
    {
        private readonly ApiService _apiService;
        private readonly Timer _connectionCheckTimer;
        private readonly ConfigurationService _configurationService;
        private bool _disposed;

        [ObservableProperty]
        private SwitchModel _model;

        [ObservableProperty]
        private string? _configuration;

        public string IpAddress => Model.IpAddress;
        public string Hostname => Model.Hostname;

        public SwitchViewModel(SwitchModel model, ApiService apiService, ConfigurationService configurationService)
        {
            _model = model;
            _apiService = apiService;
            _configurationService = configurationService;
            Model.Status = "Connecting...";

            var pollingInterval = _configurationService.Settings.ApiSettings.PollingInterval;
            // Create a timer to check connection status periodically
            _connectionCheckTimer = new Timer(pollingInterval * 1000); // Convert seconds to milliseconds
            _connectionCheckTimer.Elapsed += async (s, e) => await CheckConnectionStatus();
            _connectionCheckTimer.Start();

            // Initial connection check and details fetch
            _ = InitializeSwitch();
        }

        private async Task InitializeSwitch()
        {
            await CheckConnectionStatus();
            await UpdateSwitchDetails();
        }

        private async Task UpdateSwitchDetails()
        {
            try
            {
                // Fetch hostname first
                var hostname = await _apiService.GetSwitchHostnameAsync(Model.IpAddress);
                Model.Hostname = hostname;

                // Then fetch other details
                var details = await _apiService.GetSwitchDetailsAsync(Model.IpAddress);
                Model.Version = details.Version;
                Model.Model = details.Model;
                Model.SerialNumber = details.SerialNumber;
                Model.SystemMacAddress = details.SystemMacAddress;
                Model.Uptime = details.Uptime;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error fetching switch details: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async Task CheckConnectionStatus()
        {
            try
            {
                Model.Status = "Checking connection...";
                if (await _apiService.TestConnectionAsync(Model.IpAddress))
                {
                    Model.Status = "Connected";
                    Model.IsConnected = true;
                    
                    // Update hostname when connection is established
                    var hostname = await _apiService.GetSwitchHostnameAsync(Model.IpAddress);
                    Model.Hostname = hostname;
                }
                else
                {
                    Model.Status = "Disconnected";
                    Model.IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                Model.Status = $"Error: {ex.Message}";
                Model.IsConnected = false;
            }
        }

        [RelayCommand]
        private async Task UpdatePassword(PasswordBox passwordBox)
        {
            try
            {
                var newPassword = passwordBox.Password;
                if (string.IsNullOrEmpty(newPassword))
                {
                    MessageBox.Show(
                        "Please enter a new password.",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Model.Status = "Updating password...";
                if (await _apiService.UpdateSwitchPasswordAsync(Model.IpAddress, newPassword))
                {
                    Model.Password = newPassword;
                    passwordBox.Clear();
                    MessageBox.Show(
                        "Password updated successfully.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Failed to update password. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error updating password: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                await CheckConnectionStatus();
            }
        }

        [RelayCommand]
        private async Task FetchConfiguration()
        {
            try
            {
                Model.Status = "Fetching configuration...";
                var config = await _apiService.GetSwitchConfigAsync(Model.IpAddress);
                Configuration = config?.ToString();
                await CheckConnectionStatus();
            }
            catch (Exception ex)
            {
                Configuration = $"Error fetching configuration: {ex.Message}";
                Model.Status = $"Error: {ex.Message}";
                Model.IsConnected = false;
            }
        }

        [RelayCommand]
        private async Task UpdateConfiguration(string newConfig)
        {
            try
            {
                Model.Status = "Updating configuration...";
                if (await _apiService.UpdateSwitchConfigAsync(Model.IpAddress, newConfig))
                {
                    await FetchConfiguration();
                }
            }
            catch (Exception ex)
            {
                Configuration = $"Error updating configuration: {ex.Message}";
                Model.Status = $"Error: {ex.Message}";
                Model.IsConnected = false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connectionCheckTimer.Stop();
                _connectionCheckTimer.Dispose();
                _apiService.Dispose();
                _disposed = true;
            }
        }
    }
}
