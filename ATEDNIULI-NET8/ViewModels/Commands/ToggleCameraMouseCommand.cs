using System.Diagnostics;
using ATEDNIULI_NET8.Views;
using System.Windows;

namespace ATEDNIULI_NET8.ViewModels.Commands
{
    public class ToggleCameraMouseCommand : CommandBase
    {
        // maraot ini kun mvvm but for now adi la anay
        // TODO: instantiate pereferrable in the service requesting it and use events as toggles
        // hahays damo na an todo sh*ts
        private readonly CameraMouseWindowViewModel? _cameraMouseWindowViewModel;
        private readonly CameraMouseWindow? _cameraMouseWindow;

        public ToggleCameraMouseCommand(CameraMouseWindowViewModel cameraMouseVM)
        {
            _cameraMouseWindowViewModel = cameraMouseVM;

            // temporary la ini kay di ini ma-a-access hin close command class
            // or mas guds kun an open and close command aadi ha usa na class
            // need ig edit an cammandbase kay an override methods kun ma proceed an above na idea
            _cameraMouseWindow = new CameraMouseWindow
            {
                DataContext = _cameraMouseWindowViewModel
            };
        }

        public override void Execute(object? parameter)
        {
            Debug.WriteLine("ToggleCameraMouse executed");

            // convert parameter to bool
            var param = parameter as bool?;

            if (param == false && _cameraMouseWindowViewModel._isRunning)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _cameraMouseWindow?.Hide();
                });

                _cameraMouseWindowViewModel?.StopCamera();
            }
            else if (param == true || !_cameraMouseWindowViewModel._isRunning)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _cameraMouseWindow?.Show();
                });

                _cameraMouseWindowViewModel?.StartCamera();
            }
        }
    }
}
