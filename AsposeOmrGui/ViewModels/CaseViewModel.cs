using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AsposeOmrGui.Models;
using AsposeOmrGui.Services;

namespace AsposeOmrGui.ViewModels;

/// <summary>Editable wrapper around a <see cref="RecognitionCase"/> plus in-session view data.</summary>
public sealed class CaseViewModel : ViewModelBase
{
    private readonly Action _onChanged;

    public CaseViewModel(RecognitionCase model, Action onChanged)
    {
        Case = model;
        _onChanged = onChanged;
        WriteInImages = new ObservableCollection<BitmapSource>();
    }

    public RecognitionCase Case { get; }

    public string Id => Case.Id;

    public string Name
    {
        get => Case.Name;
        set
        {
            string v = string.IsNullOrWhiteSpace(value) ? Case.Name : value.Trim();
            if (Case.Name == v) return;
            Case.Name = v;
            OnPropertyChanged();
            _onChanged();
        }
    }

    public int FileCount => Case.FilePaths.Count;

    public string FilesDisplay => Case.FilePaths.Count == 0
        ? "(no files)"
        : string.Join(", ", Case.FilePaths.Select(System.IO.Path.GetFileName));

    public string FilesTooltip => string.Join(Environment.NewLine, Case.FilePaths);

    public CaseStatus Status => Case.Status;

    public string StatusText => Case.Status switch
    {
        CaseStatus.NotRun => "Not run yet",
        CaseStatus.Running => "Recognizing…",
        CaseStatus.Success => "Done",
        CaseStatus.Failed => "Failed",
        _ => ""
    };

    public Brush StatusBrush => Case.Status switch
    {
        CaseStatus.Success => Brushes.SeaGreen,
        CaseStatus.Failed => Brushes.IndianRed,
        CaseStatus.Running => Brushes.SteelBlue,
        _ => Brushes.Gray
    };

    public string LastRunDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Case.LastRunUtc))
                return "";
            if (DateTime.TryParse(Case.LastRunUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out DateTime utc))
                return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            return Case.LastRunUtc;
        }
    }

    public string Error => Case.Error;

    public string RawCsv => Case.RawCsv;

    public IReadOnlyList<QuestionAnswer> Answers => Case.Answers;

    public ObservableCollection<BitmapSource> WriteInImages { get; }

    public bool HasWriteInImages => WriteInImages.Count > 0;

    public bool IsSuccess => Case.Status == CaseStatus.Success;

    public void SetRunning()
    {
        Case.Status = CaseStatus.Running;
        Case.Error = "";
        RaiseAll();
    }

    public void ApplyResult(CaseRecognitionResult result)
    {
        Case.Status = CaseStatus.Success;
        Case.Error = "";
        Case.LastRunUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        Case.Answers = result.Answers;
        Case.RawCsv = result.RawCsv;
        Case.RawJson = result.RawJson;

        WriteInImages.Clear();
        foreach (var bmp in result.WriteInImages)
        {
            BitmapSource? src = ImageUtil.ToBitmapSource(bmp);
            if (src is not null)
                WriteInImages.Add(src);
        }
        RaiseAll();
    }

    public void SetFailed(string message)
    {
        Case.Status = CaseStatus.Failed;
        Case.Error = message;
        Case.LastRunUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        RaiseAll();
    }

    public void RaiseFilesChanged()
    {
        OnPropertyChanged(nameof(FileCount));
        OnPropertyChanged(nameof(FilesDisplay));
        OnPropertyChanged(nameof(FilesTooltip));
    }

    private void RaiseAll()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusBrush));
        OnPropertyChanged(nameof(LastRunDisplay));
        OnPropertyChanged(nameof(Error));
        OnPropertyChanged(nameof(RawCsv));
        OnPropertyChanged(nameof(Answers));
        OnPropertyChanged(nameof(IsSuccess));
        OnPropertyChanged(nameof(HasWriteInImages));
    }
}
