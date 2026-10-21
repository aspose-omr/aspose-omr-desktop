using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AsposeOmrGui.Charting;

namespace AsposeOmrGui.Controls;

/// <summary>
/// A lightweight, dependency-free pie chart: slices are drawn with WPF geometry and the legend
/// is data-bound. Kept simple on purpose so it renders cleanly to PNG with no third-party chart
/// library (and nothing that could clash with Aspose's own dependencies).
/// </summary>
public partial class PieChartControl : UserControl
{
    public PieChartControl()
    {
        InitializeComponent();
        Loaded += (_, _) => Redraw();
    }

    public static readonly DependencyProperty SlicesProperty = DependencyProperty.Register(
        nameof(Slices),
        typeof(IEnumerable<PieSliceData>),
        typeof(PieChartControl),
        new PropertyMetadata(null, OnSlicesChanged));

    public IEnumerable<PieSliceData>? Slices
    {
        get => (IEnumerable<PieSliceData>?)GetValue(SlicesProperty);
        set => SetValue(SlicesProperty, value);
    }

    private static void OnSlicesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (PieChartControl)d;

        if (e.OldValue is INotifyCollectionChanged oldNcc)
            oldNcc.CollectionChanged -= control.OnSlicesCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged newNcc)
            newNcc.CollectionChanged += control.OnSlicesCollectionChanged;

        control.Redraw();
    }

    private void OnSlicesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Redraw();

    private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e) => Redraw();

    private void Redraw()
    {
        PieCanvas.Children.Clear();

        List<PieSliceData> slices = Slices?.Where(s => s is not null).ToList() ?? new List<PieSliceData>();
        double w = PieCanvas.ActualWidth, h = PieCanvas.ActualHeight;
        if (w <= 0 || h <= 0 || slices.Count == 0)
            return;

        int total = slices.Sum(s => Math.Max(0, s.Count));
        if (total <= 0)
            return;

        double radius = Math.Min(w, h) / 2.0 - 2;
        if (radius <= 0)
            return;

        var center = new Point(w / 2.0, h / 2.0);
        List<PieSliceData> nonZero = slices.Where(s => s.Count > 0).ToList();

        // A single value fills the whole circle: an arc from a point back to itself draws
        // nothing, so render a plain ellipse instead.
        if (nonZero.Count == 1)
        {
            var ellipse = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = nonZero[0].Color,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };
            Canvas.SetLeft(ellipse, center.X - radius);
            Canvas.SetTop(ellipse, center.Y - radius);
            PieCanvas.Children.Add(ellipse);
            return;
        }

        double startAngle = -90.0; // begin at the top of the circle
        foreach (PieSliceData slice in slices)
        {
            if (slice.Count <= 0)
                continue;

            double sweep = 360.0 * slice.Count / total;
            double endAngle = startAngle + sweep;
            PieCanvas.Children.Add(CreateSlice(center, radius, startAngle, endAngle, slice.Color));
            startAngle = endAngle;
        }
    }

    private static Path CreateSlice(Point center, double radius, double startAngle, double endAngle, Brush fill)
    {
        Point PointOnCircle(double angleDegrees)
        {
            double rad = angleDegrees * Math.PI / 180.0;
            return new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
        }

        Point start = PointOnCircle(startAngle);
        Point end = PointOnCircle(endAngle);
        bool isLargeArc = (endAngle - startAngle) > 180.0;

        var figure = new PathFigure { StartPoint = center, IsClosed = true };
        figure.Segments.Add(new LineSegment(start, true));
        figure.Segments.Add(new ArcSegment(end, new Size(radius, radius), 0, isLargeArc,
            SweepDirection.Clockwise, true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);

        return new Path
        {
            Data = geometry,
            Fill = fill,
            Stroke = Brushes.White,
            StrokeThickness = 1
        };
    }
}
