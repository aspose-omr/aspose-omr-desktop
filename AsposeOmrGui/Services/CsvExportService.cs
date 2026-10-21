using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsposeOmrGui.Models;

namespace AsposeOmrGui.Services;

/// <summary>
/// Exports every executed case to a single CSV table: one row per case, one column per
/// question (plus a leading "Case" column). This "rows-to-columns" pivot turns Aspose's
/// name/value result into a spreadsheet-friendly table.
/// </summary>
public sealed class CsvExportService
{
    /// <summary>Builds the pivoted CSV text for the supplied cases.</summary>
    public string BuildCsv(IEnumerable<RecognitionCase> cases)
    {
        var successful = cases.Where(c => c.Status == CaseStatus.Success).ToList();

        // Column order: by sort weight, then first appearance — matching the recognition order.
        var firstSeen = new List<string>();
        var minWeight = new Dictionary<string, int>();
        foreach (RecognitionCase c in successful)
        {
            foreach (QuestionAnswer qa in c.Answers)
            {
                string name = qa.QuestionName ?? "";
                if (!minWeight.ContainsKey(name))
                {
                    minWeight[name] = qa.SortWeight;
                    firstSeen.Add(name);
                }
                else if (qa.SortWeight < minWeight[name])
                {
                    minWeight[name] = qa.SortWeight;
                }
            }
        }

        List<string> columns = firstSeen
            .OrderBy(n => minWeight[n])
            .ThenBy(n => firstSeen.IndexOf(n))
            .ToList();

        var sb = new StringBuilder();

        // Header row.
        sb.Append(Escape("Case"));
        foreach (string col in columns)
        {
            sb.Append(',');
            sb.Append(Escape(col));
        }
        sb.Append("\r\n");

        // One row per case.
        foreach (RecognitionCase c in successful)
        {
            var lookup = new Dictionary<string, string>();
            foreach (QuestionAnswer qa in c.Answers)
                lookup[qa.QuestionName ?? ""] = qa.Answer ?? "";

            sb.Append(Escape(c.Name));
            foreach (string col in columns)
            {
                sb.Append(',');
                sb.Append(Escape(lookup.TryGetValue(col, out string? v) ? v : ""));
            }
            sb.Append("\r\n");
        }

        return sb.ToString();
    }

    /// <summary>Writes the pivoted CSV to disk as UTF-8 with a BOM (so Excel opens it cleanly).</summary>
    public void Export(IEnumerable<RecognitionCase> cases, string path)
    {
        string csv = BuildCsv(cases);
        File.WriteAllText(path, csv, new UTF8Encoding(true));
    }

    private static string Escape(string field)
    {
        field ??= "";
        if (field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r'))
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        return field;
    }
}
