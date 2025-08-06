using ATEDNIULI_NET8.Views;
using ATEDNIULI_NET8.Services;
using System.Windows;

namespace ATEDNIULI_NET8.ViewModels.Commands
{
    public class ShowItemsCommand : CommandBase
    {
        private readonly TagOverlayWindowViewModel _tagOverlayWindowViewModel;
        private readonly TagOverlayWindow _tagOverlayWindow;

        // uiautomation service
        private UIAutomationService? _uiAutomationService;

        public ShowItemsCommand(TagOverlayWindowViewModel tagOverlayWindowViewModel, UIAutomationService uiAutomationService)
        {
            _tagOverlayWindowViewModel = tagOverlayWindowViewModel;
            _uiAutomationService = uiAutomationService;

            // This is the part that connects the View and ViewModel.
            // You should probably handle window creation/showing in a different part of your app,
            // but for this example, we'll keep it here.
            _tagOverlayWindow = new TagOverlayWindow
            {
                DataContext = _tagOverlayWindowViewModel
            };
            _tagOverlayWindow.Show();
        }

        public override void Execute(object? parameter)
        {
            var param = parameter as bool?;

            if (param == true)
            {
                // Dummy coordinates to demonstrate the functionality.
                var clickableItems = _uiAutomationService.GetClickableItems();

                // Clear existing tags and add the new ones.
                _tagOverlayWindowViewModel.ClearTags();

                // add each items to the list in the viewmodel and with numbered text
                for (int i = 0; i < clickableItems.Count; i++)
                {
                    _tagOverlayWindowViewModel.AddTag(clickableItems[i].X, clickableItems[i].Y, $"{i+1}"); // $"{i+1}" for numbered text for tags
                }
            }
            else
            {
                _tagOverlayWindowViewModel.ClearTags();
            } 
        }
    }
}

// TODO: implement an arrow connecting to the item that is tagged so that it will be very clear what is the item associated to the number
