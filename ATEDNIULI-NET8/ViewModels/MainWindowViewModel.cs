using ATEDNIULI_NET8.Services;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace ATEDNIULI_NET8.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Wake word detection services
        private readonly PorcupineService? _porcupineWakeWordDetector;
        private readonly WhisperService? _whisperService;

        private ImageSource? _listeningIcon;

        // Adjust this so models can be dynamically switched
        public MainWindowViewModel(PorcupineService? wakeWordDetector, WhisperService? whisperService)
        {
            _porcupineWakeWordDetector = wakeWordDetector;
            _whisperService = whisperService;

            if (_porcupineWakeWordDetector != null) _porcupineWakeWordDetector.WakeWordDetected += OnWakeWordDetected;
            if (_whisperService != null) _whisperService.DoneTranscription += OnDoneTranscription;

            // Default icon niya
            Application.Current.Dispatcher.Invoke(() =>
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListeningIcon = bi;
            });

            // pause the wake word for performance purposes
            _porcupineWakeWordDetector?.PauseWakeWordDetection();
            _whisperService?.RecordAudioInput();
        }

        // fix ths it makes the icon change back instantly
        public void OnDoneTranscription()
        {
            var bi = new BitmapImage();

            bi.BeginInit();
            bi.UriSource = new Uri("pack://application:,,,/Assets/Icons/listening-disabled.png");
            bi.EndInit();
            bi.Freeze(); // do the same here

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListeningIcon = bi;
            });

            // pause the wake word for performance purposes
            _porcupineWakeWordDetector?.ResumeWakeWordDetection();
        }
    }
}
