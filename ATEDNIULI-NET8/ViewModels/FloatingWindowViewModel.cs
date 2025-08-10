using ATEDNIULI_NET8.Models;
using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.ViewModels.Commands;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using VoskTest;
using static Emgu.CV.OCR.Tesseract;

namespace ATEDNIULI_NET8.ViewModels
{
    public class FloatingWindowViewModel : ViewModelBase
    {
        private readonly PorcupineService? _porcupineWakeWordDetector;
        private readonly WhisperService? _whisperService;   // keep here for typing i think
        private readonly VoskService? _voskService;
        private readonly IntentService? _intentService;
        private readonly UIAutomationService? _uiAutomationService;

        private readonly Vector2 _screenDimentions;
        private readonly int _taskBarHeight;

        // view model instances para commands that deals with another window
        private CameraMouseWindowViewModel? _cameraMouseWindowViewModel;
        private TagOverlayWindowViewModel? _tagOverlayWindowViewModel;

        // floating window properties
        private Visibility _visibilityState;
        private int _leftState;
        private int _topState;

        // Flags
        private bool _itemsShowed = false;  // magagamitan ini para command kun ano an i cli-click

        // Models
        private readonly TranscriptionModel _transcriptionModel = new();

        // Commands
        // an floating window VM naagi an mga commands kay i found it na more accessible an mga variables dinhi
        public ICommand? ToggleCameraMouse { get; }
        public ICommand? ShowItemsCommand { get; }
        public ICommand? ClickItemCommand { get; }

        private int _numberToClick = 0;

        // keep la an whisper na naka pass banign gamiton ha typing or searching
        public FloatingWindowViewModel(PorcupineService? wakeWordDetector, WhisperService? whisperService, VoskService? voskService, IntentService? intentService, UIAutomationService uiAutomationService, CameraMouseWindowViewModel cameraMouseWindowViewModel, TagOverlayWindowViewModel tagOverlayWindowViewModel)
        {
            _porcupineWakeWordDetector = wakeWordDetector;
            _whisperService = whisperService;
            _voskService = voskService;
            _intentService = intentService;
            _uiAutomationService = uiAutomationService;

            // connect camera mouse window view model
            // an mga events na gin hook dinhi pag handle la hin ui states
            _cameraMouseWindowViewModel = cameraMouseWindowViewModel;
            _tagOverlayWindowViewModel = tagOverlayWindowViewModel;

            if (_porcupineWakeWordDetector != null) _porcupineWakeWordDetector.WakeWordDetected += OnWakeWordDetected;

            if (_voskService != null)
            {
                _voskService.DoneTranscription += OnDoneTranscription;
                _voskService.TranscriptionResultReady += OnTranscriptionResultReady;
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
            // para ma pass an show item command class kan click item command para connected hira
            var showItemsCommand = new ShowItemsCommand(_tagOverlayWindowViewModel, _uiAutomationService);

            ToggleCameraMouse = new ToggleCameraMouseCommand(_cameraMouseWindowViewModel);
            ShowItemsCommand = showItemsCommand;
            ClickItemCommand = new ClickItemCommand(_uiAutomationService, showItemsCommand);
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

        private void OnDoneTranscription()
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

            // kun an items naka show, ig pass an intent didi para ma process an first number word na ma-memention
            // tapos amo an i-cliclick
            // pero refactor alter na pirme na abri la anay pirme transcriptor para yumakan an floating window
            // yana kay kailangan mag wake word utro para mkaa click utro
            // "pick a number to click" or "what number you want to click or along those lines
            // pero for now adi la anay para pan test la kun nadara a clicking
            if (_numberToClick == 0)
            {
                Debug.WriteLine("modifying number to click");
                StringToNumberConverter(transcriptionResult);
            }

            if (_itemsShowed && _numberToClick != 0)
            {
                Debug.WriteLine($"clicking the item {_numberToClick}");
                ClickItemCommand?.Execute(_numberToClick);
                _numberToClick = 0;
            }

            var intent = _intentService?.PredictIntent(transcriptionResult);

            CommandHandler(intent);
        }

        // make this prettier, prefer to not use if else statement para kada usa na intent kay maraot lol
        private void CommandHandler(string? command)
        {
            // TODO: refactor this later para diri repetitive an pag toggle hi item showed na flag
            // ginsugad ko ini na repetitive kay para kun bisan ano an command na ma execute an tag overlay
            // ma clear anay, for ux purposes
            if (command == "OpenCameraMouse")
            {
                ToggleCameraMouse?.Execute(true);
                ShowItemsCommand?.Execute(false);
                _itemsShowed = false;
            }
            else if (command == "CloseCameraMouse")
            {
                ToggleCameraMouse?.Execute(false);
                ShowItemsCommand?.Execute(false);
                _itemsShowed = false;
            }
            else if (command == "ShowItems")
            {
                ShowItemsCommand?.Execute(true);
                _itemsShowed = true;
            }
            else if (command == "HideItems")
            {
                ShowItemsCommand?.Execute(false);
                _itemsShowed = false;
            }
        }

        // method para ma convert an number word to actual nga number(int)
        private void StringToNumberConverter(string words)
        {
            if (string.IsNullOrWhiteSpace(words))
                throw new ArgumentException("Input cannot be null or empty.");

            var numberMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                {"zero", 0}, {"one", 1}, {"two", 2}, {"three", 3}, {"four", 4},
                {"five", 5}, {"six", 6}, {"seven", 7}, {"eight", 8}, {"nine", 9},
                {"ten", 10}, {"eleven", 11}, {"twelve", 12}, {"thirteen", 13}, {"fourteen", 14},
                {"fifteen", 15}, {"sixteen", 16}, {"seventeen", 17}, {"eighteen", 18}, {"nineteen", 19},
                {"twenty", 20}, {"thirty", 30}, {"forty", 40}, {"fifty", 50},
                {"sixty", 60}, {"seventy", 70}, {"eighty", 80}, {"ninety", 90}
            };

            var scaleMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                {"hundred", 100},
                {"thousand", 1000},
                {"million", 1_000_000},
                {"billion", 1_000_000_000}
            };

            string[] tokens = words.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int total = 0;
            int current = 0;

            foreach (string token in tokens)
            {
                if (numberMap.TryGetValue(token, out int value))
                {
                    current += value;
                }
                else if (scaleMap.TryGetValue(token, out int scale))
                {
                    if (scale == 100)
                    {
                        current *= scale;
                    }
                    else
                    {
                        total += current * scale;
                        current = 0;
                    }
                }
                // If the token is not recognized, just skip it
            }

            _numberToClick = total + current;
        }
    }
}

// TODO: Do something about the magic numbers
// maybe floating window just serves as a state for transcription, or for typing

// TODO: implement an auto remove function that will monitor window state so if anything changed in the screen while tag is present
// this ShowItemsCommand?.Execute(false); will execute
// be it mouse movement, window appearing dissapearing, and window moving
// implement smart way of clicking and don't rely on intents i think. kay uusa-usahon an every number kun sugad lol

// TODO: cleanup codebase, labi didi na class kay masarang tapos diri consistent an style ngan syntax 
