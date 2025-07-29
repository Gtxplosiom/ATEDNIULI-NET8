using System.ComponentModel;

namespace ATEDNIULI_NET8.Models
{
    public class TranscriptionModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _text = "";
        private string _notificationText = "";

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public string NotificationText
        {
            get => _notificationText;
            set
            {
                if (_notificationText != value)
                {
                    _notificationText = value;
                    OnPropertyChanged(nameof(NotificationText));
                }
            }
        }
    }
}
