using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Models;
using System.Windows;
using System.Numerics;
using System.Windows.Input;
using ATEDNIULI_NET8.ViewModels.Commands;
using System.Diagnostics;

namespace ATEDNIULI_NET8.ViewModels
{
    public class FloatingWindowViewModel : ViewModelBase
    {
        private readonly PorcupineService? _porcupineWakeWordDetector;
        private readonly WhisperService? _whisperService;
        private readonly IntentService? _intentService;

        private readonly Vector2 _screenDimentions;
        private readonly int _taskBarHeight;

        private CameraMouseWindowViewModel? _cameraMouseWindowViewModel;

        // floating window properties
        private Visibility _visibilityState;
        private int _leftState;
        private int _topState;

        // Models
        private readonly TranscriptionModel _transcriptionModel = new();

        // Commands
        // an floating window VM naagi an mga commands kay i found it na more accessible an mga variables dinhi
        public ICommand? ToggleCameraMouse { get; }
        // TODO: Implement this
        public ICommand? ShowItems { get; }

        public FloatingWindowViewModel(PorcupineService? wakeWordDetector, WhisperService? whisperService, IntentService? intentService, CameraMouseWindowViewModel cameraMouseWindowViewModel)
        {
            _porcupineWakeWordDetector = wakeWordDetector;
            _whisperService = whisperService;
            _intentService = intentService;

            // connect camera mouse window view model
            // an mga events na gin hook dinhi pag handle la hin ui states
            _cameraMouseWindowViewModel = cameraMouseWindowViewModel;

            if (_porcupineWakeWordDetector != null) _porcupineWakeWordDetector.WakeWordDetected += OnWakeWordDetected;

            if (_whisperService != null)
            {
                _whisperService.ProcessingTranscription += OnProcessingTranscription;
                _whisperService.DoneProcessing += OnDoneProcessing;
                _whisperService.TranscriptionResultReady += OnTranscriptionResultReady;
            }

            _screenDimentions = new Vector2(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight
            );
            _taskBarHeight = (int)_screenDimentions.Y - (int)SystemParameters.WorkArea.Height;

            VisibilityState = Visibility.Collapsed; // Default state

            // is this right??
            if (_transcriptionModel != null) TranscriptionText = "Listening";

            LeftState = (int)_screenDimentions.X - 270; // Magic numbers (for now) 200(floating window width) + 70(main window width)
            TopState = (int)_screenDimentions.Y - (70 + _taskBarHeight + 25); // 70(height of floating window) and 25(height of notification window)

            // Initialize commands
            ToggleCameraMouse = new ToggleCameraMouseCommand(_cameraMouseWindowViewModel);
        }

        // Properties
        // TODO: kinda useless pa an pag pass hin transcription model text, bangin i consider nala an string pareho han una.
        // since service class man ini
        public string? TranscriptionText
        {
            get => _transcriptionModel.Text;
            set
            {
                _transcriptionModel.Text = value;
                OnPropertyChanged(nameof(TranscriptionText));
            }
        }

        // pati adi pero for now adi la anay
        public string? NotificationText
        {
            get => _transcriptionModel.NotificationText;
            set
            {
                _transcriptionModel.NotificationText = value;
                OnPropertyChanged(nameof(NotificationText));
            }
        }

        public Visibility VisibilityState
        {
            get => _visibilityState;
            set
            {
                _visibilityState = value;
                OnPropertyChanged(nameof(VisibilityState));
            }
        }

        public int LeftState
        {
            get => _leftState;
            set
            {
                _leftState = value;
                OnPropertyChanged(nameof(LeftState));
            }
        }

        public int TopState
        {
            get => _topState;
            set
            {
                _topState = value;
                OnPropertyChanged(nameof(TopState));
            }
        }

        private void OnWakeWordDetected()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                VisibilityState = Visibility.Visible;
            });
        }

        private void OnProcessingTranscription()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TranscriptionText = "Processing...";
            });
        }

        private void OnDoneProcessing()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                VisibilityState = Visibility.Collapsed;
                TranscriptionText = "Listening";
            });
        }

        private void OnTranscriptionResultReady(string transcriptionResult)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotificationText = transcriptionResult;
            });

            var intent = _intentService?.PredictIntent(transcriptionResult);

            CommandHandler(intent);
        }

        private void CommandHandler(string? command)
        {
            // TODO: ig change ini para an command toggle or mas better ada kun an cameramousewindowviewmodel instance ig instance nala didi
            // kay para ma access an mga properties tikang didi kaysa ha cameramouse command class
            if (command == "OpenCameraMouse")
            {
                ToggleCameraMouse?.Execute(true);
            }
            else if (command == "CloseCameraMouse")
            {
                ToggleCameraMouse?.Execute(false);
            }
            else if (command == "ShowItems")
            {
                Debug.WriteLine("Showing clickable items on screen");
            }
        }
    }
}

// TODO: Do something about the magic numbers
// TODO: maybe floating window just serves as a state for transcription, or for typing
