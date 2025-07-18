using System.Numerics;
using System.Windows;

namespace ATEDNIULI_NET8;

public partial class MainWindow : Window
{
    private readonly Vector2 _screenSize;
    private readonly int _taskBarHeight;

    public MainWindow()
    {
        InitializeComponent();

        _screenSize = new Vector2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        _taskBarHeight = (int)SystemParameters.PrimaryScreenHeight - (int)SystemParameters.WorkArea.Height;

        PositionWindow();
    }

    private void PositionWindow()
    {
        Left = _screenSize.X - this.Width;
        Top = _screenSize.Y - (this.Height + _taskBarHeight);
    }
}
