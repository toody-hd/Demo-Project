using System.Windows;
using System.Windows.Controls;

namespace WPF
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            DataContext = new LoginPageViewModel(this);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (((PasswordBox)sender).Template.FindName("passwordTB", (PasswordBox)sender) as TextBlock).Visibility = 
                ((PasswordBox)sender).SecurePassword.Length != 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
