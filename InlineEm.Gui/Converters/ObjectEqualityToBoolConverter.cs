namespace InlineEm.Gui.Converters;

using System;
using System.Globalization;
using System.Windows.Data;

public class ObjectEqualityToBoolConverter : IValueConverter
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
