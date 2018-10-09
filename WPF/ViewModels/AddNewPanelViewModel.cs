using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace WPF
{
    public class AddNewPanelViewModel : MainViewModel
    {
        public bool ok = false;
        public string name;
        public string link;
        public string version;
        public string site;
        public string[] tags = { };
        public bool completed;
        public DateTime updateDate;
        public Bitmap image = null;
        public System.Windows.Media.ImageSource imageSource;

        private Window wnd;
        private string _tags = null;
        private string updateSite;

        public AddNewPanelViewModel(Window window) : base(window)
        {
            wnd = window;
            AddCommand = new RelayCommand(() => AddBClick());
            BrowseCommand = new RelayCommand(() => BrowseB());
            UnknownCommand = new RelayCommand(() => UpdateCB());
            PictureCommand = new RelayCommand(() => ImageBClick());
            UpdateLinkCommand = new RelayCommand(() => UpdateLink());
            CustomsCommand = new RelayCommand(() => UseCustoms());

            Initializer();
            Tags();
        }

        public ICommand AddCommand { get; private set; }
        public ICommand BrowseCommand { get; private set; }
        public ICommand UnknownCommand { get; private set; }
        public ICommand CompletedCommand { get; private set; }
        public ICommand PictureCommand { get; private set; }
        public ICommand UpdateLinkCommand { get; private set; }
        public ICommand CustomsCommand { get; private set; }

        private void Initializer()
        {
            (wnd as AddNewPanel).linkTB.AllowDrop = true;
            (wnd as AddNewPanel).imageB.AllowDrop = true;

            (wnd as AddNewPanel).linkTB.PreviewDragOver += LinkTB_PreviewDragOver;
            (wnd as AddNewPanel).linkTB.Drop += LinkTB_Drop;
            (wnd as AddNewPanel).imageB.PreviewDragOver += ImageB_PreviewDragOver;
            (wnd as AddNewPanel).imageB.Drop += ImageB_Drop;
            (wnd as AddNewPanel).tagsCB.Items.Add(Properties.Resources.NoneSelected);
            (wnd as AddNewPanel).tagsCB.SelectedIndex = 0;
        }

        public void EditMode()
        {
            (wnd as AddNewPanel).titleTB.Text = Properties.Resources.Edit_Title + " \"" + name + "\"";
            (wnd as AddNewPanel).nameTB.Text = name;
            (wnd as AddNewPanel).linkTB.Text = link;
            (wnd as AddNewPanel).versionTB.Text = version;
            (wnd as AddNewPanel).siteTB.Text = updateSite = site;
            int i = 0;
            foreach (var x in (wnd as AddNewPanel).tagsCB.Items)
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
            (wnd as AddNewPanel).tagsCB.Items[0] = _tags;
            (wnd as AddNewPanel).tagsCB.SelectedIndex = 0;
            (wnd as AddNewPanel).completedCB.IsChecked = completed;
            if (updateDate != null && updateDate.ToShortDateString() != "1/1/0001")
                (wnd as AddNewPanel).updateDTP.SelectedDate = updateDate;
            else
            {
                (wnd as AddNewPanel).updateCB.IsChecked = true;
                (wnd as AddNewPanel).updateDTP.IsEnabled = false;
            }
            (wnd as AddNewPanel).imagePB.Source = imageSource;
            (wnd as AddNewPanel).addB.Content = Properties.Resources.Save_Button;

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
                (wnd as AddNewPanel).imagePB.Source = BitmapConversion.BitmapToBitmapSource(bmpTemp);
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
            (wnd as AddNewPanel).linkTB.Text = link[0];
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
                    (wnd as AddNewPanel).linkTB.Text = ofd.FileName;
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message + ".", Properties.Resources.Error, MessageBoxButton.OK);
                }
            }
        }

        private void UpdateCB()
        {
            if ((wnd as AddNewPanel).updateCB.IsChecked == true)
                (wnd as AddNewPanel).updateDTP.IsEnabled = false;
            else
                (wnd as AddNewPanel).updateDTP.IsEnabled = true;
        }

        private void AddBClick()
        {
            if ((wnd as AddNewPanel).nameTB.Text != "" && (wnd as AddNewPanel).linkTB.Text.EndsWith(".exe") && (wnd as AddNewPanel).versionTB.Text != "" && (wnd as AddNewPanel).siteTB.Text != "" && (wnd as AddNewPanel).imagePB.Source != null /* image != null */ && (((wnd as AddNewPanel).updateCB.IsChecked == false && (wnd as AddNewPanel).updateDTP.SelectedDate != null) || ((wnd as AddNewPanel).updateCB.IsChecked == true)))
            {
                ok = true;
                name = (wnd as AddNewPanel).nameTB.Text;
                link = (wnd as AddNewPanel).linkTB.Text;
                version = (wnd as AddNewPanel).versionTB.Text;
                if (updateSite != (wnd as AddNewPanel).siteTB.Text)
                {
                    foreach (var x in (wnd as AddNewPanel).tagsCB.Items)
                        if (x is System.Windows.Controls.CheckBox && (x as System.Windows.Controls.CheckBox).Name == "Update")
                        { (x as System.Windows.Controls.CheckBox).IsChecked = false; break; }
                }
                site = (wnd as AddNewPanel).siteTB.Text;
                int i = 0;
                tags = new string[] { };
                foreach (var x in (wnd as AddNewPanel).tagsCB.Items)
                {
                    if (x is System.Windows.Controls.CheckBox && (x as System.Windows.Controls.CheckBox).IsChecked == true)
                    {
                        i++;
                        Array.Resize(ref tags, i);
                        tags[i - 1] = (x as System.Windows.Controls.CheckBox).Content.ToString();
                    }
                }
                completed = (bool)(wnd as AddNewPanel).completedCB.IsChecked;
                if ((wnd as AddNewPanel).updateDTP.SelectedDate != null && !(wnd as AddNewPanel).updateCB.IsChecked == true)
                    updateDate = (DateTime)(wnd as AddNewPanel).updateDTP.SelectedDate;
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
                                (wnd as AddNewPanel).imagePB.Source = BitmapConversion.BitmapToBitmapSource(bmpTemp);
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
            if ((System.Windows.Application.Current.MainWindow as MainWindow).filterFile != null)
            {
                for (int i = 1; i <= int.Parse((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count")); i++)
                {
                    var _cb = new System.Windows.Controls.CheckBox {Tag = (System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name"), Content = (System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name") };
                    _cb.Click += _cb_Click;
                    (wnd as AddNewPanel).tagsCB.Items.Add(_cb);
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
                (wnd as AddNewPanel).tagsCB.Items[0] = _tags;
                (wnd as AddNewPanel).tagsCB.SelectedIndex = 0;
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
                (wnd as AddNewPanel).tagsCB.Items[0] = _tags;
                (wnd as AddNewPanel).tagsCB.SelectedIndex = 0;
            }
        }

        private void UpdateLink()
        {
            System.Windows.Controls.WebBrowser _webBrowser = new System.Windows.Controls.WebBrowser();
            _webBrowser.Navigated += _webBrowser_Navigated;
            if(Uri.IsWellFormedUriString((wnd as AddNewPanel).siteTB.Text, UriKind.Absolute))
            {
                _webBrowser.Navigate((wnd as AddNewPanel).siteTB.Text);
                (wnd as AddNewPanel).updateLinkBT.Content = "Updating...";
            }
            else
                (wnd as AddNewPanel).updateLinkBT.Content = "Invalid Link";
        }

        private void _webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if ((wnd as AddNewPanel).siteTB.Text != (sender as System.Windows.Controls.WebBrowser).Source.ToString())
            {
                foreach (var x in (wnd as AddNewPanel).tagsCB.Items)
                    if (x is System.Windows.Controls.CheckBox && (x as System.Windows.Controls.CheckBox).Tag.ToString() == "Update" && (x as System.Windows.Controls.CheckBox).IsChecked == true)
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
                        (wnd as AddNewPanel).tagsCB.Items[0] = _tags;
                        (wnd as AddNewPanel).tagsCB.SelectedIndex = 0;
                        break;
                    }
                (wnd as AddNewPanel).siteTB.Text = (sender as System.Windows.Controls.WebBrowser).Source.ToString();
            }
            (wnd as AddNewPanel).updateLinkBT.Content = "Done";
        }

        private void UseCustoms()
        {
            
        }
    }
}
