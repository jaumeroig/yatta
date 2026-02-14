using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TimeTracker.App.Controls;

/// <summary>
/// Barra tipus "iOS storage" però amb escala temporal real (WorkdayStart..WorkdayEnd).
/// Dibuixa segments posicionats segons la seva hora real dins la jornada.
/// </summary>
public sealed class WorkdayTimelineBar : FrameworkElement
{
    private INotifyCollectionChanged? _notifyCollection;
    private TimeSegment? _hovered;
    private TimeSegment? _selected;

    // Drag-to-resize state
    private bool _isDragging;
    private TimeSegment? _dragSegment;
    private ResizeEdge _dragEdge;
    private DateTime _dragOriginalTime;
    private TimeSegment? _edgeHoverSegment;
    private ResizeEdge? _edgeHoverEdge;
    private TimeSegment? _dragNeighbor;
    private DateTime _dragNeighborOriginalTime;

    private const double EdgeThreshold = 6.0;
    private static readonly TimeSpan SnapInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MinSegmentDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MinPushedDuration = TimeSpan.FromMinutes(1);

    private enum ResizeEdge { Start, End }

    static WorkdayTimelineBar()
    {
        // Fa que el control repinti quan canvien propietats visuals bàsiques
        SnapsToDevicePixelsProperty.OverrideMetadata(typeof(WorkdayTimelineBar), new FrameworkPropertyMetadata(true));
    }

    public WorkdayTimelineBar()
    {
        Focusable = true;
        Cursor = Cursors.Hand;

        MouseMove += OnMouseMove;
        MouseLeave += OnMouseLeave;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        KeyDown += OnKeyDown;
    }

    #region Dependency Properties

    public static readonly DependencyProperty WorkdayStartProperty =
        DependencyProperty.Register(
            nameof(WorkdayStart),
            typeof(DateTime),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(DateTime.Today.AddHours(9), FrameworkPropertyMetadataOptions.AffectsRender));

    public DateTime WorkdayStart
    {
        get => (DateTime)GetValue(WorkdayStartProperty);
        set => SetValue(WorkdayStartProperty, value);
    }

    public static readonly DependencyProperty WorkdayEndProperty =
        DependencyProperty.Register(
            nameof(WorkdayEnd),
            typeof(DateTime),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(DateTime.Today.AddHours(18), FrameworkPropertyMetadataOptions.AffectsRender));

    public DateTime WorkdayEnd
    {
        get => (DateTime)GetValue(WorkdayEndProperty);
        set => SetValue(WorkdayEndProperty, value);
    }

    public static readonly DependencyProperty SegmentsProperty =
        DependencyProperty.Register(
            nameof(Segments),
            typeof(IEnumerable),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(null, OnSegmentsChanged));

