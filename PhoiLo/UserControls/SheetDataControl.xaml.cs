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
using PhoiLo.Helpers;

namespace PhoiLo.UserControls
{
    public partial class SheetDataControl : UserControl
    {
        private bool _isRecalculating = false;
        private bool _isInitializing = true;

        public SheetDataControl()
        {
            InitializeComponent();
            LoadDataFromGoogle();
            DataGridHelper.EnableWidthAutoSave(MainDataGrid, "Pulpit1");
            
            // [Suy luận] Khôi phục độ rộng của cái Panel Tổng hợp từ file JSON
            if (App.Config.ColumnWidths.TryGetValue("Pulpit1_RightPanelWidth", out double w) && w > 50) {
                RightPanelColumn.Width = new GridLength(w);
            }
            _isInitializing = false;
        }

        // Sự kiện này nổ ra mỗi khi anh hai dùng GridSplitter để kéo panel
        private void SummaryBorder_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (_isInitializing || App.Config.ColumnWidths == null) return;
            
            double currentWidth = SummaryBorder.ActualWidth;
            // Đã sửa lại đúng tên biến currentWidth
            if (currentWidth > 50) {
                App.Config.ColumnWidths["Pulpit1_RightPanelWidth"] = currentWidth;
                DataGridHelper.SaveConfig(); // Nhờ thằng Helper lưu dùm
            }
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
                        for (int j = 0; j < headers.Length; j++) {
                            if (j < r.Count) row[j] = r[j]?.ToString() ?? "";
                        }
                        dt.Rows.Add(row);
                    }

                    RecalculatePulpitData(dt);
                    dt.ColumnChanged += (s, e) => {
                        if (!_isRecalculating && e.Column != null && (e.Column.ColumnName == "Số cây nạp lò" || e.Column.ColumnName == "Hồi lò" || e.Column.ColumnName == "Hư công nghệ"))
                        {
                            _isRecalculating = true;
                            RecalculatePulpitData(e.Row.Table);
                            CalculateTotal(e.Row.Table);
                            _isRecalculating = false;
                        }
                    };

                    MainDataGrid.ItemsSource = dt.DefaultView;
                    CalculateTotal(dt); 
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void MainDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGridHelper.HandleExcelActions(MainDataGrid, e, () => {
                var dv = MainDataGrid.ItemsSource as DataView;
                if (dv != null && dv.Table != null) {
                    _isRecalculating = true;
                    RecalculatePulpitData(dv.Table);
                    CalculateTotal(dv.Table);
                    _isRecalculating = false;
                }
            });
        }

        private void RecalculatePulpitData(DataTable dt)
        {
            double tongSoThanh = 0; 
            foreach (DataRow row in dt.Rows) {
                if (row.RowState == DataRowState.Deleted) continue;
                double n = 0; double.TryParse(row["Số cây nạp lò"]?.ToString(), out n);
                double hcn = 0; double.TryParse(row["Hư công nghệ"]?.ToString(), out hcn);
                double hl = 0; double.TryParse(row["Hồi lò"]?.ToString(), out hl);
                row["Ra sàn nguội"] = (n - hcn - hl).ToString();
                tongSoThanh += n;
                row["Tổng số thanh khi hết mẻ"] = tongSoThanh.ToString();
            }
        }

        private void CalculateTotal(DataTable dt)
        {
            double tNap = 0; double tHoi = 0; double tNong = 0;
            var lengthStats = new Dictionary<string, double>();

            foreach (DataRow row in dt.Rows) {
                if (row.RowState != DataRowState.Deleted) {
                    double n; double.TryParse(row["Số cây nạp lò"]?.ToString(), out n);
                    tNap += n;
                    double h; if (double.TryParse(row["Hồi lò"]?.ToString(), out h)) tHoi += h;

                    string pt = row["Phương thức nạp"]?.ToString() ?? "";
                    if (pt.ToLower().Contains("nóng")) tNong += n;

                    string cd = row["Chiều dài"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(cd) && n > 0) {
                        if (lengthStats.ContainsKey(cd)) lengthStats[cd] += n;
                        else lengthStats.Add(cd, n);
                    }
                }
            }
            Dispatcher.Invoke(() => { 
                if (TxtTongPhoi != null) TxtTongPhoi.Text = tNap.ToString(); 
                if (TxtTongHoiLo != null) TxtTongHoiLo.Text = tHoi.ToString(); 
                if (TxtTongPhoiNong != null) TxtTongPhoiNong.Text = tNong.ToString();
                if (LengthStatsGrid != null) LengthStatsGrid.ItemsSource = lengthStats.Select(x => new { ChieuDai = x.Key, SoLuong = x.Value }).OrderBy(x => x.ChieuDai).ToList();
            });
        }

        private async void BtnPush_Click(object sender, RoutedEventArgs e)
        {
            try {
                var dt = (MainDataGrid.ItemsSource as DataView)?.Table;
                if (dt == null) return;
                var values = new List<IList<object>>();
                foreach (DataRow row in dt.Rows) values.Add(row.ItemArray.Select(x => x ?? "").ToList());

                UserCredential credential = await GetCredential();
                var service = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "PhoiLo" });
                var request = service.Spreadsheets.Values.Update(new ValueRange { Values = values }, App.Config.SheetId, App.Config.Range);
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                await request.ExecuteAsync();
                MessageBox.Show("🚀 Đã gởi phôi lên Google Sheets thành công!");
            } catch (Exception ex) { MessageBox.Show("Lỗi gởi dữ liệu: " + ex.Message); }
        }

        private async System.Threading.Tasks.Task<UserCredential> GetCredential()
        {
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = App.Config.ClientId, ClientSecret = App.Config.ClientSecret },
                new[] { SheetsService.Scope.Spreadsheets }, "user", CancellationToken.None, new FileDataStore("PhoiLo.Auth"));
        }
    }
}