using System.Numerics;
using System.Windows;

namespace ATEDNIULI_NET8.Views
{
    public partial class NotificationWindow : Window
    {
        private readonly Vector2 _screenDimentions;
        private readonly int _taskBarHeight;

        public NotificationWindow()
        {
            InitializeComponent();

            _screenDimentions = new Vector2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
            _taskBarHeight = (int)_screenDimentions.Y - (int)SystemParameters.WorkArea.Height;

            PositionWindow();
        }

        private void PositionWindow()
        {
            Left = _screenDimentions.X - this.Width;
            Top = _screenDimentions.Y - (this.Height + _taskBarHeight);
        }
    }
}