    public IEnumerable? Segments
    {
        get => (IEnumerable?)GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    public static readonly DependencyProperty EmptyBrushProperty =
        DependencyProperty.Register(
            nameof(EmptyBrush),
            typeof(Brush),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(229, 229, 234)), FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush EmptyBrush
    {
        get => (Brush)GetValue(EmptyBrushProperty);
        set => SetValue(EmptyBrushProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(double),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(8.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty BarHeightProperty =
        DependencyProperty.Register(
            nameof(BarHeight),
            typeof(double),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(18.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    public static readonly DependencyProperty SegmentClickedCommandProperty =
        DependencyProperty.Register(
            nameof(SegmentClickedCommand),
            typeof(ICommand),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(null));

    public ICommand? SegmentClickedCommand
    {
        get => (ICommand?)GetValue(SegmentClickedCommandProperty);
        set => SetValue(SegmentClickedCommandProperty, value);
    }

    public static readonly DependencyProperty SegmentResizedCommandProperty =
        DependencyProperty.Register(
            nameof(SegmentResizedCommand),
            typeof(ICommand),
            typeof(WorkdayTimelineBar),
            new FrameworkPropertyMetadata(null));

    public ICommand? SegmentResizedCommand
    {
        get => (ICommand?)GetValue(SegmentResizedCommandProperty);
        set => SetValue(SegmentResizedCommandProperty, value);
    }

    #endregion

    #region Routed event

    public static readonly RoutedEvent SegmentClickedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SegmentClicked),
            RoutingStrategy.Bubble,
            typeof(EventHandler<TimeSegmentClickedEventArgs>),
            typeof(WorkdayTimelineBar));

    public event EventHandler<TimeSegmentClickedEventArgs> SegmentClicked
    {
        add => AddHandler(SegmentClickedEvent, value);
        remove => RemoveHandler(SegmentClickedEvent, value);
    }

    public static readonly RoutedEvent SegmentResizedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SegmentResized),
            RoutingStrategy.Bubble,
            typeof(EventHandler<TimeSegmentResizedEventArgs>),
            typeof(WorkdayTimelineBar));

    public event EventHandler<TimeSegmentResizedEventArgs> SegmentResized
    {
        add => AddHandler(SegmentResizedEvent, value);
        remove => RemoveHandler(SegmentResizedEvent, value);
    }

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        // Alçada fixa, ample el que li donin
        var height = BarHeight;
        return new Size(availableSize.Width, double.IsInfinity(availableSize.Height) ? height : Math.Min(height, availableSize.Height));
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var width = ActualWidth;
        var height = BarHeight;

        if (width <= 0 || height <= 0)
            return;

        // Rect principal
        var rect = new Rect(0, 0, width, height);

        // Fons (part "buida" com iOS)
        dc.DrawRoundedRectangle(EmptyBrush, null, rect, CornerRadius, CornerRadius);

        // Validacions de temps
        var start = WorkdayStart;
        var end = WorkdayEnd;

        if (end <= start)
            return;

        var total = end - start;
        if (total.TotalMilliseconds <= 0)
            return;

        // Clip arrodonit perquè els segments no surtin fora
        var clip = new RectangleGeometry(rect, CornerRadius, CornerRadius);
        dc.PushClip(clip);

        // Dibuix segments
        foreach (var s in EnumerateSegments())
        {
            // Clamp al rang de la jornada
            var segStart = s.Start < start ? start : s.Start;
            var segEnd = s.End > end ? end : s.End;

            if (segEnd <= segStart)
                continue;

            var offset = segStart - start;
            var duration = segEnd - segStart;

            var x = width * (offset.TotalMilliseconds / total.TotalMilliseconds);
            var w = width * (duration.TotalMilliseconds / total.TotalMilliseconds);

            // evita rectangles massa petits perquè es vegin
            if (w < 1) w = 1;

            var segRect = new Rect(x, 0, w, height);

            dc.DrawRectangle(new SolidColorBrush(s.Color), null, segRect);

            // Hover overlay (subtil)
            if (ReferenceEquals(s, _hovered) && !ReferenceEquals(s, _selected))
            {
                var overlay = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255));
                dc.DrawRectangle(overlay, null, segRect);
            }

            // Selected outline
            if (ReferenceEquals(s, _selected))
            {
                var pen = new Pen(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), 2);
                pen.Freeze();
                dc.DrawRectangle(null, pen, segRect);
            }

