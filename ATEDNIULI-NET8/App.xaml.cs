using System.Windows;
using ATEDNIULI_NET8.ViewModels;
using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Views;

namespace ATEDNIULI_NET8;

public partial class App : Application
{
    private const string _ACCESSKEY = "Ubdx/XnkxCBeLVpW6g67NBTCBRv5+pF/J/3jE9noNbPYXE98zJY09w==";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ig connect an mainwindowviewmodel
        // by making it the datacontext han mainwindow
        var wakeWordDetector = new WakeWordDetector(_ACCESSKEY);

        // Connect datacontext para binding purposes
        var floatingWindow = new FloatingWindow()   // temp la ini
        {
            DataContext = new FloatingWindowViewModel(wakeWordDetector)
        };

        var mainWindow = new MainWindow()
        {
            DataContext = new MainWindowViewModel(wakeWordDetector)
        };
        mainWindow.Show();
    }
}
