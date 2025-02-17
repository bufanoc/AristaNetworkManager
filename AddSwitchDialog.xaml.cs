using System.Windows;

namespace AristaNetworkManager
{
    public partial class AddSwitchDialog : Window
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public AddSwitchDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(IpAddressTextBox.Text))
            {
                MessageBox.Show("IP Address is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IpAddress = IpAddressTextBox.Text;
            Hostname = HostnameTextBox.Text;
            Username = UsernameTextBox.Text;
            Password = PasswordBox.Password;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
