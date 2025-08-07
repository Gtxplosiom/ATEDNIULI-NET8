using ATEDNIULI_NET8.ViewModels;
using ATEDNIULI_NET8.Services;
using ATEDNIULI_NET8.Views;
using System.Windows;

namespace ATEDNIULI_NET8.ViewModels.Commands
{
    public class ShowItemsCommand : CommandBase
    {
        private readonly TagOverlayWindowViewModel _tagOverlayWindowViewModel;
        private readonly UIAutomationService _uiAutomationService;
        private readonly TagOverlayWindow _tagOverlayWindow;

        public ShowItemsCommand(TagOverlayWindowViewModel tagOverlayWindowViewModel, UIAutomationService uiAutomationService)
        {
            _tagOverlayWindowViewModel = tagOverlayWindowViewModel;
            _uiAutomationService = uiAutomationService;

            _tagOverlayWindow = new TagOverlayWindow
            {
                DataContext = _tagOverlayWindowViewModel,
                // Set the overlay to cover the entire primary screen
                Left = 0,
                Top = 0,
                Width = SystemParameters.PrimaryScreenWidth,
                Height = SystemParameters.PrimaryScreenHeight
            };
            _tagOverlayWindow.Show();
        }

        public override void Execute(object? parameter)
        {
            var param = parameter as bool?;

            if (param == true)
            {
                var clickableItems = _uiAutomationService.GetClickableItems();
                _tagOverlayWindowViewModel.ClearTags();

                // Get the horizontal center of the screen to decide tag placement
                double screenCenterX = SystemParameters.PrimaryScreenWidth / 2;

                for (int i = 0; i < clickableItems.Count; i++)
                {
                    var itemPoint = clickableItems[i];
                    PointerDirection direction;

                    // If the item is on the left half of the screen...
                    if (itemPoint.X < screenCenterX)
                    {
                        // ...place the tag to its RIGHT (so the pointer aims LEFT).
                        direction = PointerDirection.Left;
                    }
                    else // If the item is on the right half...
                    {
                        // ...place the tag to its LEFT (so the pointer aims RIGHT).
                        direction = PointerDirection.Right;
                    }

                    _tagOverlayWindowViewModel.AddTag(itemPoint, $"{i + 1}", direction);
                }
            }
            else
            {
                _tagOverlayWindowViewModel.ClearTags();
            }
        }
    }
}
