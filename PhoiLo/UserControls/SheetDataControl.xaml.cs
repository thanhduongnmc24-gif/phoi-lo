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
    public class AppConfig
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string SheetId { get; set; } = "";
        public string Range { get; set; } = "Phoi!A11:J410";
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
            catch { }
        }

        private void SaveConfig()
        {
            try
            {
                AppConfig config = new AppConfig {
                    ClientId = TxtClientId.Text.Trim(),
                    ClientSecret = TxtClientSecret.Text.Trim(),
                    SheetId = TxtSheetId.Text.Trim(),
                    Range = TxtRange.Text.Trim()
                };
                File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(config));
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
                MessageBox.Show("Anh hai điền thiếu thông tin kìa!", "Nhắc nhở");
                return;
            }

            SaveConfig();
            BtnConnect.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            TxtStatus.Text = "Đang lấy dữ liệu từ Phoi!A11:J410...";
            SheetDataGrid.Visibility = Visibility.Collapsed;

            try
            {
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                    Scopes, "user", CancellationToken.None, new FileDataStore("PhoiLo.GoogleAuth.Store"));

                var service = new SheetsService(new BaseClientService.Initializer() {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = await request.ExecuteAsync();
                var values = response.Values;

                if (values != null && values.Count > 0)
                {
                    DataTable dt = new DataTable();
                    // Thiết lập tiêu đề theo đúng yêu cầu của anh hai
                    string[] headers = { "STT", "Phương thức nạp", "Mác phôi", "Mẻ số", "Số cây nạp lò", "Ra sàn nguội", "Hư công nghệ", "Hồi lò", "Tổng số thanh khi hết mẻ", "Chiều dài" };
                    foreach (var h in headers) dt.Columns.Add(h);

                    foreach (var r in values)
                    {
                        var row = dt.NewRow();
                        bool hasValue = false;
                        for (int j = 0; j < headers.Length; j++)
                        {
                            if (j < r.Count) {
                                row[j] = r[j]?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(row[j].ToString())) hasValue = true;
                            }
                        }
                        // Chỉ thêm dòng nếu có ít nhất 1 ô có dữ liệu
                        if (hasValue) dt.Rows.Add(row);
                    }

                    SheetDataGrid.ItemsSource = dt.DefaultView;
                    TxtStatus.Text = "";
                    SheetDataGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    TxtStatus.Text = "Vùng này không có dữ liệu anh hai ơi!";
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                BtnConnect.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }
    }
}