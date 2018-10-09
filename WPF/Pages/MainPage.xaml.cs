using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace WPF
{
    public partial class MainPage : Page
    {
        private TextBox tb;
        private Button btn;
        private Grid grd;
        private TextBlock sortTB;
        private ComboBox sortCB;
        private Button sortB;
        private Button sortDelB;
        private string sortAddT = Properties.Resources.Sort;
        private int counter = 0;
        private bool doOnce = true;
        private bool newFilter = false;
        //private Uri _uri;
        //private WebBrowser _webBrowser;
        private bool mouseDownOnElement;
        private ObservableCollection<string> filterList = new ObservableCollection<string>();
        private bool[] _panelStateChanged = new bool[0];

        public ObservableCollection<ListBoxItem> _itemCollection = new ObservableCollection<ListBoxItem>();
        public string[] currentFilter = new string[0];

        public MainPage()
        {
            InitializeComponent();
            DataContext = new MainPageViewModel(this);
            InitializeFilterList();
            InitializeSortCB();
            mainLB.ItemsSource = _itemCollection;
        }

        private void InitializeFilterList()
        {
            filterList.Add(Properties.Resources.NoneSelected);
            if ((Application.Current.MainWindow as MainWindow).filterFile != null)
            {
                for (int i = 1; i <= int.Parse((Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count")); i++)
                {
                    filterList.Add((Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name"));
                }
            }
        }

        private void InitializeSortCB()
        {
            counter++;
            if (sortSP.Children.Contains(sortB))
            {
                sortSP.Children.Remove(sortB);
                if (doOnce)
                {
                    sortDelB = new Button() { Content = "-", VerticalAlignment = VerticalAlignment.Center, Width = 10, HorizontalAlignment = HorizontalAlignment.Right };
                    sortDelB.Click += (ss, ee) => DeleteSortCB();
                    gridSP.Children.Add(sortDelB);
                    doOnce = false;
                }
            }

            //Style itemContainerStyle = new Style(typeof(ComboBoxItem));
            //itemContainerStyle.Setters.Add(new EventSetter(ComboBoxItem.MouseLeftButtonDownEvent, new MouseButtonEventHandler(_CBI_MouseLeftButtonDown)));

            sortCB = new ComboBox() { Tag = counter, VerticalAlignment = VerticalAlignment.Center };
            sortCB.ItemContainerStyle = (Style)Resources["ComboBoxItemStyle"];
            btn = new Button() { Content = Properties.Resources.AddNewSort, Style = FindResource("StripButton") as Style };
            tb = new TextBox() { Width = 70, Visibility = Visibility.Collapsed };
            grd = new Grid();
            grd.Children.Add(btn);
            grd.Children.Add(tb);
            //ComboBoxItem _tbItem = (new ComboBoxItem() { Content = tb });
            //btn.Click += (object ss, RoutedEventArgs ee) => { filterList.RemoveAt(filterList.Count - 1); filterList.Add(_tbItem); (_tbItem.Content as TextBox).Focus(); };
            btn.Click += (object ss, RoutedEventArgs ee) => {
                (ss as Button).Visibility = Visibility.Collapsed;
                (((ss as Button).Parent as Grid).Children[1] as TextBox).Visibility = Visibility.Visible;
                (((ss as Button).Parent as Grid).Children[1] as TextBox).Focus();
            };
            sortCB.ItemsSource = (new ObservableCollection<object>(filterList) { grd });
            sortCB.SelectedIndex = 0;
            sortCB.SelectionChanged += SortCB_SelectionChanged;
            sortCB.DropDownOpened += SortCB_DropDownOpened;
            sortCB.GotFocus += SortCB_GotFocus;
            //sortCB.DropDownClosed += SortCB_DropDownClosed;
            sortB = new Button() { Content = "+", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
            sortB.Click += (ss, ee) => InitializeSortCB();
            if ((Application.Current.MainWindow as MainWindow).filterFile == null ||
                int.Parse((Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count")) == counter || counter > 4)
                sortB.Visibility = Visibility.Collapsed;
            sortTB = new TextBlock() { Text = sortAddT, Tag = new object[2] { counter, false }, VerticalAlignment = VerticalAlignment.Center };
            sortTB.MouseLeftButtonDown += SortTB_MouseLeftButtonDown;
            sortSP.Children.Add(sortTB);
            sortSP.Children.Add(sortCB);
            sortSP.Children.Add(sortB);
            sortAddT = "  &  ";
        }

        private void SortCB_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTB.Text != null)
            {
                searchTB.Width = 0;
                searchTB.Text = null;
                searchTB.IsEnabled = false;
            }
        }

        private void UpdateSortCB()
        {
            for (int i = 1; i < (sortSP.Children.Count - 1); i += 2)
                ((ComboBox)sortSP.Children[i]).ItemsSource = (new ObservableCollection<object>(filterList) { grd });
        }

        private void SortTB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as TextBlock).Text == Properties.Resources.Sort)
            {
                (sender as TextBlock).Text = Properties.Resources.Exclude;
                ((sender as TextBlock).Tag as object[])[1] = true;
            }
            else if ((sender as TextBlock).Text == Properties.Resources.Exclude)
            {
                (sender as TextBlock).Text = Properties.Resources.Sort;
                ((sender as TextBlock).Tag as object[])[1] = false;
            }
            else if ((sender as TextBlock).Text == "  &  ")
            {
                (sender as TextBlock).Text = " - ";
                ((sender as TextBlock).Tag as object[])[1] = true;
            }
            else if ((sender as TextBlock).Text == " - ")
            {
                (sender as TextBlock).Text = "  &  ";
                ((sender as TextBlock).Tag as object[])[1] = false;
            }

            if ((sortSP.Children[((int)(((sender as TextBlock).Tag as object[])[0]) * 2) - 1] as ComboBox).SelectedIndex > 0)
            {
                SortCB_SelectionChanged((sortSP.Children[((int)(((sender as TextBlock).Tag as object[])[0]) * 2) - 1] as ComboBox), null);
            }
        }

        private void SortCB_SelectionChanged(object sender, EventArgs e)
        {
            newFilter = false;
            if (((ComboBox)sender).SelectedIndex == ((ComboBox)sender).Items.Count - 1)
            {
                if(!string.IsNullOrEmpty(tb.Text))
                {
                bool found = false;
                foreach (var x in ((ComboBox)sender).Items)
                {
                    if (x.ToString() == tb.Text)
                    {
                        found = true;
                        CustomMessageBox.Show(Properties.Resources.Message_Add_FilterExist_Begin + "\"" + tb.Text + "\"" + Properties.Resources.Message_Add_FilterExist_End, Properties.Resources.Warning);
                        ((ComboBox)sender).SelectedIndex = 0;
                        break;
                    }
                }
                    if (!found)
                    {
                        MessageBoxResult result = CustomMessageBox.Show(Properties.Resources.Message_Add_Filter_Begin + "\"" + tb.Text + "\"" + Properties.Resources.Message_Add_Filter_End, Properties.Resources.Message, MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            filterList.Add(tb.Text);
                            UpdateSortCB();
                            if ((Application.Current.MainWindow as MainWindow).filterFile == null)
                            {
                                using (File.Create("filters.ini"))
                                    (Application.Current.MainWindow as MainWindow).filterFile = new INIHandler.INIFile("filters.ini");
                            }
                            (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + (filterList.Count - 1).ToString(), "Name", tb.Text);
                            (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + (filterList.Count - 1).ToString(), "ExcludeUpdate", "False");
                            (Application.Current.MainWindow as MainWindow).filterFile.DeleteSection("FilterCount");
                            (Application.Current.MainWindow as MainWindow).filterFile.Write("FilterCount", "Count", (filterList.Count - 1).ToString());
                            newFilter = true;
                            ((ComboBox)sender).SelectedIndex = filterList.Count - 1;
                        }
                        if (!sortB.IsVisible && ((ComboBox)sender).Items.Count >= 4)
                            sortB.Visibility = Visibility.Visible;
                        else
                        {
                            ((ComboBox)sender).SelectedIndex = 0;
                        }
                    }
                }
                (((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[0].Visibility = Visibility.Visible;
                (((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[1].Visibility = Visibility.Collapsed;
                tb.Text = "";
                ((ComboBox)sender).SelectedIndex = 0;
            }

            if (!newFilter)
            {
                Array.Resize(ref currentFilter, counter);
                if (((ComboBox)sender).SelectedIndex > 0)
                {
                    currentFilter[(int)((ComboBox)sender).Tag - 1] = ((ComboBox)sender).SelectedItem.ToString();
                }
                else
                {
                    currentFilter[(int)((ComboBox)sender).Tag - 1] = null;
                }
                FilterPanels();
            }
        }

        private void FilterPanels()
        {
            foreach (var x in mainLB.Items)
            {
                int _cont = -1;
                foreach (var y in currentFilter)
                {
                    _cont++;
                    if ((bool)((sortSP.Children[_cont] as TextBlock).Tag as object[])[1] == false)
                        if (y != null && !((StackPanel)(x as ListBoxItem).Content).Tag.ToString().Contains(y))
                        {
                            (x as ListBoxItem).Visibility = Visibility.Collapsed;
                            break;
                        }
                        else
                        {
                            (x as ListBoxItem).Visibility = Visibility.Visible;
                        }
                    else
                        if (y != null && ((StackPanel)(x as ListBoxItem).Content).Tag.ToString().Contains(y) && (sortSP.Children[_cont + 1] as ComboBox).SelectedIndex > 0)
                    {
                        (x as ListBoxItem).Visibility = Visibility.Collapsed;
                        break;
                    }
                    else
                    {
                        (x as ListBoxItem).Visibility = Visibility.Visible;
                    }
                    _cont++;
                }
            }
        }

        private void DeleteSortCB()
        {
            counter--;
            if (counter == 1)
            {
                sortDelB.Visibility = Visibility.Collapsed;
                doOnce = true;
            }
            sortSP.Children.RemoveAt(sortSP.Children.Count - 2);
            sortSP.Children.RemoveAt(sortSP.Children.Count - 2);
            sortB.Visibility = Visibility.Visible;
            Array.Resize(ref currentFilter, counter);
            FilterPanels();
        }
        
        private void SortCB_DropDownOpened(object sender, EventArgs e)
        {
            if((((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[1].Visibility == Visibility.Visible)
            {
                (((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[0].Visibility = Visibility.Visible;
                (((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[1].Visibility = Visibility.Collapsed;
            }
            var _list = new ObservableCollection<string>(filterList);
            int y = 0;
            foreach (var x in currentFilter)
            {
                y++;
                if ((int)((ComboBox)sender).Tag != y)
                    _list.Remove(x);
            }
            ((ComboBox)sender).ItemsSource = (new ObservableCollection<object>(_list) { grd });
        }

        private void _CBI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Middle && ((ComboBoxItem)sender).Content is string && ((ComboBoxItem)sender).Content.ToString() != Properties.Resources.NoneSelected)
            {
                MessageBoxResult _result = CustomMessageBox.Show(Properties.Resources.Message_DeleteFilter_Begin + ((ComboBoxItem)sender).Content.ToString() + Properties.Resources.Message_DeleteFilter_End, Properties.Resources.Delete, MessageBoxButton.YesNo);
                if (_result == MessageBoxResult.Yes)
                {
                    int _filterCounter = int.Parse((Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count"));
                    for (int i = filterList.IndexOf(((ComboBoxItem)sender).DataContext.ToString()) + 1; i <= _filterCounter; i++)
                    {
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + (i - 1), "Name", (Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name"));
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + (i - 1), "ExcludeUpdate", (Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "ExcludeUpdate"));
                    }
                    filterList.Remove(((ComboBoxItem)sender).DataContext.ToString());
                    UpdateSortCB();
                    (Application.Current.MainWindow as MainWindow).filterFile.DeleteSection("Filter - " + _filterCounter);
                    (Application.Current.MainWindow as MainWindow).filterFile.DeleteSection("FilterCount");
                    (Application.Current.MainWindow as MainWindow).filterFile.Write("FilterCount", "Count", (_filterCounter - 1).ToString());
                    if (sortB.IsVisible && (sortSP.Children[1] as ComboBox).Items.Count <= 4)
                        sortB.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SortCB_DropDownClosed(object sender, EventArgs e)
        {
            Array.Resize(ref currentFilter, counter);
            if (((ComboBox)sender).SelectedIndex != 0 && ((ComboBox)sender).SelectedIndex != ((ComboBox)sender).Items.Count - 1)
            {
                currentFilter[(int)((ComboBox)sender).Tag - 1] = ((ComboBox)sender).SelectedItem.ToString();
                foreach (var x in mainLB.Items)
                {
                    foreach (var y in currentFilter)
                    {
                        if (y != null && !((StackPanel)(x as ListBoxItem).Content).Tag.ToString().Contains(y))
                        {
                            (x as ListBoxItem).Visibility = Visibility.Collapsed;
                            break;
                        }
                        else
                            (x as ListBoxItem).Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                currentFilter[(int)((ComboBox)sender).Tag - 1] = null;
                foreach (var x in mainLB.Items)
                {
                    foreach (var y in currentFilter)
                    {
                        if (y != null && !((StackPanel)(x as ListBoxItem).Content).Tag.ToString().Contains(y))
                        {
                            (x as ListBoxItem).Visibility = Visibility.Collapsed;
                            break;
                        }
                        else
                            (x as ListBoxItem).Visibility = Visibility.Visible;
                    }
                }
            }

            if (!string.IsNullOrEmpty(tb.Text))
            {
                newFilter = false;
                bool found = false;
                foreach (var x in ((ComboBox)sender).Items)
                {
                    if (x.ToString() == tb.Text)
                    {
                        found = true;
                        CustomMessageBox.Show(Properties.Resources.Message_Add_FilterExist_Begin + "\"" + tb.Text + "\"" + Properties.Resources.Message_Add_FilterExist_End, Properties.Resources.Warning);
                        ((ComboBox)sender).Items.RemoveAt(((ComboBox)sender).SelectedIndex);
                        ((ComboBox)sender).SelectedIndex = 0;
                        break;
                    }
                }
                if (!found)
                {
                    MessageBoxResult result = CustomMessageBox.Show(Properties.Resources.Message_Add_Filter_Begin + "\"" + tb.Text + "\"" + Properties.Resources.Message_Add_Filter_End, Properties.Resources.Message, MessageBoxButton.YesNo);
                    ((ComboBox)sender).Items.RemoveAt(((ComboBox)sender).Items.Count - 1);
                    if (result == MessageBoxResult.Yes)
                    {
                        for (int i = 1; i < (sortSP.Children.Count - 1); i += 2)
                        {
                            ((ComboBox)sortSP.Children[i]).Items.Add(tb.Text);
                        }
                        if ((Application.Current.MainWindow as MainWindow).filterFile == null)
                        {
                            using (File.Create("filters.ini"))
                                (Application.Current.MainWindow as MainWindow).filterFile = new INIHandler.INIFile("filters.ini");
                        }
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + (((ComboBox)sender).Items.Count - 1).ToString(), "Name", tb.Text);
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("Filter - " + (((ComboBox)sender).Items.Count - 1).ToString(), "ExcludeUpdate", "False");
                        (Application.Current.MainWindow as MainWindow).filterFile.DeleteSection("FilterCount");
                        (Application.Current.MainWindow as MainWindow).filterFile.Write("FilterCount", "Count", (((ComboBox)sender).Items.Count - 1).ToString());
                        newFilter = true;
                        ((ComboBox)sender).SelectedIndex = ((ComboBox)sender).Items.Count - 1;
                    }
                    else
                    {
                        ((ComboBox)sender).SelectedIndex = 0;
                    }
                }
            }
            else
            {
                ((ComboBox)sender).Items.RemoveAt(((ComboBox)sender).Items.Count - 1);
            }

            if (((ComboBox)sender).SelectedIndex == -1)
            {
                ((ComboBox)sender).SelectedIndex = 0;
            }
        }

        private void SearchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(searchTB.Text))
            //    _panelStateChanged = new bool[0];
            Array.Resize(ref _panelStateChanged, mainLB.Items.Count);
            foreach (var x in mainLB.Items)
            {
                if (!(((StackPanel)(x as ListBoxItem).Content).Children[1] as TextBlock).Text.ToLower().Contains(searchTB.Text.ToLower()))
                {
                    if ((x as ListBoxItem).IsVisible)
                    {
                        _panelStateChanged[int.Parse((((StackPanel)(x as ListBoxItem).Content).Children[0] as Button).Tag.ToString()) - 1] = true;
                        (x as ListBoxItem).Visibility = Visibility.Collapsed;
                    }
                }
                else
                    if(_panelStateChanged[int.Parse((((StackPanel)(x as ListBoxItem).Content).Children[0] as Button).Tag.ToString()) - 1])
                {
                    _panelStateChanged[int.Parse((((StackPanel)(x as ListBoxItem).Content).Children[0] as Button).Tag.ToString()) - 1] = false;
                    (x as ListBoxItem).Visibility = Visibility.Visible;
                }
            }
        }

        private void SearchTB_LostFocus(object sender, RoutedEventArgs e)
        {
            /*
            if (Keyboard.FocusedElement is ComboBox)
            {
                searchTB.Width = 0;
                searchTB.Text = null;
                searchTB.IsEnabled = false;
            }
            */
        }

        private void MainLB_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is ListBoxItem && e.LeftButton == MouseButtonState.Pressed)
            {
                var draggedItem = sender as ListBoxItem;
                if (mouseDownOnElement)
                {
                    mouseDownOnElement = false;
                    DragDrop.DoDragDrop(draggedItem, draggedItem, DragDropEffects.Move);
                }
            }
        }

        void MainLB_Drop(object sender, DragEventArgs e)
        {
            var droppedData = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
            var target = ((ListBoxItem)(sender)) as ListBoxItem;

            int removedIdx = mainLB.Items.IndexOf(droppedData);
            int targetIdx = mainLB.Items.IndexOf(target);

            int _removedIdx = 0;
            int _targetIdx = 0;
            int inverseContor = 1;
            int _cont = 0;
            int _incriser = 0;
            int _decreser = 0;

            if (removedIdx < targetIdx)
            {
                if (targetIdx + 1 == _itemCollection.Count)
                {
                    _itemCollection.Add(new ListBoxItem());
                    _itemCollection.Insert(targetIdx + 1, droppedData);
                    _itemCollection.RemoveAt(removedIdx);
                    _itemCollection.RemoveAt(targetIdx + 1);
                }
                else
                {
                    _itemCollection.Insert(targetIdx + 1, droppedData);
                    _itemCollection.RemoveAt(removedIdx);
                }
                    

                _removedIdx = removedIdx;
                _targetIdx = targetIdx;
                inverseContor = 1;
                _cont = removedIdx + 2;
                _incriser = 1;
                _decreser = 0;
            }
            else
            {
                if (_itemCollection.Count > removedIdx)
                {
                    _itemCollection.Insert(targetIdx, droppedData);
                    _itemCollection.RemoveAt(removedIdx + 1);

                    _removedIdx = targetIdx;
                    _targetIdx = removedIdx;
                    inverseContor = -1;
                    _cont = removedIdx + 1;
                    _incriser = 0;
                    _decreser = 1;
                }
            }
            mainLB.Items.Refresh();
            string[] _filderToCopy = new string[7];
            _filderToCopy[0] = (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (removedIdx + 1), "Name");
            _filderToCopy[1] = (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (removedIdx + 1), "Link");
            _filderToCopy[2] = (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (removedIdx + 1), "Version");
            _filderToCopy[3] = (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (removedIdx + 1), "Update");
            _filderToCopy[4] = (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (removedIdx + 1), "Site");
            _filderToCopy[5] = (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (removedIdx + 1), "Completed");
            _filderToCopy[6] = (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (removedIdx + 1), "Tags");
            if (!Directory.Exists(@"pictures"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\pictures");
            }
            else
            {
                if (File.Exists(Directory.GetCurrentDirectory() +
                    @"\pictures\IMG-" + (removedIdx + 1) + ".png"))
                {
                    File.Copy(Directory.GetCurrentDirectory() +
                        @"\pictures\IMG-" + (removedIdx + 1) + ".png", Directory.GetCurrentDirectory() +
                        @"\pictures\IMG-" + (removedIdx + 1) + "_.png");
                    File.Delete(Directory.GetCurrentDirectory() +
                        @"\pictures\IMG-" + (removedIdx + 1) + ".png");
                }
            }
            for (int i = _removedIdx; i < _targetIdx; i++)
            {
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (_cont - _incriser), "Name", (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (_cont - _decreser), "Name"));
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (_cont - _incriser), "Link", (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (_cont - _decreser), "Link"));
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (_cont - _incriser), "Version", (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (_cont - _decreser), "Version"));
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (_cont - _incriser), "Update", (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (_cont - _decreser), "Update"));
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (_cont - _incriser), "Site", (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (_cont - _decreser), "Site"));
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (_cont - _incriser), "Completed", (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (_cont - _decreser), "Completed"));
                (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (_cont - _incriser), "Tags", (Application.Current.MainWindow as MainWindow).pannelFile.Read("Panel - " + (_cont - _decreser), "Tags"));
                (((mainLB.Items[(_cont - _incriser) - 1] as ListBoxItem).Content as StackPanel).Children[0] as Button).Tag = (_cont - _decreser) - inverseContor;
                if (File.Exists(Directory.GetCurrentDirectory() +
                    @"\pictures\IMG-" + (_cont - _decreser) + ".png"))
                {
                    File.Copy(Directory.GetCurrentDirectory() +
                        @"\pictures\IMG-" + (_cont - _decreser) + ".png", Directory.GetCurrentDirectory() +
                        @"\pictures\IMG-" + (_cont - _incriser) + ".png");
                    File.Delete(Directory.GetCurrentDirectory() +
                        @"\pictures\IMG-" + (_cont - _decreser) + ".png");
                }
                _cont = _cont + inverseContor;
            }
            (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (targetIdx + 1), "Name", _filderToCopy[0]);
            (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (targetIdx + 1), "Link", _filderToCopy[1]);
            (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (targetIdx + 1), "Version", _filderToCopy[2]);
            (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (targetIdx + 1), "Update", _filderToCopy[3]);
            (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (targetIdx + 1), "Site", _filderToCopy[4]);
            (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (targetIdx + 1), "Completed", _filderToCopy[5]);
            (Application.Current.MainWindow as MainWindow).pannelFile.Write("Panel - " + (targetIdx + 1), "Tags", _filderToCopy[6]);
            (((mainLB.Items[(_cont - _incriser) - 1] as ListBoxItem).Content as StackPanel).Children[0] as Button).Tag = (_cont - _decreser) - inverseContor;
            if (File.Exists(Directory.GetCurrentDirectory() +
                @"\pictures\IMG-" + (removedIdx + 1) + "_.png"))
            {
                File.Copy(Directory.GetCurrentDirectory() +
                    @"\pictures\IMG-" + (removedIdx + 1) + "_.png", Directory.GetCurrentDirectory() +
                    @"\pictures\IMG-" + (targetIdx + 1) + ".png");
                File.Delete(Directory.GetCurrentDirectory() +
                    @"\pictures\IMG-" + (removedIdx + 1) + "_.png");
            }
                _filderToCopy = null;
        }

        private void MainLBItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (ss, ee) =>
            {
                Thread.Sleep(200);
            };

            backgroundWorker.RunWorkerCompleted += (ss, ee) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    mouseDownOnElement = true;
                else
                    mouseDownOnElement = false;
            };

            backgroundWorker.RunWorkerAsync();
        }
    }
}
