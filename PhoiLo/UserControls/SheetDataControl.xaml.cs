using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;

namespace PhoiLo.UserControls
{
    // Class nhỏ để cấu trúc dữ liệu lưu trữ
    public class AppConfig
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string SheetId { get; set; } = "";
        public string Range { get; set; } = "Sheet1!A1:Z100";
    }

    public partial class SheetDataControl : UserControl
    {
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "PhoiLo App";
        static string ConfigFilePath = "config.json";

        public SheetDataControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }

        // Đọc thông tin đã lưu
        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    AppConfig config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                    TxtClientId.Text = config.ClientId;
                    TxtClientSecret.Text = config.ClientSecret;
                    TxtSheetId.Text = config.SheetId;
                    TxtRange.Text = config.Range;
                }
            }
            catch { /* Lỗi đọc file thì kệ, cho người dùng nhập lại */ }
        }

        // Ghi nhớ thông tin
        private void SaveConfig()
        {
            try
            {
                AppConfig config = new AppConfig
                {
                    ClientId = TxtClientId.Text.Trim(),
                    ClientSecret = TxtClientSecret.Text.Trim(),
                    SheetId = TxtSheetId.Text.Trim(),
                    Range = TxtRange.Text.Trim()
                };
                string json = JsonSerializer.Serialize(config);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch { }
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            string clientId = TxtClientId.Text.Trim();
            string clientSecret = TxtClientSecret.Text.Trim();
            string spreadsheetId = TxtSheetId.Text.Trim();
            string range = TxtRange.Text.Trim();

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(spreadsheetId))
            {
                MessageBox.Show("Anh hai vui lòng điền đầy đủ Client ID, Client Secret và Sheet ID nha!", "Thiếu thông tin");
                return;
            }

            // Lưu tự động
            SaveConfig();

            // Hiệu ứng chờ (Loading)
            BtnConnect.IsEnabled = false;
            BtnConnect.Content = "⏳ ĐANG LẤY DỮ LIỆU...";
            Mouse.OverrideCursor = Cursors.Wait;
            TxtStatus.Text = "Đang kết nối thần giao cách cảm với Google...";
            SheetDataGrid.Visibility = Visibility.Collapsed;

            try
            {
                ClientSecrets secrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("PhoiLo.GoogleAuth.Store"));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;

                if (values != null && values.Count > 0)
                {
                    DataTable dt = new DataTable();
                    var headers = values[0];
                    
                    // Xử lý tiêu đề cột để tránh bị rỗng gây lỗi bảng
                    for (int j = 0; j < headers.Count; j++)
                    {
                        string colName = headers[j]?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(colName)) colName = $"Cột_Trống_{j + 1}";
                        
                        // Chống trùng tên cột
                        while (dt.Columns.Contains(colName)) colName += "_1";
                        dt.Columns.Add(colName);
                    }

                    // Nạp dữ liệu
                    for (int i = 1; i < values.Count; i++)
                    {
                        var row = dt.NewRow();
                        var rowData = values[i];
                        bool hasData = false;

                        for (int j = 0; j < headers.Count; j++)
                        {
                            if (j < rowData.Count)
                            {
                                string cellValue = rowData[j]?.ToString() ?? "";
                                row[j] = cellValue;
                                if (!string.IsNullOrEmpty(cellValue)) hasData = true;
                            }
                        }
                        
                        // Chỉ thêm dòng nếu có dữ liệu thật sự
                        if (hasData) dt.Rows.Add(row);
                    }

                    SheetDataGrid.ItemsSource = dt.DefaultView;
                    
                    // Biến hình! Ẩn chữ, hiện bảng
                    TxtStatus.Text = "";
                    SheetDataGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtStatus.Text = "Kéo về được nguyên một vùng trống trơn anh hai ơi! Đổi Range thử xem.";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Gãy cánh rồi: " + ex.Message;
                TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
            finally
            {
                // Trả lại nguyên trạng cho giao diện
                BtnConnect.IsEnabled = true;
                BtnConnect.Content = "🚀 KẾT NỐI & LẤY DỮ LIỆU";
                Mouse.OverrideCursor = null;
            }
        }
    }
}