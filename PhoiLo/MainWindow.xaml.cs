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
            MainContentArea.Content = new SheetDataControl();
        }

        private void BtnLoadSheet_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new SheetDataControl();
        }

        // Sự kiện khi bấm nút Phôi gởi KCS
        private void BtnLoadKcs_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new KcsDataControl();
        }

        private void BtnOpenSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow setWin = new SettingWindow();
            setWin.Owner = this;
            setWin.ShowDialog();
        }
    }
}