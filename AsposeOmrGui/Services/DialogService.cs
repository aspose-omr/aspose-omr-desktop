using System.Windows;
using Microsoft.Win32;

namespace AsposeOmrGui.Services;

/// <summary>Centralizes all file pickers and message boxes so the view-models stay UI-agnostic.</summary>
public sealed class DialogService
{
    public string? OpenLicenseFile()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select your Aspose.OMR license file",
            Filter = "License files (*.lic)|*.lic|All files (*.*)|*.*",
            CheckFileExists = true
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? OpenTemplateFile()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select an OMR template (.omr) file",
            Filter = "OMR templates (*.omr)|*.omr|All files (*.*)|*.*",
            CheckFileExists = true
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string[]? OpenScanFiles()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select the scan(s) or photo(s) to recognize",
            Filter = "Scans & photos (*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.bmp;*.gif;*.pdf)" +
                     "|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.bmp;*.gif;*.pdf|All files (*.*)|*.*",
            Multiselect = true,
            CheckFileExists = true
        };
        return dlg.ShowDialog() == true ? dlg.FileNames : null;
    }

    public string? SaveCsvFile(string suggestedName)
    {
        var dlg = new SaveFileDialog
        {
            Title = "Export results to CSV",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            FileName = suggestedName,
            DefaultExt = ".csv",
            AddExtension = true
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? SavePngFile(string suggestedName)
    {
        var dlg = new SaveFileDialog
        {
            Title = "Save chart as PNG image",
            Filter = "PNG images (*.png)|*.png|All files (*.*)|*.*",
            FileName = suggestedName,
            DefaultExt = ".png",
            AddExtension = true
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public void Info(string message, string title = "Aspose OMR")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void Error(string message, string title = "Something went wrong")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirm(string message, string title = "Please confirm")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
