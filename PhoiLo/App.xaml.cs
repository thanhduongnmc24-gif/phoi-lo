using System.Windows;
using PhoiLo.Models;

namespace PhoiLo
{
    public partial class App : Application
    {
        // Tạo một đối tượng cấu hình duy nhất
        public static AppConfig Config { get; } = new AppConfig();
    }
}