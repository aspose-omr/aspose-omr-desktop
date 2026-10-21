using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AsposeOmrGui.Services;

/// <summary>Renders an on-screen WPF element to a PNG file at a crisp, scaled resolution.</summary>
public static class PngExporter
{
    public static void ExportElement(FrameworkElement element, string path, double scale = 2.0)
    {
        if (element is null) throw new ArgumentNullException(nameof(element));

        double width = element.ActualWidth;
        double height = element.ActualHeight;
        if (width <= 0 || height <= 0)
        {
            // Not yet laid out (e.g. collapsed/virtualized): force a measure/arrange pass.
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));
            width = element.ActualWidth;
            height = element.ActualHeight;
        }

        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("The chart has no size to export yet.");

        int pixelWidth = Math.Max(1, (int)Math.Ceiling(width * scale));
        int pixelHeight = Math.Max(1, (int)Math.Ceiling(height * scale));
        double dpi = 96.0 * scale;

        var target = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);

        // Paint an opaque white background first so the PNG isn't transparent.
        var visual = new DrawingVisual();
        using (DrawingContext ctx = visual.RenderOpen())
        {
            ctx.DrawRectangle(Brushes.White, null, new Rect(new Size(width, height)));
            var brush = new VisualBrush(element) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top };
            ctx.DrawRectangle(brush, null, new Rect(new Size(width, height)));
        }
        target.Render(visual);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(target));

        using FileStream stream = File.Create(path);
        encoder.Save(stream);
    }
}