            // Resize edge handle (visual indicator at segment edges)
            if (!s.IsActive)
            {
                var isEdgeHover = ReferenceEquals(s, _edgeHoverSegment);
                var isDrag = ReferenceEquals(s, _dragSegment) && _isDragging;

                if (isEdgeHover || isDrag)
                {
                    var handleWidth = isDrag ? 3.0 : 2.0;
                    var alpha = (byte)(isDrag ? 230 : 160);
                    var handleBrush = new SolidColorBrush(Color.FromArgb(alpha, 255, 255, 255));
                    handleBrush.Freeze();

                    var edge = isDrag ? _dragEdge : _edgeHoverEdge!.Value;

                    if (edge == ResizeEdge.Start)
                    {
                        dc.DrawRectangle(handleBrush, null, new Rect(segRect.X, 0, handleWidth, height));
                    }
                    else
                    {
                        dc.DrawRectangle(handleBrush, null, new Rect(segRect.Right - handleWidth, 0, handleWidth, height));
                    }
                }
            }
        }

        dc.Pop();

        // Stroke subtil de la barra
        var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(25, 0, 0, 0)), 1);
        borderPen.Freeze();
        dc.DrawRoundedRectangle(null, borderPen, rect, CornerRadius, CornerRadius);

        // Rendered tooltip above the bar during drag
        if (_isDragging && _dragSegment != null)
        {
            DrawDragTooltip(dc, width, height);
        }
    }

    private IEnumerable<TimeSegment> EnumerateSegments()
    {
        if (Segments is null)
            yield break;

        foreach (var item in Segments)
        {
            if (item is TimeSegment seg)
                yield return seg;
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var p = e.GetPosition(this);

        // Handle active drag
        if (_isDragging && _dragSegment != null)
        {
            HandleDragMove(p.X);
            return;
        }

        // Check if near an edge for resize (only if a command is bound)
        if (SegmentResizedCommand != null)
        {
            var (edgeSeg, edge) = HitTestEdge(p.X);

            if (edgeSeg != null && edge != null)
            {
                Cursor = Cursors.SizeWE;
                _edgeHoverSegment = edgeSeg;
                _edgeHoverEdge = edge;
                _hovered = edgeSeg;

                ToolTip = edge == ResizeEdge.Start
                    ? $"↔ {edgeSeg.Label} · {edgeSeg.Start:HH:mm}"
                    : $"↔ {edgeSeg.Label} · {edgeSeg.End:HH:mm}";

                InvalidateVisual();
                return;
            }
        }

        // Clear edge hover if we moved away from an edge
        if (_edgeHoverSegment != null)
        {
            _edgeHoverSegment = null;
            _edgeHoverEdge = null;
            Cursor = Cursors.Hand;
        }

        // Normal hover behavior
        var seg = HitTestSegment(p.X);

        if (!ReferenceEquals(seg, _hovered))
        {
            _hovered = seg;

            ToolTip = seg is null
                ? null
                : $"{seg.Label} · {seg.Start:HH:mm}–{seg.End:HH:mm} · {FormatDuration(seg.Duration)}";

            InvalidateVisual();
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isDragging)
        {
            _hovered = null;
            _edgeHoverSegment = null;
            _edgeHoverEdge = null;
            Cursor = Cursors.Hand;
        }

        InvalidateVisual();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        var p = e.GetPosition(this);

        // Check if starting a resize drag
        if (SegmentResizedCommand != null)
        {
            var (edgeSeg, edge) = HitTestEdge(p.X);

            if (edgeSeg != null && edge != null)
            {
                _isDragging = true;
                _dragSegment = edgeSeg;
                _dragEdge = edge.Value;
                _dragOriginalTime = edge == ResizeEdge.Start ? edgeSeg.Start : edgeSeg.End;
                _selected = edgeSeg;

                // Identify the neighbor that could be pushed during this drag
                var (prev, next) = GetNeighbors(edgeSeg);
                _dragNeighbor = edge == ResizeEdge.End ? next : prev;
                _dragNeighborOriginalTime = _dragNeighbor != null
                    ? (edge == ResizeEdge.End ? _dragNeighbor.Start : _dragNeighbor.End)
                    : default;

                CaptureMouse();
                e.Handled = true;
                InvalidateVisual();
                return;
            }
        }

        // Normal click behavior
        var seg = HitTestSegment(p.X);

        if (seg is null)
            return;

        _selected = seg;
        InvalidateVisual();

        RaiseEvent(new TimeSegmentClickedEventArgs(SegmentClickedEvent, this, seg));

        if (SegmentClickedCommand?.CanExecute(seg) == true)
            SegmentClickedCommand.Execute(seg);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _dragSegment == null)
            return;

        var segment = _dragSegment;
        var originalTime = _dragOriginalTime;
        var edge = _dragEdge;
        var neighbor = _dragNeighbor;
        var neighborOriginalTime = _dragNeighborOriginalTime;

        // Check if the neighbor was actually modified
        var neighborWasModified = neighbor != null &&
            (edge == ResizeEdge.End
                ? neighbor.Start != neighborOriginalTime
                : neighbor.End != neighborOriginalTime);

        var result = new SegmentResizeResult
        {
            ResizedSegment = segment,
            AffectedNeighbor = neighborWasModified ? neighbor : null
        };

        FinalizeDrag();

        // If no command bound, revert all changes
        if (SegmentResizedCommand == null || !SegmentResizedCommand.CanExecute(result))
        {
            // Revert primary segment
            if (edge == ResizeEdge.Start)
                segment.Start = originalTime;
            else
                segment.End = originalTime;

            // Revert neighbor
            if (neighbor != null)
            {
                if (edge == ResizeEdge.End)
                    neighbor.Start = neighborOriginalTime;
                else
                    neighbor.End = neighborOriginalTime;
            }

            InvalidateVisual();
            return;
        }

        // Raise event and execute command to persist both segments
        RaiseEvent(new TimeSegmentResizedEventArgs(SegmentResizedEvent, this, result));
        SegmentResizedCommand.Execute(result);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _isDragging && _dragSegment != null)
        {
            // Restore primary segment's original time
            if (_dragEdge == ResizeEdge.Start)
                _dragSegment.Start = _dragOriginalTime;
            else
                _dragSegment.End = _dragOriginalTime;

            // Restore neighbor's original time
            if (_dragNeighbor != null)
            {
                if (_dragEdge == ResizeEdge.End)
                    _dragNeighbor.Start = _dragNeighborOriginalTime;
                else
                    _dragNeighbor.End = _dragNeighborOriginalTime;
            }

            FinalizeDrag();
            e.Handled = true;
        }
    }

    private void FinalizeDrag()
    {
        _isDragging = false;
        _dragSegment = null;
        _dragNeighbor = null;
        _edgeHoverSegment = null;
        _edgeHoverEdge = null;
        ReleaseMouseCapture();
        Cursor = Cursors.Hand;
        InvalidateVisual();
    }

    private TimeSegment? HitTestSegment(double x)
    {
        var width = ActualWidth;
        if (width <= 0) return null;

        var start = WorkdayStart;
        var end = WorkdayEnd;
        if (end <= start) return null;

        var total = end - start;

        // Converteix posició x a DateTime dins la jornada
        var ratio = x / width;
        ratio = Math.Max(0, Math.Min(1, ratio));
        var t = start + TimeSpan.FromMilliseconds(total.TotalMilliseconds * ratio);

        // Troba primer segment que contingui t (ja clamped a la jornada)
        foreach (var s in EnumerateSegments())
        {
            var segStart = s.Start < start ? start : s.Start;
            var segEnd = s.End > end ? end : s.End;

            if (t >= segStart && t <= segEnd)
                return s;
        }

        return null;
    }

    #region Drag-to-resize helpers

    private void HandleDragMove(double mouseX)
    {
        if (_dragSegment == null) return;

        var newTime = PixelToDateTime(mouseX);
        newTime = SnapToInterval(newTime, SnapInterval);

        if (_dragEdge == ResizeEdge.End)
        {
            // Minimum: segment must keep MinSegmentDuration
            var minEnd = _dragSegment.Start + MinSegmentDuration;
            // Maximum: if neighbor exists, allow pushing it but keep MinPushedDuration
            var maxEnd = _dragNeighbor != null
                ? _dragNeighbor.End - MinPushedDuration
                : WorkdayEnd;

            if (newTime < minEnd) newTime = minEnd;
            if (newTime > maxEnd) newTime = maxEnd;

            _dragSegment.End = newTime;

            // Push neighbor's start if we're overlapping past its original position
            if (_dragNeighbor != null)
            {
                _dragNeighbor.Start = newTime > _dragNeighborOriginalTime
                    ? newTime
                    : _dragNeighborOriginalTime;
            }
        }
        else // ResizeEdge.Start
        {
            // Maximum: segment must keep MinSegmentDuration
            var maxStart = _dragSegment.End - MinSegmentDuration;
            // Minimum: if neighbor exists, allow pushing it but keep MinPushedDuration
            var minStart = _dragNeighbor != null
                ? _dragNeighbor.Start + MinPushedDuration
                : WorkdayStart;

            if (newTime > maxStart) newTime = maxStart;
            if (newTime < minStart) newTime = minStart;

            _dragSegment.Start = newTime;

            // Push neighbor's end if we're overlapping past its original position
            if (_dragNeighbor != null)
            {
                _dragNeighbor.End = newTime < _dragNeighborOriginalTime
                    ? newTime
                    : _dragNeighborOriginalTime;
            }
        }

        ToolTip = null; // Tooltip is rendered directly above the bar via OnRender
        InvalidateVisual();
    }

    /// <summary>
    /// Detects if the mouse is near the left or right edge of any non-active segment.
    /// </summary>
    private (TimeSegment? segment, ResizeEdge? edge) HitTestEdge(double mouseX)
    {
        var width = ActualWidth;
        if (width <= 0) return (null, null);

        var start = WorkdayStart;
        var end = WorkdayEnd;
        if (end <= start) return (null, null);

        foreach (var s in EnumerateSegments())
        {
            if (s.IsActive) continue;

            var (segX, segW) = GetSegmentPixelBounds(s);

            if (segW <= 0) continue;

            // Small segments: if the mouse is near/on the segment at all,
            // use the center as divider between Start and End edges.
            if (segW < EdgeThreshold * 3)
            {
                var center = segX + segW / 2;
                var nearSegment = (mouseX >= segX - EdgeThreshold) && (mouseX <= segX + segW + EdgeThreshold);

                if (nearSegment)
                {
                    return mouseX <= center
                        ? (s, ResizeEdge.Start)
                        : (s, ResizeEdge.End);
                }

                continue;
            }

            if (Math.Abs(mouseX - segX) <= EdgeThreshold)
                return (s, ResizeEdge.Start);

            if (Math.Abs(mouseX - (segX + segW)) <= EdgeThreshold)
                return (s, ResizeEdge.End);
        }

        return (null, null);
    }

    /// <summary>
    /// Returns the pixel X position and width for a segment within the bar.
    /// </summary>
    private (double x, double width) GetSegmentPixelBounds(TimeSegment segment)
    {
        var barWidth = ActualWidth;
        var start = WorkdayStart;
        var end = WorkdayEnd;
        var total = (end - start).TotalMilliseconds;

        if (total <= 0) return (0, 0);

        var segStart = segment.Start < start ? start : segment.Start;
        var segEnd = segment.End > end ? end : segment.End;

        var x = barWidth * ((segStart - start).TotalMilliseconds / total);
        var w = barWidth * ((segEnd - segStart).TotalMilliseconds / total);

        return (x, w);
    }

    /// <summary>
    /// Converts a pixel X position to a DateTime within the workday range.
    /// </summary>
    private DateTime PixelToDateTime(double mouseX)
    {
        var width = ActualWidth;
        var start = WorkdayStart;
        var end = WorkdayEnd;
        var total = (end - start).TotalMilliseconds;

        var ratio = Math.Clamp(mouseX / width, 0, 1);
        return start + TimeSpan.FromMilliseconds(total * ratio);
    }

    /// <summary>
    /// Snaps a DateTime to the nearest interval (e.g., 1 minute).
    /// </summary>
    private static DateTime SnapToInterval(DateTime dt, TimeSpan interval)
    {
        var ticks = dt.Ticks;
        var intervalTicks = interval.Ticks;
        var snapped = (ticks + intervalTicks / 2) / intervalTicks * intervalTicks;
        return new DateTime(snapped, dt.Kind);
    }

    /// <summary>
    /// Renders a floating tooltip above the bar showing the new time ranges during drag.
    /// Drawn directly via DrawingContext so it's always visible (unlike WPF ToolTip
    /// which is suppressed during mouse capture).
    /// </summary>
    private void DrawDragTooltip(DrawingContext dc, double barWidth, double barHeight)
    {
        if (_dragSegment == null) return;

        var dpi = VisualTreeHelper.GetDpi(this);
        var typeface = new Typeface("Segoe UI");
        const double fontSize = 12.0;

        // Primary segment line
        var line1 = $"{_dragSegment.Label}  ·  {_dragSegment.Start:HH:mm} – {_dragSegment.End:HH:mm}  ·  {FormatDuration(_dragSegment.Duration)}";
        var ft1 = new FormattedText(
            line1,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface, fontSize, Brushes.White,
            dpi.PixelsPerDip);

        // Neighbor line (if it was pushed/shrunk)
        FormattedText? ft2 = null;
        if (_dragNeighbor != null)
        {
            var neighborChanged = _dragEdge == ResizeEdge.End
                ? _dragNeighbor.Start != _dragNeighborOriginalTime
                : _dragNeighbor.End != _dragNeighborOriginalTime;

            if (neighborChanged)
            {
                var dimWhite = new SolidColorBrush(Color.FromArgb(170, 255, 255, 255));
                dimWhite.Freeze();

                var line2 = $"{_dragNeighbor.Label}  ·  {_dragNeighbor.Start:HH:mm} – {_dragNeighbor.End:HH:mm}  ·  {FormatDuration(_dragNeighbor.Duration)}";
                ft2 = new FormattedText(
                    line2,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface, fontSize, dimWhite,
                    dpi.PixelsPerDip);
            }
        }

        // Measure tooltip size
        const double padH = 10.0;
        const double padV = 6.0;
        const double lineGap = 3.0;

        var textW = ft1.Width;
        var textH = ft1.Height;
        if (ft2 != null)
        {
            textW = Math.Max(textW, ft2.Width);
            textH += lineGap + ft2.Height;
        }

        var tipW = textW + padH * 2;
        var tipH = textH + padV * 2;

        // Position: centered on the dragged edge, above the bar
        var (segX, segW) = GetSegmentPixelBounds(_dragSegment);
        var edgeX = _dragEdge == ResizeEdge.End ? segX + segW : segX;
        var tipX = edgeX - tipW / 2;

        // Clamp horizontally to bar bounds
        if (tipX < 0) tipX = 0;
        if (tipX + tipW > barWidth) tipX = barWidth - tipW;

        const double gap = 8.0;
        var tipY = -tipH - gap;

        // Draw background
        var tipRect = new Rect(tipX, tipY, tipW, tipH);

        var bgBrush = new SolidColorBrush(Color.FromArgb(230, 32, 32, 32));
        bgBrush.Freeze();
        var tipBorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
        tipBorderBrush.Freeze();
        var tipBorderPen = new Pen(tipBorderBrush, 1);
        tipBorderPen.Freeze();

        dc.DrawRoundedRectangle(bgBrush, tipBorderPen, tipRect, 5, 5);

        // Draw text
        dc.DrawText(ft1, new Point(tipX + padH, tipY + padV));
        if (ft2 != null)
        {
            dc.DrawText(ft2, new Point(tipX + padH, tipY + padV + ft1.Height + lineGap));
        }
    }

    /// <summary>
    /// Returns the previous and next segments relative to the given segment.
    /// Assumes segments are enumerated in chronological order.
    /// </summary>
    private (TimeSegment? previous, TimeSegment? next) GetNeighbors(TimeSegment segment)
    {
        TimeSegment? prev = null;
        TimeSegment? next = null;
        var foundCurrent = false;

        foreach (var s in EnumerateSegments())
        {
            if (ReferenceEquals(s, segment))
            {
                foundCurrent = true;
                continue;
            }

            if (!foundCurrent)
                prev = s;
            else
            {
                next = s;
                break;
            }
        }

        return (prev, next);
    }

    #endregion

    private static string FormatDuration(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        return $"{ts.Minutes}m";
    }

    private static void OnSegmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (WorkdayTimelineBar)d;

        c.DetachCollectionChanged(e.OldValue);
        c.AttachCollectionChanged(e.NewValue);

        c.InvalidateVisual();
    }

    private void AttachCollectionChanged(object? value)
    {
        if (value is INotifyCollectionChanged incc)
        {
            _notifyCollection = incc;
            incc.CollectionChanged += OnSegmentsCollectionChanged;
        }
    }

    private void DetachCollectionChanged(object? value)
    {
        if (_notifyCollection is not null)
        {
            _notifyCollection.CollectionChanged -= OnSegmentsCollectionChanged;
            _notifyCollection = null;
        }
    }

    private void OnSegmentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Si canvia la col·lecció, redibuixa
        InvalidateVisual();
    }
}

public sealed class TimeSegmentClickedEventArgs : RoutedEventArgs
{
    public TimeSegmentClickedEventArgs(RoutedEvent routedEvent, object source, TimeSegment segment)
        : base(routedEvent, source)
    {
        Segment = segment;
    }

    public TimeSegment Segment { get; }
}

public sealed class TimeSegmentResizedEventArgs : RoutedEventArgs
{
    public TimeSegmentResizedEventArgs(RoutedEvent routedEvent, object source, SegmentResizeResult result)
        : base(routedEvent, source)
    {
        Result = result;
    }

    public SegmentResizeResult Result { get; }
}

