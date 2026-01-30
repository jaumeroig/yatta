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
        MouseLeave += (_, _) => { _hovered = null; InvalidateVisual(); };
        MouseLeftButtonDown += OnMouseLeftButtonDown;
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
        }

        dc.Pop();

        // Stroke subtil de la barra
        var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(25, 0, 0, 0)), 1);
        borderPen.Freeze();
        dc.DrawRoundedRectangle(null, borderPen, rect, CornerRadius, CornerRadius);
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
        var seg = HitTestSegment(p.X);

        if (!ReferenceEquals(seg, _hovered))
        {
            _hovered = seg;

            // Tooltip: el segment actual
            ToolTip = seg is null
                ? null
                : $"{seg.Label} · {seg.Start:HH:mm}–{seg.End:HH:mm} · {FormatDuration(seg.Duration)}";

            InvalidateVisual();
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        var p = e.GetPosition(this);
        var seg = HitTestSegment(p.X);

        if (seg is null)
            return;

        _selected = seg;
        InvalidateVisual();

        RaiseEvent(new TimeSegmentClickedEventArgs(SegmentClickedEvent, this, seg));

        if (SegmentClickedCommand?.CanExecute(seg) == true)
            SegmentClickedCommand.Execute(seg);
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

