using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeTracker.App.Converters;

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
            // Normalitzar els colors per comparar-los (majúscules)
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
/// Inverteix un valor booleà (true → false, false → true).
/// S'utilitza per a radio buttons amb comparació inversa.
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
/// Converteix un valor double a amplada de barra (mínim 4px si hi ha valor).
/// S'utilitza pel gràfic de barres de la jornada.
/// </summary>
public class BarWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            // Si hi ha hores però l'amplada és 0, mostra una barra mínima
            return width > 0 ? width : 0;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

