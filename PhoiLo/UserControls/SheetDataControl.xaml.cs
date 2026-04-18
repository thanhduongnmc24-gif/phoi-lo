using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace PhoiLo.UserControls
{
    public partial class SheetDataControl : UserControl
    {
        private bool _isRecalculating = false;

        public SheetDataControl()
        {
            InitializeComponent();
            LoadDataFromGoogle();
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e) => LoadDataFromGoogle();

        public async void LoadDataFromGoogle()
        {
            var config = App.Config;
            if (string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.SheetId)) return;

            try
            {
                UserCredential credential = await GetCredential();
                var service = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "PhoiLo" });
                var response = await service.Spreadsheets.Values.Get(config.SheetId, config.Range).ExecuteAsync();

                if (response.Values != null)
                {
                    DataTable dt = new DataTable();
                    string[] headers = { "STT", "Phương thức nạp", "Mác phôi", "Mẻ số", "Số cây nạp lò", "Ra sàn nguội", "Hư công nghệ", "Hồi lò", "Tổng số thanh khi hết mẻ", "Chiều dài" };
                    foreach (var h in headers) dt.Columns.Add(h);

                    foreach (var r in response.Values)
                    {
                        var row = dt.NewRow();
                        for (int j = 0; j < headers.Length; j++)
                        {
                            if (j < r.Count) row[j] = r[j]?.ToString() ?? "";
                        }
                        dt.Rows.Add(row);
                    }

                    RecalculatePulpitData(dt);

                    dt.RowChanged += (s, e) => { if (!_isRecalculating) CalculateTotal(dt); };
                    dt.RowDeleted += (s, e) => { if (!_isRecalculating) CalculateTotal(dt); };
                    dt.ColumnChanged += Dt_ColumnChanged;

                    MainDataGrid.ItemsSource = dt.DefaultView;
                    CalculateTotal(dt); 
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void MainDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dv = MainDataGrid.ItemsSource as DataView;
            if (dv?.Table == null) return;
            var dt = dv.Table;

            // Xử lý dán dữ liệu (Ctrl + V)
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                string clipboardText = Clipboard.GetText();
                if (string.IsNullOrEmpty(clipboardText)) return;

                string[] lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                int startRow = MainDataGrid.SelectedIndex < 0 ? 0 : MainDataGrid.SelectedIndex;
                
                int startCol = 0;
                if (MainDataGrid.CurrentColumn != null)
                {
                    startCol = MainDataGrid.CurrentColumn.DisplayIndex;
                }

                // [Suy luận] Khóa tự động tính để dán mượt mà, không bị lag tung chảo
                _isRecalculating = true; 

                for (int i = 0; i < lines.Length && (startRow + i) < dt.Rows.Count; i++)
                {
                    string[] c = lines[i].Split('\t');
                    var row = dt.Rows[startRow + i];
                    
                    for (int j = 0; j < c.Length; j++)
                    {
                        int targetCol = startCol + j;
                        // Điền dữ liệu nếu chưa văng ra khỏi bảng
                        if (targetCol < dt.Columns.Count)
                        {
                            row[targetCol] = c[j];
                        }
                    }
                }

                RecalculatePulpitData(dt);
                CalculateTotal(dt);
                _isRecalculating = false;

                e.Handled = true;
            }
            // Xử lý nút Delete để xóa nhiều ô
            else if (e.Key == Key.Delete)
            {
                var selectedCells = MainDataGrid.SelectedCells;
                if (selectedCells.Count > 0)
                {
                    _isRecalculating = true;

                    foreach (var cellInfo in selectedCells)
                    {
                        var rowView = cellInfo.Item as DataRowView;
                        if (rowView != null && cellInfo.Column != null)
                        {
                            int colIndex = cellInfo.Column.DisplayIndex;
                            if (colIndex >= 0 && colIndex < dt.Columns.Count)
                            {
                                rowView.Row[colIndex] = ""; 
                            }
                        }
                    }

                    RecalculatePulpitData(dt);
                    CalculateTotal(dt);
                    _isRecalculating = false;

                    e.Handled = true;
                }
            }
        }

        private void Dt_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (_isRecalculating) return;

            if (e.Column?.ColumnName == "Số cây nạp lò" || 
                e.Column?.ColumnName == "Hồi lò" || 
                e.Column?.ColumnName == "Hư công nghệ") 
            {
                _isRecalculating = true;
                RecalculatePulpitData(e.Row.Table);
                CalculateTotal(e.Row.Table);
                _isRecalculating = false;
            }
        }

        private void RecalculatePulpitData(DataTable dt)
        {
            double tongSoThanh = 0; 
            
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                double soCayNap = 0;
                double.TryParse(row["Số cây nạp lò"]?.ToString(), out soCayNap);
                
                double huCongNghe = 0;
                double.TryParse(row["Hư công nghệ"]?.ToString(), out huCongNghe);
                
                double hoiLo = 0;
                double.TryParse(row["Hồi lò"]?.ToString(), out hoiLo);

                row["Ra sàn nguội"] = (soCayNap - huCongNghe - hoiLo).ToString();

                tongSoThanh += soCayNap;
                row["Tổng số thanh khi hết mẻ"] = tongSoThanh.ToString();
            }
        }

        private void CalculateTotal(DataTable dt)
        {
            double totalNapLo = 0;
            double totalHoiLo = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState != DataRowState.Deleted) 
                {
                    if (double.TryParse(row["Số cây nạp lò"]?.ToString(), out double valNap))
                    {
                        totalNapLo += valNap;
                    }
                    if (double.TryParse(row["Hồi lò"]?.ToString(), out double valHoi))
                    {
                        totalHoiLo += valHoi;
                    }
                }
            }
            
            Dispatcher.Invoke(() => 
            {
                if (TxtTongPhoi != null) TxtTongPhoi.Text = totalNapLo.ToString();
                if (TxtTongHoiLo != null) TxtTongHoiLo.Text = totalHoiLo.ToString();
            });
        }

        private async void BtnPush_Click(object sender, RoutedEventArgs e)
        {
            var config = App.Config;
            if (MainDataGrid.ItemsSource == null) return;

            try
            {
                var dv = MainDataGrid.ItemsSource as DataView;
                if (dv?.Table == null) return;

                var dt = dv.Table;

                _isRecalculating = true;
                RecalculatePulpitData(dt);
                CalculateTotal(dt);
                _isRecalculating = false;

                var values = new List<IList<object>>();

                foreach (DataRow row in dt.Rows)
                {
                    var rowData = row.ItemArray.Select(x => x ?? "").ToList();
                    values.Add(rowData);
                }

                UserCredential credential = await GetCredential();
                var service = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "PhoiLo" });
                
                ValueRange body = new ValueRange() { Values = values };
                var request = service.Spreadsheets.Values.Update(body, config.SheetId, config.Range);
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                
                await request.ExecuteAsync();
                MessageBox.Show("🚀 Đã gởi phôi lên Google Sheets thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gởi dữ liệu: " + ex.Message); }
        }

        private async System.Threading.Tasks.Task<UserCredential> GetCredential()
        {
            var config = App.Config;
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = config.ClientId, ClientSecret = config.ClientSecret },
                new[] { SheetsService.Scope.Spreadsheets }, 
                "user", CancellationToken.None, new FileDataStore("PhoiLo.Auth"));
        }
    }
}