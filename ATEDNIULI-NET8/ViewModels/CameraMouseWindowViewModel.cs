using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ATEDNIULI_NET8.Services;
using System.Diagnostics;
using Microsoft.VisualBasic.ApplicationServices;

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
                        if (pts == null || pts.Length <= 45) continue;

                        // Get center of head (e.g. between eyes)
                        OpenCvSharp.Point headLeft = pts[2];
                        OpenCvSharp.Point headRight = pts[14];
                        OpenCvSharp.Point headCenter = new OpenCvSharp.Point(
                            (headLeft.X + headRight.X) / 2,
                            (headLeft.Y + headRight.Y) / 2
                        );

                        // Draw dynamic tracking circles around the head
                        Cv2.Circle(frame, headCenter, 20, Scalar.Red, 2);   // Inner circle
                        Cv2.Circle(frame, headCenter, 60, Scalar.Cyan, 2);  // Outer circle

                        // Get nose tip position
                        OpenCvSharp.Point noseTip = pts[30];
                        Cv2.Circle(frame, noseTip, 5, Scalar.Yellow, -1);  // Draw nose tip

                        // Calculate vector from head center to nose tip
                        int dx = noseTip.X - headCenter.X;
                        int dy = noseTip.Y - headCenter.Y;

                        // Compute distance from center
                        double dist = Math.Sqrt(dx * dx + dy * dy);

                        // Check if nose is outside inner circle
                        int innerRadius = 20;
                        int outerRadius = 60;

                        if (dist > innerRadius)
                        {
                            // Normalize direction vector
                            double length = Math.Max(dist, 1); // avoid divide-by-zero
                            double dirX = dx / length;
                            double dirY = dy / length;

                            // Speed scaling: small if within outer, large if beyond outer
                            // co-consider nala ada na fixed speed factor
                            double speedFactor = dist > outerRadius ? 10 : 4;

                            int moveX = (int)(dirX * speedFactor);
                            int moveY = (int)(dirY * speedFactor);

                            // Optional: visual feedback line
                            Cv2.Line(frame, headCenter, noseTip, Scalar.Green, 2);

                            // Now move the mouse by relative amount
                            MoveMouse(moveX, moveY);
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

        // TODO: apply kalman filter to relative incremental movement
        private void MoveMouse(int x, int y)
        {
            System.Drawing.Point currentPos = System.Windows.Forms.Cursor.Position;
            int newX = currentPos.X + x;
            int newY = currentPos.Y + y;

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(newX, newY);
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
