using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;

namespace PhoiLo.Models
{
    public class StaffMember
    {
        public string Kip { get; set; } = "A"; 
        public string HoTen { get; set; } = "";
        public string ChucVu { get; set; } = "Vận hành"; 
    }

    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
        public object Data { get => GetValue(DataProperty); set => SetValue(DataProperty, value); }
    }

    public class AppConfig : INotifyPropertyChanged
    {
        private string _clientId = "", _clientSecret = "", _sheetId = "", _range = "Phoi!A11:J410", _tableFontFamily = "Segoe UI", _menuFontFamily = "Segoe UI";
        private double _tableFontSize = 14, _menuFontSize = 16;
        
        // Túi thần kỳ lưu độ rộng cột tự động
        private Dictionary<string, double> _columnWidths = new Dictionary<string, double>();
        public Dictionary<string, double> ColumnWidths { get => _columnWidths; set { _columnWidths = value; OnPropertyChanged(); } }

        public string ClientId { get => _clientId; set { _clientId = value; OnPropertyChanged(); } }
        public string ClientSecret { get => _clientSecret; set { _clientSecret = value; OnPropertyChanged(); } }
        public string SheetId { get => _sheetId; set { _sheetId = value; OnPropertyChanged(); } }
        public string Range { get => _range; set { _range = value; OnPropertyChanged(); } }
        public double TableFontSize { get => _tableFontSize; set { _tableFontSize = value; OnPropertyChanged(); } }
        public double MenuFontSize { get => _menuFontSize; set { _menuFontSize = value; OnPropertyChanged(); } }
        public string TableFontFamily { get => _tableFontFamily; set { _tableFontFamily = value; OnPropertyChanged(); } }
        public string MenuFontFamily { get => _menuFontFamily; set { _menuFontFamily = value; OnPropertyChanged(); } }

        private ObservableCollection<StaffMember> _staffList = new ObservableCollection<StaffMember>();
        public ObservableCollection<StaffMember> StaffList { get => _staffList; set { _staffList = value; OnPropertyChanged(); } }
        
        private DateTime _currentDate = DateTime.Now;
        public DateTime CurrentDate { get => _currentDate; set { _currentDate = value; OnPropertyChanged(); } }
        
        private string _currentKip = "1A";
        public string CurrentKip { get => _currentKip; set { _currentKip = value; OnPropertyChanged(); } }
        
        private string _currentToTruong = "";
        public string CurrentToTruong { get => _currentToTruong; set { _currentToTruong = value; OnPropertyChanged(); } }
        
        private string _currentVanHanh = "";
        public string CurrentVanHanh { get => _currentVanHanh; set { _currentVanHanh = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void SaveToFile() => File.WriteAllText("config.json", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        
        public static AppConfig LoadFromFile() {
            try { 
                if (File.Exists("config.json")) {
                    var cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText("config.json"));
                    if (cfg != null) {
                        if (cfg.StaffList == null) cfg.StaffList = new ObservableCollection<StaffMember>();
                        if (cfg.ColumnWidths == null) cfg.ColumnWidths = new Dictionary<string, double>();
                        
                        // [Suy luận] Thay vì kiểm tra năm < 2000, 
                        // Tèo ép nó luôn luôn lấy ngày hiện tại của máy tính khi mở app.
                        cfg.CurrentDate = DateTime.Now; 
                        
                        return cfg;
                    }
                }
            } catch { }
            return new AppConfig();
        }
    }
}