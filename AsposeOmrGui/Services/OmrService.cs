using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Aspose.OMR.Api;
using Aspose.OMR.Model;
using Aspose.OMR.Recognition.Enums;
using AsposeOmrGui.Models;

namespace AsposeOmrGui.Services;

/// <summary>
/// Wraps the Aspose.OMR engine: applies the license, keeps engines (with and without the
/// Handwriting plugin) alive so the 60&#160;MB OCR model is loaded at most once, and recognizes
/// the files of a case into a single merged result.
/// </summary>
public sealed class OmrService
{
    private readonly object _gate = new();

    private OmrEngine? _enginePlain;
    private OmrEngine? _enginePlugin;

    /// <summary>Full-name prefix of the Handwriting result element types (free_text_input / text_input).</summary>
    private const string HandwritingModelNamespace = "Aspose.OMR.Handwriting.Model.Omr.";

    /// <summary>True once a license has been successfully applied in this process.</summary>
    public bool IsLicensed { get; private set; }

    /// <summary>
    /// Applies the Aspose.OMR license from the given <c>.lic</c> file. Throws a friendly
    /// exception if the file is missing or the license is invalid/expired.
    /// </summary>
    public void ApplyLicense(string licensePath)
    {
        if (string.IsNullOrWhiteSpace(licensePath))
            throw new InvalidOperationException("No license file was selected.");

        if (!File.Exists(licensePath))
            throw new FileNotFoundException($"License file not found:\n{licensePath}");

        try
        {
            var license = new Aspose.OMR.License();
            license.SetLicense(licensePath);
            IsLicensed = true;
        }
        catch (Exception ex)
        {
            IsLicensed = false;
            throw new InvalidOperationException(
                "The license file could not be applied. It may be invalid, corrupted or expired.\n\n"
                + ex.Message, ex);
        }
    }

    /// <summary>Whether the Handwriting plugin type is present and can be instantiated.</summary>
    public bool IsHandwritingPluginAvailable()
    {
        try
        {
            _ = new Aspose.OMR.Handwriting.Api.Plugins.HandwritingPlugin();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Recognizes every file of a case against the template, merging them into one result
    /// (multiple files are treated as pages of a single form). Honors the template's plugin,
    /// threshold and light-shade settings.
    /// </summary>
    public CaseRecognitionResult RecognizeCase(TemplateEntry template, IReadOnlyList<string> files)
    {
        if (template is null) throw new ArgumentNullException(nameof(template));
        if (files is null || files.Count == 0)
            throw new InvalidOperationException("This case has no files to recognize. Add at least one scan or photo.");

        if (string.IsNullOrWhiteSpace(template.Path) || !File.Exists(template.Path))
            throw new FileNotFoundException($"Template file not found:\n{template.Path}");

        var missing = files.Where(f => !File.Exists(f)).ToList();
        if (missing.Count > 0)
            throw new FileNotFoundException("These files no longer exist:\n" + string.Join("\n", missing));

        lock (_gate)
        {
            OmrEngine engine = template.UsePlugin ? GetPluginEngine() : GetPlainEngine();

            TemplateProcessor processor =
                engine.GetTemplateProcessor(template.Path, FormValidationLogic.Ignore);
            processor.ApplyLightShadeProcessing = template.ApplyLightShadeProcessing;

            RecognitionResult result = files.Count == 1
                ? processor.Recognize(files[0], template.Threshold)
                : processor.Recognize(files.ToArray(), template.Threshold);

            return BuildResult(result);
        }
    }

    private static CaseRecognitionResult BuildResult(RecognitionResult result)
    {
        var dto = new CaseRecognitionResult
        {
            RawCsv = SafeGet(result.GetCsv),
            RawJson = SafeGet(result.GetJson)
        };

        foreach (IOmrElement element in result.OmrElements ?? Enumerable.Empty<IOmrElement>())
        {
            string answer;
            try { answer = element.GetAnswer() ?? ""; }
            catch { answer = ""; }

            dto.Answers.Add(new QuestionAnswer
            {
                QuestionName = element.QuestionName ?? "",
                Answer = answer,
                SortWeight = element.SortWeight,
                IsFreeInput = IsFreeInputElement(element)
            });
        }

        if (result.Images is not null)
        {
            foreach (Bitmap? img in result.Images)
                if (img is not null)
                    dto.WriteInImages.Add(img);
        }

        return dto;
    }

    /// <summary>
    /// Detects a handwritten free-text / character field by the runtime type produced by the
    /// Handwriting plugin, so no parsing of the template JSON is required.
    /// </summary>
    private static bool IsFreeInputElement(IOmrElement element)
    {
        string? fullName = element.GetType().FullName;
        return fullName is not null && fullName.StartsWith(HandwritingModelNamespace, StringComparison.Ordinal);
    }

    private static string SafeGet(Func<string> getter)
    {
        try { return getter() ?? ""; }
        catch { return ""; }
    }

    private OmrEngine GetPlainEngine() => _enginePlain ??= new OmrEngine();

    private OmrEngine GetPluginEngine()
    {
        if (_enginePlugin is null)
        {
            var engine = new OmrEngine();
            engine.AddPlugin(new Aspose.OMR.Handwriting.Api.Plugins.HandwritingPlugin());
            _enginePlugin = engine;
        }
        return _enginePlugin;
    }
}
