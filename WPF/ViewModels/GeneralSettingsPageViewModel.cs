using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF
{
    public class GeneralSettingsPageViewModel
    {
        #region Local Variables
        private GeneralSettingsPage mPage;
        private string _excludedTags = null;
        private string _hideTags = null;
        private CheckBox _cb = new CheckBox();
        private string _selExcl = "_";
        private string _selHide = "_";
        #endregion

        public GeneralSettingsPageViewModel(Page page)
        {
            mPage = (GeneralSettingsPage)page;
            MiniModeCommand = new RelayCommand(() => MiniMode());
            TopmostModeCommand = new RelayCommand(() => TopmostMode());
            MessageModeCommand = new RelayCommand(() => MessageMode());
            CustomScriptCommand = new RelayCommand(() => CustomScript());
            Initializer();
        }

        private void Initializer()
        {
            InitializeFilterList();
            InitializeScripts();
            if (Settings._lang == "ro-RO")
                mPage.langCB.SelectedIndex = 1;
            else if(Settings._lang == "de-DE")
                mPage.langCB.SelectedIndex = 2;
            else if(Settings._lang == "ja-JP")
                mPage.langCB.SelectedIndex = 3;
            else
                mPage.langCB.SelectedIndex = 0;
            mPage.miniCB.IsChecked = Settings._miniMode;
            mPage.topCB.IsChecked = Settings._topmostMode;
            mPage.messageCB.IsChecked = Settings._messageMode;
        }

        private void MiniMode()
        {
            Settings._miniMode = mPage.miniCB.IsChecked.Value;
        }

        private void TopmostMode()
        {
            Settings._topmostMode = mPage.topCB.IsChecked.Value;
        }

        private void MessageMode()
        {
            Settings._messageMode = mPage.messageCB.IsChecked.Value;
        }

        private void InitializeFilterList()
        {
            mPage.excludeCB.Items.Add(Properties.Resources.NoneSelected);
            mPage.hideOnStartUpCB.Items.Add(Properties.Resources.NoneSelected);

            if (Settings._filters.Count > 0)
            {
                foreach (var x in Settings._filters)
                {
                    _populateCB(x[0], "ExcludeUpdate", bool.Parse(x[1]), mPage.excludeCB);
                    _populateCB(x[0], "HideOnStartUp", bool.Parse(x[2]), mPage.hideOnStartUpCB);
                }

                if (_selExcl != "_")
                {
                    _excludedTags = _selExcl.Replace("_", ",").Remove(0, 1).Remove(_selExcl.Length - 2, 1);
                    mPage.excludeCB.Items[0] = _excludedTags;
                }
                if (_selHide != "_")
                {
                    _hideTags = _selHide.Replace("_", ",").Remove(0, 1).Remove(_selHide.Length - 2, 1);
                    mPage.hideOnStartUpCB.Items[0] = _hideTags;
                }
            }

            if (mPage.excludeCB.SelectedIndex == -1)
                mPage.excludeCB.SelectedIndex = 0;
            if (mPage.hideOnStartUpCB.SelectedIndex == -1)
                mPage.hideOnStartUpCB.SelectedIndex = 0;
        }

        private void _populateCB(string section, string key, bool value, ComboBox cb)
        {
            _cb = new CheckBox { IsChecked = value, Content = section };
            if (_cb.IsChecked == true)
            {
                if (key == "ExcludeUpdate")
                {
                    _selExcl = _selExcl + _cb.Content + "_";
                }
                else if (key == "HideOnStartUp")
                {
                    _selHide = _selHide + _cb.Content + "_";
                }
            }

            _cb.Click += _cb_Click;
            cb.Items.Add(_cb);
            _cb = null;
        }

        private void _cb_Click(object sender, RoutedEventArgs e)
        {
            string _s = "";
            if ((((CheckBox)sender).Parent as ComboBox) == mPage.excludeCB)
                _s = _excludedTags;
            else if ((((CheckBox)sender).Parent as ComboBox) == mPage.hideOnStartUpCB)
                _s = _hideTags;

            if (((CheckBox)sender).IsChecked == true)
            {
                if (_s == null || _s == Properties.Resources.NoneSelected)
                    _s = ((CheckBox)sender).Content.ToString();
                else
                    _s = _s + "," + ((CheckBox)sender).Content.ToString();
                //(((CheckBox)sender).Parent as ComboBox).Items[0] = _s;
                //(((CheckBox)sender).Parent as ComboBox).SelectedIndex = 0;
                //Settings.excList[int.Parse(((CheckBox)sender).Tag.ToString())].Value = true;
            }
            else
            {
                _s = _s.Replace(((CheckBox)sender).Content.ToString(), "");
                _s = _s.Replace(",,", ",");
                if (_s.EndsWith(","))
                    _s = _s.Remove(_s.Length - 1, 1);
                if (_s.StartsWith(","))
                    _s = _s.Remove(0, 1);
                if (string.IsNullOrWhiteSpace(_s))
                    _s = Properties.Resources.NoneSelected;
                //(((CheckBox)sender).Parent as ComboBox).Items[0] = _s;
                //(((CheckBox)sender).Parent as ComboBox).SelectedIndex = 0;
                //Settings.excList[int.Parse(((CheckBox)sender).Tag.ToString())].Value = false;
            }

            if ((((CheckBox)sender).Parent as ComboBox) == mPage.excludeCB)
            {
                _excludedTags = _s;
                //Settings._excUpd = _s.Split(',');
                (Settings._filters[(((CheckBox)sender).Parent as ComboBox).Items.IndexOf((CheckBox)sender) - 1] as string[])[1] = ((CheckBox)sender).IsChecked.ToString().ToLower();
            }
            else if ((((CheckBox)sender).Parent as ComboBox) == mPage.hideOnStartUpCB)
            {
                _hideTags = _s;
                //Settings._hideSU = _s.Split(',');
                (Settings._filters[(((CheckBox)sender).Parent as ComboBox).Items.IndexOf((CheckBox)sender) - 1] as string[])[2] = ((CheckBox)sender).IsChecked.ToString().ToLower();
            }

            (((CheckBox)sender).Parent as ComboBox).Items[0] = _s;
            (((CheckBox)sender).Parent as ComboBox).SelectedIndex = 0;
        }

        private void InitializeScripts()
        {
            if (Settings._scripts.Count > 0)
            {
                foreach (var x in Settings._scripts)
                {
                    CreateCustomScript(x[0], " -> " +  x[1], "FontAwesomeEditIcon", false, Visibility.Collapsed);
                }
            }
        }

        private void CustomScript()
        {
            CreateCustomScript("Name ", " Data ", "FontAwesomeSaveIcon", true, Visibility.Visible);
        }

        private void CreateCustomScript(string name, string data, string iconResource, bool isEdit, Visibility isEditVisible)
        {
            if (!((Expander)mPage.CustomScripSP.Parent).IsExpanded)
                ((Expander)mPage.CustomScripSP.Parent).IsExpanded = true;

            var _sp = new StackPanel() { Orientation = Orientation.Horizontal };
            _sp.Children.Add(new TextBlock() { Text = name });
            _sp.Children.Add(new TextBox() { MinWidth = 100, MaxWidth = 200, Visibility = isEditVisible });
            _sp.Children.Add(new TextBlock() { Text = data });
            _sp.Children.Add(new TextBox() { MinWidth = 100, MaxWidth = 200, Visibility = isEditVisible });
            var _save_editB = new Button() { Tag = isEdit, Height = 25, Content = (string)Application.Current.Resources[iconResource], FontFamily = (FontFamily)Application.Current.Resources["FontAwesome"], Style = (Style)Application.Current.Resources["IconGrowButton"] };
            var _delB = new Button() { Height = 25, Content = (string)Application.Current.Resources["FontAwesomeMinusIcon"], FontFamily = (FontFamily)Application.Current.Resources["FontAwesome"], Style = (Style)Application.Current.Resources["IconGrowButton"] };
            _save_editB.PreviewMouseLeftButtonUp += _save_editB_MouseLeftButtonUp;
            _delB.PreviewMouseLeftButtonUp += _delB_MouseLeftButtonUp;
            _sp.Children.Add(_save_editB);
            _sp.Children.Add(_delB);
            mPage.CustomScripSP.Children.Add(_sp);
        }

        private void _save_editB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((bool)((Button)sender).Tag)
            {
                if (Settings._scripts.Count == mPage.CustomScripSP.Children.Count)
                {
                    Settings._scripts[mPage.CustomScripSP.Children.IndexOf((StackPanel)((Button)sender).Parent)] = 
                        new string[] { ((((Button)sender).Parent as StackPanel).Children[1] as TextBox).Text,
                                       ((((Button)sender).Parent as StackPanel).Children[3] as TextBox).Text};
                }
                else
                {
                    Settings._scripts.Add(new string[] {
                        ((((Button)sender).Parent as StackPanel).Children[1] as TextBox).Text,
                        ((((Button)sender).Parent as StackPanel).Children[3] as TextBox).Text});
                }

                ((Button)sender).Content = (string)Application.Current.Resources["FontAwesomeEditIcon"];
                ((Button)sender).Tag = false;
                ((((Button)sender).Parent as StackPanel).Children[0] as TextBlock).Text =
                    ((((Button)sender).Parent as StackPanel).Children[1] as TextBox).Text;
                ((((Button)sender).Parent as StackPanel).Children[2] as TextBlock).Text = " -> " +
                    ((((Button)sender).Parent as StackPanel).Children[3] as TextBox).Text;
                (((Button)sender).Parent as StackPanel).Children[1].Visibility =
                    (((Button)sender).Parent as StackPanel).Children[3].Visibility = Visibility.Collapsed;
            }
            else
            {
                ((Button)sender).Content = (string)Application.Current.Resources["FontAwesomeSaveIcon"];
                ((Button)sender).Tag = true;
                ((((Button)sender).Parent as StackPanel).Children[1] as TextBox).Text =
                    ((((Button)sender).Parent as StackPanel).Children[0] as TextBlock).Text;
                ((((Button)sender).Parent as StackPanel).Children[3] as TextBox).Text =
                    ((((Button)sender).Parent as StackPanel).Children[2] as TextBlock).Text.Substring(4);
                ((((Button)sender).Parent as StackPanel).Children[0] as TextBlock).Text = "Name ";
                ((((Button)sender).Parent as StackPanel).Children[2] as TextBlock).Text = " Data ";
                (((Button)sender).Parent as StackPanel).Children[1].Visibility =
                    (((Button)sender).Parent as StackPanel).Children[3].Visibility = Visibility.Visible;
            }
        }

        private void _delB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (((StackPanel)((StackPanel)((Button)sender).Parent).Parent).Children.Count == 1)
            {
                ((Expander)mPage.CustomScripSP.Parent).IsExpanded = false;
            }
            Settings._scripts.RemoveAt(mPage.CustomScripSP.Children.IndexOf((StackPanel)((Button)sender).Parent));
            mPage.CustomScripSP.Children.Remove((StackPanel)((Button)sender).Parent);
        }

        #region ICommands
        public ICommand MiniModeCommand { get; private set; }
        public ICommand TopmostModeCommand { get; private set; }
        public ICommand MessageModeCommand { get; private set; }
        public ICommand CustomScriptCommand { get; private set; }
        #endregion
    }
}
