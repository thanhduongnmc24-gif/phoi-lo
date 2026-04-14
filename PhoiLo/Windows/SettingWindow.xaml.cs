using System.Windows;

namespace PhoiLo.Windows
{
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            // Sử dụng chung bản Config của App để đồng bộ Real-time
            this.DataContext = App.Config;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Gọi lệnh lưu toàn bộ thông số vào file
            App.Config.SaveToFile();
            this.Close();
        }
    }
}