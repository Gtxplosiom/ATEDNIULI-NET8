using System.Windows;
using ATEDNIULI_NET8.ViewModels;
using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Views;

namespace ATEDNIULI_NET8;

public partial class App : Application
{
    private const string AccessKey = "Ubdx/XnkxCBeLVpW6g67NBTCBRv5+pF/J/3jE9noNbPYXE98zJY09w==";
    private const string WhisperModelPath = "Assets/Models/ggml-base.en.bin";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ig connect an mainwindowviewmodel
        // by making it the datacontext han mainwindow
        var porcupineService = new PorcupineService(AccessKey);
        var whisperService = new WhisperService(WhisperModelPath);

        // Connect datacontext para binding purposes
        var floatingWindow = new FloatingWindow()
        {
            DataContext = new FloatingWindowViewModel(porcupineService)
        };

        var mainWindow = new MainWindow()
        {
            DataContext = new MainWindowViewModel(porcupineService)
        };
        mainWindow.Show();
    }
}
