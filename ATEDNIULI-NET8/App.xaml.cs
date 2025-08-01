using System.Windows;
using ATEDNIULI_NET8.ViewModels;
using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Views;

namespace ATEDNIULI_NET8;

public partial class App : Application
{
    // porcupine access key
    private const string AccessKey = "Ubdx/XnkxCBeLVpW6g67NBTCBRv5+pF/J/3jE9noNbPYXE98zJY09w==";

    // model paths
    private const string WhisperModelPath = "Assets/Models/ggml-base.en.bin";
    private const string SileroVADModelPath = "Assets/Models/silero_vad.onnx";
    private const string IntentModelPath = "Assets/Models/intent-model.zip";
    private const string ShapePredictorModelPath = "Assets/Models/shape_predictor_68_face_landmarks.dat";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ig connect an mainwindowviewmodel
        // by making it the datacontext han mainwindow
        var porcupineService = new PorcupineService(AccessKey);
        var whisperService = new WhisperService(WhisperModelPath, SileroVADModelPath);
        var intentService = new IntentService(IntentModelPath);
        var facialLandmarkService = new FacialLandmarkService(ShapePredictorModelPath);

        // para ma prevent an dadamo na instances hin foating viewmodel ngan multiple subscription for services as well
        // cleaner and safer
        var mainWindowViewModel = new MainWindowViewModel(porcupineService, whisperService);
        var cameraMouseWindowViewModel = new CameraMouseWindowViewModel(facialLandmarkService);

        // ig connect an camera mouse window view model to the floating window view model
        // para madali matawag ha commands
        // TODO: make this cleaner?? for now adi la anay
        var floatingWindowViewModel = new FloatingWindowViewModel(porcupineService, whisperService, intentService, cameraMouseWindowViewModel);

        // Connect datacontext para bindings
        var floatingWindow = new FloatingWindow()
        {
            DataContext = floatingWindowViewModel
        };

        var notificationWindow = new NotificationWindow()
        {
            DataContext = floatingWindowViewModel
        };
        notificationWindow.Show();

        var mainWindow = new MainWindow()
        {
            DataContext = mainWindowViewModel
        };
        mainWindow.Show();
    }
}

// TODO: Maybe use a config file or something
