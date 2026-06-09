using System.Windows.Input;

namespace WPF_Test.Vista.Commands
{
    public class SwitchDataGridCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (parameter is Action action)
                action?.Invoke();
        }
    }
}
