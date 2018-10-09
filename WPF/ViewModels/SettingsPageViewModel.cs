using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace WPF
{
    public class SettingsPageViewModel
    {
        private Page _page;
        private int pageNr = 1;
        public SettingsPageViewModel(Page page)
        {
            _page = page;

            SaveCommand = new RelayCommand(() => Save());
            ResetSettingsCommand = new RelayCommand(() => ResetSettings());
            ResetDataCommand = new RelayCommand(() => ResetData());
            BackCommand = new RelayCommand(() => { Settings._miniMode = Settings.miniMode; Settings._topmostMode = Settings.topmostMode; Settings._lang = Settings.lang; Settings._messageMode = Settings.messageMode; AnimationEffect(); });
            GeneralSettingsCommand = new RelayCommand(() => GeneralSettings_Clik());
            VisualSettingsCommand = new RelayCommand(() => VisualSettings_Clik());

            Initializer();
        }

        private void Initializer()
        {
            
        }

        private void Save()
        {
            Settings.miniMode = Settings._miniMode;
            Settings.topmostMode = Settings._topmostMode;
            Settings.lang = Settings._lang;
            Settings.messageMode = Settings._messageMode;
            Settings.excUpd = null;
            Settings.excUpd = Settings._excUpd;

            Application.Current.MainWindow.Topmost = Settings.topmostMode;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Settings.lang);
            if ((Application.Current.MainWindow as MainWindow).settingsFile != null)
            {
                (Application.Current.MainWindow as MainWindow).settingsFile.Write("General", "MiniMode", Settings.miniMode.ToString());
                (Application.Current.MainWindow as MainWindow).settingsFile.Write("General", "TopMode", Settings.topmostMode.ToString());
                (Application.Current.MainWindow as MainWindow).settingsFile.Write("General", "Language", Settings.lang);
                (Application.Current.MainWindow as MainWindow).settingsFile.Write("General", "MessageMode", Settings.messageMode.ToString());
            }

            if ((Application.Current.MainWindow as MainWindow).filterFile != null)
            {
                var _found = false;
                for (int i = 1; i <= int.Parse((Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count")); i++)
                {
                    _found = false;
                    foreach (var x in Settings.excUpd)
                        if ((Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name") == x)
                        {
                            _found = true;
                            break;
                        }
                    if(_found)
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + i, "ExcludeUpdate", "True");
                    else
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + i, "ExcludeUpdate", "False");
                }
                /*
                for (int i = 1; i <= Settings.excList.Length - 1; i++)
                {
                    if (Settings.excList[i].Value == true)
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + i, "ExcludeUpdate", "True");
                    else
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + i, "ExcludeUpdate", "False");
                }
                */
            }

            AnimationEffect();
        }

        private void ResetSettings()
        {
            MessageBoxResult result = CustomMessageBox.Show(Properties.Resources.Message_ResetSettings, Properties.Resources.Warning, MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                _ResetSettings();
            }
        }

        private void _ResetSettings()
        {
            if (pageNr == 1)
            {
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).langCB.SelectedIndex = 0;
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).miniCB.IsChecked = false;
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).topCB.IsChecked = false;
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).messageCB.IsChecked = true;
            }
            else if (pageNr == 2)
                ((_page as SettingsPage).settingsPage.NavigationService.Content as VisualSettingsPage)._testingCB.IsChecked = false;
            Settings.Reset();
        }

        private void ResetData()
        {
            MessageBoxResult result = CustomMessageBox.Show(Properties.Resources.Message_ResetData, Properties.Resources.Warning, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if ((Application.Current.MainWindow as MainWindow).pannelFile != null)
                {
                    for (int i = 1; i <= int.Parse((Application.Current.MainWindow as MainWindow).pannelFile.Read("PanelCount", "Count")); i++)
                    {
                        (Application.Current.MainWindow as MainWindow).pannelFile.DeleteSection("Panel - " + i);
                    }
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("PanelCount", "Count", "0");
                }

                if ((Application.Current.MainWindow as MainWindow).filterFile != null)
                {
                    for (int i = 1; i <= int.Parse((Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count")); i++)
                    {
                        (Application.Current.MainWindow as MainWindow).filterFile.DeleteSection("Filter - " + i);
                    }
                    (Application.Current.MainWindow as MainWindow).filterFile.Write("FilterCount", "Count", "0");
                }

                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\pictures"))
                {
                    Directory.Delete(Directory.GetCurrentDirectory() + @"\pictures", true);
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\pictures");
                }

                _ResetSettings();
            }
        }

        private void AnimationEffect()
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (ss, ee) =>
            {
                Thread.Sleep(500);
            };

            backgroundWorker.RunWorkerCompleted += (ss, ee) =>
            {
                _page.NavigationService.Navigate(new MainPage());
            };

            backgroundWorker.RunWorkerAsync();
            (_page.FindResource("Fade_Shrink") as Storyboard).Begin(_page);
        }

        private void GeneralSettings_Clik()
        {
            pageNr = 1;
            (_page as SettingsPage).settingsPage.NavigationService.Navigate(new GeneralSettingsPage());
        }

        private void VisualSettings_Clik()
        {
            pageNr = 2;
            (_page as SettingsPage).settingsPage.NavigationService.Navigate(new VisualSettingsPage());
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand ResetSettingsCommand { get; private set; }
        public ICommand ResetDataCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand GeneralSettingsCommand { get; private set; }
        public ICommand VisualSettingsCommand { get; private set; }
    }
}