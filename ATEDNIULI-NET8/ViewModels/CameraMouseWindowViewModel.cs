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

        // variables para EMA
        private double _smoothedX = 0;
        private double _smoothedY = 0;
        private const double EmaAlpha = 0.2; // Adjust this value to control smoothing

        private double _speedFactor = 0; // placeholder variable la ini an speed dynamic depende an distance han nose point ha inner circle
        private int _cursorSensitivity = 15; // mas guti mas sensitive/malaksi, mas dako mas less sensitive

        // timer based mouse movement instead of basing mouse movement on camera frame rate
        // para mas smooth diri jumpy
        private Timer? _mouseMoveTimer;
        private readonly object _lockObject = new object();
        private int _targetMoveX;
        private int _targetMoveY;

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
                            double length = Math.Max(dist, 1);
                            double dirX = dx / length;
                            double dirY = dy / length;

                            _speedFactor = dist / _cursorSensitivity;

                            // RAW coordinates
                            var newMoveX = (int)(dirX * _speedFactor);
                            var newMoveY = (int)(dirY * _speedFactor);

                            // kailangan ini para diri mag katuyaw an loop thread
                            lock (_lockObject)
                            {
                                _targetMoveX = newMoveX;
                                _targetMoveY = newMoveY;
                            }

                            Cv2.Line(frame, headCenter, noseTip, Scalar.Green, 2);
                        }
                        else
                        {
                            // If the nose is inside the deadzone, set movement to zero
                            lock (_lockObject)
                            {
                                _targetMoveX = 0;
                                _targetMoveY = 0;
                            }
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

        private void MoveMouse(object? state)
        {
            int currentTargetX, currentTargetY;

            // thread safety sheesh
            lock (_lockObject)
            {
                currentTargetX = _targetMoveX;
                currentTargetY = _targetMoveY;
            }

            // EMA application babyyy
            _smoothedX = (currentTargetX * EmaAlpha) + (_smoothedX * (1 - EmaAlpha));
            _smoothedY = (currentTargetY * EmaAlpha) + (_smoothedY * (1 - EmaAlpha));

            int finalMoveX = (int)_smoothedX;
            int finalMoveY = (int)_smoothedY;

            // kun may movement la tikang ha relative position amo an pag execute hini
            if (Math.Abs(finalMoveX) > 0 || Math.Abs(finalMoveY) > 0)
            {
                System.Drawing.Point currentPos = System.Windows.Forms.Cursor.Position;
                int newX = currentPos.X + finalMoveX;
                int newY = currentPos.Y + finalMoveY;

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(newX, newY);
            }
        }

        public void StartCamera()
        {
            _capture = new VideoCapture(1, VideoCaptureAPIs.DSHOW);

            // para ma de-couple an camera thread han camera framerate
            // ig separate thread para ma smooth
            _cameraThread = new Thread(CameraLoop)
            {
                IsBackground = true
            };

            isRunning = true;
            _cameraThread.Start();

            // amo ini an ma proc han movemouse method
            _mouseMoveTimer = new Timer(MoveMouse, null, 0, 10);
        }

        public void StopCamera()
        {
            isRunning = false;
            _cameraThread?.Join();
            _capture?.Dispose();

            _mouseMoveTimer?.Dispose();
            _mouseMoveTimer = null;
        }
    }
}

// TODO: pagbutang na hin facial gesture controls para mouse functions
// tapos fix an direction steering hin cursor while moving kailangan smooth diri la limitado ha north, north-east, east, south-east, south, south-west, west, north-west
