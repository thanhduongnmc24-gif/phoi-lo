using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PhoiLo.Models
{
    public class AppConfig : INotifyPropertyChanged
    {
        private string _clientId = "";
        private string _clientSecret = "";
        private string _sheetId = "";
        private string _range = "Phoi!A11:J410";

        public string ClientId { get => _clientId; set { _clientId = value; OnPropertyChanged(); } }
        public string ClientSecret { get => _clientSecret; set { _clientSecret = value; OnPropertyChanged(); } }
        public string SheetId { get => _sheetId; set { _sheetId = value; OnPropertyChanged(); } }
        public string Range { get => _range; set { _range = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}