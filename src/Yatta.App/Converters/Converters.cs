namespace Yatta.App.Converters;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

/// <summary>
/// Converts a boolean to Visibility (Visible if true, Collapsed if false).
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

/// <summary>
/// Converts a boolean to inverse Visibility (Collapsed if true, Visible if false).
/// Used to show indicators when an activity is NOT active.
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }
        return true;
    }
}

/// <summary>
/// Converts a string to Visibility (Visible if not empty, Collapsed otherwise).
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a count to Visibility (Collapsed if count > 0, Visible if 0).
/// Used for empty state messages.
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a count to Visibility (Visible if count > 0, Collapsed if 0).
/// Inverse of CountToVisibilityConverter. Used for lists with items.
/// </summary>
public class InverseCountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a count to Visibility (Visible if count > 0, Collapsed if 0).
/// Alias for InverseCountToVisibilityConverter. Used for showing statistics when records exist.
/// </summary>
public class PositiveCountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a count to boolean (true if count > 0, false if 0).
/// Used for data triggers that need to check if there are records.
/// </summary>
public class PositiveCountToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsEditing boolean to dialog title.
/// </summary>
public class EditingToTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditing)
        {
            return isEditing 
                ? Resources.Resources.Dialog_EditRecord_Title
                : Resources.Resources.Dialog_NewRecord_Title;
        }
        return Resources.Resources.Dialog_NewRecord_Title;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsEditing boolean to button text.
/// </summary>
public class EditingToButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditing)
        {
            return isEditing 
                ? Resources.Resources.Button_SaveChanges
                : Resources.Resources.Button_AddRecord;
        }
        return Resources.Resources.Button_AddRecord;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsEditing boolean for activities to dialog title.
/// </summary>
public class ActivityEditingToTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditing)
        {
            return isEditing
                ? Resources.Resources.Dialog_EditActivity_Title
                : Resources.Resources.Dialog_NewActivity_Title;
        }
        return Resources.Resources.Dialog_NewActivity_Title;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts IsEditing boolean for activities to button text.
/// </summary>
public class ActivityEditingToButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditing)
        {
            return isEditing
                ? Resources.Resources.Button_SaveChanges
                : Resources.Resources.Button_CreateActivity;
        }
        return Resources.Resources.Button_CreateActivity;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts activity status (Active boolean) to background color.
/// </summary>
public class ActivityStatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return new SolidColorBrush(isActive ? Color.FromRgb(220, 252, 231) : Color.FromRgb(243, 244, 246));
        }
        return new SolidColorBrush(Color.FromRgb(243, 244, 246));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts activity status (Active boolean) to foreground color.
/// </summary>
public class ActivityStatusToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return new SolidColorBrush(isActive ? Color.FromRgb(22, 163, 74) : Color.FromRgb(107, 114, 128));
        }
        return new SolidColorBrush(Color.FromRgb(107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a hex color string (e.g., "#FF003E92") to a SolidColorBrush.
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrWhiteSpace(colorString))
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color.ToString();
        }
        return "#000000";
    }
}

/// <summary>
/// Converts the selected color to a BorderThickness (3 if selected, 0 otherwise).
/// </summary>
public class SelectedColorToBorderThicknessConverter : IValueConverter
{
    public string? TargetColor { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentColor && TargetColor != null)
        {
            // Normalize colors for comparison (uppercase)
            var current = currentColor.ToUpperInvariant();
            var target = TargetColor.ToUpperInvariant();

            return current == target ? new Thickness(3) : new Thickness(0);
        }
        return new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts a boolean value (true → false, false → true).
/// Used for radio buttons with inverse comparison.
/// </summary>
public class BoolInverterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

/// <summary>
/// Multivalue converter: returns Visible if the provided date exists in ANY of the provided collections.
/// values[0] = DateTime (day button date), values[1..n] = IEnumerable<DateTime> (dates collections).
/// </summary>
public class DateInMultipleCollectionsToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (values.Length >= 2)
            {
                DateTime? maybeDate = values[0] as DateTime?;
                if (maybeDate.HasValue)
                {
                    // Check if the date exists in any of the provided collections (values[1..n])
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (values[i] is IEnumerable<DateTime> dates)
                        {
                            if (dates.Any(d => d.Date == maybeDate.Value.Date))
                            {
                                return Visibility.Visible;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore
        }
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// Returns a SolidColorBrush for the calendar indicator dot based on date membership in
/// Telework, Office or Both collections. Priority: Both (purple) > Telework (blue) > Office (green) > Transparent.
/// values[0] = DateTime (day date), values[1] = IEnumerable<DateTime> TeleworkDates,
/// values[2] = IEnumerable<DateTime> OfficeDates, values[3] = IEnumerable<DateTime> BothDates.
/// </summary>
public class DateToIndicatorBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var date = values.Length > 0 ? values[0] as DateTime? : null;
            if (!date.HasValue) return new SolidColorBrush(Colors.Transparent);

            var tele = values.Length > 1 ? values[1] as IEnumerable<DateTime> : null;
            var off = values.Length > 2 ? values[2] as IEnumerable<DateTime> : null;
            var both = values.Length > 3 ? values[3] as IEnumerable<DateTime> : null;

            var d = date.Value.Date;
            if (both != null && both.Any(x => x.Date == d))
            {
                return new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)); // purple
            }
            if (tele != null && tele.Any(x => x.Date == d))
            {
                return new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)); // blue
            }
            if (off != null && off.Any(x => x.Date == d))
            {
                return new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // green
            }
        }
        catch
        {
            // ignore
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a DateTime to the workday start time (same date at 07:00).
/// </summary>
public class DateToWorkdayStartConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            return date.Date.AddHours(7);
        }
        return DateTime.Today.AddHours(7);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a DateTime to the workday end time (same date at 20:00).
/// </summary>
public class DateToWorkdayEndConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            return date.Date.AddHours(20);
        }
        return DateTime.Today.AddHours(20);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


