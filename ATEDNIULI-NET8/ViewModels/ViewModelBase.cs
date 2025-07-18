﻿using System.ComponentModel;

// Para magin reusable an mga property event handler
// ig i-inherit la hin mga viewmodel classes
namespace ATEDNIULI_NET8.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
