using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ATEDNIULI_NET8.ViewModels
{
    public class CameraMouseWindowViewModel : ViewModelBase
    {
        private VideoCapture? _capture;
        private Thread? _cameraThread;

        // flag/switch hin camera loop
        public bool _isRunning = false;

        private BitmapSource? _webcamFrame;

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
            while (_isRunning)
            {
                using var frame = _capture.QueryFrame();
                if (frame != null)
                {
                    var image = frame.ToImage<Bgr, byte>();

                    // TODO: put facial landmark detection here

                    var bitmap = ConvertToBitmapSource(image);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WebcamFrame = bitmap;
                    });
                }

                Thread.Sleep(33);
            }
        }

        private BitmapSource ConvertToBitmapSource(Image<Bgr, byte> image)
        {
            var bitmap = image.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();

            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // freeze para thread safe ha ui
            bitmapSource.Freeze();

            // unmanaged resource cleanup para fix ha toggling camera mouse memory buildup
            DeleteObject(hBitmap);

            return bitmapSource;
        }

        public void StartCamera()
        {
            // TODO: make this dynamic kay an camera index depende ha system
            // an common 0 takay may instances pareho han akon na duha an cam an front is index 1
            // make this front cam or webcam ha computer
            _capture = new VideoCapture(1, VideoCapture.API.DShow);

            _cameraThread = new Thread(CameraLoop)
            {
                IsBackground = true
            };

            _isRunning = true;
            _cameraThread.Start();
        }

        // TODO: need pa ig apply hin todo na cleanup kay nag titikadako la an ram kun kakadamo ko gin to-toggle an camera mouse
        public void StopCamera()
        {
            _isRunning = false;
            _cameraThread?.Join();
            _capture?.Dispose();
        }

        // Pan delete hin bitmap
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
