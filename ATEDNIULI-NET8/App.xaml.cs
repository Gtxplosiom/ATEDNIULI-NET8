using System.Windows;
using ATEDNIULI_NET8.ViewModels;
using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Views;
using System.IO.Packaging;

namespace ATEDNIULI_NET8;

public partial class App : Application
{
    // porcupine access key
    private const string AccessKey = "Ubdx/XnkxCBeLVpW6g67NBTCBRv5+pF/J/3jE9noNbPYXE98zJY09w==";

    // model paths
    private const string WhisperModelPath = "Assets/Models/ggml-base.en.bin";
    private const string SileroVADModelPath = "Assets/Models/silero_vad.onnx";
    private const string IntentModelPath = "Assets/Models/intent-model.zip";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ig connect an mainwindowviewmodel
        // by making it the datacontext han mainwindow
        var porcupineService = new PorcupineService(AccessKey);
        var whisperService = new WhisperService(WhisperModelPath, SileroVADModelPath);
        var intentService = new IntentService(IntentModelPath);

        // para ma prevent an dadamo na instances hin foating viewmodel ngan multiple subscription for services as well
        // cleaner and safer
        var mainWindowViewModel = new MainWindowViewModel(porcupineService, whisperService);
        var floatingWindoViewModel = new FloatingWindowViewModel(porcupineService, whisperService, intentService);

        // Connect datacontext para bindings
        var floatingWindow = new FloatingWindow()
        {
            DataContext = floatingWindoViewModel
        };

        var notificationWindow = new NotificationWindow()
        {
            DataContext = floatingWindoViewModel
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
