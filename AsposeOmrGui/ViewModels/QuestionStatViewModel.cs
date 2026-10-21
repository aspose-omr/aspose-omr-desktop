using System.Collections.Generic;
using AsposeOmrGui.Charting;
using AsposeOmrGui.Services;

namespace AsposeOmrGui.ViewModels;

/// <summary>Presents one question's statistics as either a pie chart or a list of free-text values.</summary>
public sealed class QuestionStatViewModel : ViewModelBase
{
    public QuestionStatViewModel(QuestionStat stat)
    {
        Stat = stat;
        Slices = BuildSlices(stat);
    }

    public QuestionStat Stat { get; }

    public string QuestionName => Stat.QuestionName;

    public bool IsFreeInput => Stat.IsFreeInput;

    public bool ShowChart => !Stat.IsFreeInput;

    public bool ShowList => Stat.IsFreeInput;

    public int TotalResponses => Stat.TotalResponses;

    public string HeaderText =>
        $"{Stat.QuestionName}   ·   {Stat.TotalResponses} response{(Stat.TotalResponses == 1 ? "" : "s")}";

    public string ModeText => Stat.IsFreeInput
        ? "Handwritten free-text answers"
        : "Answer distribution";

    public IReadOnlyList<PieSliceData> Slices { get; }

    public IReadOnlyList<FreeTextValue> FreeValues => Stat.FreeValues;

    /// <summary>Safe file-name stem for the exported PNG.</summary>
    public string SuggestedPngName
    {
        get
        {
            string name = QuestionName;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            if (string.IsNullOrWhiteSpace(name))
                name = "chart";
            return name + ".png";
        }
    }

    private static IReadOnlyList<PieSliceData> BuildSlices(QuestionStat stat)
    {
        var slices = new List<PieSliceData>();
        if (stat.IsFreeInput)
            return slices;

        int total = 0;
        foreach (var vc in stat.Distribution)
            total += vc.Count;
        if (total <= 0)
            return slices;

        int i = 0;
        foreach (ValueCount vc in stat.Distribution)
        {
            slices.Add(new PieSliceData
            {
                Label = vc.Value,
                Count = vc.Count,
                Percentage = 100.0 * vc.Count / total,
                Color = ChartPalette.BrushAt(i++)
            });
        }
        return slices;
    }
}
