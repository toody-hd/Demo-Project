using System.Windows;
using System.Windows.Input;

namespace WPF
{
    public class CustomMessageBoxViewModel : MainViewModel
    {
        public CustomMessageBoxViewModel(Window window) : base(window)
        {
            //CloseCommand = new RelayCommand(() => window.Close());
        }

        //public ICommand CloseCommand { get; private set; }
    }
}
