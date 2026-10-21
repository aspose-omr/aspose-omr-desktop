using System;

namespace AsposeOmrGui.Models;

/// <summary>
/// One registered <c>.omr</c> template the user can recognize against. The user may
/// register several and switch between them freely; each remembers its own settings.
/// </summary>
public sealed class TemplateEntry
{
    /// <summary>Stable identifier used to link <see cref="RecognitionCase"/>s to this template.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Friendly name shown in the UI. Defaults to the file name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Absolute path to the <c>.omr</c> template file on disk.</summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// When <c>true</c>, the Aspose.OMR.Handwriting plugin is enabled for recognition so
    /// free-text (handwritten) fields are read as text.
    /// </summary>
    public bool UsePlugin { get; set; }

    /// <summary>
    /// Recognition fill threshold, 0..100. The sentinel <c>-100</c> means "let the engine
    /// decide automatically" and is the friendly default.
    /// </summary>
    public int Threshold { get; set; } = -100;

    /// <summary>Extra pre-processing that boosts faint pencil marks on poor scans.</summary>
    public bool ApplyLightShadeProcessing { get; set; }
}
