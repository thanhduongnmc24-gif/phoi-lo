using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            LoadStaffToHeader();
        }

        private void CbGlobalKip_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadStaffToHeader();

        private void LoadStaffToHeader()
        {
            // Chốt chặn an toàn: Nếu giao diện chưa load xong thì không làm gì cả
            if (CbGlobalKip == null || CbGlobalToTruong == null || CbGlobalVanHanh == null) return;
            
            string kip = App.Config.CurrentKip;
            if (string.IsNullOrEmpty(kip)) return;
            string group = kip.Substring(kip.Length - 1).ToUpper();

            var staff = App.Config.StaffList;
            if (staff == null) return;

            CbGlobalToTruong.ItemsSource = staff.Where(s => s.Kip.ToUpper() == group && s.ChucVu.Contains("Tổ trưởng")).ToList();
            CbGlobalVanHanh.ItemsSource = staff.Where(s => s.Kip.ToUpper() == group && s.ChucVu.Contains("Vận hành")).ToList();
            
            if (CbGlobalToTruong.Items.Count > 0) CbGlobalToTruong.SelectedIndex = 0;
            if (CbGlobalVanHanh.Items.Count > 0) CbGlobalVanHanh.SelectedIndex = 0;
        }

        private void BtnLoadSheet_Click(object sender, RoutedEventArgs e) => MainContentArea.Content = new SheetDataControl();
        private void BtnLoadKcs_Click(object sender, RoutedEventArgs e) => MainContentArea.Content = new KcsDataControl();
        private void BtnOpenSetting_Click(object sender, RoutedEventArgs e) {
            SettingWindow setWin = new SettingWindow { Owner = this };
            setWin.ShowDialog();
            LoadStaffToHeader(); 
        }
    }
}