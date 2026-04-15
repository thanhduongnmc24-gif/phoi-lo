using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;

namespace PhoiLo.UserControls
{
    public partial class KcsDataControl : UserControl
    {
        private string exportFolder = "ExportKCS";

        public KcsDataControl()
        {
            InitializeComponent();
            EnsureExportFolderExists();
            UpdateFileCount();
            InitTable();
        }

        private void InitTable()
        {
            // Tèo tạo sẵn vài cột cơ bản để DataGrid có chỗ nhập liệu
            DataTable dt = new DataTable();
            dt.Columns.Add("STT");
            dt.Columns.Add("Mác phôi");
            dt.Columns.Add("Mẻ số");
            dt.Columns.Add("Số lượng");
            dt.Columns.Add("Ghi chú");
            KcsDataGrid.ItemsSource = dt.DefaultView;
        }

        private void EnsureExportFolderExists()
        {
            if (!Directory.Exists(exportFolder))
                Directory.CreateDirectory(exportFolder);
        }

        private void UpdateFileCount()
        {
            try
            {
                string today = DateTime.Now.ToString("dd-MM");
                int count = Directory.GetFiles(exportFolder, $"*-{today}-*.xlsx").Length;
                TxtFileCount.Text = $"Số file đã xuất trong ngày: {count}";
            }
            catch
            {
                TxtFileCount.Text = "Số file đã xuất trong ngày: 0";
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string today = DateTime.Now.ToString("dd-MM");
                // Đếm số file hiện có của ngày hôm nay để tạo số thứ tự tiếp theo
                int fileCount = Directory.GetFiles(exportFolder, $"*-{today}-*.xlsx").Length;
                int nextNumber = fileCount + 1;
                
                // Định dạng tên file: x-ngày-tháng-giờ-phút.xlsx
                string fileName = $"{nextNumber}-{DateTime.Now.ToString("dd-MM-HH-mm")}.xlsx";
                string filePath = Path.Combine(exportFolder, fileName);

                using (var workbook = new XLWorkbook())
                {
                    var dt = ((DataView)KcsDataGrid.ItemsSource).Table;
                    var worksheet = workbook.Worksheets.Add("DuLieuKCS");
                    worksheet.Cell(1, 1).InsertTable(dt);
                    worksheet.Columns().AdjustToContents(); // Tự động căn chỉnh độ rộng cột
                    workbook.SaveAs(filePath);
                }

                MessageBox.Show($"Đã xuất file Excel thành công!\nTên file: {fileName}\nThư mục: {exportFolder}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Cập nhật lại số đếm hiển thị
                UpdateFileCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}