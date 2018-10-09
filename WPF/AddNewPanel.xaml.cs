using System.Windows;

namespace WPF
{
    public partial class AddNewPanel : Window
    {
        public AddNewPanel()
        {
            InitializeComponent();
            DataContext = new AddNewPanelViewModel(this);
        }
    }
}
