using System.Windows.Controls;

namespace WPF
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsPageViewModel(this);
        }
    }
}
