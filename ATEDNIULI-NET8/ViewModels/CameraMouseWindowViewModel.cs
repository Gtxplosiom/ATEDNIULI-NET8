using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Views;
using Microsoft.VisualBasic.ApplicationServices;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

        // sirkol radii
        private readonly int _innerCircleRadius = 20;
        private readonly int _outerCircleRadius = 60;

        // thresholds
        private readonly int _smileThreshold = 100;

        // variables para EMA
        private double _smoothedX = 0;
        private double _smoothedY = 0;
        private readonly double _emaAlpha = 0.2; // Adjust this value to control smoothing

        private double _speedFactor = 0; // placeholder variable la ini an speed dynamic depende an distance han nose point ha inner circle
        private int _cursorSensitivity = 15; // mas guti mas sensitive/malaksi, mas dako mas less sensitive

        // timer based mouse movement instead of basing mouse movement on camera frame rate
        // para mas smooth diri jumpy
        private Timer? _mouseMoveTimer;
        private readonly object _lockObject = new object(); // thread lock sheesh para safe kuno
        private int _targetMoveX;
        private int _targetMoveY;

        // mouse function control baryabols
        private Timer? _smileTimer;
        private SmileMouseState? _smileState;

        // bool/flags
        private bool _noseInsideInnerCircle = false;

        // Instances
        private MouseActionLabelWindowViewModel? _mouseActionLabelWindowViewModel;

        public CameraMouseWindowViewModel(FacialLandmarkService facialLandmarkService, MouseActionLabelWindowViewModel mouseActionLabelWindowViewModel)
        {
            _facialLandmarkService = facialLandmarkService;
            _mouseActionLabelWindowViewModel = mouseActionLabelWindowViewModel;
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
                        Cv2.Circle(frame, headCenter, _innerCircleRadius, Scalar.Red, 2);   // Inner sirkol
                        Cv2.Circle(frame, headCenter, _outerCircleRadius, Scalar.Cyan, 2);  // Outer sirkol

                        // Nose tip
                        OpenCvSharp.Point noseTip = pts[30];
                        Cv2.Circle(frame, noseTip, 5, Scalar.Yellow, -1);

                        // Mouth landmarks
                        OpenCvSharp.Point mouthLeft = pts[48];
                        OpenCvSharp.Point mouthRight = pts[54];

                        // draw left and right mouth points and a line between them
                        Cv2.Circle(frame, mouthLeft, 5, Scalar.Green, -1);
                        Cv2.Circle(frame, mouthRight, 5, Scalar.Green, -1);
                        Cv2.Line(frame, mouthLeft, mouthRight, Scalar.Azure, 2);

                        int mouthLeftRightVecX = mouthRight.X - mouthLeft.X;
                        int mouthLeftRightVecY = mouthRight.Y - mouthLeft.Y;

                        // gamiton ini para masabtan kun na smile
                        // TODO: make this more robust/dynamic para no matter bisan ano an size hin face, or bisan dumaop ngan hirayo
                        // ma normalize la gihap an distance
                        double distLeftRight = Math.Sqrt(mouthLeftRightVecX * mouthLeftRightVecX + mouthLeftRightVecY * mouthLeftRightVecY);

                        // Calculate vector from head center to nose tip
                        int noseHeadVecX = noseTip.X - headCenter.X;
                        int noseHeadvecY = noseTip.Y - headCenter.Y;

                        // distance between center of head and nose tip
                        double distNoseHead = Math.Sqrt(noseHeadVecX * noseHeadVecX + noseHeadvecY * noseHeadvecY);

                        if (distNoseHead > _innerCircleRadius)
                        {
                            _noseInsideInnerCircle = false;

                            double length = Math.Max(distNoseHead, 1);
                            double dirX = noseHeadVecX / length;
                            double dirY = noseHeadvecY / length;

                            // mas dako an distance mas dako an speed factor
                            _speedFactor = distNoseHead / _cursorSensitivity;

                            // RAAAW coordinates
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
                            _noseInsideInnerCircle = true;

                            IsSmiling(distLeftRight);

                            // If the nose is inside the inner sirkol, set movement to zero
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
            _smoothedX = (currentTargetX * _emaAlpha) + (_smoothedX * (1 - _emaAlpha));
            _smoothedY = (currentTargetY * _emaAlpha) + (_smoothedY * (1 - _emaAlpha));

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

        private void IsSmiling(double distMouthLeftRight)
        {
            if (distMouthLeftRight > _smileThreshold && _noseInsideInnerCircle)
            {
                // Ma detect na smile ma tikang an counter
                if (_smileTimer == null)
                {
                    _smileState = new SmileMouseState();
                    _smileTimer = new Timer(MouseAction, _smileState, 0, 1000);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_mouseActionLabelWindowViewModel != null) _mouseActionLabelWindowViewModel.VisibilityState = Visibility.Visible;
                });
            }
            else
            {
                // Kun mag end pag smile, ig-geget an action based han current counter when stopped smiling
                if (_smileTimer != null && _smileState != null)
                {
                    // Smile ended — execute the current action one last time
                    string finalAction = GetActionFromCounter(_smileState.Counter);
                    ExecuteMouseFunction(finalAction);
                }

                _smileTimer?.Dispose();
                _smileTimer = null;
                _smileState = null;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_mouseActionLabelWindowViewModel != null) _mouseActionLabelWindowViewModel.VisibilityState = Visibility.Collapsed;
                });
            }
        }

        // Callback para ma increase an sile counter
        private void MouseAction(object? state)
        {
            if (state is not SmileMouseState mouseState)
                return;

            mouseState.Counter++;

            if (mouseState.Counter > 5)
                mouseState.Counter = 1;

            string action = GetActionFromCounter(mouseState.Counter);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_mouseActionLabelWindowViewModel != null) _mouseActionLabelWindowViewModel.MouseAction = action;
            });
        }

        // Return action strings basehan counter value yoh
        private string GetActionFromCounter(int counter)
        {
            return counter switch
            {
                1 => "left-click",
                2 => "double-click",
                3 => "right-click",
                4 => "hold",
                5 => "release",
                _ => "none"
            };
        }

        // Execute mouse function method yeah
        private void ExecuteMouseFunction(string action)
        {
            switch (action)
            {
                case "left-click":
                    Debug.WriteLine("Executing left click");
                    break;
                case "double-click":
                    Debug.WriteLine("Executing double click");
                    break;
                case "right-click":
                    Debug.WriteLine("Executing right click");
                    break;
                case "hold":
                    Debug.WriteLine("Holding mouse button down");
                    break;
                case "release":
                    Debug.WriteLine("Releasing mouse button");
                    break;
            }

            Console.WriteLine($"Mouse Action: {action}");
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

    class SmileMouseState
    {
        public int Counter { get; set; } = 0;
    }
}

// TODO: fix an direction steering hin cursor while moving kailangan smooth diri la limitado ha north, north-east, east, south-east, south, south-west, west, north-west
// ngan implement na an mouse function mismo
// nganin make an distance left mouth to right mouth more robust/dynamic para no matter bisan ano an size hin face, or bisan dumaop ngan hirayo
// ma normalize la gihap an distance
