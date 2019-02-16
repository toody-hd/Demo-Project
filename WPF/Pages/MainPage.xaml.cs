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
        #region Variables
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
        private bool mouseDownOnElement;
        private bool[] _panelStateChanged = new bool[0];

        public ObservableCollection<string> filterList = new ObservableCollection<string>();
        public ObservableCollection<ListBoxItem> _itemCollection = new ObservableCollection<ListBoxItem>();
        public string[] currentFilter = new string[0];
        #endregion

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
            if (InI.FiltersFileExist())
            {
                foreach (var x in InI.filtersFile.ReadSections())
                {
                    filterList.Add(x);
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

            sortCB = new ComboBox() { Tag = counter, VerticalAlignment = VerticalAlignment.Center };
            sortCB.ItemContainerStyle = (Style)Resources["ComboBoxItemStyle"];
            btn = new Button() { Content = Properties.Resources.AddNewSort, Style = FindResource("StripButton") as Style };
            tb = new TextBox() { Width = 70, Visibility = Visibility.Collapsed };
            grd = new Grid();
            grd.Children.Add(btn);
            grd.Children.Add(tb);
            btn.Click += (object ss, RoutedEventArgs ee) => {
                btn.Visibility = Visibility.Collapsed;
                tb.Visibility = Visibility.Visible;
                tb.Text = "";
                tb.Focus();
            };
            sortCB.ItemsSource = (new ObservableCollection<object>(filterList) { grd });
            sortCB.SelectedIndex = 0;
            sortCB.SelectionChanged += SortCB_SelectionChanged;
            sortCB.DropDownOpened += SortCB_DropDownOpened;
            sortCB.GotFocus += SortCB_GotFocus;
            sortB = new Button() { Content = "+", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
            sortB.Click += (ss, ee) => InitializeSortCB();
            if(VisibleFiltersCB() < 2 || VisibleFiltersCB() == counter || counter > 4)
                sortB.Visibility = Visibility.Collapsed;
            sortTB = new TextBlock() { Text = sortAddT, Tag = new object[2] { counter, false }, VerticalAlignment = VerticalAlignment.Center };
            sortTB.MouseLeftButtonDown += SortTB_MouseLeftButtonDown;
            sortSP.Children.Add(sortTB);
            sortSP.Children.Add(sortCB);
            sortSP.Children.Add(sortB);
            sortAddT = "  &  ";
        }

        private int VisibleFiltersCB()
        {
            int _count = 0;
            if (InI.FiltersFileExist())
                for (int i = 1; i < filterList.Count; i++)
                {
                    if (InI.filtersFile.Read("Filter - " + i, "HideOnStartUp") == "False")
                        _count++;
                }
            return _count;
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
                if (!string.IsNullOrEmpty(tb.Text))
                {
                    bool found = false;
                    foreach (var x in filterList)
                    {
                        if (x == tb.Text)
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
                            InI.filtersFile.Write(tb.Text, "ExcludeUpdate", "false");
                            InI.filtersFile.Write(tb.Text, "HideOnStartUp", "false");
                            newFilter = true;
                            ((ComboBox)sender).SelectedIndex = filterList.Count - 1;
                        }
                        if (!sortB.IsVisible && VisibleFiltersCB() > 1)
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
            foreach (ListBoxItem x in mainLB.Items)
            {
                int _cont = -1;
                foreach (var y in currentFilter)
                {
                    _cont++;
                    if ((bool)((sortSP.Children[_cont] as TextBlock).Tag as object[])[1] == false)
                    {
                        if (y != null && !((StackPanel)x.Content).Tag.ToString().Contains(y))
                        {
                            x.Visibility = Visibility.Collapsed;
                            break;
                        }
                        else
                        if (x.IsEnabled)
                        {
                            x.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (y != null && ((StackPanel)x.Content).Tag.ToString().Contains(y)
                            && (sortSP.Children[_cont + 1] as ComboBox).SelectedIndex > 0)
                        {
                            x.Visibility = Visibility.Collapsed;
                            break;
                        }
                        else
                        if (x.IsEnabled)
                        {
                            x.Visibility = Visibility.Visible;
                        }
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
            if ((((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[1].Visibility == Visibility.Visible)
            {
                (((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[0].Visibility = Visibility.Visible;
                (((ComboBox)sender).Items[((ComboBox)sender).Items.Count - 1] as Grid).Children[1].Visibility = Visibility.Collapsed;
            }

            var _list = new ObservableCollection<object>() { Properties.Resources.NoneSelected };
            int y = -1;
            if (InI.FiltersFileExist())
                foreach (var x in filterList)
                {
                    y++;
                    if (InI.filtersFile.Read(x, "HideOnStartUp") == "false")
                        _list.Add(x);
                }

            _list.Add(grd);
            y = 0;

            foreach (var x in currentFilter)
            {
                y++;
                if ((int)((ComboBox)sender).Tag != y)
                    _list.Remove(x);
            }

            ((ComboBox)sender).ItemsSource = _list;
        }

        private void _CBI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && ((ComboBoxItem)sender).Content is string && ((ComboBoxItem)sender).Content.ToString() != Properties.Resources.NoneSelected)
            {
                MessageBoxResult _result = CustomMessageBox.Show(Properties.Resources.Message_DeleteFilter_Begin + ((ComboBoxItem)sender).Content.ToString() + Properties.Resources.Message_DeleteFilter_End, Properties.Resources.Delete, MessageBoxButton.YesNo);
                if (_result == MessageBoxResult.Yes)
                {
                    if (InI.FiltersFileExist())
                    {
                        InI.filtersFile.DeleteSection(((ComboBoxItem)sender).Content.ToString());
                    }
                    filterList.Remove(((ComboBoxItem)sender).DataContext.ToString());
                    if (counter > VisibleFiltersCB() && counter > 1)
                        DeleteSortCB();
                    if (sortB.IsVisible && (VisibleFiltersCB() < 2 || VisibleFiltersCB() == counter))
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

            if (((ComboBox)sender).SelectedIndex == -1)
            {
                ((ComboBox)sender).SelectedIndex = 0;
            }
        }

        private void SearchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Array.Resize(ref _panelStateChanged, mainLB.Items.Count);
            foreach (var x in mainLB.Items)
            {
                if (!(((StackPanel)(x as ListBoxItem).Content).Children[1] as TextBlock).Text.ToLower().Contains(searchTB.Text.ToLower()))
                {
                    if ((x as ListBoxItem).IsVisible)
                    {
                        _panelStateChanged[mainLB.Items.IndexOf(x as ListBoxItem)] = true;
                        (x as ListBoxItem).Visibility = Visibility.Collapsed;
                    }
                }
                else
                    if (_panelStateChanged[mainLB.Items.IndexOf(x as ListBoxItem)])
                {
                    _panelStateChanged[mainLB.Items.IndexOf(x as ListBoxItem)] = false;
                    (x as ListBoxItem).Visibility = Visibility.Visible;
                }
            }
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
                draggedItem = null;
            }
        }

        void MainLB_Drop(object sender, DragEventArgs e)
        {
            var target = (((ListBoxItem)sender) as ListBoxItem).Content;

            ((ListBoxItem)sender).Content = (e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem).Content;
            (mainLB.Items.GetItemAt(mainLB.Items.IndexOf(e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem)) as ListBoxItem).Content = target;

            InI.panelsFile.InterchangeSections(
                ((TextBlock)((StackPanel)((ListBoxItem)sender).Content).Children[1]).Text,
                ((TextBlock)((StackPanel)target).Children[1]).Text);
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