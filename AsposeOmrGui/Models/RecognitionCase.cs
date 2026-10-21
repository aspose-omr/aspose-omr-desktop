using System;
using System.Collections.Generic;

namespace AsposeOmrGui.Models;

public enum CaseStatus
{
    NotRun,
    Running,
    Success,
    Failed
}

/// <summary>
/// One recognition "case": a named bundle of one or more scanned files that are recognized
/// together into a single result (the engine merges multiple files as pages of one form).
/// A case keeps its recognized answers so statistics and export survive app restarts.
/// </summary>
public sealed class RecognitionCase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Links the case to its <see cref="TemplateEntry.Id"/>.</summary>
    public string TemplateId { get; set; } = "";

    public string Name { get; set; } = "";

    /// <summary>The scanned image/PDF files that make up this case.</summary>
    public List<string> FilePaths { get; set; } = new();

    public CaseStatus Status { get; set; } = CaseStatus.NotRun;

    /// <summary>ISO-8601 timestamp of the last successful/attempted run; empty if never run.</summary>
    public string LastRunUtc { get; set; } = "";

    /// <summary>Per-question recognized answers from the last successful run.</summary>
    public List<QuestionAnswer> Answers { get; set; } = new();

    /// <summary>Raw CSV exactly as Aspose returned it (for the advanced view).</summary>
    public string RawCsv { get; set; } = "";

    /// <summary>Raw JSON exactly as Aspose returned it (for the advanced view).</summary>
    public string RawJson { get; set; } = "";

    /// <summary>Error message when <see cref="Status"/> is <see cref="CaseStatus.Failed"/>.</summary>
    public string Error { get; set; } = "";
}
