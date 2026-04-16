using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PhoiLo.Models
{
    public class StaffMember
    {
        public string Kip { get; set; } = "A"; 
        public string HoTen { get; set; } = "";
        // Đổi thành không dấu để tránh lỗi build
        public string ChucVu { get; set; } = "Vận hành"; 
    }

    public class AppConfig : INotifyPropertyChanged
    {
        private string _clientId = "";
        private string _clientSecret = "";
        private string _sheetId = "";
        private string _range = "Phoi!A11:J410";
        private double _col1Width = 50; private double _col2Width = 120; private double _col3Width = 90;
        private double _col4Width = 80; private double _col5Width = 110; private double _col6Width = 110;
        private double _col7Width = 110; private double _col8Width = 80; private double _col9Width = 180;
        private double _col10Width = 100;
        private double _tableFontSize = 14; private double _menuFontSize = 16;
        private string _tableFontFamily = "Segoe UI"; private string _menuFontFamily = "Segoe UI";

        private ObservableCollection<StaffMember> _staffList = new ObservableCollection<StaffMember>();
        private DateTime _currentDate = DateTime.Now;
        private string _currentKip = "1A";
        private string _currentToTruong = "";
        private string _currentVanHanh = "";

        public string ClientId { get => _clientId; set { _clientId = value; OnPropertyChanged(); } }
        public string ClientSecret { get => _clientSecret; set { _clientSecret = value; OnPropertyChanged(); } }
        public string SheetId { get => _sheetId; set { _sheetId = value; OnPropertyChanged(); } }
        public string Range { get => _range; set { _range = value; OnPropertyChanged(); } }
        public double Col1Width { get => _col1Width; set { _col1Width = value; OnPropertyChanged(); } }
        public double Col2Width { get => _col2Width; set { _col2Width = value; OnPropertyChanged(); } }
        public double Col3Width { get => _col3Width; set { _col3Width = value; OnPropertyChanged(); } }
        public double Col4Width { get => _col4Width; set { _col4Width = value; OnPropertyChanged(); } }
        public double Col5Width { get => _col5Width; set { _col5Width = value; OnPropertyChanged(); } }
        public double Col6Width { get => _col6Width; set { _col6Width = value; OnPropertyChanged(); } }
        public double Col7Width { get => _col7Width; set { _col7Width = value; OnPropertyChanged(); } }
        public double Col8Width { get => _col8Width; set { _col8Width = value; OnPropertyChanged(); } }
        public double Col9Width { get => _col9Width; set { _col9Width = value; OnPropertyChanged(); } }
        public double Col10Width { get => _col10Width; set { _col10Width = value; OnPropertyChanged(); } }
        public double TableFontSize { get => _tableFontSize; set { _tableFontSize = value; OnPropertyChanged(); } }
        public double MenuFontSize { get => _menuFontSize; set { _menuFontSize = value; OnPropertyChanged(); } }
        public string TableFontFamily { get => _tableFontFamily; set { _tableFontFamily = value; OnPropertyChanged(); } }
        public string MenuFontFamily { get => _menuFontFamily; set { _menuFontFamily = value; OnPropertyChanged(); } }

        public ObservableCollection<StaffMember> StaffList { get => _staffList; set { _staffList = value; OnPropertyChanged(); } }
        public DateTime CurrentDate { get => _currentDate; set { _currentDate = value; OnPropertyChanged(); } }
        public string CurrentKip { get => _currentKip; set { _currentKip = value; OnPropertyChanged(); } }
        public string CurrentToTruong { get => _currentToTruong; set { _currentToTruong = value; OnPropertyChanged(); } }
        public string CurrentVanHanh { get => _currentVanHanh; set { _currentVanHanh = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void SaveToFile() => File.WriteAllText("config.json", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        
        public static AppConfig LoadFromFile() {
            try { 
                if (File.Exists("config.json")) {
                    var cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText("config.json"));
                    if (cfg != null) {
                        // Vá lỗi file config cũ không có dữ liệu
                        if (cfg.StaffList == null) cfg.StaffList = new ObservableCollection<StaffMember>();
                        if (cfg.CurrentDate.Year < 2000) cfg.CurrentDate = DateTime.Now; // Sửa lỗi văng DatePicker
                        if (string.IsNullOrEmpty(cfg.CurrentKip)) cfg.CurrentKip = "1A";
                        return cfg;
                    }
                }
            } catch { }
            return new AppConfig();
        }
    }
}