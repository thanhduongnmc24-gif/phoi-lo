using System.Windows;
using PhoiLo.Models;

namespace PhoiLo
{
    public partial class App : Application
    {
        // App sẽ giữ một bản Config duy nhất xuyên suốt quá trình chạy
        public static AppConfig Config { get; private set; } = AppConfig.LoadFromFile();
    }
}