using System.Collections.Generic;

namespace AsposeOmrGui.Services;

/// <summary>Aggregated statistics for a single question across all executed cases.</summary>
public sealed class QuestionStat
{
    public string QuestionName { get; set; } = "";

    /// <summary>When true this is a handwritten free-text question: list values, don't chart.</summary>
    public bool IsFreeInput { get; set; }

    public int SortWeight { get; set; }

    /// <summary>Total number of cases that contributed an answer to this question.</summary>
    public int TotalResponses { get; set; }

    /// <summary>Value → count distribution (used for the pie chart).</summary>
    public List<ValueCount> Distribution { get; } = new();

    /// <summary>Per-case values (used when <see cref="IsFreeInput"/> is true).</summary>
    public List<FreeTextValue> FreeValues { get; } = new();
}

public sealed class ValueCount
{
    public string Value { get; set; } = "";
    public int Count { get; set; }
}

public sealed class FreeTextValue
{
    public string CaseName { get; set; } = "";
    public string Value { get; set; } = "";
}
