using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AsposeOmrGui.Models;
using AsposeOmrGui.Services;

namespace AsposeOmrGui.ViewModels;

public sealed class StatsViewModel : ViewModelBase
{
    private readonly IShell _shell;

    public StatsViewModel(IShell shell)
    {
        _shell = shell;
        Questions = new ObservableCollection<QuestionStatViewModel>();
        ExportCsvCommand = new RelayCommand(_ => ExportCsv(), _ => RunCount > 0);
        Refresh();
    }

    public ObservableCollection<QuestionStatViewModel> Questions { get; }

    public ICommand ExportCsvCommand { get; }

    public bool HasQuestions => Questions.Count > 0;

    public int RunCount { get; private set; }

    public string SummaryText => _shell.ActiveTemplate is null
        ? "Choose a template to see statistics."
        : RunCount == 0
            ? "No cases have been run yet. Add and run a case on the Cases tab."
            : $"Based on {RunCount} completed case{(RunCount == 1 ? "" : "s")} of \"{_shell.ActiveTemplate.Name}\".";

    public void Refresh()
    {
        Questions.Clear();

        TemplateEntry? template = _shell.ActiveTemplate;
        var cases = template is null
            ? Enumerable.Empty<RecognitionCase>()
            : _shell.Settings.Cases.Where(c => c.TemplateId == template.Id);

        var materialized = cases.ToList();
        RunCount = materialized.Count(c => c.Status == CaseStatus.Success);

        foreach (QuestionStat stat in _shell.Stats.Build(materialized))
            Questions.Add(new QuestionStatViewModel(stat));

        OnPropertyChanged(nameof(HasQuestions));
        OnPropertyChanged(nameof(RunCount));
        OnPropertyChanged(nameof(SummaryText));
    }

    private void ExportCsv()
    {
        TemplateEntry? template = _shell.ActiveTemplate;
        if (template is null)
            return;

        var cases = _shell.Settings.Cases.Where(c => c.TemplateId == template.Id).ToList();
        if (cases.All(c => c.Status != CaseStatus.Success))
        {
            _shell.Dialogs.Info("There are no completed cases to export yet.");
            return;
        }

        string suggested = SafeFileStem(template.Name) + "-results.csv";
        string? path = _shell.Dialogs.SaveCsvFile(suggested);
        if (path is null)
            return;

        try
        {
            _shell.CsvExport.Export(cases, path);
            _shell.SetStatus($"Exported results to {path}");
            _shell.Dialogs.Info($"Results exported to:\n{path}", "Export complete");
        }
        catch (Exception ex)
        {
            _shell.Dialogs.Error("Could not export the CSV file.\n\n" + ex.Message, "Export failed");
        }
    }

    /// <summary>Saves a chart card element as a PNG (invoked from the view code-behind).</summary>
    public void SaveChartPng(QuestionStatViewModel question, FrameworkElement cardElement)
    {
        if (question is null || cardElement is null)
            return;

        string? path = _shell.Dialogs.SavePngFile(question.SuggestedPngName);
        if (path is null)
            return;

        try
        {
            PngExporter.ExportElement(cardElement, path);
            _shell.SetStatus($"Saved chart to {path}");
            _shell.Dialogs.Info($"Chart saved to:\n{path}", "Chart saved");
        }
        catch (Exception ex)
        {
            _shell.Dialogs.Error("Could not save the chart image.\n\n" + ex.Message, "Save failed");
        }
    }

    private static string SafeFileStem(string name)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "template" : name;
    }
}
