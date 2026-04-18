using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClosedXML.Excel;
using PhoiLo.Helpers;

namespace PhoiLo.UserControls
{
    public partial class KcsDataControl : UserControl
    {
        //private DataTable _dtKcs;
        private DataTable _dtKcs = new DataTable();
        public KcsDataControl() {
            InitializeComponent();
            InitTable();
        }

        private void InitTable() {
            _dtKcs = new DataTable();
            _dtKcs.Columns.Add("STT");
            _dtKcs.Columns.Add("PhuongThuc");
            _dtKcs.Columns.Add("MacPhoi");
            _dtKcs.Columns.Add("MeSo");
            _dtKcs.Columns.Add("SoCayNap");
            _dtKcs.Columns.Add("ChieuDai");

            for (int i = 1; i <= 50; i++) {
                var row = _dtKcs.NewRow();
                row["STT"] = i.ToString();
                _dtKcs.Rows.Add(row);
            }
            KcsDataGrid.ItemsSource = _dtKcs.DefaultView;
            UpdateFileCount();
        }

        private void KcsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e) {
            // [Suy luận] Gọi "trái tim" dùng chung, dán và xóa chuẩn Excel
            DataGridHelper.HandleExcelActions(KcsDataGrid, e);
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("Anh hai muốn XÓA SẠCH bảng KCS?", "Cảnh báo", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes) {
                foreach (DataRow row in _dtKcs.Rows) {
                    for (int i = 0; i < _dtKcs.Columns.Count; i++) row[i] = "";
                }
            }
        }

        private void UpdateFileCount() {
            try {
                var cfg = App.Config;
                string path = Path.Combine("ExportKCS", $"Kip-{cfg.CurrentKip}-Ngay-{cfg.CurrentDate:dd-MM-yyyy}");
                TxtFileCount.Text = $"Số file trong ngày: {(Directory.Exists(path) ? Directory.GetFiles(path, "*.xlsx").Length : 0)}";
            } catch { }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e) {
            try {
                var cfg = App.Config;
                string folder = Path.Combine("ExportKCS", $"Kip-{cfg.CurrentKip}-Ngay-{cfg.CurrentDate:dd-MM-yyyy}");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                
                string fileName = Path.Combine(folder, $"{Directory.GetFiles(folder, "*.xlsx").Length + 1}-{cfg.CurrentKip}-{DateTime.Now:HHmm}.xlsx");
                
                using (var wb = new XLWorkbook()) {
                    var ws = wb.Worksheets.Add("KCS");
                    ws.Cell(1, 1).Value = $"Kíp {cfg.CurrentKip} ngày {cfg.CurrentDate:dd/MM/yyyy}";
                    ws.Range("A1:F1").Merge().Style.Font.Bold = true;

                    string[] headers = { "STT", "Phương thức nạp", "Mác phôi", "Mẻ số", "Số cây nạp lò", "Chiều dài" };
                    for (int i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
                    ws.Range("A3:F3").Style.Font.Bold = true;

                    int r = 4;
                    foreach (DataRow row in _dtKcs.Rows) {
                        if (string.IsNullOrEmpty(row["MacPhoi"]?.ToString())) continue;
                        for (int i = 0; i < 6; i++) ws.Cell(r, i + 1).Value = row[i]?.ToString();
                        r++;
                    }
                    wb.SaveAs(fileName);
                }
                MessageBox.Show("Đã xuất file!"); UpdateFileCount();
            } catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
    }
}