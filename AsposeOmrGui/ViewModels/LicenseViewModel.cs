using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Input;

namespace AsposeOmrGui.ViewModels;

public sealed class LicenseViewModel : ViewModelBase
{
    private readonly IShell _shell;

    private string _licensePath = "";
    private string _statusText = "No license applied yet.";
    private Brush _statusBrush = Brushes.Gray;
    private bool _isLicensed;
    private bool _isEvaluation;

    public LicenseViewModel(IShell shell)
    {
        _shell = shell;
        _licensePath = shell.Settings.LicensePath ?? "";

        ChooseCommand = new RelayCommand(_ => ChooseAndApply());
        ReapplyCommand = new RelayCommand(_ => Apply(_licensePath, announce: true),
            _ => !string.IsNullOrWhiteSpace(_licensePath));
        UseEvaluationCommand = new RelayCommand(_ => UseEvaluation());
    }

    public ICommand ChooseCommand { get; }
    public ICommand ReapplyCommand { get; }
    public ICommand UseEvaluationCommand { get; }

    public string LicensePath
    {
        get => _licensePath;
        private set => SetProperty(ref _licensePath, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public Brush StatusBrush
    {
        get => _statusBrush;
        private set => SetProperty(ref _statusBrush, value);
    }

    public bool IsLicensed
    {
        get => _isLicensed;
        private set => SetProperty(ref _isLicensed, value);
    }

    public bool IsEvaluation
    {
        get => _isEvaluation;
        private set => SetProperty(ref _isEvaluation, value);
    }

    /// <summary>At startup, try to apply the remembered license path (per the workflow requirement).</summary>
    public void TryAutoApply()
    {
        if (string.IsNullOrWhiteSpace(_licensePath))
        {
            SetStatus("Choose your Aspose.OMR license file to begin.", Brushes.Gray);
            return;
        }

        if (!File.Exists(_licensePath))
        {
            SetStatus($"Saved license file was not found:\n{_licensePath}\nPlease choose it again.", Brushes.IndianRed);
            _shell.NotifyLicenseAccepted(false, false);
            return;
        }

        Apply(_licensePath, announce: false);
    }

    private void ChooseAndApply()
    {
        string? path = _shell.Dialogs.OpenLicenseFile();
        if (path is null)
            return;

        LicensePath = path;
        Apply(path, announce: true);
    }

    private void Apply(string path, bool announce)
    {
        try
        {
            _shell.Omr.ApplyLicense(path);

            LicensePath = path;
            _shell.Settings.LicensePath = path;
            _shell.Save();

            IsLicensed = true;
            IsEvaluation = false;
            SetStatus("License applied successfully. You can now choose a template.", Brushes.SeaGreen);
            _shell.SetStatus("License applied.");
            _shell.NotifyLicenseAccepted(true, false);
        }
        catch (Exception ex)
        {
            IsLicensed = false;
            IsEvaluation = false;
            SetStatus(ex.Message, Brushes.IndianRed);
            _shell.NotifyLicenseAccepted(false, false);
            if (announce)
                _shell.Dialogs.Error(ex.Message, "License could not be applied");
        }
    }

    private void UseEvaluation()
    {
        bool ok = _shell.Dialogs.Confirm(
            "Evaluation mode has limits: a watermark on generated forms, and only the first 5 answers " +
            "of each form are recognized.\n\nContinue without a license anyway?",
            "Use evaluation mode?");
        if (!ok)
            return;

        IsLicensed = false;
        IsEvaluation = true;
        SetStatus("Running in evaluation mode (first 5 answers only).", Brushes.DarkGoldenrod);
        _shell.SetStatus("Evaluation mode.");
        _shell.NotifyLicenseAccepted(true, true);
    }

    private void SetStatus(string text, Brush brush)
    {
        StatusText = text;
        StatusBrush = brush;
    }
}
