# Arista Network Manager

A WPF application for managing Arista network switches. This application provides a user-friendly interface for monitoring and configuring Arista switches through their eAPI interface.

## Features

- Connect to multiple Arista switches simultaneously
- View switch details including model, version, and status
- Monitor switch connection status
- View and edit switch configurations
- Update switch passwords securely
- Automatic hostname detection

## Requirements

- .NET 7.0 or later
- Windows operating system
- Network access to Arista switches
- Arista switches with eAPI enabled

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Copy `appsettings.example.json` to `appsettings.json` and update with your settings
4. Build and run the application
5. Use the "Add Switch" button to connect to your first switch

## Configuration

The application uses `appsettings.json` for configuration. You can set default values for:
- Default credentials
- Connection timeout
- Polling intervals
- SSL verification settings

When adding a switch, you'll need to provide:
- IP Address
- Username (default from configuration)
- Password
- Hostname (optional, will be automatically detected)

## Dependencies

- CommunityToolkit.Mvvm
- Newtonsoft.Json
- Microsoft.Xaml.Behaviors.Wpf
- Microsoft.Extensions.Configuration

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
