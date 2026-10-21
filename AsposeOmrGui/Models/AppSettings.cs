using System.Collections.Generic;

namespace AsposeOmrGui.Models;

/// <summary>
/// The full persisted state of the application, serialized to
/// <c>%AppData%\AsposeOmrGui\settings.json</c>.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Path to the Aspose.OMR <c>.lic</c> license file, remembered between sessions.</summary>
    public string LicensePath { get; set; } = "";

    /// <summary>All registered templates the user can switch between.</summary>
    public List<TemplateEntry> Templates { get; set; } = new();

    /// <summary>The <see cref="TemplateEntry.Id"/> of the currently active template.</summary>
    public string ActiveTemplateId { get; set; } = "";

    /// <summary>All recognition cases across every template (filtered by template in the UI).</summary>
    public List<RecognitionCase> Cases { get; set; } = new();
}
