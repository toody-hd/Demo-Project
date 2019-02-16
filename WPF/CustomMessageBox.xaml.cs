using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WPF
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox()
        {
            InitializeComponent();
            DataContext = new CustomMessageBoxViewModel(this);
            Owner = (MainWindow)Application.Current.MainWindow;
            ((MainWindow)Application.Current.MainWindow).oL.Visibility = Visibility.Visible;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).oL.Visibility = Visibility.Collapsed;
        }

        static CustomMessageBox _messageBox;
        static MessageBoxResult _result = MessageBoxResult.No;

        public static MessageBoxResult Show
        (string message, string title, MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.ConfirmationWithYesNo:
                    return Show(message, title, MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                case MessageBoxType.ConfirmationWithYesNoCancel:
                    return Show(message, title, MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                case MessageBoxType.Information:
                    return Show(message, title, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                case MessageBoxType.Error:
                    return Show(message, title, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                case MessageBoxType.Warning:
                    return Show(message, title, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                default:
                    return MessageBoxResult.No;
            }
        }

        public static MessageBoxResult Show(string message, MessageBoxType type)
        {
            return Show(message, string.Empty, type);
        }

        public static MessageBoxResult Show(string message)
        {
            return Show(message, string.Empty,
            MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show
        (string message, string title)
        {
            return Show(message, title,
            MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show
        (string message, string title, MessageBoxButton button)
        {
            return Show(message, title, button,
            MessageBoxImage.None);
        }

        public static MessageBoxResult Show
        (string message, string title,
        MessageBoxButton button, MessageBoxImage image)
        {
            _messageBox = new CustomMessageBox
            { Message = { Text = message }, TitleM = { Text = title } };
            SetVisibilityOfButtons(button);
            SetImageOfMessageBox(image);
            _messageBox.ShowDialog();
            return _result;
        }

        public static MessageBoxResult Show
        (object obj, MessageBoxButton button, MessageBoxImage image)
        {
            _messageBox = new CustomMessageBox
            { Object = { Content = obj } };
            SetVisibilityOfButtons(button);
            SetImageOfMessageBox(image);
            _messageBox.ShowDialog();
            return _result;
        }

        public static MessageBoxResult Show
        (object obj)
        {
            _messageBox = new CustomMessageBox
            { Object = { Content = obj , Visibility = Visibility.Visible }, Message = { Visibility = Visibility.Collapsed } };
            SetVisibilityOfButtons(MessageBoxButton.OK);
            _messageBox.ShowDialog();
            return _result;
        }

        private static void SetVisibilityOfButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    _messageBox.cancelBtn.Visibility = Visibility.Collapsed;
                    _messageBox.noBtn.Visibility = Visibility.Collapsed;
                    _messageBox.yesBtn.Visibility = Visibility.Collapsed;
                    _messageBox.okBtn.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    _messageBox.noBtn.Visibility = Visibility.Collapsed;
                    _messageBox.yesBtn.Visibility = Visibility.Collapsed;
                    _messageBox.cancelBtn.Focus();
                    break;
                case MessageBoxButton.YesNo:
                    _messageBox.okBtn.Visibility = Visibility.Collapsed;
                    _messageBox.cancelBtn.Visibility = Visibility.Collapsed;
                    _messageBox.noBtn.Focus();
                    break;
                case MessageBoxButton.YesNoCancel:
                    _messageBox.okBtn.Visibility = Visibility.Collapsed;
                    _messageBox.cancelBtn.Focus();
                    break;
                default:
                    break;
            }
        }

        private static void SetImageOfMessageBox(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Warning:
                    _messageBox.SetImage("Warning.png");
                    break;
                case MessageBoxImage.Question:
                    _messageBox.SetImage("Question.png");
                    break;
                case MessageBoxImage.Information:
                    _messageBox.SetImage("Information.png");
                    break;
                case MessageBoxImage.Error:
                    _messageBox.SetImage("Error.png");
                    break;
                default:
                    _messageBox.img.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == okBtn)
                _result = MessageBoxResult.OK;
            else if (sender == yesBtn)
                _result = MessageBoxResult.Yes;
            else if (sender == noBtn)
                _result = MessageBoxResult.No;
            else if (sender == cancelBtn)
                _result = MessageBoxResult.Cancel;
            else
                _result = MessageBoxResult.None;
            _messageBox.Close();
            _messageBox = null;
        }

        private void SetImage(string imageName)
        {
            string uri = string.Format("/Resources/images/{0}", imageName);
            var uriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            img.Source = new BitmapImage(uriSource);
        }

        public enum MessageBoxType
        {
            ConfirmationWithYesNo = 0,
            ConfirmationWithYesNoCancel,
            Information,
            Error,
            Warning
        }

        public enum MessageBoxImage
        {
            Warning = 0,
            Question,
            Information,
            Error,
            None
        }
    }
}
