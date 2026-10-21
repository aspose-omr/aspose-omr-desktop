using System.Collections.Generic;
using System.Windows.Media;

namespace AsposeOmrGui.Charting;

/// <summary>A fixed, readable categorical color palette for pie charts (repeats if exceeded).</summary>
public static class ChartPalette
{
    private static readonly Color[] Colors =
    {
        Color.FromRgb(0x4E, 0x79, 0xA7), // blue
        Color.FromRgb(0xF2, 0x8E, 0x2B), // orange
        Color.FromRgb(0x59, 0xA1, 0x4F), // green
        Color.FromRgb(0xE1, 0x57, 0x59), // red
        Color.FromRgb(0x76, 0xB7, 0xB2), // teal
        Color.FromRgb(0xED, 0xC9, 0x48), // yellow
        Color.FromRgb(0xB0, 0x7A, 0xA1), // purple
        Color.FromRgb(0xFF, 0x9D, 0xA7), // pink
        Color.FromRgb(0x9C, 0x75, 0x5F), // brown
        Color.FromRgb(0xBA, 0xB0, 0xAC), // gray
    };

    public static Brush BrushAt(int index)
    {
        var brush = new SolidColorBrush(Colors[((index % Colors.Length) + Colors.Length) % Colors.Length]);
        brush.Freeze();
        return brush;
    }

    public static IReadOnlyList<Brush> Build(int count)
    {
        var list = new List<Brush>(count);
        for (int i = 0; i < count; i++)
            list.Add(BrushAt(i));
        return list;
    }
}
