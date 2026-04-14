using System.Windows;

namespace PhoiLo.Windows
{
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            // Gán DataContext vào đối tượng Config dùng chung để đồng bộ real-time
            this.DataContext = App.Config;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}