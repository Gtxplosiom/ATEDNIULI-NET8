using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Models;
using System.Windows.Threading;
using System.Windows;
using System.Numerics;

namespace ATEDNIULI_NET8.ViewModels
{
    public class FloatingWindowViewModel : ViewModelBase
    {
        private readonly PorcupineService? _porcupineWakeWordDetector;
        private readonly WhisperService? _whisperService;

        private readonly Vector2 _screenDimentions;
        private readonly int _taskBarHeight;

        // floating window properties
        private Visibility _visibilityState;
        private int _leftState;
        private int _topState;

        // models
        private readonly TranscriptionModel _transcriptionModel = new();

        public FloatingWindowViewModel(PorcupineService? wakeWordDetector, WhisperService? whisperService)
        {
            _porcupineWakeWordDetector = wakeWordDetector;
            _whisperService = whisperService;

            if (_porcupineWakeWordDetector != null) _porcupineWakeWordDetector.WakeWordDetected += OnWakeWordDetected;

            if (_whisperService != null)
            {
                _whisperService.ProcessingTranscription += OnProcessingTranscription;
                _whisperService.DoneProcessing += OnDoneProcessing;
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
            TopState = (int)_screenDimentions.Y - (70 + _taskBarHeight); // 70(height of floating window)
        }

        // Properties
        public string? TranscriptionText
        {
            get => _transcriptionModel.Text;
            set
            {
                _transcriptionModel.Text = value;
                OnPropertyChanged(nameof(TranscriptionText));
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
    }
}

// TODO: Do something about the magic numbers
