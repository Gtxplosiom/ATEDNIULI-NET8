using ATEDNIULI_NET8.Services;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VoskTest;

namespace ATEDNIULI_NET8.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Wake word detection services
        private readonly PorcupineService? _porcupineWakeWordDetector;
        private readonly VoskService? _voskService;

        // viewmodels
        private readonly FloatingWindowViewModel? _floatingWindowViewModel;

        private ImageSource? _listeningIcon;

        // Adjust this so models can be dynamically switched
        // an pag hook hin events dinhi amo gud an na handle han mga life cycle hin mga voice services
        // note to self: mainwindowVM an core an handling tanan
        public MainWindowViewModel(PorcupineService? wakeWordDetector, WhisperService? whisperService, VoskService? voskService, FloatingWindowViewModel? floatingWindowViewModel)
        {
            _porcupineWakeWordDetector = wakeWordDetector;
            _voskService = voskService;

            _floatingWindowViewModel = floatingWindowViewModel;

            if (_porcupineWakeWordDetector != null) _porcupineWakeWordDetector.WakeWordDetected += OnWakeWordDetected;
            if (_voskService != null) _voskService.DoneTranscription += OnDoneTranscription;

            // Default icon niya
            ListeningIcon = IconSetter("listening-disabled.png");
        }

        // Properties para binding
        public ImageSource? ListeningIcon
        {
            get => _listeningIcon;
            set
            {
                _listeningIcon = value;
                OnPropertyChanged(nameof(ListeningIcon));
            }
        }

        // note to self: this is the one that handles the activation of whisper transcription
        // an mainwindowVM an nag hahandle talaga hin pag start nfan end hin mga voice services
        public void OnWakeWordDetected()
        {
            ListeningIcon = IconSetter("listening.png");

            // pause the wake word for performance purposes
            _porcupineWakeWordDetector?.PauseWakeWordDetection();
            _voskService?.StartRecording();
        }

        public void OnDoneTranscription()
        {
            ListeningIcon = IconSetter("listening-disabled.png");
            _porcupineWakeWordDetector?.ResumeWakeWordDetection();
        }

        private BitmapImage IconSetter(string iconFileName)
        {
            var uri = new Uri($"pack://application:,,,/Assets/Icons/{iconFileName}");

            var bi = new BitmapImage();

            bi.BeginInit();
            bi.UriSource = uri;
            bi.EndInit();
            bi.Freeze();

            return bi;
        }
    }
}
