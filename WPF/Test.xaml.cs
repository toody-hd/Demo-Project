using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WPF
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : Window
    {
        private ObservableCollection<object> aa;

        public Test()
        {
            InitializeComponent();
            aa = new ObservableCollection<object>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4"
            };
            comboBox1.ItemsSource = new ObservableCollection<object>(aa)
            {
                new Button() { Content = "Button 1" }
            };
            comboBox2.ItemsSource = new ObservableCollection<object>(aa)
            {
                new Button() { Content = "Button 1" }
            };
            //comboBox1.Items.Add(new Button() { Content = "Button 1" });
            //comboBox2.Items.Add(new Button() { Content = "Button 1" });

            //comboBox1.ItemsSource = aa;
            //comboBox1.Items.Add(new Button() { Content = "Button 1" });
            //comboBox2.ItemsSource = aa;
            //comboBox2.Items.Add(new Button() { Content = "Button 1" });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            aa.RemoveAt(0);
            comboBox1.ItemsSource = new ObservableCollection<object>(aa) { new Button() { Content = "Button 1" } };
            comboBox2.ItemsSource = new ObservableCollection<object>(aa) { new Button() { Content = "Button 1" } };
        }
    }
}
