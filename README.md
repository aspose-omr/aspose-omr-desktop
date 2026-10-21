# Aspose OMR Recognizer (WPF)

A simple, friendly Windows desktop app for recognizing **already-existing** Aspose.OMR
templates. It wraps [Aspose.OMR for .NET](https://products.aspose.com/omr/net/) **26.1** and the
**Aspose.OMR.Handwriting** plugin behind a four-step workflow that anyone can follow — with an
"Advanced" view in each step for power users.

> This app **recognizes** filled-in forms against a template you already have (`.omr`). It does
> not design/generate templates.

## The four steps

1. **License** — Pick your Aspose.OMR `.lic` file. The path is remembered and applied automatically
   on every launch. If it's missing or invalid, the app tells you and asks again. (You can also
   continue in evaluation mode, which only reads the first 5 answers of each form.)
2. **Templates** — Add one or more `.omr` templates and switch between them freely. Each template
   remembers whether to use the **Handwriting plugin** (for handwritten text fields) and, under
   *Advanced*, its mark-sensitivity threshold and faint-mark boosting.
3. **Cases** — A *case* is one or more scanned files (PNG/JPG/TIFF/BMP/GIF/PDF) recognized together
   into a single result. Add a case, pick its file(s), and it runs immediately. Cases are
   auto-named `Case1`, `Case2`, … (rename freely). View recognized answers, re-run, or remove.
4. **Statistics** — For every question, a **pie chart** of how the answers were distributed across
   all completed cases — each chart can be **saved as a PNG**. Handwritten free-text questions
   (when the plugin is on) are shown as a **list of values** instead of a chart. One click
   **exports every case to a CSV** pivoted so that **rows = cases** and **columns = question names**.

## Requirements

- Windows 10/11 (x64)
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (or the SDK to build)
- An Aspose.OMR license file (`.lic`) for unlimited recognition

## Build & run

```bash
dotnet build AsposeOmrGui/AsposeOmrGui.csproj -c Release
```

Then run the produced `AsposeOmrGui.exe` (under `AsposeOmrGui/bin/Release/net8.0-windows/win-x64/`),
or during development:

```bash
dotnet run --project AsposeOmrGui/AsposeOmrGui.csproj
```

## Where your data is stored

All settings — the license path, your templates, and every case with its recognized answers — are
saved as JSON at:

```
%AppData%\AsposeOmrGui\settings.json
```

So your cases and results survive restarts. (Write-in/handwriting image crops are shown only in the
current session and are not persisted.)

## How it works (for developers)

- **MVVM**, net8.0-windows, no third-party charting library — the pie chart is drawn with native WPF
  geometry (`Controls/PieChartControl`) and exported to PNG via `RenderTargetBitmap`
  (`Services/PngExporter`). This keeps the dependency surface to just Aspose's packages.
- `Services/OmrService` applies the license, keeps one engine **with** and one **without** the
  Handwriting plugin (so the ~60&#160;MB OCR model loads at most once), and recognizes a case's files
  with `TemplateProcessor.Recognize(string[], threshold)` (multiple files merge into one result).
- Free-text questions are detected from the runtime result type produced by the plugin
  (`Aspose.OMR.Handwriting.Model.Omr.FreeTextInput` / `TextInput`) — no template-JSON parsing needed.
- NuGet packages: `Aspose.OMR` 26.1.0, `Aspose.OMR.Handwriting` 24.7.0.

## Project layout

```
AsposeOmrGui/
  Models/        settings, templates, cases, answers
  Services/      OMR engine wrapper, settings store, stats, CSV export, PNG export, dialogs
  ViewModels/    one per tab + item wrappers + MVVM helpers
  Views/         one UserControl per tab
  Controls/      the pie-chart control
  Charting/      slice data + color palette
  Converters/    XAML value converters
  Themes/        styles & palette
```
