using System.Windows;

namespace AristaNetworkManager.Views
{
    public partial class AddSwitchDialog : Window
    {
        public string IpAddress => IpAddressTextBox.Text;
        public string Hostname => HostnameTextBox.Text;
        public string Username => UsernameTextBox.Text;
        public string Password => PasswordBox.Password;

        public AddSwitchDialog()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(IpAddress))
            {
                MessageBox.Show("IP Address is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Username and Password are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
