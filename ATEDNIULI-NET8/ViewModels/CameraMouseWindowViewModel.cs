using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ATEDNIULI_NET8.Services;

namespace ATEDNIULI_NET8.ViewModels
{
    public class CameraMouseWindowViewModel : ViewModelBase
    {
        private VideoCapture? _capture;
        private Thread? _cameraThread;

        // flag/switch hin camera loop
        public bool isRunning = false;

        // property variable
        private BitmapSource? _webcamFrame;

        private readonly FacialLandmarkService? _facialLandmarkService;

        public CameraMouseWindowViewModel(FacialLandmarkService facialLandmarkService)
        {
            _facialLandmarkService = facialLandmarkService;
        }

        // Properties
        public BitmapSource? WebcamFrame
        {
            get => _webcamFrame;
            set
            {
                _webcamFrame = value;
                OnPropertyChanged(nameof(WebcamFrame));
            }
        }

        // Methods
        private void CameraLoop()
        {
            using var frame = new Mat();
            while (_capture != null && isRunning)
            {
                if (!_capture.Read(frame) || frame.Empty()) { Thread.Sleep(10); continue; }

                // Flip camera frame horizontally para "mirrored" an preview
                Cv2.Flip(frame, frame, FlipMode.Y);

                // usagr here
                var faces = _facialLandmarkService.DetectLandmarks(frame);
                foreach (var pts in faces)
                {
                    foreach (var p in pts)
                    {
                        Cv2.Circle(frame, p, 2, Scalar.Red, -1);
                    }    
                }

                var wb = frame.ToWriteableBitmap();

                // kailangan i freeze para ui thread safe
                // kun diri ma crash
                wb.Freeze();

                Application.Current.Dispatcher.Invoke(() => WebcamFrame = wb);
                Thread.Sleep(33);
            }
        }

        public void StartCamera()
        {
            // Change to OpenCvSharp.VideoCapture
            _capture = new VideoCapture(1, VideoCaptureAPIs.DSHOW);

            _cameraThread = new Thread(CameraLoop)
            {
                IsBackground = true
            };

            isRunning = true;
            _cameraThread.Start();
        }

        public void StopCamera()
        {
            isRunning = false;
            _cameraThread?.Join();
            _capture?.Dispose();
        }
    }
}

// TODO: position camera preview on the top right (previous) or ano mas maupay
// then kailangan i-apply lat an preview or possibly an ui ma move kun an cursor aadto para diri makasalipod ha user
