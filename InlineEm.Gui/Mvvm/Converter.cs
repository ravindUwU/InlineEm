namespace InlineEm.Gui.Mvvm;

using System;
using System.Windows.Markup;

public abstract class Converter : MarkupExtension
{
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}
}
