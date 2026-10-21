using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AsposeOmrGui.Models;

namespace AsposeOmrGui.ViewModels;

public sealed class TemplatesViewModel : ViewModelBase
{
    private readonly IShell _shell;
    private TemplateItemViewModel? _selectedTemplate;
    private bool _suppressSelectionNotify;

    public TemplatesViewModel(IShell shell)
    {
        _shell = shell;
        Templates = new ObservableCollection<TemplateItemViewModel>();

        AddTemplateCommand = new RelayCommand(_ => AddTemplate());
        RemoveTemplateCommand = new RelayCommand(_ => RemoveTemplate(), _ => SelectedTemplate is not null);

        LoadFromSettings();
    }

    public ObservableCollection<TemplateItemViewModel> Templates { get; }

    public ICommand AddTemplateCommand { get; }
    public ICommand RemoveTemplateCommand { get; }

    public bool PluginAvailable => _shell.Omr.IsHandwritingPluginAvailable();

    public bool HasTemplates => Templates.Count > 0;

    public TemplateItemViewModel? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (!SetProperty(ref _selectedTemplate, value))
                return;

            OnPropertyChanged(nameof(HasSelection));

            if (_suppressSelectionNotify)
                return;

            _shell.Settings.ActiveTemplateId = value?.Id ?? "";
            _shell.Save();
            _shell.NotifyActiveTemplateChanged();
        }
    }

    public bool HasSelection => _selectedTemplate is not null;

    private void LoadFromSettings()
    {
        _suppressSelectionNotify = true;
        Templates.Clear();
        foreach (TemplateEntry entry in _shell.Settings.Templates)
            Templates.Add(new TemplateItemViewModel(entry, _shell.Save));

        TemplateItemViewModel? active =
            Templates.FirstOrDefault(t => t.Id == _shell.Settings.ActiveTemplateId)
            ?? Templates.FirstOrDefault();

        SelectedTemplate = active;
        _shell.Settings.ActiveTemplateId = active?.Id ?? "";
        _suppressSelectionNotify = false;

        OnPropertyChanged(nameof(HasTemplates));
    }

    private void AddTemplate()
    {
        string? path = _shell.Dialogs.OpenTemplateFile();
        if (path is null)
            return;

        // Reuse an existing entry if the same file is already registered.
        TemplateItemViewModel? existing = Templates.FirstOrDefault(
            t => string.Equals(t.Path, path, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            SelectedTemplate = existing;
            _shell.SetStatus("That template is already in the list.");
            return;
        }

        var entry = new TemplateEntry
        {
            Path = path,
            Name = Path.GetFileNameWithoutExtension(path)
        };
        _shell.Settings.Templates.Add(entry);

        var vm = new TemplateItemViewModel(entry, _shell.Save);
        Templates.Add(vm);
        OnPropertyChanged(nameof(HasTemplates));

        SelectedTemplate = vm;       // also makes it active + saves + notifies
        _shell.SetStatus($"Added template \"{vm.Name}\".");
    }

    private void RemoveTemplate()
    {
        TemplateItemViewModel? vm = SelectedTemplate;
        if (vm is null)
            return;

        int caseCount = _shell.Settings.Cases.Count(c => c.TemplateId == vm.Id);
        string message = caseCount > 0
            ? $"Remove template \"{vm.Name}\" and its {caseCount} case(s)?"
            : $"Remove template \"{vm.Name}\"?";

        if (!_shell.Dialogs.Confirm(message, "Remove template"))
            return;

        _shell.Settings.Cases.RemoveAll(c => c.TemplateId == vm.Id);
        _shell.Settings.Templates.Remove(vm.Entry);
        Templates.Remove(vm);
        OnPropertyChanged(nameof(HasTemplates));

        SelectedTemplate = Templates.FirstOrDefault();   // triggers save + notify
        _shell.NotifyCasesChanged();
        _shell.SetStatus("Template removed.");
    }

    public void RefreshPluginAvailability() => OnPropertyChanged(nameof(PluginAvailable));
}
