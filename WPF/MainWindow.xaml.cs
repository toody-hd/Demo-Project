using INIHandler;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

namespace WPF
{
    public partial class MainWindow : Window
    {
        public INIFile filterFile = null;
        public INIFile pannelFile = null;
        public INIFile settingsFile = null;

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
            if (File.Exists("filters.ini"))
            {
               filterFile = new INIFile("filters.ini");
            }

            if (File.Exists("panels.ini"))
            {
                pannelFile = new INIFile("panels.ini");
            }

            if (File.Exists("settings.ini"))
            {
                settingsFile = new INIFile("settings.ini");
                InitializeSettings();
            }
            else
            {
                settingsFile = new INIFile("settings.ini");
                settingsFile.Write("General", "MiniMode", "False");
                settingsFile.Write("General", "TopMode", "False");
                settingsFile.Write("General", "Language", "en-US");
                settingsFile.Write("General", "MessageMode", "True");
            }
        }

        private void InitializeSettings()
        {
            Settings._miniMode = Settings.miniMode = bool.Parse(settingsFile.Read("General", "MiniMode"));
            Settings._topmostMode = Settings.topmostMode = bool.Parse(settingsFile.Read("General", "TopMode"));
            Settings._lang = Settings.lang = settingsFile.Read("General", "Language");
            Settings._messageMode = Settings.messageMode = bool.Parse(settingsFile.Read("General", "MessageMode"));
            this.Topmost = Settings.topmostMode;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Settings.lang);
        }

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
