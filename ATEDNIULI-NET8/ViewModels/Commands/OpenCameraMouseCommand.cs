using System.Diagnostics;

namespace ATEDNIULI_NET8.ViewModels.Commands
{
    public class OpenCameraMouseCommand : CommandBase
    {
        public override void Execute(object? parameter)
        {
            Debug.WriteLine("OpenCameraMouseCommand executed");
        }
    }
}
