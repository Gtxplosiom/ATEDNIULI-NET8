using ATEDNIULI_NET8.Services;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ATEDNIULI_NET8.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private WakeWordDetector? _wakeWordDetector;

        private ImageSource? _listeningIcon;

        public MainWindowViewModel(WakeWordDetector wakeWordDetector)
        {
            _wakeWordDetector = wakeWordDetector;

            _wakeWordDetector.WakeWordDetected += OnWakeWordDetected;

            // Default icon niya
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                ListeningIcon = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/listening-disabled.png"));
            });
        }

        // Properties pra binding
        public ImageSource? ListeningIcon
        {
            get => _listeningIcon;
            set
            {
                _listeningIcon = value;
                OnPropertyChanged(nameof(ListeningIcon));
            }
        }

        public void OnWakeWordDetected()
        {
            var bi = new BitmapImage();

            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,,/Assets/Icons/listening.png");
            bi.EndInit();
            bi.Freeze(); // Apparently need anay i freeze an image kun ig change via bindings

            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                ListeningIcon = bi;
            });
        }
    }
}
