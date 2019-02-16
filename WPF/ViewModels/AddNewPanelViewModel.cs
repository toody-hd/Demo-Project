using INIHandler;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace WPF
{
    public class AddNewPanelViewModel : MainViewModel
    {
        #region Variables
        public bool ok = false;
        public string name;
        public string path;
        public string version;
        public string site;
        public string[] tags = { };
        public bool completed;
        public DateTime updateDate;
        public Bitmap image = null;
        public System.Windows.Media.ImageSource imageSource;

        private AddNewPanel wnd;
        private string _tags = null;
        private string updateSite;
        #endregion

        public AddNewPanelViewModel(Window window) : base(window)
        {
            wnd = (AddNewPanel)window;
            AddCommand = new RelayCommand(() => AddBClick());
            BrowseCommand = new RelayCommand(() => BrowseB());
            UnknownCommand = new RelayCommand(() => UpdateCB());
            PictureCommand = new RelayCommand(() => ImageBClick());
            UpdateLinkCommand = new RelayCommand(() => UpdateLink());

            Initializer();
            Tags();
        }

        #region ICommands
        public ICommand AddCommand { get; private set; }
        public ICommand BrowseCommand { get; private set; }
        public ICommand UnknownCommand { get; private set; }
        public ICommand CompletedCommand { get; private set; }
        public ICommand PictureCommand { get; private set; }
        public ICommand UpdateLinkCommand { get; private set; }
        #endregion

        private void Initializer()
        {
            wnd.pathTB.AllowDrop = true;
            wnd.imageB.AllowDrop = true;

            wnd.pathTB.PreviewDragOver += LinkTB_PreviewDragOver;
            wnd.pathTB.Drop += LinkTB_Drop;
            wnd.imageB.PreviewDragOver += ImageB_PreviewDragOver;
            wnd.imageB.Drop += ImageB_Drop;
            wnd.tagsCB.Items.Add(Properties.Resources.NoneSelected);
            wnd.tagsCB.SelectedIndex = 0;
            wnd.siteTB.TextChanged += SiteTB_TextChanged;
        }
        
        public void EditMode()
        {
            wnd.titleTB.Text = Properties.Resources.Edit_Title + " \"" + name + "\"";
            wnd.nameTB.Text = name;
            wnd.pathTB.Text = path;
            wnd.versionTB.Text = version;
            wnd.siteTB.Text = updateSite = site;
            int i = 0;
            foreach (var x in wnd.tagsCB.Items)
            {
                if (x.GetType() == typeof(System.Windows.Controls.CheckBox) && (x as System.Windows.Controls.CheckBox).Content.ToString() == tags[i])
                {
                    i++;
                    (x as System.Windows.Controls.CheckBox).IsChecked = true;
                    if (_tags == null)
                        _tags = (x as System.Windows.Controls.CheckBox).Content.ToString();
                    else
                        _tags = _tags + "," + (x as System.Windows.Controls.CheckBox).Content.ToString();
                    if (tags.Length == i)
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(_tags))
                _tags = Properties.Resources.NoneSelected;
            wnd.tagsCB.Items[0] = _tags;
            wnd.tagsCB.SelectedIndex = 0;
            wnd.completedCB.IsChecked = completed;
            if (updateDate != null && updateDate.ToShortDateString() != "1/1/0001")
                wnd.updateDTP.SelectedDate = updateDate;
            else
            {
                wnd.updateCB.IsChecked = true;
                wnd.updateDTP.IsEnabled = false;
            }
            wnd.imagePB.Source = imageSource;
            wnd.addB.Content = Properties.Resources.Save_Button;

        }

        private void ImageB_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Handled = true;

            bool IsImage = false;
            try
            {
                string[] link = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (link.Length == 1)
                {
                    using (var bmpTemp = new Bitmap(link[0]))
                    {
                        Bitmap img = new Bitmap(bmpTemp);
                    }
                    IsImage = true;
                }
            }
            catch
            {
                IsImage = false;
            }

            if (IsImage)
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;
        }

        private void ImageB_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] link = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

            var imgTemp = new Bitmap(link[0]);
            System.Drawing.Size size = new System.Drawing.Size();

            if (imgTemp.Width == imgTemp.Height)
                size = new System.Drawing.Size(300, 300);
            else if (imgTemp.Width > imgTemp.Height)
            {
                size = new System.Drawing.Size(300, (int)(300 / ((float)imgTemp.Width / (float)imgTemp.Height)));
            }
            else
            {
                size = new System.Drawing.Size((int)(300 / ((float)imgTemp.Height / (float)imgTemp.Width)), 300);
            }

            using (var bmpTemp = new Bitmap(new Bitmap(link[0]), size))
            {
                wnd.imagePB.Source = BitmapConversion.BitmapToBitmapSource(bmpTemp);
                image = new Bitmap(bmpTemp);
            }
        }

        private void LinkTB_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Handled = true;
            string[] link = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (link.Length == 1 && link[0].EndsWith(".exe"))
                e.Effects = System.Windows.DragDropEffects.Copy;
            else
                e.Effects = System.Windows.DragDropEffects.None;
        }

        private void LinkTB_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] link = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            wnd.pathTB.Text = link[0];
        }

        private void BrowseB()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Executable files| *.exe",
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wnd.pathTB.Text = ofd.FileName;
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message + ".", Properties.Resources.Error, MessageBoxButton.OK);
                }
            }
        }

        private void UpdateCB()
        {
            if (wnd.updateCB.IsChecked == true)
                wnd.updateDTP.IsEnabled = false;
            else
                wnd.updateDTP.IsEnabled = true;
        }

        private void AddBClick()
        {
            if (wnd.nameTB.Text != "" && wnd.pathTB.Text.EndsWith(".exe") && wnd.versionTB.Text != "" && wnd.siteTB.Text != "" && wnd.imagePB.Source != null /* image != null */ && ((wnd.updateCB.IsChecked == false && wnd.updateDTP.SelectedDate != null) || (wnd.updateCB.IsChecked == true)))
            {
                ok = true;
                name = wnd.nameTB.Text;
                path = wnd.pathTB.Text;
                version = wnd.versionTB.Text;
                if (updateSite != wnd.siteTB.Text)
                {
                    foreach (var x in wnd.tagsCB.Items)
                        if (x is System.Windows.Controls.CheckBox && (x as System.Windows.Controls.CheckBox).Name == "Update")
                        { (x as System.Windows.Controls.CheckBox).IsChecked = false; break; }
                }
                site = wnd.siteTB.Text;
                int i = 0;
                tags = new string[] { };
                foreach (var x in wnd.tagsCB.Items)
                {
                    if (x is System.Windows.Controls.CheckBox && (x as System.Windows.Controls.CheckBox).IsChecked == true)
                    {
                        i++;
                        Array.Resize(ref tags, i);
                        tags[i - 1] = (x as System.Windows.Controls.CheckBox).Content.ToString();
                    }
                }
                completed = (bool)wnd.completedCB.IsChecked;
                if (wnd.updateDTP.SelectedDate != null && !wnd.updateCB.IsChecked == true)
                    updateDate = (DateTime)wnd.updateDTP.SelectedDate;
                else
                    updateDate = new DateTime();
                wnd.Close();
            }
            else
            {
                CustomMessageBox.Show(Properties.Resources.Message_Invalid_Parameters, Properties.Resources.Warning, MessageBoxButton.OK);
            }
        }

        private void ImageBClick()
        {
            Stream myStream = null;
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image files| *.bmp; *.png; *.jpg; *.jpeg; *.gif |All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = ofd.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            var imgTemp = new Bitmap(ofd.FileName);
                            System.Drawing.Size size = new System.Drawing.Size();

                            if (imgTemp.Width == imgTemp.Height)
                                size = new System.Drawing.Size(300, 300);
                            else if (imgTemp.Width > imgTemp.Height)
                            {
                                size = new System.Drawing.Size(300, (int)(300 / ((float)imgTemp.Width / (float)imgTemp.Height)));
                            }
                            else
                            {
                                size = new System.Drawing.Size((int)(300 / ((float)imgTemp.Height / (float)imgTemp.Width)), 300);
                            }

                            using (var bmpTemp = new Bitmap(new Bitmap(ofd.FileName), size))
                            {
                                wnd.imagePB.Source = BitmapConversion.BitmapToBitmapSource(bmpTemp);
                                image = new Bitmap(bmpTemp);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, Properties.Resources.Error, MessageBoxButton.OK);
                }
            }
        }

        private void Tags()
        {
            if (InI.FiltersFileExist())
            {
                foreach (var x in InI.filtersFile.ReadSections())
                {
                    var _cb = new System.Windows.Controls.CheckBox { Content = x };
                    _cb.Click += _cb_Click;
                    wnd.tagsCB.Items.Add(_cb);
                    _cb = null;
                }
            }
        }

        private void _cb_Click(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.Controls.CheckBox)sender).IsChecked == true)
            {
                if (_tags == null || _tags == Properties.Resources.NoneSelected)
                    _tags = ((System.Windows.Controls.CheckBox)sender).Content.ToString();
                else
                    _tags = _tags + "," + ((System.Windows.Controls.CheckBox)sender).Content.ToString();
                wnd.tagsCB.Items[0] = _tags;
                wnd.tagsCB.SelectedIndex = 0;
            }
            else
            {
                _tags = _tags.Replace(((System.Windows.Controls.CheckBox)sender).Content.ToString(), "");
                _tags = _tags.Replace(",,", ",");
                if (_tags.EndsWith(","))
                    _tags = _tags.Remove(_tags.Length - 1, 1);
                if (_tags.StartsWith(","))
                    _tags = _tags.Remove(0, 1);
                if (string.IsNullOrWhiteSpace(_tags))
                    _tags = Properties.Resources.NoneSelected;
                wnd.tagsCB.Items[0] = _tags;
                wnd.tagsCB.SelectedIndex = 0;
            }
        }

        private void UpdateLink()
        {
            System.Windows.Controls.WebBrowser _webBrowser = new System.Windows.Controls.WebBrowser();
            _webBrowser.Navigated += _webBrowser_Navigated;
            if(Uri.IsWellFormedUriString(wnd.siteTB.Text, UriKind.Absolute))
            {
                _webBrowser.Navigate(wnd.siteTB.Text);
                wnd.updateLinkBT.Content = "Updating...";
            }
            else
                wnd.updateLinkBT.Content = "Invalid Link";
        }

        private void _webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (wnd.siteTB.Text != (sender as System.Windows.Controls.WebBrowser).Source.ToString())
            {
                foreach (var x in wnd.tagsCB.Items)
                    if (x is System.Windows.Controls.CheckBox && 
                        (x as System.Windows.Controls.CheckBox).IsChecked == true)
                    {
                        (x as System.Windows.Controls.CheckBox).IsChecked = false;
                        _tags = _tags.Replace((x as System.Windows.Controls.CheckBox).Content.ToString(), "");
                        _tags = _tags.Replace(",,", ",");
                        if (_tags.EndsWith(","))
                            _tags = _tags.Remove(_tags.Length - 1, 1);
                        if (_tags.StartsWith(","))
                            _tags = _tags.Remove(0, 1);
                        if (string.IsNullOrWhiteSpace(_tags))
                            _tags = Properties.Resources.NoneSelected;
                        wnd.tagsCB.Items[0] = _tags;
                        wnd.tagsCB.SelectedIndex = 0;
                        break;
                    }
                wnd.siteTB.Text = (sender as System.Windows.Controls.WebBrowser).Source.ToString();
            }
            wnd.updateLinkBT.Content = "Done";
        }

        private void SiteTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (InI.ScriptsFileExist() && InI.scriptsFile.SectionExists("Scripts"))
            {
                if (string.IsNullOrEmpty(wnd.nameTB.Text) && InI.scriptsFile.KeyExists("Scripts", "Name") &&
                    !string.IsNullOrEmpty(Regex.Match(wnd.siteTB.Text,
                        InI.scriptsFile.Read("Scripts", "Name")).Groups["name"].Value.Replace("-", " ")))
                    wnd.nameTB.Text = Regex.Match(wnd.siteTB.Text,
                        InI.scriptsFile.Read("Scripts", "Name")).Groups["name"].Value.Replace("-", " ");
                if (InI.scriptsFile.KeyExists("Scripts", "Version") && 
                    !string.IsNullOrEmpty(Regex.Match(wnd.siteTB.Text,
                        InI.scriptsFile.Read("Scripts", "Version")).Value.Replace("-", ".").TrimStart('.').TrimEnd('.')))
                    wnd.versionTB.Text = Regex.Match(wnd.siteTB.Text,
                        InI.scriptsFile.Read("Scripts", "Version")).Value.Replace("-", ".").TrimStart('.').TrimEnd('.');
            }
        }
    }
}
