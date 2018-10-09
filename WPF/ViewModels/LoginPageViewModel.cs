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
        private Page mPage;
        public LoginPageViewModel(Page page)
        {
            mPage = page;

            LoginCommand = new RelayCommand(() => LogIn());
        }

        private void LogIn()
        {
            if ((mPage as LoginPage).usernameTB.Text == "" || (mPage as LoginPage).passwordPB.SecurePassword.Length == 0)
            {
                CustomMessageBox.Show(Properties.Resources.Message_Empty_User_Password, Properties.Resources.Error, MessageBoxButton.OK);
            }
            else if((mPage as LoginPage).usernameTB.Text == "admin" && new System.Net.NetworkCredential(string.Empty, (mPage as LoginPage).passwordPB.SecurePassword).Password == "passx")
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
