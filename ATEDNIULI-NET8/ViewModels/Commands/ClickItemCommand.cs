using ATEDNIULI_NET8.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace ATEDNIULI_NET8.ViewModels.Commands
{
    public class ClickItemCommand : CommandBase
    {
        // para may connection ini na class ha clickable items list han uiatuomation service ngan an execute command han showitemscommand
        private readonly UIAutomationService _uiAutomationService;
        private readonly ShowItemsCommand _showItemsCommand;

        public ClickItemCommand(UIAutomationService uiAutomationService, ShowItemsCommand showItemsCommand)
        {
            _uiAutomationService = uiAutomationService;
            _showItemsCommand = showItemsCommand;
        }

        public override void Execute(object? parameter)
        {
            var param = parameter as int?;

            Debug.WriteLine($"trying {param}");

            if (param != 0)
            {
                foreach (var coord in _uiAutomationService.clickableItems)
                {
                    if (_uiAutomationService.clickableItems.IndexOf(coord) + 1 == param)
                    {
                        _showItemsCommand.Execute(false);

                        MouseSimulator.ClickAt(coord);
                        return;
                    }
                }
            }
        }
    }

    public class MouseSimulator
    {
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void ClickAt(Point point)
        {
            SetCursorPos((int)point.X, (int)point.Y);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }
    }
}
