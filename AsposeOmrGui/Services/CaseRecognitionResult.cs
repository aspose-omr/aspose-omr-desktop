using System.Collections.Generic;
using System.Drawing;
using AsposeOmrGui.Models;

namespace AsposeOmrGui.Services;

/// <summary>
/// The outcome of recognizing one <see cref="RecognitionCase"/>. Produced by
/// <see cref="OmrService.RecognizeCase"/>. Bitmaps are kept only for the current session
/// (they are not persisted), everything else is stored on the case.
/// </summary>
public sealed class CaseRecognitionResult
{
    public List<QuestionAnswer> Answers { get; } = new();
    public string RawCsv { get; set; } = "";
    public string RawJson { get; set; } = "";

    /// <summary>Write-in (handwriting) image crops extracted from the form, if any.</summary>
    public List<Bitmap> WriteInImages { get; } = new();
}
