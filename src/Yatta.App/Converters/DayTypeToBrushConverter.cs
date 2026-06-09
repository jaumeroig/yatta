namespace Yatta.App.Converters;

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Yatta.Core.Models;

/// <summary>
/// Converts a <see cref="DayType"/> to a <see cref="SolidColorBrush"/>.
/// Use the ConverterParameter "Background" or "Foreground" to select the intended brush.
/// </summary>
public class DayTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DayType dayType)
            return new SolidColorBrush(Colors.Transparent);

        var isBackground = parameter?.ToString() == "Background";

        return dayType switch
        {
            DayType.WorkDay => isBackground
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1F4E0")!)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B4332")!),
            DayType.IntensiveDay => isBackground
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8E1")!)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D4037")!),
            DayType.Holiday => isBackground
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")!)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B71C1C")!),
            DayType.FreeChoice => isBackground
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")!)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D47A1")!),
            DayType.Vacation => isBackground
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")!)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")!),
            DayType.NonWorkingDay => isBackground
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")!)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242")!),
            _ => new SolidColorBrush(Colors.Transparent)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
