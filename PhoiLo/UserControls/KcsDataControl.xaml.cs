using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClosedXML.Excel;

namespace PhoiLo.UserControls
{
    public class KcsRow {
        public string STT { get; set; } = "";
        public string PhuongThuc { get; set; } = "";
        public string MacPhoi { get; set; } = "";
        public string MeSo { get; set; } = "";
        public string SoCayNap { get; set; } = "";
        public string ChieuDai { get; set; } = "";
    }

    public partial class KcsDataControl : UserControl
    {
        private List<KcsRow> _dataList = new List<KcsRow>();
        
        public KcsDataControl() {
            InitializeComponent();
            _dataList = Enumerable.Range(1, 50).Select(i => new KcsRow { STT = i.ToString() }).ToList();
            KcsDataGrid.ItemsSource = _dataList;
        }

        private void KcsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e) {
            // Xử lý dán dữ liệu (Ctrl + V)
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                string clipboardText = Clipboard.GetText();
                if (string.IsNullOrEmpty(clipboardText)) return;

                string[] lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                int startRow = KcsDataGrid.SelectedIndex < 0 ? 0 : KcsDataGrid.SelectedIndex;
                
                int startCol = 0;
                if (KcsDataGrid.CurrentColumn != null) {
                    startCol = KcsDataGrid.CurrentColumn.DisplayIndex;
                }

                for (int i = 0; i < lines.Length && (startRow + i) < _dataList.Count; i++) {
                    string[] c = lines[i].Split('\t');
                    var row = _dataList[startRow + i];
                    
                    for (int j = 0; j < c.Length; j++) {
                        int targetCol = startCol + j;
                        switch (targetCol) {
                            case 0: row.STT = c[j]; break;
                            case 1: row.PhuongThuc = c[j]; break;
                            case 2: row.MacPhoi = c[j]; break;
                            case 3: row.MeSo = c[j]; break;
                            case 4: row.SoCayNap = c[j]; break;
                            case 5: row.ChieuDai = c[j]; break;
                        }
                    }
                }
                KcsDataGrid.Items.Refresh(); 
                e.Handled = true;
            }
            // [Suy luận] Xử lý sự kiện nhấn phím Delete để xóa nhiều ô đang chọn
            else if (e.Key == Key.Delete) {
                var selectedCells = KcsDataGrid.SelectedCells;
                if (selectedCells.Count > 0) {
                    foreach (var cellInfo in selectedCells) {
                        var row = cellInfo.Item as KcsRow;
                        if (row != null && cellInfo.Column != null) {
                            int colIndex = cellInfo.Column.DisplayIndex;
                            // Quét trúng cột nào, dọn sạch cột đó (kể cả STT)
                            switch (colIndex) {
                                case 0: row.STT = ""; break;
                                case 1: row.PhuongThuc = ""; break;
                                case 2: row.MacPhoi = ""; break;
                                case 3: row.MeSo = ""; break;
                                case 4: row.SoCayNap = ""; break;
                                case 5: row.ChieuDai = ""; break;
                            }
                        }
                    }
                    KcsDataGrid.Items.Refresh();
                    e.Handled = true; // Báo cho hệ thống biết là mình đã xử lý xong phím Delete
                }
            }
        }

        // [Suy luận] Xử lý nút bấm xóa sạch toàn bộ nội dung bảng
        private void BtnClearAll_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("Anh hai có chắc chắn muốn XÓA SẠCH toàn bộ dữ liệu trong bảng này không?\n(Bao gồm cả cột STT)", "Cảnh báo xóa dữ liệu", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes) {
                foreach (var row in _dataList) {
                    row.STT = "";
                    row.PhuongThuc = "";
                    row.MacPhoi = "";
                    row.MeSo = "";
                    row.SoCayNap = "";
                    row.ChieuDai = "";
                }
                KcsDataGrid.Items.Refresh();
            }
        }

        private void UpdateFileCount() {
            var cfg = App.Config;
            string dateStr = cfg.CurrentDate.ToString("dd-MM-yyyy");
            string subFolderName = $"Kip-{cfg.CurrentKip}-Ngay-{dateStr}";
            string fullFolderPath = Path.Combine("ExportKCS", subFolderName);
            
            if (!Directory.Exists(fullFolderPath)) {
                TxtFileCount.Text = "Số file trong ngày: 0";
                return;
            }
            TxtFileCount.Text = $"Số file trong ngày: {Directory.GetFiles(fullFolderPath, "*.xlsx").Length}";
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e) {
            try {
                var cfg = App.Config;
                string dateExcelStr = cfg.CurrentDate.ToString("dd/MM/yyyy");
                string dateFolderStr = cfg.CurrentDate.ToString("dd-MM-yyyy");
                string kipStr = cfg.CurrentKip;
                
                string subFolderName = $"Kip-{kipStr}-Ngay-{dateFolderStr}";
                string fullFolderPath = Path.Combine("ExportKCS", subFolderName);
                if (!Directory.Exists(fullFolderPath)) Directory.CreateDirectory(fullFolderPath);
                
                int x = Directory.GetFiles(fullFolderPath, "*.xlsx").Length + 1;
                string fileName = $"{x}-{kipStr}-{dateFolderStr}-{DateTime.Now:HHmm}.xlsx";
                string filePath = Path.Combine(fullFolderPath, fileName);
                
                using (var wb = new XLWorkbook()) {
                    var ws = wb.Worksheets.Add("KCS");
                    
                    ws.Cell(1, 1).Value = $"Kíp {kipStr} ngày {dateExcelStr}";
                    ws.Range("A1:F1").Merge();
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    ws.Cell(3, 1).Value = "STT"; 
                    ws.Cell(3, 2).Value = "Phương thức nạp"; 
                    ws.Cell(3, 3).Value = "Mác phôi";
                    ws.Cell(3, 4).Value = "Mẻ số"; 
                    ws.Cell(3, 5).Value = "Số cây nạp lò"; 
                    ws.Cell(3, 6).Value = "Chiều dài";
                    ws.Range("A3:F3").Style.Font.Bold = true;

                    int r = 4;
                    foreach (var item in _dataList.Where(i => !string.IsNullOrEmpty(i.MacPhoi) || !string.IsNullOrEmpty(i.MeSo) || !string.IsNullOrEmpty(i.PhuongThuc))) {
                        ws.Cell(r, 1).Value = item.STT; 
                        ws.Cell(r, 2).Value = item.PhuongThuc;
                        ws.Cell(r, 3).Value = item.MacPhoi; 
                        ws.Cell(r, 4).Value = item.MeSo; 
                        ws.Cell(r, 5).Value = item.SoCayNap;
                        ws.Cell(r, 6).Value = item.ChieuDai;
                        r++;
                    }
                    ws.Columns().AdjustToContents();
                    wb.SaveAs(filePath);
                }
                MessageBox.Show($"Đã xuất file vào thư mục: ExportKCS\\{subFolderName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information); 
                UpdateFileCount();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}