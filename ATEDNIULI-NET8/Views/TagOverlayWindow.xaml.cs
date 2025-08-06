using System.Numerics;
using System.Windows;

namespace ATEDNIULI_NET8.Views
{
    public partial class TagOverlayWindow : Window
    {
        private readonly Vector2 _screenBounds = new Vector2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

        public TagOverlayWindow()
        {
            InitializeComponent();

            this.Width = _screenBounds.X;
            this.Height = _screenBounds.Y;

            this.Left = 0;
            this.Top = 0;
        }
    }
}
