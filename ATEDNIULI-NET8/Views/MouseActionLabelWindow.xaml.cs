using System.Windows;

namespace ATEDNIULI_NET8.Views
{
    public partial class MouseActionLabelWindow : Window
    {
        public MouseActionLabelWindow()
        {
            InitializeComponent();

            PositionNearCursor();
        }

        private void PositionNearCursor()
        {
            var pos = System.Windows.Forms.Cursor.Position;
            Left = pos.X + 10;
            Top = pos.Y - 50;
        }
    }
}
