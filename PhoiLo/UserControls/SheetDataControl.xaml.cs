using System;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
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

        private async void LoadDataFromGoogle()
        {
            var config = App.Config;
            // Lưu lại cấu hình hiện tại trước khi gọi API
            config.SaveToFile();

            if (string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.SheetId)) return;

            try
            {
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = config.ClientId, ClientSecret = config.ClientSecret },
                    new[] { SheetsService.Scope.SpreadsheetsReadonly }, 
                    "user", CancellationToken.None, new FileDataStore("PhoiLo.Auth"));

                var service = new SheetsService(new BaseClientService.Initializer() 
                { 
                    HttpClientInitializer = credential, 
                    ApplicationName = "PhoiLo" 
                });

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
                    MainDataGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show("Lỗi kết nối: " + ex.Message); 
            }
        }
    }
}