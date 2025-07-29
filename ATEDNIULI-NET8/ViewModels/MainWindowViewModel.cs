using ATEDNIULI_NET8.Services;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ATEDNIULI_NET8.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Wake word detection services
        private readonly PorcupineService? _porcupineWakeWordDetector;

        private ImageSource? _listeningIcon;

        // Adjust this so models can be dynamically switched
        public MainWindowViewModel(PorcupineService? wakeWordDetector)
        {
            _porcupineWakeWordDetector = wakeWordDetector;

            if (_porcupineWakeWordDetector != null) _porcupineWakeWordDetector.WakeWordDetected += OnWakeWordDetected;

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
