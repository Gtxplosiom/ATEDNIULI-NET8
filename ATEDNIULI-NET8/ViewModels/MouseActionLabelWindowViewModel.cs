using System.Windows;

namespace ATEDNIULI_NET8.ViewModels
{
    public class MouseActionLabelWindowViewModel : ViewModelBase
    {
        public MouseActionLabelWindowViewModel()
        {
            // Constructor logic can be added here if needed
        }

        private string _mouseAction = "None";
        private Visibility _visibility = Visibility.Collapsed;

        public string MouseAction
        {
            get => _mouseAction;
            set
            {
                if (_mouseAction != value)
                {
                    _mouseAction = value;
                    OnPropertyChanged(nameof(MouseAction));
                }
            }
        }

        public Visibility VisibilityState
        {
            get => _visibility;
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(VisibilityState));
                }
            }
        }
    }
}

// TODO: pagdugang position property lat para dynamic sheesh, ma base na hiya ha current position han mouse cursor
