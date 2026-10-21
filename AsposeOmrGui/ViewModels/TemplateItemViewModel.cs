using System;
using System.IO;
using AsposeOmrGui.Models;

namespace AsposeOmrGui.ViewModels;

/// <summary>Editable wrapper around a <see cref="TemplateEntry"/>; changes persist via a callback.</summary>
public sealed class TemplateItemViewModel : ViewModelBase
{
    private readonly Action _onChanged;

    public TemplateItemViewModel(TemplateEntry entry, Action onChanged)
    {
        Entry = entry;
        _onChanged = onChanged;
    }

    public TemplateEntry Entry { get; }

    public string Id => Entry.Id;

    public string Name
    {
        get => Entry.Name;
        set
        {
            string v = string.IsNullOrWhiteSpace(value) ? FileName : value.Trim();
            if (Entry.Name == v) return;
            Entry.Name = v;
            OnPropertyChanged();
            _onChanged();
        }
    }

    public string Path => Entry.Path;

    public string FileName =>
        string.IsNullOrWhiteSpace(Entry.Path) ? "(no file)" : System.IO.Path.GetFileName(Entry.Path);

    public bool FileExists => !string.IsNullOrWhiteSpace(Entry.Path) && File.Exists(Entry.Path);

    public bool UsePlugin
    {
        get => Entry.UsePlugin;
        set
        {
            if (Entry.UsePlugin == value) return;
            Entry.UsePlugin = value;
            OnPropertyChanged();
            _onChanged();
        }
    }

    public bool UseAutoThreshold
    {
        get => Entry.Threshold < 0;
        set
        {
            int newThreshold = value ? -100 : ThresholdValue;
            if (Entry.Threshold == newThreshold) return;
            Entry.Threshold = newThreshold;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UseAutoThreshold));
            _onChanged();
        }
    }

    /// <summary>The 0..100 manual threshold used when <see cref="UseAutoThreshold"/> is false.</summary>
    public int ThresholdValue
    {
        get => Entry.Threshold < 0 ? 50 : Entry.Threshold;
        set
        {
            int clamped = Math.Clamp(value, 0, 100);
            if (!UseAutoThreshold)
                Entry.Threshold = clamped;
            OnPropertyChanged();
            _onChanged();
        }
    }

    public bool ApplyLightShadeProcessing
    {
        get => Entry.ApplyLightShadeProcessing;
        set
        {
            if (Entry.ApplyLightShadeProcessing == value) return;
            Entry.ApplyLightShadeProcessing = value;
            OnPropertyChanged();
            _onChanged();
        }
    }

    public void RaiseFileStateChanged()
    {
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(FileExists));
        OnPropertyChanged(nameof(Path));
    }
}
