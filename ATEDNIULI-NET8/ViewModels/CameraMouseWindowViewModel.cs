using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ATEDNIULI_NET8.Services;
using System.Diagnostics;

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

        // variables para kalman filter
        private OpenCvSharp.KalmanFilter _kalman;
        private Mat _state;
        private Mat _measurement;
        private bool _kalmanInitialized = false;

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

                // usage here
                var faceLandmarks = _facialLandmarkService.DetectLandmarks(frame);

                // may null checks para diri mag crash kun waray detected na landmarks
                if (faceLandmarks != null)
                {
                    for (int i = 0; i < faceLandmarks.Length; i++)
                    {
                        var pts = faceLandmarks[i];
                        if (pts == null) continue;

                        for (int j = 0; j < pts.Length; j++)
                        {
                            var p = pts[j];

                            Cv2.Circle(frame, p, 2, Scalar.Red, -1);

                            // hmmm dapat ada ha iba ini na thread kay "jumpy" la gihap
                            MoveMouse(pts[0]);
                        }
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

        // apply kalman filter para ma smoothen an mouse movement
        // su-sugad hini kay an mouse kailangan ma move bisan guti la an head movement
        // takay kun sugad kun waray smoothening, magiging jittery an movement kun ha raw coords la
        private OpenCvSharp.Point KalmanFilter(OpenCvSharp.Point point)
        {
            if (!_kalmanInitialized)
                InitializeKalman();

            var prediction = _kalman.Predict();

            _measurement.Set<float>(0, 0, point.X);
            _measurement.Set<float>(1, 0, point.Y);

            var estimated = _kalman.Correct(_measurement);

            return new OpenCvSharp.Point(
                (int)estimated.At<float>(0),
                (int)estimated.At<float>(1)
            );
        }

        // temporary move logic la anay ini
        // add a joystick-like mouse control
        private void MoveMouse(OpenCvSharp.Point point)
        {
            var smoothPoint = KalmanFilter(point);
            Debug.WriteLine($"Moving mouse to: {point}");

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(smoothPoint.X, smoothPoint.Y);
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

        // helper method para ma-initialize an Kalman filter
        private void InitializeKalman()
        {
            _kalman = new KalmanFilter(4, 2);
            _state = new Mat(4, 1, MatType.CV_32F);
            _measurement = new Mat(2, 1, MatType.CV_32F);

            var transitionMatrix = new Mat(4, 4, MatType.CV_32F);
            transitionMatrix.SetArray<float>(new float[]
            {
                1, 0, 1, 0,
                0, 1, 0, 1,
                0, 0, 1, 0,
                0, 0, 0, 1
            });
            _kalman.TransitionMatrix = transitionMatrix;

            _kalman.MeasurementMatrix = Mat.Eye(2, 4, MatType.CV_32F);
            _kalman.ProcessNoiseCov = Mat.Eye(4, 4, MatType.CV_32F) * 1e-4;
            _kalman.MeasurementNoiseCov = Mat.Eye(2, 2, MatType.CV_32F) * 1e-1;
            _kalman.ErrorCovPost = Mat.Eye(4, 4, MatType.CV_32F);

            _state.Set<float>(0, 0, 0);
            _state.Set<float>(1, 0, 0);
            _state.Set<float>(2, 0, 0);
            _state.Set<float>(3, 0, 0);
            _kalman.StatePost = _state.Clone();

            _kalmanInitialized = true;
        }
    }
}

// TODO: position camera preview on the top right (previous) or ano mas maupay
// then kailangan i-apply lat an preview or possibly an ui ma move kun an cursor aadto para diri makasalipod ha user
// apply joystick-like mouse cursor control
