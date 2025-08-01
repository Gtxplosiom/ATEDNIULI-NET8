using DlibDotNet;
using OpenCvSharp;
using CvPoint = OpenCvSharp.Point;
using System.Runtime.InteropServices; // Required for Marshal.Copy

public class FacialLandmarkService : IDisposable
{
    private readonly FrontalFaceDetector _det;
    private readonly ShapePredictor _sp;

    public FacialLandmarkService(string predictorModelPath)
    {
        _det = Dlib.GetFrontalFaceDetector();
        _sp = ShapePredictor.Deserialize(predictorModelPath);
    }

    public CvPoint[][] DetectLandmarks(Mat bgrFrame)
    {
        if (bgrFrame.Empty())
            return Array.Empty<CvPoint[]>();

        // Convert an BGR image data to RGB
        using var rgb = new Mat();
        Cv2.CvtColor(bgrFrame, rgb, ColorConversionCodes.BGR2RGB);

        IntPtr rgbDataPtr = rgb.Data;

        int bufferSize = rgb.Rows * (int)rgb.Step();

        byte[] pixelBuffer = new byte[bufferSize];

        // copy an unmanaged memory to a managed byte array
        // kay kun diri memory corruption chuchu basta need ma "own" anay an memory, c++ pointer sheets
        Marshal.Copy(rgbDataPtr, pixelBuffer, 0, bufferSize);

        using var dImg = Dlib.LoadImageData<RgbPixel>(
            pixelBuffer,
            (uint)rgb.Rows,
            (uint)rgb.Cols,
            (uint)rgb.Step());

        var rects = _det.Operator(dImg);
        if (rects.Length == 0)
            return Array.Empty<CvPoint[]>();

        var results = new CvPoint[rects.Length][];
        for (int i = 0; i < rects.Length; i++)
        {
            using var shape = _sp.Detect(dImg, rects[i]);
            var pts = new CvPoint[shape.Parts];
            for (uint j = 0; j < shape.Parts; j++)
            {
                var p = shape.GetPart(j);
                pts[j] = new CvPoint(p.X, p.Y);
            }
            results[i] = pts;
        }
        return results;
    }

    public void Dispose()
    {
        _sp?.Dispose();
        _det?.Dispose();
    }
}

//TODO: make sure an dispose an memory kun ma off, then apply mouse control here na service class, or ha viewmodel?
// then apply kalman filter to mouse movement tapos an smoothened movement amo an i-follow/reference hin mouse pointer
// kay kun diri ma jittery kun tikang gud la ha raw predicted landmarks
