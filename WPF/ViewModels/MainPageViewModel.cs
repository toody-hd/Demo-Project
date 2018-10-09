using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using INIHandler;
using System.Linq;
using System.Windows.Threading;

namespace WPF
{
    public class MainPageViewModel
    {
        private Page mPage;
        private INIFile ini;
        private INIFile ini2;
        private bool isInI;
        private int count = 0;
        private string[] updates = { };
        private Uri _uri;
        private WebBrowser _webBrowser;
        private bool navigating = true;
        private string updateList;
        private string tagId;
        private bool alreadyClicked;
        private int clickCounter;
        private bool longPress;
        private bool doOnce;

        public MainPageViewModel(Page page)
        {
            mPage = page;

            SettingsCommand = new RelayCommand(() => ChangePage(new SettingsPage()));
            LogOutCommand = new RelayCommand(() => ChangePage(new LoginPage()));
            AddNewPanelCommand = new RelayCommand(() => AddNewPanel_Click());
            UpdateCommand = new RelayCommand(() => Update());
            SearchCommand = new RelayCommand(() => Search());

            Initializer();
        }

        public ICommand SettingsCommand { get; private set; }
        public ICommand LogOutCommand { get; private set; }
        public ICommand AddNewPanelCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }

        private void Initializer()
        {
            if (File.Exists(@"panels.ini"))
            {
                ini = new INIFile("panels.ini");
                isInI = true;
                if (ini.SectionExists("PanelCount") == true)
                {
                    System.Drawing.Bitmap img;
                    for (int i = 1; i <= int.Parse(ini.Read("PanelCount", "Count")); i++)
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() +
                            @"\pictures\IMG-" + i + ".png"))
                        {
                            using (var bmpTemp = new System.Drawing.Bitmap(Directory.GetCurrentDirectory() +
                                    @"\pictures\IMG-" + i + ".png"))
                            {
                                img = new System.Drawing.Bitmap(bmpTemp);
                            }
                        }
                        else
                            img = null;
                        if (ini.Read("Panel - " + i, "Update") != "Unknown" && DateTime.Parse(ini.Read("Panel - " + i, "Update")) <= DateTime.Now.Date)
                        {
                            count++;
                            Array.Resize(ref updates, count);
                            updates[count - 1] = ini.Read("Panel - " + i, "Name");
                        }
                        NewPanel(i, ini.Read("Panel - " + i, "Name"), ini.Read("Panel - " + i, "Tags"), img);
                    }
                }
                else
                    ini.Write("PanelCount", "Count", "0");
            }
            else
                isInI = false;
        }

        private void ChangePage(object nav)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (ss, ee) =>
            {
                Thread.Sleep(500);
            };

            backgroundWorker.RunWorkerCompleted += (ss, ee) =>
            {
                mPage.NavigationService.Navigate(nav);
            };

            backgroundWorker.RunWorkerAsync();
            (mPage.FindResource("Fade_Shrink") as Storyboard).Begin(mPage);
        }

        private void AddNewPanel_Click()
        {
            AddNewPanel addF = new AddNewPanel() { Owner = (MainWindow)Application.Current.MainWindow };
            addF.ShowDialog();
            if ((addF.DataContext as AddNewPanelViewModel).ok && (addF.DataContext as AddNewPanelViewModel).image != null)
            {
                string tempCounter;
                if (isInI)
                    NewPanel(int.Parse(ini.Read("PanelCount", "Count")) + 1, (addF.DataContext as AddNewPanelViewModel).name, string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags), (addF.DataContext as AddNewPanelViewModel).image);
                else
                {
                    NewPanel(1, (addF.DataContext as AddNewPanelViewModel).name, string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags), (addF.DataContext as AddNewPanelViewModel).image);
                    ini = new INIFile("panels.ini");
                    ini.Write("PanelCount", "Count","0");
                    isInI = true;
                }
                tempCounter = (int.Parse(ini.Read("PanelCount", "Count")) + 1).ToString();
                ini.Write("Panel - " + tempCounter, "Name", (addF.DataContext as AddNewPanelViewModel).name);
                ini.Write("Panel - " + tempCounter, "Link", (addF.DataContext as AddNewPanelViewModel).link);
                ini.Write("Panel - " + tempCounter, "Version", (addF.DataContext as AddNewPanelViewModel).version);
                if ((addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString() != "1/1/0001")
                    ini.Write("Panel - " + tempCounter, "Update", (addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString());
                else
                    ini.Write("Panel - " + tempCounter, "Update", "Unknown");
                ini.Write("Panel - " + tempCounter, "Site", (addF.DataContext as AddNewPanelViewModel).site);
                ini.Write("Panel - " + tempCounter, "Completed", (addF.DataContext as AddNewPanelViewModel).completed.ToString());
                ini.Write("Panel - " + tempCounter, "Tags", string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags));
                ini.DeleteSection("PanelCount");
                ini.Write("PanelCount", "Count", tempCounter);
                if ((addF.DataContext as AddNewPanelViewModel).image != null)
                {
                    if (!Directory.Exists(@"pictures"))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\pictures");
                    }
                    else
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() +
                            @"\pictures\IMG-" + tempCounter + ".png"))
                        {
                            File.Delete(Directory.GetCurrentDirectory() +
                            @"\pictures\IMG-" + tempCounter + ".png");
                        }
                    }
                    (addF.DataContext as AddNewPanelViewModel).image.Save(Directory.GetCurrentDirectory() + @"\pictures\IMG-" + tempCounter + ".png", ImageFormat.Png);
                }
            }
        }

        private void NewPanel(int count, string name, string tags, System.Drawing.Bitmap imageS)
        {
            var spTemp = new StackPanel() { Tag = tags, Width = 140, Height = 110 };
            var bTemp = new Button() { Tag = count.ToString(), Width = 140, Height = 95, Content = new Image() { Width = 160, Height = 90, Source = BitmapConversion.BitmapToBitmapSource(imageS) } };
            bTemp.MouseEnter += BTemp_MouseEnter;
            bTemp.MouseLeave += BTemp_MouseLeave;
            bTemp.PreviewMouseLeftButtonUp += BTemp_PreviewMouseLeftButtonUp;
            bTemp.MouseRightButtonUp += BTemp_MouseRightButtonUp;
            bTemp.PreviewMouseUp += BTemp_PreviewMouseUp;
            //bTemp.PreviewMouseLeftButtonDown += BTemp_PreviewMouseLeftButtonDown;
            //spTemp.MouseMove += SpTemp_MouseMove;
            spTemp.Children.Add(bTemp);
            spTemp.Children.Add(new TextBlock() { Text = name, HorizontalAlignment = HorizontalAlignment.Center });
            (mPage as MainPage)._itemCollection.Add(new ListBoxItem() { Content = spTemp });
        }

        private void BTemp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!doOnce)
            {
                doOnce = true;
                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (ss, ee) =>
                {
                    Thread.Sleep(300);
                };

                backgroundWorker.RunWorkerCompleted += (ss, ee) =>
                {
                    if (e.ButtonState == MouseButtonState.Pressed)
                    {
                        Mouse.OverrideCursor = Cursors.Hand;
                        longPress = true;
                    }
                };

                backgroundWorker.RunWorkerAsync();
            }
        }

        private void BTemp_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            clickCounter++;
            if (!alreadyClicked && !longPress)
            {
                alreadyClicked = true;
                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += (ss, ee) =>
                {
                    Thread.Sleep(200);
                    //Thread.Sleep(System.Windows.Forms.SystemInformation.DoubleClickTime);
                };

                backgroundWorker.RunWorkerCompleted += (ss, ee) =>
                {
                    if (clickCounter == 2)
                        Process.Start(@"chrome.exe", "--incognito " + ini.Read("Panel - " + (sender as Button).Tag, "Site") + "?VERSION=" + ini.Read("Panel - " + (sender as Button).Tag, "Version"));
                    else
                    {
                        bool execute = true;
                        if (ini.Read("Panel - " + ((Button)sender).Tag, "Completed") == "True")
                        {
                            MessageBoxResult result = CustomMessageBox.Show("\"" + ((((Button)sender).Parent as StackPanel).Children[1] as TextBlock).Text + "\"" + Properties.Resources.Message_Completed_Begin + "\n" + Properties.Resources.Message_Completed_End, Properties.Resources.Completed, MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.No)
                            {
                                execute = false;
                            }
                        }
                        if (execute)
                        {
                            string path = ini.Read("Panel - " + ((Button)sender).Tag, "Link");
                            try
                            {
                                Process process = Process.Start(path);
                                ((MainWindow)Application.Current.MainWindow).WindowState = WindowState.Minimized;
                                //Process process = new Process();
                                //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                //process.StartInfo.FileName = "cmd.exe";
                                //process.StartInfo.Arguments = "'/C start /w " + path;
                                //process.Start();
                                process.WaitForExit();
                                ((MainWindow)Application.Current.MainWindow).WindowState = WindowState.Normal;
                                ((MainWindow)Application.Current.MainWindow).Activate();
                            }
                            catch (Exception ex)
                            {
                                CustomMessageBox.Show(ex.Message + ".", Properties.Resources.Error, MessageBoxButton.OK);
                            }

                        }
                    }
                    alreadyClicked = false;
                    clickCounter = 0;

                };
                backgroundWorker.RunWorkerAsync();
            }
            longPress = false;
        }

        private void BTemp_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            AddNewPanel addF = new AddNewPanel() { Owner = (MainWindow)Application.Current.MainWindow };
            (addF.DataContext as AddNewPanelViewModel).name = ini.Read("Panel - " + ((Button)sender).Tag, "Name");
            (addF.DataContext as AddNewPanelViewModel).link = ini.Read("Panel - " + ((Button)sender).Tag, "Link");
            (addF.DataContext as AddNewPanelViewModel).version = ini.Read("Panel - " + ((Button)sender).Tag, "Version");
            if (ini.Read("Panel - " + ((Button)sender).Tag, "Update") != "Unknown")
                (addF.DataContext as AddNewPanelViewModel).updateDate = DateTime.Parse(ini.Read("Panel - " + ((Button)sender).Tag, "Update"));
            (addF.DataContext as AddNewPanelViewModel).site = ini.Read("Panel - " + ((Button)sender).Tag, "Site");
            (addF.DataContext as AddNewPanelViewModel).tags = (ini.Read("Panel - " + ((Button)sender).Tag, "Tags")).Split(new string[] { ";"}, StringSplitOptions.None);
            (addF.DataContext as AddNewPanelViewModel).completed = bool.Parse(ini.Read("Panel - " + ((Button)sender).Tag, "Completed"));
            (addF.DataContext as AddNewPanelViewModel).imageSource = (((Button)sender).Content as Image).Source;
            (addF.DataContext as AddNewPanelViewModel).EditMode();
            addF.ShowDialog();
            if ((addF.DataContext as AddNewPanelViewModel).ok)
            {
                ini.Write("Panel - " + ((Button)sender).Tag, "Name", (addF.DataContext as AddNewPanelViewModel).name);
                ini.Write("Panel - " + ((Button)sender).Tag, "Link", (addF.DataContext as AddNewPanelViewModel).link);
                ini.Write("Panel - " + ((Button)sender).Tag, "Version", (addF.DataContext as AddNewPanelViewModel).version);
                if ((addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString() != "1/1/0001")
                    ini.Write("Panel - " + ((Button)sender).Tag, "Update", (addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString());
                else
                    ini.Write("Panel - " + ((Button)sender).Tag, "Update", "Unknown");
                ini.Write("Panel - " + ((Button)sender).Tag, "Site", (addF.DataContext as AddNewPanelViewModel).site);
                ini.Write("Panel - " + ((Button)sender).Tag, "Completed", (addF.DataContext as AddNewPanelViewModel).completed.ToString());
                ini.Write("Panel - " + ((Button)sender).Tag, "Tags", string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags));

                if ((addF.DataContext as AddNewPanelViewModel).image != null)
                {
                    if (!Directory.Exists(@"pictures"))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\pictures");
                    }
                    else
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() +
                            @"\pictures\IMG-" + ((Button)sender).Tag + ".png"))
                        {
                            File.Delete(Directory.GetCurrentDirectory() +
                            @"\pictures\IMG-" + ((Button)sender).Tag + ".png");
                        }
                    }

                    (addF.DataContext as AddNewPanelViewModel).image.Save(Directory.GetCurrentDirectory() + @"\pictures\IMG-" + ((Button)sender).Tag + ".png", ImageFormat.Png);
                }
                (((Button)sender).Content as Image).Source = (addF.DataContext as AddNewPanelViewModel).imageSource;
                ((((Button)sender).Parent as StackPanel).Children[1] as TextBlock).Text = (addF.DataContext as AddNewPanelViewModel).name;
                (((Button)sender).Parent as StackPanel).Tag = string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags);
                ((Button)sender).Content = new Image() { Width = 160, Height = 90, Source = (addF as AddNewPanel).imagePB.Source };
                foreach(var x in (mPage as MainPage).currentFilter)
                {
                    if (x != null && !string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags).Contains(x))
                    {
                        ((((Button)sender).Parent as StackPanel).Parent as ListBoxItem).Visibility = Visibility.Collapsed;
                        break;
                    }
                }
            }
        }

        private void BTemp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                MessageBoxResult result = CustomMessageBox.Show(Properties.Resources.Message_Delete_Item_Begin + "\"" + ((((Button)sender).Parent as StackPanel).Children[1] as TextBlock).Text + "\"" + Properties.Resources.Message_Delete_Item_End, Properties.Resources.Delete, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    (mPage as MainPage)._itemCollection.Remove((((Button)sender).Parent as StackPanel).Parent as ListBoxItem);
                    if (!Directory.Exists(@"pictures"))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\pictures");
                    }
                    else
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() +
                            @"\pictures\IMG-" + ((Button)sender).Tag + ".png"))
                        {
                            File.Delete(Directory.GetCurrentDirectory() +
                            @"\pictures\IMG-" + ((Button)sender).Tag + ".png");
                        }
                    }
                    for (int i = int.Parse(((Button)sender).Tag.ToString()); i < int.Parse(ini.Read("PanelCount", "Count")); i++)
                    {
                        ini.Write("Panel - " + i, "Name", ini.Read("Panel - " + (i + 1), "Name"));
                        ini.Write("Panel - " + i, "Link", ini.Read("Panel - " + (i + 1), "Link"));
                        ini.Write("Panel - " + i, "Version", ini.Read("Panel - " + (i + 1), "Version"));
                        ini.Write("Panel - " + i, "Update", ini.Read("Panel - " + (i + 1), "Update"));
                        ini.Write("Panel - " + i, "Site", ini.Read("Panel - " + (i + 1), "Site"));
                        ini.Write("Panel - " + i, "Completed", ini.Read("Panel - " + (i + 1), "Completed"));
                        if (File.Exists(Directory.GetCurrentDirectory() + @"\pictures\IMG-" + (i + 1) + ".png"))
                            File.Move(Directory.GetCurrentDirectory() + @"\pictures\IMG-" + (i + 1) + ".png", Directory.GetCurrentDirectory() + @"\pictures\IMG-" + i + ".png");
                    }
                    ini.DeleteSection("Panel - " + ini.Read("PanelCount", "Count"));
                    ini.Write("PanelCount", "Count", (int.Parse(ini.Read("PanelCount", "Count")) - 1).ToString());
                }
            }
        }

        private void BTemp_MouseEnter(object sender, MouseEventArgs e)
        {
            //var sndr = (Button)sender;
           // var prnt = sndr.Parent as StackPanel;
        }

        private void BTemp_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void Update()
        {
            ((MainWindow)Application.Current.MainWindow).oL.Visibility = Visibility.Visible;
            ((MainWindow)Application.Current.MainWindow).oLText.Text = Properties.Resources.Message_Updating;

            if (File.Exists(@"panels.ini"))
            {
                ini = new INIFile("panels.ini");
                _webBrowser = new WebBrowser();
                _webBrowser.Navigated += _webBrowser_Navigated;

                foreach (var x in (mPage as MainPage)._itemCollection.OfType<ListBoxItem>())
                {
                    if (ini.SectionExists("Panel - " + ((x.Content as StackPanel).Children[0] as Button).Tag.ToString()))
                    {
                        if (Uri.IsWellFormedUriString(ini.Read("Panel - " + ((x.Content as StackPanel).Children[0] as Button).Tag.ToString(), "Site"), UriKind.Absolute))
                        {
                            var _found = false;

                            if (Settings.excUpd.Length == 0 && File.Exists(@"filters.ini"))
                            {
                                var _ini = new INIFile("filters.ini");
                                for (int i = 1; i <= int.Parse(_ini.Read("FilterCount", "Count")); i++)
                                {
                                    if (_ini.KeyExists("Filter - " + i, "ExcludeUpdate") && _ini.Read("Filter - " + i, "ExcludeUpdate") == "True")
                                    {
                                        Array.Resize(ref Settings.excUpd, Settings.excUpd.Length + 1);
                                        Settings.excUpd[Settings.excUpd.Length - 1] = _ini.Read("Filter - " + i, "Name");
                                    }
                                }
                            }

                            foreach (var y in Settings.excUpd)
                            {
                                if (("_" + ini.Read("Panel - " + ((x.Content as StackPanel).Children[0] as Button).Tag.ToString(), "Tags") + "_").Replace(";", "_").Contains("_" + y + "_"))
                                {
                                    _found = true;
                                    break;
                                }
                            }

                            if (!_found)
                            {
                                tagId = ((x.Content as StackPanel).Children[0] as Button).Tag.ToString();
                                navigating = true;
                                _uri = new Uri(ini.Read("Panel - " + ((x.Content as StackPanel).Children[0] as Button).Tag.ToString(), "Site"));
                                _webBrowser.Navigate(_uri);
                                while (navigating)
                                {
                                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                                }
                            }
                        }
                    }
                }
                _webBrowser = null;

                ((MainWindow)Application.Current.MainWindow).oLText.Text = "";
                if (updateList != null)
                    CustomMessageBox.Show(updateList.Substring(0, updateList.Length - 1), Properties.Resources.Warning);
                else
                    CustomMessageBox.Show(Properties.Resources.Message_NoUpdates, Properties.Resources.Warning);
                updateList = null;
                ((MainWindow)Application.Current.MainWindow).oL.Visibility = Visibility.Collapsed;
            }
        }

        private void _webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            navigating = false;
            bool _found = false;
            if (_uri != _webBrowser.Source && File.Exists(@"panels.ini"))
            {
                if (File.Exists(@"filters.ini"))
                {
                    ini2 = new INIFile("filters.ini");
                    for (int i = 1; i <= int.Parse(ini2.Read("FilterCount", "Count")); i++)
                    {
                        if (ini2.Read("Filter - " + i, "Name") == "Update")
                        {
                            _found = true;
                            break;
                        }
                    }
                    if (!_found)
                    {
                        ini2.Write("Filter - " + (int.Parse(ini2.Read("FilterCount", "Count")) + 1), "Name", "Update");
                        int _filterCount = int.Parse(ini2.Read("FilterCount", "Count")) + 1;
                        ini2.DeleteSection("FilterCount");
                        ini2.Write("FilterCount", "Count", _filterCount.ToString());
                    }
                }
                updateList += _webBrowser.Source + "\n";
                if (!ini.Read("Panel - " + tagId, "Tags").Contains("Update"))
                {
                    if (!string.IsNullOrWhiteSpace(ini.Read("Panel - " + tagId, "Tags")))
                        ini.Write("Panel - " + tagId, "Tags", ini.Read("Panel - " + tagId, "Tags") + ";Update");
                    else
                        ini.Write("Panel - " + tagId, "Tags", ini.Read("Panel - " + tagId, "Tags") + "Update");
                }

                if(!(((StackPanel)((mPage as MainPage).mainLB.Items[int.Parse(tagId) - 1] as ListBoxItem).Content).Tag).ToString().Contains("Update"))
                    if (!string.IsNullOrWhiteSpace((((StackPanel)((mPage as MainPage).mainLB.Items[1] as ListBoxItem).Content).Tag).ToString()))
                        ((StackPanel)((mPage as MainPage).mainLB.Items[int.Parse(tagId) - 1] as ListBoxItem).Content).Tag += ";Update";
                    else
                        ((StackPanel)((mPage as MainPage).mainLB.Items[int.Parse(tagId) - 1] as ListBoxItem).Content).Tag += "Update";
            }
        }

        private void Search()
        {
            if((mPage as MainPage).searchTB.Width == 0)
            {
                (mPage as MainPage).searchTB.Width = 100;
                (mPage as MainPage).searchTB.IsEnabled = true;
                (mPage as MainPage).searchTB.Focus();
            }
            else
            {
                (mPage as MainPage).searchTB.Width = 0;
                (mPage as MainPage).searchTB.Text = null;
                (mPage as MainPage).searchTB.IsEnabled = false;
            }
        }
    }
}
