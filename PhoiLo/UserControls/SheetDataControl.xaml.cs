using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace PhoiLo.UserControls
{
    public partial class SheetDataControl : UserControl
    {
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

                    dt.RowChanged += (s, e) => CalculateTotal(dt);
                    dt.RowDeleted += (s, e) => CalculateTotal(dt);
                    dt.ColumnChanged += (s, e) => { 
                        if (e.Column?.ColumnName == "Số cây nạp lò" || e.Column?.ColumnName == "Hồi lò") 
                            CalculateTotal(dt); 
                    };

                    MainDataGrid.ItemsSource = dt.DefaultView;
                    CalculateTotal(dt); 
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void CalculateTotal(DataTable dt)
        {
            double totalNapLo = 0;
            double totalHoiLo = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState != DataRowState.Deleted) 
                {
                    // Tính tổng Số cây nạp lò
                    if (double.TryParse(row["Số cây nạp lò"]?.ToString(), out double valNap))
                    {
                        totalNapLo += valNap;
                    }
                    // Tính tổng Hồi lò
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