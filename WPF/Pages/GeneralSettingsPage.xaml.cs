using System.Windows.Controls;

namespace WPF
{
    public partial class GeneralSettingsPage : Page
    {
        public GeneralSettingsPage()
        {
            InitializeComponent();
            DataContext = new GeneralSettingsPageViewModel(this);
        }

        private void ComboBoxEnglish_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings._lang = "en-US";
        }

        private void ComboBoxRomanien_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings._lang = "ro-RO";
        }

        private void ComboBoxGerman_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings._lang = "de-DE";
        }

        private void ComboBoxJapanese_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            Settings._lang = "ja-JP";
        }
    }
}