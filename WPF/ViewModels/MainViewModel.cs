using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace WPF
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotiftyProperyChange

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        protected Window mWindow;

        public MainViewModel(Window window)
        {
            mWindow = window;

            MinimizeCommand = new RelayCommand(() => mWindow.WindowState = WindowState.Minimized);
            //minimizeCommand = new RelayCommand(() => mWindow.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand(() => { mWindow.WindowState ^= WindowState.Maximized; ((MainWindow)System.Windows.Application.Current.MainWindow).MaxHeight = SystemParameters.WorkArea.Height + 15; ((MainWindow)System.Windows.Application.Current.MainWindow).MaxWidth = SystemParameters.WorkArea.Width + 15; });
            //CloseCommand = new RelayCommand(() => { DialogResult res = System.Windows.Forms.MessageBox.Show(Properties.Resources.Message_Exit, Properties.Resources.Closing, MessageBoxButtons.YesNo, MessageBoxIcon.Question); if (res == DialogResult.Yes) mWindow.Close(); });
            CloseCommand = new RelayCommand(() => { MessageBoxResult result = CustomMessageBox.Show(Properties.Resources.Message_Exit, Properties.Resources.Closing, MessageBoxButton.YesNo); if (result == MessageBoxResult.Yes) mWindow.Close(); });
            MenuCommand = new RelayCommand(() => { if (mWindow.WindowState == 0) SystemCommands.ShowSystemMenu(mWindow, new Point(mWindow.Left + 20, mWindow.Top + 35)); else SystemCommands.ShowSystemMenu(mWindow, new Point(Control.MousePosition.X, Control.MousePosition.Y)); });
            WindowCloseCommand = new RelayCommand(() => mWindow.Close());
            //mWindow.WindowState == 0 ? SystemCommands.ShowSystemMenu(mWindow, new Point(mWindow.Left + 20, mWindow.Top + 35)) : SystemCommands.ShowSystemMenu(mWindow, new Point(mWindow.Left + 20, mWindow.Top + 35)));
            //menuCommand = new RelayCommand(() => SystemCommands.ShowSystemMenu(mWindow, new Point(Control.MousePosition.X, Control.MousePosition.Y)));
        }

        #region ICommands
        public ICommand MinimizeCommand { get; private set; }
        public ICommand MaximizeCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand MenuCommand { get; private set; }
        public ICommand WindowCloseCommand { get; private set; }
        #endregion
    }
}