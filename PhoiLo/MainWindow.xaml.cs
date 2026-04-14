using System.Windows;
using PhoiLo.UserControls;
using PhoiLo.Windows;

namespace PhoiLo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Tự động nạp màn hình dữ liệu lò khi khởi động
            MainContentArea.Content = new SheetDataControl();
        }

        private void BtnLoadSheet_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new SheetDataControl();
        }

        private void BtnOpenSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow setWin = new SettingWindow();
            setWin.Owner = this;
            setWin.ShowDialog();
        }
    }
}