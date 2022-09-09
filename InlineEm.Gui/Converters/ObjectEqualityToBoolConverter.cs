namespace InlineEm.Gui.Converters;

using InlineEm.Gui.Mvvm;
using System;
using System.Globalization;
using System.Windows.Data;

public class ObjectEqualityToBoolConverter : Converter, IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Object.Equals(value, parameter);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
