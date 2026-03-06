using System.Windows.Media;

namespace Yatta.App.Controls;

public sealed class TimeSegment
{
    /// <summary>
    /// Links this segment back to the underlying TimeRecord for persistence.
    /// </summary>
    public Guid? RecordId { get; init; }

    public string Label { get; init; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Color Color { get; init; } = Colors.Gray;

    /// <summary>
    /// Active segments (currently recording) are not resizable.
    /// </summary>
    public bool IsActive { get; init; }

    public TimeSpan Duration => End - Start;
}

/// <summary>
/// Result of a segment resize operation, including the primary segment
/// and optionally a neighbor that was pushed/shrunk.
/// </summary>
public sealed class SegmentResizeResult
{
    public required TimeSegment ResizedSegment { get; init; }
    public TimeSegment? AffectedNeighbor { get; init; }
}
