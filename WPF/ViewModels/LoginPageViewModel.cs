using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace WPF
{
    public class LoginPageViewModel
    {
        private LoginPage mPage;

        public LoginPageViewModel(Page page)
        {
            mPage = (LoginPage)page;

            LoginCommand = new RelayCommand(() => LogIn());
        }

        private void LogIn()
        {
            if (mPage.usernameTB.Text == "" || mPage.passwordPB.SecurePassword.Length == 0)
            {
                CustomMessageBox.Show(Properties.Resources.Message_Empty_User_Password, Properties.Resources.Error, MessageBoxButton.OK);
            }
            else if(mPage.usernameTB.Text == "admin" && new System.Net.NetworkCredential(string.Empty, mPage.passwordPB.SecurePassword).Password == "passx")
            {
                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (ss, ee) =>
                {
                    Thread.Sleep(500);
                };

                backgroundWorker.RunWorkerCompleted += (ss, ee) =>
                {
                    mPage.NavigationService.Navigate(new MainPage());
                };

                backgroundWorker.RunWorkerAsync();
                (mPage.FindResource("Fade_Shrink") as Storyboard).Begin(mPage);
            }
            else
            {
                CustomMessageBox.Show(Properties.Resources.Message_Incorect_User_Password, Properties.Resources.Error, MessageBoxButton.OK);
            }
        }

        public ICommand LoginCommand { get; private set; }
    }
}
