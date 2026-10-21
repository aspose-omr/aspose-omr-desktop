using System.Collections.Generic;
using System.Linq;
using AsposeOmrGui.Models;

namespace AsposeOmrGui.Services;

/// <summary>
/// Aggregates the answers of every successfully-run case into per-question statistics.
/// </summary>
public sealed class StatsService
{
    public const string BlankLabel = "(blank)";

    /// <summary>
    /// Builds one <see cref="QuestionStat"/> per distinct question, preserving the engine's
    /// natural ordering (sort weight, then first appearance).
    /// </summary>
    public IReadOnlyList<QuestionStat> Build(IEnumerable<RecognitionCase> cases)
    {
        var order = new List<string>();                       // question names in first-seen order
        var stats = new Dictionary<string, QuestionStat>();
        var distinctValues = new Dictionary<string, Dictionary<string, int>>();

        foreach (RecognitionCase c in cases)
        {
            if (c.Status != CaseStatus.Success)
                continue;

            foreach (QuestionAnswer qa in c.Answers)
            {
                string name = qa.QuestionName ?? "";
                if (!stats.TryGetValue(name, out QuestionStat? stat))
                {
                    stat = new QuestionStat
                    {
                        QuestionName = name,
                        SortWeight = qa.SortWeight,
                        IsFreeInput = qa.IsFreeInput
                    };
                    stats[name] = stat;
                    distinctValues[name] = new Dictionary<string, int>();
                    order.Add(name);
                }

                // A question counts as free-input if any case recognized it as such.
                stat.IsFreeInput |= qa.IsFreeInput;
                stat.SortWeight = System.Math.Min(stat.SortWeight, qa.SortWeight);
                stat.TotalResponses++;

                string value = string.IsNullOrWhiteSpace(qa.Answer) ? BlankLabel : qa.Answer.Trim();

                var counts = distinctValues[name];
                counts[value] = counts.TryGetValue(value, out int n) ? n + 1 : 1;

                stat.FreeValues.Add(new FreeTextValue { CaseName = c.Name, Value = value });
            }
        }

        foreach (string name in order)
        {
            QuestionStat stat = stats[name];
            foreach (var kv in distinctValues[name].OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key))
                stat.Distribution.Add(new ValueCount { Value = kv.Key, Count = kv.Value });
        }

        return order
            .Select(n => stats[n])
            .OrderBy(s => s.SortWeight)
            .ThenBy(s => order.IndexOf(s.QuestionName))
            .ToList();
    }
}
