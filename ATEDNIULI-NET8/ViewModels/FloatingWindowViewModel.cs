using ATEDNIULI_NET8.Services;
using System.Windows.Threading;
using System.Windows;
using System.Numerics;

namespace ATEDNIULI_NET8.ViewModels
{
    public class FloatingWindowViewModel : ViewModelBase
    {
        private WakeWordDetector? _wakeWordDetector;

        private readonly Vector2 _screenDimentions;
        private readonly int _taskBarHeight;

        private string? _visibilityState;
        private int _leftState;
        private int _topState;

        public FloatingWindowViewModel(WakeWordDetector? wakeWordDetector)
        {
            _wakeWordDetector = wakeWordDetector;

            _wakeWordDetector.WakeWordDetected += OnWakeWordDetected;

            _screenDimentions = new Vector2(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight
            );
            _taskBarHeight = (int)_screenDimentions.Y - (int)SystemParameters.WorkArea.Height;

            VisibilityState = "Collapsed"; // Default state
            LeftState = (int)_screenDimentions.X - 270; // Magic numbers (for now) 200(floating window width) + 70(main window width)
            TopState = (int)_screenDimentions.Y - (70 + _taskBarHeight); // 70(height of floating window)
        }

        // Properties
        public string? VisibilityState
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
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                VisibilityState = "Visible";
            });
        }
    }
}

// TODO: Do something about the magic numbers
