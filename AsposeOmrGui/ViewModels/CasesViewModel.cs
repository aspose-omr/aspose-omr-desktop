using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using AsposeOmrGui.Models;
using AsposeOmrGui.Services;

namespace AsposeOmrGui.ViewModels;

public sealed class CasesViewModel : ViewModelBase
{
    private readonly IShell _shell;
    private CaseViewModel? _selectedCase;
    private bool _working;

    public CasesViewModel(IShell shell)
    {
        _shell = shell;
        Cases = new ObservableCollection<CaseViewModel>();

        AddCaseCommand = new AsyncRelayCommand(_ => AddCaseAsync(), _ => CanWork);
        RunCaseCommand = new AsyncRelayCommand(p => RunCaseAsync(p as CaseViewModel ?? _selectedCase), _ => CanWork && _selectedCase is not null);
        RunAllCommand = new AsyncRelayCommand(_ => RunAllAsync(), _ => CanWork && Cases.Any(c => c.Status != CaseStatus.Success));
        RemoveCaseCommand = new RelayCommand(p => RemoveCase(p as CaseViewModel ?? _selectedCase), _ => _selectedCase is not null);

        ReloadForActiveTemplate();
    }

    public ObservableCollection<CaseViewModel> Cases { get; }

    public ICommand AddCaseCommand { get; }
    public ICommand RunCaseCommand { get; }
    public ICommand RunAllCommand { get; }
    public ICommand RemoveCaseCommand { get; }

    private bool CanWork => !_working && _shell.ActiveTemplate is not null;

    public CaseViewModel? SelectedCase
    {
        get => _selectedCase;
        set
        {
            if (SetProperty(ref _selectedCase, value))
                OnPropertyChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection => _selectedCase is not null;

    public bool HasCases => Cases.Count > 0;

    public string ActiveTemplateName => _shell.ActiveTemplate?.Name ?? "(no template selected)";

    public void ReloadForActiveTemplate()
    {
        Cases.Clear();
        TemplateEntry? template = _shell.ActiveTemplate;
        if (template is not null)
        {
            foreach (RecognitionCase model in _shell.Settings.Cases.Where(c => c.TemplateId == template.Id))
                Cases.Add(new CaseViewModel(model, _shell.Save));
        }

        SelectedCase = Cases.FirstOrDefault();
        OnPropertyChanged(nameof(HasCases));
        OnPropertyChanged(nameof(ActiveTemplateName));
    }

    private async Task AddCaseAsync()
    {
        TemplateEntry? template = _shell.ActiveTemplate;
        if (template is null)
        {
            _shell.Dialogs.Info("Please choose a template on the Templates tab first.");
            return;
        }

        string[]? files = _shell.Dialogs.OpenScanFiles();
        if (files is null || files.Length == 0)
            return;

        var model = new RecognitionCase
        {
            TemplateId = template.Id,
            Name = NextCaseName(),
            FilePaths = files.ToList()
        };
        _shell.Settings.Cases.Add(model);

        var vm = new CaseViewModel(model, _shell.Save);
        Cases.Add(vm);
        SelectedCase = vm;
        OnPropertyChanged(nameof(HasCases));
        _shell.Save();

        await RunCaseAsync(vm);
    }

    private async Task RunAllAsync()
    {
        foreach (CaseViewModel vm in Cases.Where(c => c.Status != CaseStatus.Success).ToList())
            await RunCaseAsync(vm);
    }

    private async Task RunCaseAsync(CaseViewModel? vm)
    {
        if (vm is null)
            return;

        TemplateEntry? template = _shell.ActiveTemplate;
        if (template is null)
        {
            _shell.Dialogs.Info("Please choose a template on the Templates tab first.");
            return;
        }

        _working = true;
        CommandManager.InvalidateRequerySuggested();
        vm.SetRunning();
        _shell.SetBusy(true, $"Recognizing \"{vm.Name}\"…");

        try
        {
            var files = vm.Case.FilePaths.ToList();
            CaseRecognitionResult result = await Task.Run(() => _shell.Omr.RecognizeCase(template, files));
            vm.ApplyResult(result);
            _shell.SetStatus($"Recognized \"{vm.Name}\" — {result.Answers.Count} question(s) read.");
        }
        catch (Exception ex)
        {
            vm.SetFailed(ex.Message);
            _shell.SetStatus($"Recognition failed for \"{vm.Name}\".");
            _shell.Dialogs.Error(ex.Message, "Recognition failed");
        }
        finally
        {
            _working = false;
            _shell.SetBusy(false);
            _shell.Save();
            _shell.NotifyCasesChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void RemoveCase(CaseViewModel? vm)
    {
        if (vm is null)
            return;

        if (!_shell.Dialogs.Confirm($"Remove case \"{vm.Name}\"?", "Remove case"))
            return;

        _shell.Settings.Cases.Remove(vm.Case);
        Cases.Remove(vm);
        if (ReferenceEquals(SelectedCase, vm))
            SelectedCase = Cases.FirstOrDefault();

        OnPropertyChanged(nameof(HasCases));
        _shell.Save();
        _shell.NotifyCasesChanged();
        _shell.SetStatus("Case removed.");
    }

    private string NextCaseName()
    {
        int max = 0;
        foreach (CaseViewModel c in Cases)
        {
            Match m = Regex.Match(c.Name, @"^Case(\d+)$");
            if (m.Success && int.TryParse(m.Groups[1].Value, out int n))
                max = Math.Max(max, n);
        }

        int candidate = max + 1;
        string name = $"Case{candidate}";
        while (Cases.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            name = $"Case{++candidate}";
        return name;
    }
}
