namespace InlineEm.Gui.Mvvm;

using System;

public class DelegateCommand : IDelegateCommand
{
	private readonly Action execute;
	private readonly Func<bool> canExecute;

	public DelegateCommand(Action execute, Func<bool> canExecute)
	{
		this.execute = execute;
		this.canExecute = canExecute;
	}

	public DelegateCommand(Action execute) : this(execute, () => true)
	{
	}

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter)
	{
		return canExecute();
	}

	public void Execute(object? parameter)
	{
		execute();
	}

	public void RaiseCanExecuteChanged()
	{
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}
