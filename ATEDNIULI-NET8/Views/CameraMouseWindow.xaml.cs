using System.Numerics;
using System.Windows;

namespace ATEDNIULI_NET8.Views
{
    public partial class CameraMouseWindow : Window
    {
        private readonly Vector2 _screenDimentions;

        public CameraMouseWindow()
        {
            InitializeComponent();

            _screenDimentions = new Vector2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

            PositionWindow();
        }

        private void PositionWindow()
        {
            Left = _screenDimentions.X - this.Width;
            Top = 0;
        }
    }
}
