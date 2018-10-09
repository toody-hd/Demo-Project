using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPF
{
    public class GeneralSettingsPageViewModel : SettingsPageViewModel
    {
        private Page mPage;
        private string _tags = null;
        public GeneralSettingsPageViewModel(Page page) :base(page)
        {
            mPage = page;
            MiniModeCommand = new RelayCommand(() => MiniMode());
            TopmostModeCommand = new RelayCommand(() => TopmostMode());
            MessageModeCommand = new RelayCommand(() => MessageMode());
            Initializer();
        }

        private void Initializer()
        {
            InitializeFilterList();
            //(mPage as GeneralSettingsPage).miniCB.IsChecked = (System.Windows.Application.Current.MainWindow as MainWindow).notIco;
            (mPage as GeneralSettingsPage).miniCB.IsChecked = Settings._miniMode;
            (mPage as GeneralSettingsPage).topCB.IsChecked = Settings._topmostMode;
            (mPage as GeneralSettingsPage).messageCB.IsChecked = Settings._messageMode;
            //Settings._miniMode = Settings.miniMode;
            if (Settings._lang == "ro-RO")
                (mPage as GeneralSettingsPage).langCB.SelectedIndex = 1;
            else if(Settings._lang == "de-DE")
                (mPage as GeneralSettingsPage).langCB.SelectedIndex = 2;
            else if(Settings._lang == "ja-JP")
                (mPage as GeneralSettingsPage).langCB.SelectedIndex = 3;
            else
                (mPage as GeneralSettingsPage).langCB.SelectedIndex = 0;
        }

        private void MiniMode()
        {
            Settings._miniMode = (mPage as GeneralSettingsPage).miniCB.IsChecked.Value;
        }

        private void TopmostMode()
        {
            Settings._topmostMode = (mPage as GeneralSettingsPage).topCB.IsChecked.Value;
        }

        private void MessageMode()
        {
            Settings._messageMode = (mPage as GeneralSettingsPage).messageCB.IsChecked.Value;
        }

        private void InitializeFilterList()
        {
            (mPage as GeneralSettingsPage).excludeCB.Items.Add(Properties.Resources.NoneSelected);
            if ((System.Windows.Application.Current.MainWindow as MainWindow).filterFile != null)
            {
                //System.Array.Resize(ref Settings.excList, int.Parse((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count")) + 1);
                var _cb = new CheckBox();
                string _selected = "_";
                for (int i = 1; i <= int.Parse((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("FilterCount", "Count")); i++)
                {
                    if ((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.KeyExists("Filter - " + i, "ExcludeUpdate"))
                    {
                        //Settings.excList[i] = new Settings.CustomVar((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name"), bool.Parse((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "ExcludeUpdate")));
                        _cb = new CheckBox { IsChecked = bool.Parse((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "ExcludeUpdate")), Tag = i, Content = (System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name") };
                        if (_cb.IsChecked == true)
                        {
                            System.Array.Resize(ref Settings._excUpd, Settings._excUpd.Length + 1);
                            _selected = _selected + _cb.Content + "_";
                            Settings._excUpd[Settings._excUpd.Length - 1] = _cb.Content.ToString();
                        }
                    }
                    else
                    {
                        //Settings.excList[i] = new Settings.CustomVar((System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name"), false);
                        _cb = new CheckBox { IsChecked = false, Tag = i, Content = (System.Windows.Application.Current.MainWindow as MainWindow).filterFile.Read("Filter - " + i, "Name") };
                    }
                    _cb.Click += _cb_Click;
                    (mPage as GeneralSettingsPage).excludeCB.Items.Add(_cb);
                }

                if (_selected != "_")
                {
                    _tags = _selected.Replace("_", ",").Remove(0, 1).Remove(_selected.Length - 2, 1);
                    (mPage as GeneralSettingsPage).excludeCB.Items[0] = _tags;
                }
                _cb = null;
            }
            if ((mPage as GeneralSettingsPage).excludeCB.SelectedIndex == -1)
                (mPage as GeneralSettingsPage).excludeCB.SelectedIndex = 0;
        }

        private void _cb_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                if (_tags == null || _tags == Properties.Resources.NoneSelected)
                    _tags = ((CheckBox)sender).Content.ToString();
                else
                    _tags = _tags + "," + ((CheckBox)sender).Content.ToString();
                (mPage as GeneralSettingsPage).excludeCB.Items[0] = _tags;
                (mPage as GeneralSettingsPage).excludeCB.SelectedIndex = 0;
                //Settings.excList[int.Parse(((CheckBox)sender).Tag.ToString())].Value = true;
            }
            else
            {
                _tags = _tags.Replace(((CheckBox)sender).Content.ToString(), "");
                _tags = _tags.Replace(",,", ",");
                if (_tags.EndsWith(","))
                    _tags = _tags.Remove(_tags.Length - 1, 1);
                if (_tags.StartsWith(","))
                    _tags = _tags.Remove(0, 1);
                if (string.IsNullOrWhiteSpace(_tags))
                    _tags = Properties.Resources.NoneSelected;
                (mPage as GeneralSettingsPage).excludeCB.Items[0] = _tags;
                (mPage as GeneralSettingsPage).excludeCB.SelectedIndex = 0;
                //Settings.excList[int.Parse(((CheckBox)sender).Tag.ToString())].Value = false;
            }
            Settings._excUpd = _tags.Split(',');
        }

        public ICommand MiniModeCommand { get; private set; }
        public ICommand TopmostModeCommand { get; private set; }
        public ICommand MessageModeCommand { get; private set; }
    }
}
