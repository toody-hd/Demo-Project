using System.Windows;

namespace WPF
{
    class CustomWindowViewModel : MainViewModel
    {
        public CustomWindowViewModel(Window window) : base(window)
        {
            //CloseCommand = new RelayCommand(() => window.Close());
        }
    }
}
