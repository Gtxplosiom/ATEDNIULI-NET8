using System.Windows.Input;

namespace ATEDNIULI_NET8.ViewModels
{
    public abstract class CommandBase : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object? parameter)
        {
            return true;
        }

        // abstract para kailangan ig-implement
        // nabaruan ko la ini ha tutorial lol
        public abstract void Execute(object? parameter);

        protected void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
