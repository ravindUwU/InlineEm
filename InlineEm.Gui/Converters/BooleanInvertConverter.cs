namespace InlineEm.Gui.Converters;

using System;
using System.Globalization;
using System.Windows.Data;

public class BooleanInvertConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (value is bool b) && !b;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
