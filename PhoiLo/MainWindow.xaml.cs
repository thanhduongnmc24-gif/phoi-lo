using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PhoiLo.UserControls;
using PhoiLo.Windows;
using System.Data;

namespace PhoiLo
{
    public partial class MainWindow : Window
    {
        private SheetDataControl _sheetControl = new SheetDataControl();
        private KcsDataControl _kcsControl = new KcsDataControl();
        private HotDataControl _hotControl = new HotDataControl();

        public MainWindow()
        {
            InitializeComponent();
            MainContentArea.Content = _sheetControl;
            UpdateMenuHighlight(BtnMenuPulpit); // Mặc định cam cho Pulpit 1
            LoadStaffToHeader();
        }

        // [Suy luận] Hàm chuyên trách việc đổi màu nút bấm danh mục
        private void UpdateMenuHighlight(Button activeBtn)
        {
            // Reset tất cả về màu trắng
            BtnMenuPulpit.Foreground = Brushes.White;
            BtnMenuHot.Foreground = Brushes.White;
            BtnMenuKcs.Foreground = Brushes.White;
            BtnMenuSetting.Foreground = Brushes.White;

            BtnMenuPulpit.FontWeight = FontWeights.Normal;
            BtnMenuHot.FontWeight = FontWeights.Normal;
            BtnMenuKcs.FontWeight = FontWeights.Normal;

            // Nhuộm cam nút đang chọn
            activeBtn.Foreground = (Brush)new BrushConverter().ConvertFromString("#e67e22");
            activeBtn.FontWeight = FontWeights.Bold;
        }

        private void BtnLoadSheet_Click(object sender, RoutedEventArgs e)
        {
            _sheetControl.LoadDataFromGoogle(); 
            MainContentArea.Content = _sheetControl;
            UpdateMenuHighlight(BtnMenuPulpit);
        }

        private void BtnHotBillet_Click(object sender, RoutedEventArgs e)
        {
            var dv = _sheetControl.MainDataGrid.ItemsSource as DataView;
            if (dv != null && dv.Table != null) _hotControl.SetData(dv.Table);
            MainContentArea.Content = _hotControl;
            UpdateMenuHighlight(BtnMenuHot);
        }

        private void BtnLoadKcs_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = _kcsControl;
            UpdateMenuHighlight(BtnMenuKcs);
        }

        private void BtnOpenSetting_Click(object sender, RoutedEventArgs e) {
            UpdateMenuHighlight(BtnMenuSetting);
            SettingWindow setWin = new SettingWindow { Owner = this };
            setWin.ShowDialog();
            LoadStaffToHeader();
            // Sau khi đóng setting, quay lại highlight cái tab đang hiển thị thực tế
            if (MainContentArea.Content == _sheetControl) UpdateMenuHighlight(BtnMenuPulpit);
            else if (MainContentArea.Content == _hotControl) UpdateMenuHighlight(BtnMenuHot);
            else if (MainContentArea.Content == _kcsControl) UpdateMenuHighlight(BtnMenuKcs);
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
    }
}