using System.Collections.Generic;
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
        #region Variables
        private Page _page;
        private int pageNr = 1;
        #endregion

        public SettingsPageViewModel(Page page)
        {
            _page = page;
            Initializer();
            SaveCommand = new RelayCommand(() => Save());
            ResetSettingsCommand = new RelayCommand(() => ResetSettings());
            ResetDataCommand = new RelayCommand(() => ResetData());
            BackCommand = new RelayCommand(() => GoingBack());
            GeneralSettingsCommand = new RelayCommand(() => GeneralSettings_Clik());
            VisualSettingsCommand = new RelayCommand(() => VisualSettings_Clik());
        }

        private void Initializer()
        {
            Settings.scripts = Settings._scripts = new List<string[]>();
            Settings.filters = Settings._filters = new List<string[]>();

            if (InI.FiltersFileExist() && InI.filtersFile.ReadSections().Length > 0)
            {
                foreach (var x in InI.filtersFile.ReadSections())
                {
                    Settings.filters.Add(new string[] { x, InI.filtersFile.Read(x, "ExcludeUpdate"), InI.filtersFile.Read(x, "HideOnStartUp") });
                }
                Settings.filters = Settings._filters;
            }

            if (InI.ScriptsFileExist() && InI.scriptsFile.SectionExists("Scripts"))
            {
                foreach (var x in InI.scriptsFile.ReadKeys("Scripts"))
                {
                    Settings.scripts.Add(new string[] { x, InI.scriptsFile.Read("Scripts", x) });
                }
                Settings.scripts = Settings._scripts;
            }
        }

        private void GoingBack()
        {
            Settings._miniMode = Settings.miniMode;
            Settings._topmostMode = Settings.topmostMode;
            Settings._lang = Settings.lang;
            Settings._messageMode = Settings.messageMode;
            Settings._filters = Settings.filters;
            Settings._scripts = Settings.scripts;

            AnimationEffect();
        }

        private void Save()
        {
            Settings.miniMode = Settings._miniMode;
            Settings.topmostMode = Settings._topmostMode;
            Settings.lang = Settings._lang;
            Settings.messageMode = Settings._messageMode;
            Settings.filters = Settings._filters;
            Settings.scripts = Settings._scripts;

            Application.Current.MainWindow.Topmost = Settings.topmostMode;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Settings.lang);
            InI.settingsFile.Write("General", "MiniMode", Settings.miniMode.ToString());
            InI.settingsFile.Write("General", "TopMode", Settings.topmostMode.ToString());
            InI.settingsFile.Write("General", "Language", Settings.lang);
            InI.settingsFile.Write("General", "MessageMode", Settings.messageMode.ToString());

            foreach (var x in Settings.filters)
            {
                InI.filtersFile.Write(x[0], "ExcludeUpdate", x[1]);
                InI.filtersFile.Write(x[0], "HideOnStartUp", x[2]);
            }

            if (InI.ScriptsFileExist() && InI.scriptsFile.SectionExists("Scripts"))
                InI.scriptsFile.DeleteSection("Scripts");
            foreach (var x in Settings.scripts)
            {
                InI.scriptsFile.Write("Scripts", x[0], x[1]);
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
                foreach (var x in ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).excludeCB.Items)
                {
                    if (x is CheckBox)
                    {
                        if (((CheckBox)x).IsChecked == true)
                            ((CheckBox)x).IsChecked = false;
                    }
                }
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).excludeCB.Items[0] = Properties.Resources.NoneSelected;
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).excludeCB.SelectedIndex = 0;
                foreach (var x in ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).hideOnStartUpCB.Items)
                {
                    if (x is CheckBox)
                    {
                        if (((CheckBox)x).IsChecked == true)
                            ((CheckBox)x).IsChecked = false;
                    }
                }
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).hideOnStartUpCB.Items[0] = Properties.Resources.NoneSelected;
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).hideOnStartUpCB.SelectedIndex = 0;

                ((Expander)((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).CustomScripSP.Parent).IsExpanded = false;
                ((_page as SettingsPage).settingsPage.NavigationService.Content as GeneralSettingsPage).CustomScripSP.Children.Clear();

                foreach (var x in Settings._filters)
                    x[1] = x[2] = "false";

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
                if (InI.PanelsFileExist())
                {
                    File.Delete(@"panels.ini");
                }

                if (Directory.Exists(Directory.GetCurrentDirectory() + @"\pictures"))
                {
                    Directory.Delete(Directory.GetCurrentDirectory() + @"\pictures", true);
                }

                if (InI.FiltersFileExist())
                {
                    File.Delete(@"filters.ini");
                }

                if (InI.ScriptsFileExist())
                {
                    File.Delete(@"scripts.ini");
                }

                _ResetSettings();
                Settings.ResetData();
                AnimationEffect();
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

        #region ICommands
        public ICommand SaveCommand { get; private set; }
        public ICommand ResetSettingsCommand { get; private set; }
        public ICommand ResetDataCommand { get; private set; }
        public ICommand BackCommand { get; private set; }
        public ICommand GeneralSettingsCommand { get; private set; }
        public ICommand VisualSettingsCommand { get; private set; }
        #endregion
    }
}