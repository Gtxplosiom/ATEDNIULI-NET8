using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ATEDNIULI_NET8.Views
{
    public partial class TagOverlayWindow : Window
    {
        private readonly Vector2 _screenBounds = new Vector2(
            (int)SystemParameters.PrimaryScreenWidth,
            (int)SystemParameters.PrimaryScreenHeight
        );

        public TagOverlayWindow()
        {
            InitializeComponent();

            this.Width = _screenBounds.X;
            this.Height = _screenBounds.Y;

            this.Left = 0;
            this.Top = 0;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get the HWND for this WPF window
            var hwnd = new WindowInteropHelper(this).Handle;

            // Add WS_EX_TRANSPARENT and WS_EX_LAYERED styles
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    }
}
