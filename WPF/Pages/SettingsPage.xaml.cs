using System.Windows.Controls;

namespace WPF
{
    /// <summary>
    /// Interaction logic for settingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsPageViewModel(this);
        }
    }
}
