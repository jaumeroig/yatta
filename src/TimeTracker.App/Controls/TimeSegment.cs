using System.Windows.Media;

namespace TimeTracker.App.Controls;

public sealed class TimeSegment
{
    public string Label { get; init; } = "";
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public Color Color { get; init; } = Colors.Gray;
    public TimeSpan Duration => End - Start;
}
