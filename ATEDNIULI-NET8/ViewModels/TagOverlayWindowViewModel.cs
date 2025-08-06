// TagOverlayWindowViewModel.cs

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace ATEDNIULI_NET8.ViewModels
{
    public class TagOverlayWindowViewModel : ViewModelBase
    {
        // properties para bindings
        public ObservableCollection<TagViewModel> Tags { get; } = new ObservableCollection<TagViewModel>();

        // Method to add new tags to the collection.
        public void AddTag(double x, double y, string tagText)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Tags.Add(new TagViewModel(x, y, tagText));
            });
        }

        public void ClearTags()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Tags.Clear();
            });
        }
    }

    // created a class for the tag viewmodel here para kada usa na tag
    public class TagViewModel
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string TagText { get; set; }
        public SolidColorBrush Background { get; } = Brushes.Red;
        public SolidColorBrush Foreground { get; } = Brushes.White;

        public TagViewModel(double x, double y, string tagText)
        {
            X = x;
            Y = y;
            TagText = tagText;
        }
    }
}
