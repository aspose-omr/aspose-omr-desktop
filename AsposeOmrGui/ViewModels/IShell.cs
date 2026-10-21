using AsposeOmrGui.Models;
using AsposeOmrGui.Services;

namespace AsposeOmrGui.ViewModels;

/// <summary>
/// The coordination surface the tab view-models use to reach shared services, persist state,
/// report status, and notify the shell of cross-tab changes.
/// </summary>
public interface IShell
{
    AppSettings Settings { get; }
    OmrService Omr { get; }
    StatsService Stats { get; }
    CsvExportService CsvExport { get; }
    DialogService Dialogs { get; }

    TemplateEntry? ActiveTemplate { get; }

    /// <summary>Persists the current settings to disk.</summary>
    void Save();

    /// <summary>Shows a short message in the status bar.</summary>
    void SetStatus(string message);

    /// <summary>Toggles the global busy indicator.</summary>
    void SetBusy(bool busy, string? message = null);

    /// <summary>Called when the license is accepted (licensed or evaluation) or revoked.</summary>
    void NotifyLicenseAccepted(bool accepted, bool evaluation);

    /// <summary>Called when the active template changes so Cases/Stats can reload.</summary>
    void NotifyActiveTemplateChanged();

    /// <summary>Called when cases are added/removed/run so Stats/export refresh.</summary>
    void NotifyCasesChanged();
}
