namespace InlineEm.Gui.Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

public class BooleanToVisibilityConverter : IValueConverter
{
	public bool Invert { get; set; } = false;
	public bool Hidden { get; set; } = false;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (value is bool b && b ^ Invert)
			? Visibility.Visible
			: (Hidden ? Visibility.Hidden : Visibility.Collapsed);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
