using System.Windows;

namespace Monkeysoft.Screenshot
{
    /// <summary>
    /// WindowPicker.xaml 的交互逻辑
    /// </summary>
    public partial class WindowPicker : Window
    {
        public WindowPicker()
        {
            InitializeComponent();
        }

        private void OkButton(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
