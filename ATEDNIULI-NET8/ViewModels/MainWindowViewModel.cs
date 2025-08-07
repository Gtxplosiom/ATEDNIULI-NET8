using ATEDNIULI_NET8.Services;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using VoskTest;

namespace ATEDNIULI_NET8.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Wake word detection services
        private readonly PorcupineService? _porcupineWakeWordDetector;
        private readonly VoskService? _voskService;

        private ImageSource? _listeningIcon;

        // Adjust this so models can be dynamically switched
        // an pag hook hin events dinhi amo gud an na handle han mga life cycle hin mga voice services
        // note to self: mainwindowVM an core an handling tanan
        public MainWindowViewModel(PorcupineService? wakeWordDetector, WhisperService? whisperService, VoskService? voskService)
        {
            _porcupineWakeWordDetector = wakeWordDetector;
            _voskService = voskService;

            if (_porcupineWakeWordDetector != null) _porcupineWakeWordDetector.WakeWordDetected += OnWakeWordDetected;
            if (_voskService != null) _voskService.DoneTranscription += OnDoneTranscription;

            // Default icon niya
            Application.Current.Dispatcher.Invoke(() =>
            {
                ListeningIcon = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/listening-disabled.png"));
            });
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
            _voskService?.StartRecording();
        }

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

            // resume an wake word detection kun matapos na an whisper transcription
            _porcupineWakeWordDetector?.ResumeWakeWordDetection();
        }
    }
}
