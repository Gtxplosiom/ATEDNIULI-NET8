using System.Windows;

namespace ATEDNIULI_NET8.Views
{
    public partial class MouseActionLabelWindow : Window
    {
        public MouseActionLabelWindow()
        {
            InitializeComponent();
            IsVisibleChanged += MouseActionLabelWindow_IsVisibleChanged;
        }

        // Mag update an position everytime an window ma chage an visibility
        private void MouseActionLabelWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                PositionNearCursor();
            }
        }

        private void PositionNearCursor()
        {
            var pos = System.Windows.Forms.Cursor.Position;
            Left = pos.X + 10;
            Top = pos.Y - 50;
        }
    }
}
