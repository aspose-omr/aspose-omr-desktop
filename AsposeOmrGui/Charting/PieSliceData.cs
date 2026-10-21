using System.Windows.Media;

namespace AsposeOmrGui.Charting;

/// <summary>One slice of a pie chart: a recognized value, how many cases chose it, and its color.</summary>
public sealed class PieSliceData
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
    public double Percentage { get; set; }
    public Brush Color { get; set; } = Brushes.Gray;

    /// <summary>"Value — 3 (42%)" style text used in the legend.</summary>
    public string LegendText => $"{Label}  —  {Count} ({Percentage:0.#}%)";
}
