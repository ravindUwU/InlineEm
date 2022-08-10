namespace InlineEm.Gui.Mvvm;

using System.Windows.Input;

public interface IDelegateCommand : ICommand
{
	void RaiseCanExecuteChanged();
}
