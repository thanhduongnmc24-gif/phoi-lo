using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PhoiLo.Helpers;

namespace PhoiLo.Windows
{
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            this.DataContext = App.Config;
        }

        private void StaffGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Ép kiểu an toàn, không lo lỗi thiếu tên nữa
            if (sender is DataGrid grid)
            {
                DataGridHelper.HandleExcelActions(grid, e);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            App.Config.SaveToFile();
            this.Close();
        }
    }
}