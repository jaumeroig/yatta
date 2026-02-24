namespace TimeTracker.App.Models;

using System.Windows.Media;

/// <summary>
/// Display model for a time record.
/// </summary>
public class TimeRecordDisplay
{
    public Guid Id { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityColor { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsActive { get; set; }
    public bool Telework { get; set; }

    /// <summary>
    /// Returns the color as a SolidColorBrush to facilitate binding.
    /// </summary>
    public SolidColorBrush ActivityColorBrush
    {
        get
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(ActivityColor);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
    }
}