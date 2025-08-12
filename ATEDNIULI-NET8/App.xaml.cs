using System.IO;
using System.Text.Json;
using System.Windows;
using ATEDNIULI_NET8.ViewModels;
using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Views;
using VoskTest;

namespace ATEDNIULI_NET8
{
    public partial class App : Application
    {
        private AppConfig _appConfig = null!;

        // services
        private PorcupineService _porcupineService = null!;
        private WhisperService _whisperService = null!;
        private VoskService _voskService = null!;
        private IntentService _intentService = null!;
        private FacialLandmarkService _facialLandmarkService = null!;
        private UIAutomationService _uiAutomationService = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load config
            _appConfig = JsonSerializer.Deserialize<AppConfig>(
                File.ReadAllText("appsettings.json")
            ) ?? throw new Exception("Failed to load appsettings.json");

            // Initialize services from config
            _porcupineService = new PorcupineService(_appConfig.AccessKey);
            _whisperService = new WhisperService(_appConfig.ModelPaths.Whisper, _appConfig.ModelPaths.SileroVAD);
            _voskService = new VoskService(_appConfig.ModelPaths.Vosk);
            _intentService = new IntentService(_appConfig.ModelPaths.Intent);
            _facialLandmarkService = new FacialLandmarkService(_appConfig.ModelPaths.ShapePredictor);
            _uiAutomationService = new UIAutomationService();

            // ViewModels
            var mouseActionLabelWindowViewModel = new MouseActionLabelWindowViewModel();
            var cameraMouseWindowViewModel = new CameraMouseWindowViewModel(_facialLandmarkService, mouseActionLabelWindowViewModel);
            var tagOverlayWindowViewModel = new TagOverlayWindowViewModel();
            var floatingWindowViewModel = new FloatingWindowViewModel(_porcupineService, _whisperService, _voskService, _intentService, _uiAutomationService, cameraMouseWindowViewModel, tagOverlayWindowViewModel);
            var mainWindowViewModel = new MainWindowViewModel(_porcupineService, _whisperService, _voskService, floatingWindowViewModel);

            // Windows
            var floatingWindow = new FloatingWindow { DataContext = floatingWindowViewModel };
            var mouseActionLabelWindow = new MouseActionLabelWindow { DataContext = mouseActionLabelWindowViewModel };
            var notificationWindow = new NotificationWindow { DataContext = floatingWindowViewModel };
            notificationWindow.Show();

            var mainWindow = new MainWindow { DataContext = mainWindowViewModel };
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _porcupineService?.StopWakeWordDetection();
            _voskService?.StopRecording();
            _whisperService?.StopRecording();
            base.OnExit(e);
        }
    }
}
