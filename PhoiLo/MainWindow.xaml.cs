using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PhoiLo.UserControls;
using PhoiLo.Windows;

namespace PhoiLo
{
    public partial class MainWindow : Window
    {
        // [Suy luận] Giữ lại nguyên vẹn màn hình để không bị mất dữ liệu
        private SheetDataControl _sheetControl = new SheetDataControl();
        private KcsDataControl _kcsControl = new KcsDataControl();

        public MainWindow()
        {
            InitializeComponent();
            MainContentArea.Content = _sheetControl;
            LoadStaffToHeader();
        }

        private void CbGlobalKip_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadStaffToHeader();

        private void LoadStaffToHeader()
        {
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

        private void BtnLoadSheet_Click(object sender, RoutedEventArgs e) => MainContentArea.Content = _sheetControl;
        private void BtnLoadKcs_Click(object sender, RoutedEventArgs e) => MainContentArea.Content = _kcsControl;

        private void BtnOpenSetting_Click(object sender, RoutedEventArgs e) {
            SettingWindow setWin = new SettingWindow { Owner = this };
            setWin.ShowDialog();
            LoadStaffToHeader(); 
        }
    }
}