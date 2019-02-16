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
using System.Linq;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace WPF
{
    public class MainPageViewModel
    {
        #region Variables
        private MainPage mPage;
        private string[] updates = { };
        private Uri _uri;
        private WebBrowser _webBrowser;
        private bool navigating = true;
        private int panelUpdatedIndex;
        private string[] updateList = { };
        private string[] versionList = { };
        private string[] newVersionList = { };
        private string panelName;
        private bool alreadyClicked;
        private int clickCounter;
        private bool longPress;
        //private bool doOnce;
        private string[] _hideList = { };
        #endregion

        public MainPageViewModel(Page page)
        {
            mPage = (MainPage)page;

            SettingsCommand = new RelayCommand(() => ChangePage(new SettingsPage()));
            LogOutCommand = new RelayCommand(() => ChangePage(new LoginPage()));
            AddNewPanelCommand = new RelayCommand(() => AddNewPanel_Click());
            UpdateCommand = new RelayCommand(() => Update());
            SearchCommand = new RelayCommand(() => Search());

            Initializer();
        }

        #region ICommands
        public ICommand SettingsCommand { get; private set; }
        public ICommand LogOutCommand { get; private set; }
        public ICommand AddNewPanelCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        #endregion

        private void Initializer()
        {
            string[] _tagList = null;
            bool _found = false;
            Visibility _visibility = Visibility.Visible;
            bool _isEnabled = true;

            // Init _hideList
            if (InI.FiltersFileExist())
            {
                foreach (var x in InI.filtersFile.ReadSections())
                {
                    if (InI.filtersFile.Read(x, "HideOnStartUp") == "true")
                    {
                        Array.Resize(ref _hideList, _hideList.Length + 1);
                        _hideList[_hideList.Length - 1] = x;
                    }
                }
            }

            // Init panels
            if (InI.PanelsFileExist() && InI.panelsFile.ReadSections() != null)
            {
                System.Drawing.Bitmap img;
                foreach (var x in InI.panelsFile.ReadSections())
                {
                    if (File.Exists(Directory.GetCurrentDirectory() +
                        @"\pictures\" + x.Replace(":","") + ".png"))
                    {
                        using (var bmpTemp = new System.Drawing.Bitmap(Directory.GetCurrentDirectory() +
                                @"\pictures\" + x.Replace(":", "") + ".png"))
                        {
                            img = new System.Drawing.Bitmap(bmpTemp);
                        }
                    }
                    else
                        img = null;

                    _tagList = InI.panelsFile.Read(x, "Tags").Split(';');

                    if (InI.FiltersFileExist())
                    {
                        if (_hideList.Length < _tagList.Length)
                            foreach (var y in _hideList)
                            {
                                foreach (var z in _tagList)
                                {
                                    if (y == z)
                                    {
                                        _found = true;
                                        break;
                                    }
                                }
                            }
                        else
                            foreach (var y in _tagList)
                            {
                                foreach (var z in _hideList)
                                {
                                    if (y == z)
                                    {
                                        _found = true;
                                        break;
                                    }
                                }
                            }
                    }

                    if (_found)
                    {
                        _visibility = Visibility.Collapsed;
                        _isEnabled = false;
                    }
                    else
                    {
                        _visibility = Visibility.Visible;
                        _isEnabled = true;
                    }
                    _found = false;
                    NewPanel(x, InI.panelsFile.Read(x, "Tags"), img, _visibility, _isEnabled);
                }
            }
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
                Visibility _visibility = Visibility.Visible;
                bool _isEnabled = true;
                if (InI.FiltersFileExist())
                {
                    _hideList = new string[] { };
                    foreach (var x in InI.filtersFile.ReadSections())
                    {
                        if (InI.filtersFile.Read(x, "HideOnStartUp") == "true")
                        {
                            Array.Resize(ref _hideList, _hideList.Length + 1);
                            _hideList[_hideList.Length - 1] = x;
                        }
                    }

                    bool _found = false;
                    if (_hideList.Length < (addF.DataContext as AddNewPanelViewModel).tags.Length)
                        foreach (var x in _hideList)
                        {
                            foreach (var y in (addF.DataContext as AddNewPanelViewModel).tags)
                            {
                                if (x == y)
                                {
                                    _found = true;
                                    break;
                                }
                            }
                        }
                    else
                        foreach (var x in (addF.DataContext as AddNewPanelViewModel).tags)
                        {
                            foreach (var y in _hideList)
                            {
                                if (x == y)
                                {
                                    _found = true;
                                    break;
                                }
                            }
                        }

                    if (_found)
                    {
                        _visibility = Visibility.Collapsed;
                        _isEnabled = false;
                    }
                    else
                    {
                        _visibility = Visibility.Visible;
                        _isEnabled = true;
                    }
                }
                NewPanel((addF.DataContext as AddNewPanelViewModel).name, string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags), (addF.DataContext as AddNewPanelViewModel).image, _visibility, _isEnabled);
                //_counter = (int.Parse(InI.panelsFile.Read("PanelCount", "Count")) + 1).ToString();
                InI.panelsFile.Write((addF.DataContext as AddNewPanelViewModel).name, "Path", (addF.DataContext as AddNewPanelViewModel).path);
                InI.panelsFile.Write((addF.DataContext as AddNewPanelViewModel).name, "Version", (addF.DataContext as AddNewPanelViewModel).version);
                if ((addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString() != "1/1/0001")
                    InI.panelsFile.Write((addF.DataContext as AddNewPanelViewModel).name, "Update", (addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString());
                else
                    InI.panelsFile.Write((addF.DataContext as AddNewPanelViewModel).name, "Update", "Unknown");
                InI.panelsFile.Write((addF.DataContext as AddNewPanelViewModel).name, "Site", (addF.DataContext as AddNewPanelViewModel).site);
                //InI.panelsFile.Write((addF.DataContext as AddNewPanelViewModel).name, "Completed", (addF.DataContext as AddNewPanelViewModel).completed.ToString());
                InI.panelsFile.Write((addF.DataContext as AddNewPanelViewModel).name, "Tags", string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags));
                if ((addF.DataContext as AddNewPanelViewModel).image != null)
                {
                    CreateNewImage(null, 
                        (addF.DataContext as AddNewPanelViewModel).name, 
                        (addF.DataContext as AddNewPanelViewModel).image);
                }
            }
        }

        private void NewPanel(string name, string tags, System.Drawing.Bitmap imageS, Visibility visibility, bool isEnabled)
        {
            var spTemp = new StackPanel() { Tag = tags, Width = 140, Height = 110 };
            var bTemp = new Button() { Tag = name, Width = 140, Height = 95, Content = new Image() { Width = 160, Height = 90, Source = BitmapConversion.BitmapToBitmapSource(imageS) } };
            bTemp.PreviewMouseLeftButtonUp += BTemp_PreviewMouseLeftButtonUp;
            bTemp.MouseRightButtonUp += BTemp_MouseRightButtonUp;
            bTemp.PreviewMouseUp += BTemp_PreviewMouseUp;
            spTemp.Children.Add(bTemp);
            spTemp.Children.Add(new TextBlock() { Text = name, HorizontalAlignment = HorizontalAlignment.Center });
            mPage._itemCollection.Add(new ListBoxItem() { Content = spTemp, Visibility = visibility, IsEnabled = isEnabled });
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
                };

                backgroundWorker.RunWorkerCompleted += (ss, ee) =>
                {
                    if (clickCounter == 2)
                        Process.Start(@"chrome.exe", "--incognito " + InI.panelsFile.Read((sender as Button).Tag.ToString(), "Site") + "?VERSION=" + InI.panelsFile.Read((sender as Button).Tag.ToString(), "Version"));
                    else
                    {
                        bool execute = true;
                        if (InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Completed") == "True")
                        {
                            MessageBoxResult result = CustomMessageBox.Show("\"" + ((((Button)sender).Parent as StackPanel).Children[1] as TextBlock).Text + "\"" + Properties.Resources.Message_Completed_Begin + "\n" + Properties.Resources.Message_Completed_End, Properties.Resources.Completed, MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.No)
                            {
                                execute = false;
                            }
                        }
                        if (execute)
                        {
                            string path = InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Path");
                            try
                            {
                                Process process = Process.Start(path);
                                ((MainWindow)Application.Current.MainWindow).WindowState = WindowState.Minimized;
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
            (addF.DataContext as AddNewPanelViewModel).name = ((Button)sender).Tag.ToString();
            (addF.DataContext as AddNewPanelViewModel).path = InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Path");
            (addF.DataContext as AddNewPanelViewModel).version = InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Version");
            if (InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Update") != "Unknown")
                (addF.DataContext as AddNewPanelViewModel).updateDate = DateTime.Parse(InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Update"));
            (addF.DataContext as AddNewPanelViewModel).site = InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Site");
            (addF.DataContext as AddNewPanelViewModel).tags = (InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Tags")).Split(new string[] { ";" }, StringSplitOptions.None);
            //(addF.DataContext as AddNewPanelViewModel).completed = bool.Parse(InI.panelsFile.Read(((Button)sender).Tag.ToString(), "Completed"));
            (addF.DataContext as AddNewPanelViewModel).imageSource = (((Button)sender).Content as Image).Source;
            (addF.DataContext as AddNewPanelViewModel).EditMode();
            addF.ShowDialog();
            if ((addF.DataContext as AddNewPanelViewModel).ok)
            {
                if (((Button)sender).Tag.ToString() != (addF.DataContext as AddNewPanelViewModel).name)
                {
                    InI.panelsFile.DeleteSection(((Button)sender).Tag.ToString());
                    if ((addF.DataContext as AddNewPanelViewModel).image != null)
                    {
                        CreateNewImage(((Button)sender).Tag.ToString(), 
                            (addF.DataContext as AddNewPanelViewModel).name, 
                            (addF.DataContext as AddNewPanelViewModel).image);
                    }
                    else
                    {
                        CreateNewImage(((Button)sender).Tag.ToString(), 
                            (addF.DataContext as AddNewPanelViewModel).name, 
                            null);
                    }
                }
                else
                {
                    if ((addF.DataContext as AddNewPanelViewModel).image != null)
                    {
                        CreateNewImage(null,
                            (addF.DataContext as AddNewPanelViewModel).name,
                            (addF.DataContext as AddNewPanelViewModel).image);
                    }
                }

                ((Button)sender).Tag = (addF.DataContext as AddNewPanelViewModel).name;
                InI.panelsFile.Write(((Button)sender).Tag.ToString(), "Path", (addF.DataContext as AddNewPanelViewModel).path);
                InI.panelsFile.Write(((Button)sender).Tag.ToString(), "Version", (addF.DataContext as AddNewPanelViewModel).version);
                if ((addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString() != "1/1/0001")
                    InI.panelsFile.Write(((Button)sender).Tag.ToString(), "Update", (addF.DataContext as AddNewPanelViewModel).updateDate.ToShortDateString());
                else
                    InI.panelsFile.Write(((Button)sender).Tag.ToString(), "Update", "Unknown");
                InI.panelsFile.Write(((Button)sender).Tag.ToString(), "Site", (addF.DataContext as AddNewPanelViewModel).site);
                //InI.panelsFile.Write(((Button)sender).Tag.ToString(), "Completed", (addF.DataContext as AddNewPanelViewModel).completed.ToString());
                InI.panelsFile.Write(((Button)sender).Tag.ToString(), "Tags", string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags));

                (((Button)sender).Content as Image).Source = (addF.DataContext as AddNewPanelViewModel).imageSource;
                ((((Button)sender).Parent as StackPanel).Children[1] as TextBlock).Text = (addF.DataContext as AddNewPanelViewModel).name;
                (((Button)sender).Parent as StackPanel).Tag = string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags);
                ((Button)sender).Content = new Image() { Width = 160, Height = 90, Source = (addF as AddNewPanel).imagePB.Source };
                foreach(var x in mPage.currentFilter)
                {
                    if (x != null && !string.Join(";", (addF.DataContext as AddNewPanelViewModel).tags).Contains(x))
                    {
                        ((((Button)sender).Parent as StackPanel).Parent as ListBoxItem).Visibility = Visibility.Collapsed;
                        break;
                    }
                }
            }
        }

        private void CreateNewImage(string source,string destination, System.Drawing.Bitmap image)
        {
            if (!Directory.Exists(@"pictures"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\pictures");
            }
            else
            {
                if (File.Exists(Directory.GetCurrentDirectory() +
                    @"\pictures\" + destination.Replace(":", "") + ".png"))
                {
                    File.Delete(Directory.GetCurrentDirectory() +
                        @"\pictures\" + destination.Replace(":", "") + ".png");
                }

                if (!string.IsNullOrWhiteSpace(source))
                {
                    File.Move(Directory.GetCurrentDirectory() +
                        @"\pictures\" + source.Replace(":", "") + ".png",
                    Directory.GetCurrentDirectory() +
                        @"\pictures\" + destination.Replace(":", "") + ".png");
                }
            }

            if(image != null)
                image.Save(Directory.GetCurrentDirectory() + @"\pictures\" + destination + ".png", ImageFormat.Png);
        }

        private void BTemp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                MessageBoxResult result = CustomMessageBox.Show(Properties.Resources.Message_Delete_Item_Begin + "\"" + ((((Button)sender).Parent as StackPanel).Children[1] as TextBlock).Text + "\"" + Properties.Resources.Message_Delete_Item_End, Properties.Resources.Delete, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    mPage._itemCollection.Remove((((Button)sender).Parent as StackPanel).Parent as ListBoxItem);
                    if (Directory.Exists(@"pictures") &&File.Exists(Directory.GetCurrentDirectory() +
                        @"\pictures\" + ((Button)sender).Tag.ToString() + ".png"))
                    {
                        File.Delete(Directory.GetCurrentDirectory() +
                        @"\pictures\" + ((Button)sender).Tag.ToString() + ".png");
                    }
                    if (InI.PanelsFileExist())
                        InI.panelsFile.DeleteSection(((Button)sender).Tag.ToString());
                }
            }
        }

        private void Update()
        {
            ((MainWindow)Application.Current.MainWindow).oL.Visibility = Visibility.Visible;
            ((MainWindow)Application.Current.MainWindow).oLText.Text = Properties.Resources.Message_Updating;

            if (InI.PanelsFileExist())
            {
                _webBrowser = new WebBrowser();
                _webBrowser.Navigated += _webBrowser_Navigated;
                var _excludeList = new string[] { };

                if (InI.FiltersFileExist())
                {
                    foreach (var x in InI.filtersFile.ReadSections())
                    {
                        if (InI.filtersFile.Read(x, "ExcludeUpdate") == "true")
                        {
                            Array.Resize(ref _excludeList, _excludeList.Length + 1);
                            _excludeList[_excludeList.Length - 1] = x;
                        }
                    }
                }

                foreach (var x in (mPage as MainPage)._itemCollection.OfType<ListBoxItem>())
                {
                    if (InI.panelsFile.SectionExists(
                        ((x.Content as StackPanel).Children[0] as Button).Tag.ToString()))
                    {
                        if(!_excludeList.Intersect(
                            InI.panelsFile.Read(
                                ((x.Content as StackPanel).Children[0] as Button).Tag.ToString(), "Tags")
                                .Split(';')).Any() &&
                            Uri.IsWellFormedUriString(
                                 InI.panelsFile.Read(
                                 ((x.Content as StackPanel).Children[0] as Button).Tag.ToString(), "Site"), 
                                 UriKind.Absolute))
                        {
                            panelName = ((x.Content as StackPanel).Children[0] as Button).Tag.ToString();
                            panelUpdatedIndex = mPage.mainLB.Items.IndexOf(x);
                            navigating = true;
                            _uri = new Uri(InI.panelsFile.Read(
                                ((x.Content as StackPanel).Children[0] as Button).Tag.ToString(), "Site"));
                            _webBrowser.Navigate(_uri);
                            while (navigating)
                            {
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                            }
                        }
                    }
                }
                _excludeList = null;
                _webBrowser = null;

                ((MainWindow)Application.Current.MainWindow).oLText.Text = "";

                #region Creating the expander
                var _nSP = new StackPanel();
                _nSP.Children.Add(new TextBlock
                {
                    Text = Properties.Resources.Add_Name,
                    TextDecorations = TextDecorations.Underline,
                    Margin = new Thickness(0, 0, 20, 0)
                });
                _nSP.Children.Add(new TextBlock
                {
                    Text = string.Join("\n", updateList),
                    Margin = new Thickness(0, 0, 20, 0)
                });

                var _vSP = new StackPanel();
                _vSP.Children.Add(new TextBlock
                {
                    Text = Properties.Resources.Update_Current_Version,
                    TextDecorations = TextDecorations.Underline,
                    Margin = new Thickness(0, 0, 20, 0)
                });
                _vSP.Children.Add(new TextBlock
                {
                    Text = string.Join("\n", versionList),
                    Margin = new Thickness(0, 0, 20, 0)
                });

                var _nvSP = new StackPanel();
                _nvSP.Children.Add(new TextBlock
                {
                    Text = Properties.Resources.Update_New_Version,
                    TextDecorations = TextDecorations.Underline,
                    Margin = new Thickness(0, 0, 0, 0)
                });
                _nvSP.Children.Add(new TextBlock
                {
                    Text = string.Join("\n", newVersionList),
                    Margin = new Thickness(0, 0, 0, 0)
                });

                var _newSP = new StackPanel() { Orientation = Orientation.Horizontal };
                _newSP.Children.Add(_nSP);
                _newSP.Children.Add(_vSP);
                _newSP.Children.Add(_nvSP);

                var _expander = new Expander
                { Content = _newSP,
                  Header = Properties.Resources.Update_Found_Begin + updateList.Length + Properties.Resources.Update_Found_End,
                  Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
                };
                #endregion

                if (updateList.Length > 0)
                    CustomMessageBox.Show(_expander);
                else
                    CustomMessageBox.Show(Properties.Resources.Message_NoUpdates, Properties.Resources.Warning);
                updateList = new string[] { };
                versionList = new string[] { };
                newVersionList = new string[] { };
                ((MainWindow)Application.Current.MainWindow).oL.Visibility = Visibility.Collapsed;
            }
        }

        private void _webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            navigating = false;
            if (_uri != _webBrowser.Source && InI.PanelsFileExist())
            {
                if (InI.FiltersFileExist() && !InI.filtersFile.SectionExists("Update"))
                {
                    InI.filtersFile.Write("Update", "ExcludeUpdate", "true");
                    InI.filtersFile.Write("Update", "HideOnStartUp", "false");
                    (mPage as MainPage).filterList.Add("Update");
                }

                Array.Resize(ref updateList, updateList.Length + 1);
                updateList[updateList.Length - 1] = panelName;
                Array.Resize(ref versionList, versionList.Length + 1);
                versionList[versionList.Length - 1] = InI.panelsFile.Read(panelName, "Version");
                if (InI.ScriptsFileExist() && InI.scriptsFile.SectionExists("Scripts") && InI.scriptsFile.KeyExists("Scripts", "Version"))
                {
                    Array.Resize(ref newVersionList, newVersionList.Length + 1);
                    newVersionList[newVersionList.Length - 1] = Regex.Match(_webBrowser.Source.ToString(), InI.scriptsFile.Read("Scripts", "Version")).Value.Replace("-", ".").TrimStart('.').TrimEnd('.');
                }
                if (!InI.panelsFile.Read(panelName, "Tags").Contains("Update"))
                {
                    if (!string.IsNullOrWhiteSpace(InI.panelsFile.Read(panelName, "Tags")))
                        InI.panelsFile.Write(panelName, "Tags", InI.panelsFile.Read(panelName, "Tags") + ";Update");
                    else
                        InI.panelsFile.Write(panelName, "Tags", InI.panelsFile.Read(panelName, "Tags") + "Update");
                }

                if(!(((StackPanel)(mPage.mainLB.Items[panelUpdatedIndex] as ListBoxItem).Content).Tag).ToString().Contains("Update"))
                    if (!string.IsNullOrWhiteSpace((((StackPanel)(mPage.mainLB.Items[panelUpdatedIndex] as ListBoxItem).Content).Tag).ToString()))
                        ((StackPanel)(mPage.mainLB.Items[panelUpdatedIndex] as ListBoxItem).Content).Tag += ";Update";
                    else
                        ((StackPanel)(mPage.mainLB.Items[panelUpdatedIndex] as ListBoxItem).Content).Tag += "Update";
            }
        }

        private void Search()
        {
            if(mPage.searchTB.Width == 0)
            {
                mPage.searchTB.Width = 100;
                mPage.searchTB.IsEnabled = true;
                mPage.searchTB.Focus();
            }
            else
            {
                mPage.searchTB.Width = 0;
                mPage.searchTB.Text = null;
                mPage.searchTB.IsEnabled = false;
            }
        }
    }
}
