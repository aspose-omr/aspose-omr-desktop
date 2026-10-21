namespace AsposeOmrGui.Models;

/// <summary>
/// A single recognized question/element within a case: the question name, the recognized
/// value, and whether it came from a free-text (handwriting) field.
/// </summary>
public sealed class QuestionAnswer
{
    public string QuestionName { get; set; } = "";

    /// <summary>The recognized answer. Empty string means the field was left blank.</summary>
    public string Answer { get; set; } = "";

    /// <summary>
    /// True when this element is a handwritten free-text / character field (detected from the
    /// Aspose.OMR.Handwriting result type). Such questions are listed verbatim in statistics
    /// rather than charted.
    /// </summary>
    public bool IsFreeInput { get; set; }

    /// <summary>Ordering weight reported by the engine; lower sorts earlier.</summary>
    public int SortWeight { get; set; }
}
