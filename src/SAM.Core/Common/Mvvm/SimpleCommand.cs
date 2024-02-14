using System.Windows.Input;

namespace SAM.Core;

public class SimpleCommand (Func<object, bool> canExecute = null, Action<object> execute = null) : ICommand
{
	public Func<object, bool> CanExecuteDelegate { get; set; } = canExecute;

	public Action<object> ExecuteDelegate { get; set; } = execute;

	public bool CanExecute (object parameter)
	{
		var canExecute = CanExecuteDelegate;
		return canExecute == null || canExecute(parameter);
	}

	public event EventHandler CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public void Execute (object parameter)
	{
		ExecuteDelegate?.Invoke(parameter);
	}
}
