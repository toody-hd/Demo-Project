using INIHandler;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

namespace WPF
{
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon MyNotifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);

            IniInitializer();

            MyNotifyIcon = new System.Windows.Forms.NotifyIcon() { Icon = Properties.Resources.logo };
            MyNotifyIcon.MouseDoubleClick += MyNotifyIcon_DoubleClick;
        }

        private void IniInitializer()
        {
            //foreach (var x in InI.filtersFile.ReadSections())
            //{
            //    InI.filtersFile.RenameSection(x, InI.filtersFile.Read(x, "Name"));
            //    InI.filtersFile.DeleteKey(x, "Name");
            //}

            //foreach (var x in InI.panelsFile.ReadSections())
            //{
            //    File.Move(@"pictures/IMG-" + x.Substring(8) + ".png", @"pictures/" + InI.panelsFile.Read(x, "Name").Replace(":","") + ".png");
            //    InI.panelsFile.RenameSection(x, InI.panelsFile.Read(x, "Name"));
            //}
            //foreach (var x in InI.panelsFile.ReadSections())
            //{
            //    InI.panelsFile.DeleteKey(x, "Name");
            //}

            if (InI.SettingsFileExist())
            {
                InitializeSettings();
            }
            else
            {
                InI.settingsFile.Write("General", "MiniMode", "False");
                InI.settingsFile.Write("General", "TopMode", "False");
                InI.settingsFile.Write("General", "Language", "en-US");
                InI.settingsFile.Write("General", "MessageMode", "True");
            }

            if(InI.FiltersFileExist())
            {
                InitializeFilters();
            }
        }

        private void InitializeSettings()
        {
            Settings._miniMode = Settings.miniMode = bool.Parse(InI.settingsFile.Read("General", "MiniMode"));
            Settings._topmostMode = Settings.topmostMode = bool.Parse(InI.settingsFile.Read("General", "TopMode"));
            Settings._lang = Settings.lang = InI.settingsFile.Read("General", "Language");
            Settings._messageMode = Settings.messageMode = bool.Parse(InI.settingsFile.Read("General", "MessageMode"));

            this.Topmost = Settings.topmostMode;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Settings.lang);
        }


        private void InitializeFilters()
        {
            foreach(var x in InI.filtersFile.ReadSections())
            {
                Settings.filters.Add(new string[] {x, InI.filtersFile.Read(x, "ExcludeUpdate"), InI.filtersFile.Read(x, "HideOnStartUp") });
            }
            Settings._filters = Settings.filters;
        }

        //private void InitializeFilters()
        //{
        //    Settings._excUpd = Settings.excUpd;
        //    Settings._hideSU = Settings.hideSU;
        //}

        private void CreateContextMenu()
        {
            MyNotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            MyNotifyIcon.ContextMenuStrip.Items.Add(Properties.Resources.Notification_Menu_Show).Click += (s, e) => { this.Show(); this.WindowState = WindowState.Normal; this.Activate(); };
            MyNotifyIcon.ContextMenuStrip.Items.Add(Properties.Resources.Notification_Menu_Close).Click += (s, e) => {
                if (Settings.messageMode)
                {
                    MyNotifyIcon.BalloonTipTitle = this.Title;
                    MyNotifyIcon.BalloonTipText = this.Title + Properties.Resources.Notificaion_Close;
                    MyNotifyIcon.ShowBalloonTip(400);
                }
                MyNotifyIcon.Visible = false;
                this.Close();
            };
        }

        private void MyNotifyIcon_DoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (Settings.miniMode)
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.Hide();
                    MyNotifyIcon.Visible = true;
                    if (Settings.messageMode)
                    {
                        MyNotifyIcon.Text = this.Title;
                        MyNotifyIcon.BalloonTipTitle = this.Title;
                        MyNotifyIcon.BalloonTipText = this.Title + Properties.Resources.Notification_Minimization;
                        MyNotifyIcon.ShowBalloonTip(400);
                    }
                    CreateContextMenu();
                }
                else if (this.WindowState == WindowState.Normal)
                {
                    MyNotifyIcon.Visible = false;
                    this.Activate();
                }
            }
        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.Back)
                e.Cancel = true;
        }
    }
}
