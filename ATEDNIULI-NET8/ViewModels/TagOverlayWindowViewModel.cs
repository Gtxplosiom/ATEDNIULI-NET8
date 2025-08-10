using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace ATEDNIULI_NET8.ViewModels
{
    // Define the directions the pointer can face.
    // This determines where the tag is placed relative to the item.
    public enum PointerDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public class TagOverlayWindowViewModel : ViewModelBase
    {
        public ObservableCollection<TagViewModel> Tags { get; } = new ObservableCollection<TagViewModel>();

        // Method to add new tags with direction to not be outside the target window
        public void AddTag(Point itemPoint, string tagText, PointerDirection direction)
        {
            const double tagWidth = 40;
            const double tagHeight = 30;
            const double margin = 10;

            double tagX, tagY;

            switch (direction)
            {
                case PointerDirection.Left:
                    tagX = itemPoint.X + margin;
                    tagY = itemPoint.Y - tagHeight / 2;
                    break;
                case PointerDirection.Right:
                    tagX = itemPoint.X - tagWidth - margin;
                    tagY = itemPoint.Y - tagHeight / 2;
                    break;
                case PointerDirection.Up:
                    tagX = itemPoint.X - tagWidth / 2;
                    tagY = itemPoint.Y + margin;
                    break;
                case PointerDirection.Down:
                    tagX = itemPoint.X - tagWidth / 2;
                    tagY = itemPoint.Y - tagHeight - margin;
                    break;
                default:
                    tagX = itemPoint.X;
                    tagY = itemPoint.Y;
                    break;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Tags.Add(new TagViewModel(tagX, tagY, tagText, direction));
            });
        }

        public void ClearTags()
        {
            Application.Current.Dispatcher.Invoke(Tags.Clear);
        }
    }

    // Updated TagViewModel class
    public class TagViewModel : ViewModelBase
    {
        // Simplified constructor
        public TagViewModel(double tagX, double tagY, string tagText, PointerDirection direction)
        {
            _tagX = tagX;
            _tagY = tagY;
            _tagText = tagText;
            _direction = direction; // Set the direction
        }

        private double _tagX;
        private double _tagY;
        private string _tagText;
        private PointerDirection _direction; // Store the direction
        private SolidColorBrush _background = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128));
        private Brush _stroke = Brushes.Black;
        private Brush _foreground = Brushes.White;

        // prapertis
        public string TagText
        {
            get => _tagText;
            set
            {
                _tagText = value;
                OnPropertyChanged(nameof(TagText));
            }
        }

        public double TagX 
        { 
            get => _tagX;
            set
            {
                _tagX = value;
                OnPropertyChanged(nameof(TagX));
            }
        }

        public double TagY
        { 
            get => _tagY;
            set
            {
                _tagY = value;
                OnPropertyChanged(nameof(TagY));
            }
        }

        public SolidColorBrush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }

        public Brush Foreground
        {
            get => _foreground;
            set
            {
                _foreground = value;
                OnPropertyChanged(nameof(Foreground));
            }
        }

        public Brush Stroke
        {
            get => _stroke;
            set
            {
                _stroke = value;
                OnPropertyChanged(nameof(Stroke));
            }
        }

        public PointerDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }
    }
}

// TODO: consider adding a mouse listener, para kun an mouse kumiwa ma auto clear an tags
// or in general may mag change na state or may movement ha window, if possible