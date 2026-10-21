using System.Linq;
using AsposeOmrGui.Models;
using AsposeOmrGui.Services;

namespace AsposeOmrGui.ViewModels;

public sealed class MainViewModel : ViewModelBase, IShell
{
    private readonly SettingsService _settingsService;

    private bool _licenseAccepted;
    private bool _isEvaluation;
    private int _selectedTabIndex;
    private string _statusText = "Welcome! Start by choosing your license file.";
    private bool _isBusy;
    private string _busyText = "";

    public MainViewModel()
    {
        _settingsService = new SettingsService();
        Settings = _settingsService.Load();

        Omr = new OmrService();
        Stats = new StatsService();
        CsvExport = new CsvExportService();
        Dialogs = new DialogService();

        License = new LicenseViewModel(this);
        Templates = new TemplatesViewModel(this);
        Cases = new CasesViewModel(this);
        Statistics = new StatsViewModel(this);

        License.TryAutoApply();
    }

    // ---- services (IShell) ----
    public AppSettings Settings { get; }
    public OmrService Omr { get; }
    public StatsService Stats { get; }
    public CsvExportService CsvExport { get; }
    public DialogService Dialogs { get; }

    // ---- tab view-models ----
    public LicenseViewModel License { get; }
    public TemplatesViewModel Templates { get; }
    public CasesViewModel Cases { get; }
    public StatsViewModel Statistics { get; }

    public TemplateEntry? ActiveTemplate =>
        Settings.Templates.FirstOrDefault(t => t.Id == Settings.ActiveTemplateId);

    // ---- gating ----
    public bool LicenseAccepted
    {
        get => _licenseAccepted;
        private set
        {
            if (SetProperty(ref _licenseAccepted, value))
                OnPropertyChanged(nameof(CanUseTemplates));
        }
    }

    public bool IsEvaluation
    {
        get => _isEvaluation;
        private set => SetProperty(ref _isEvaluation, value);
    }

    public bool CanUseTemplates => _licenseAccepted;

    public bool HasActiveTemplate => ActiveTemplate is not null;

    public bool CanUseCases => _licenseAccepted && HasActiveTemplate;

    public bool CanUseStats => _licenseAccepted && HasActiveTemplate;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    // ---- status bar ----
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string BusyText
    {
        get => _busyText;
        private set => SetProperty(ref _busyText, value);
    }

    // ---- IShell coordination ----
    public void Save() => _settingsService.Save(Settings);

    public void SetStatus(string message) => StatusText = message;

    public void SetBusy(bool busy, string? message = null)
    {
        IsBusy = busy;
        BusyText = message ?? "";
    }

    public void NotifyLicenseAccepted(bool accepted, bool evaluation)
    {
        LicenseAccepted = accepted;
        IsEvaluation = evaluation;
        OnPropertyChanged(nameof(CanUseCases));
        OnPropertyChanged(nameof(CanUseStats));

        if (accepted && SelectedTabIndex == 0)
            SelectedTabIndex = 1; // gently advance to Templates
    }

    public void NotifyActiveTemplateChanged()
    {
        OnPropertyChanged(nameof(ActiveTemplate));
        OnPropertyChanged(nameof(HasActiveTemplate));
        OnPropertyChanged(nameof(CanUseCases));
        OnPropertyChanged(nameof(CanUseStats));

        Cases.ReloadForActiveTemplate();
        Statistics.Refresh();
    }

    public void NotifyCasesChanged() => Statistics.Refresh();
}
