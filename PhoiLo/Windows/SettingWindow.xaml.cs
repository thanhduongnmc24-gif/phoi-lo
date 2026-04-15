using System.Windows;

namespace PhoiLo.Windows
{
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            // Gán dữ liệu dùng chung để thay đổi real-time
            this.DataContext = App.Config;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Lưu toàn bộ các ô nhập liệu vào file config.json
            App.Config.SaveToFile();
            this.Close();
        }
    }
}