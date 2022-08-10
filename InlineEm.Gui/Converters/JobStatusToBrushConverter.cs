namespace InlineEm.Gui.Converters;

using InlineEm.Gui.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

public class JobStatusToBrushConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is JobStatus status)
		{
			return status switch
			{
				JobStatus.Success => Brushes.Green,
				JobStatus.Warning => Brushes.Orange,
				JobStatus.Error => Brushes.Red,
				_ => Brushes.DarkGray,
			};
		}

		return Brushes.DarkGray;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
