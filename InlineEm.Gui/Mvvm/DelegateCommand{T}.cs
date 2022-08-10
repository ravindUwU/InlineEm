namespace InlineEm.Gui.Mvvm;

using System;

public class DelegateCommand<T> : IDelegateCommand
{
	private readonly Action<T> execute;
	private readonly Predicate<T> canExecute;

	public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
	{
		this.execute = execute;
		this.canExecute = canExecute;
	}

	public DelegateCommand(Action<T> execute) : this(execute, (t) => true)
	{
	}

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter)
	{
		return canExecute((T)parameter);
	}

	public void Execute(object? parameter)
	{
		execute((T)parameter);
	}

	public void RaiseCanExecuteChanged()
	{
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}
