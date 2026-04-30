using Suity.Synchonizing.Core;
using Suity.Views;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Editor.Analyzing;

/// <summary>
/// Represents a problem found during analysis, which can contain a status, message, and associated object.
/// Supports nested problems and provides various reporting capabilities.
/// </summary>
public class AnalysisProblem : INavigable
{
    /// <summary>
    /// Gets the status of this problem (e.g., Normal, Warning, Error).
    /// </summary>
    public TextStatus Status { get; private set; }

    /// <summary>
    /// Gets the message describing the problem.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the object associated with this problem, if any.
    /// </summary>
    public object Object { get; }

    private List<AnalysisProblem> _problems;
    private readonly object _lockRoot = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisProblem"/> class with default values.
    /// </summary>
    public AnalysisProblem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisProblem"/> class with a status and message.
    /// </summary>
    /// <param name="status">The status of the problem.</param>
    /// <param name="message">The message describing the problem.</param>
    public AnalysisProblem(TextStatus status, string message)
    {
        Status = status;
        Message = message ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisProblem"/> class with a status, message, and associated object.
    /// </summary>
    /// <param name="status">The status of the problem.</param>
    /// <param name="message">The message describing the problem.</param>
    /// <param name="obj">The object associated with this problem.</param>
    public AnalysisProblem(TextStatus status, string message, object obj)
    {
        Status = status;
        Message = message ?? string.Empty;
        Object = obj;
    }


    /// <summary>
    /// Adds a child problem to this problem. If the child's status is higher, this problem's status is updated.
    /// </summary>
    /// <param name="problem">The problem to add.</param>
    public void Add(AnalysisProblem problem)
    {
        if (problem is null)
        {
            return;
        }

        lock (_lockRoot)
        {
            _problems ??= [];

            _problems.Add(problem);

            if (problem.Status > Status)
            {
                Status = problem.Status;
            }
        }
    }

    /// <summary>
    /// Clears all child problems and resets the status to Normal.
    /// </summary>
    public void Clear()
    {
        lock (_lockRoot)
        {
            _problems?.Clear();
            Status = TextStatus.Normal;
        }
    }

    /// <summary>
    /// Gets the total count of problems including nested problems.
    /// </summary>
    public int Count { get { lock (_lockRoot) { return _problems?.Count ?? 0; } } }

    /// <summary>
    /// Gets the count of problems at or above a minimum status level.
    /// </summary>
    /// <param name="minStatus">The minimum status level to count.</param>
    /// <returns>The count of problems meeting the criteria.</returns>
    public int GetCount(TextStatus minStatus)
    {
        lock (_lockRoot)
        {
            int count = 0;
            if (Status >= minStatus)
            {
                count++;
            }

            if (_problems != null)
            {
                count += _problems.Sum(problem => problem.GetCount(minStatus));
            }

            return count;
        }
    }

    /// <summary>
    /// Gets all child problems as an array.
    /// </summary>
    /// <returns>An array of child problems, or an empty array if none exist.</returns>
    public AnalysisProblem[] GetItems()
    {
        lock (_lockRoot)
        {
            if (_problems != null)
            {
                return [.. _problems];
            }
            else
            {
                return [];
            }
        }
    }

    /// <summary>
    /// Builds a logger report from this problem and its children.
    /// </summary>
    /// <returns>A <see cref="ReportItem"/> or <see cref="ReportList"/> representing the problem.</returns>
    public object BuildLoggerReport()
    {
        string msg = Message; //$"{Object?.ToDisplayText()} {Message}";
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = Object?.ToDisplayText() ?? string.Empty;
        }

        if (Count == 0)
        {
            return new ReportItem(msg, Object) { DisplayStatus = Status }; 
        }
        else
        {
            var list = new ReportList(Object)
            {
                DisplayText = msg,
                DisplayStatus = Status
            };

            AnalysisProblem[] problems = null;
            lock (_lockRoot)
            {
                if (_problems?.Count > 0)
                {
                    problems = _problems.ToArray();
                }
            }

            if (problems != null)
            {
                foreach (var problem in problems)
                {
                    list.List.Add(problem.BuildLoggerReport());
                }
            }

            return list;
        }
    }

    /// <summary>
    /// Builds a text report from this problem and its children.
    /// </summary>
    /// <param name="minStatus">The minimum status level to include in the report.</param>
    /// <returns>A string containing the text report.</returns>
    public string BuildTextReport(TextStatus minStatus)
    {
        var builder = new StringBuilder();
        BuildTextReport(builder, minStatus, 0);

        return builder.ToString();
    }

    /// <summary>
    /// Builds a text report recursively with indentation.
    /// </summary>
    /// <param name="builder">The StringBuilder to append to.</param>
    /// <param name="minStatus">The minimum status level to include.</param>
    /// <param name="indent">The current indentation level.</param>
    protected void BuildTextReport(StringBuilder builder, TextStatus minStatus, int indent)
    {
        string msg = Message; //$"{Object?.ToDisplayText()} {Message}";
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = Object?.ToDisplayText() ?? string.Empty;
        }

        if (indent > 0)
        {
            builder.Append(' ', indent * 2);
            builder.Append($"[{Status}] ");
            builder.AppendLine(msg);
        }

        AnalysisProblem[] problems = null;
        lock (_lockRoot)
        {
            if (_problems?.Count > 0)
            {
                problems = _problems.Where(o => o.Status >= minStatus).ToArray();
            }
        }

        if (problems != null)
        {
            foreach (var problem in problems)
            {
                problem.BuildTextReport(builder, minStatus, indent + 1);
            }
        }
    }


    /// <summary>
    /// Gets the navigation target object associated with this problem.
    /// </summary>
    /// <returns>The associated object for navigation.</returns>
    public object GetNavigationTarget() => Object;

    /// <summary>
    /// Creates a clone of this problem with an optional new message.
    /// </summary>
    /// <param name="msg">The new message for the cloned problem, or null to use the original message.</param>
    /// <returns>A cloned <see cref="AnalysisProblem"/>.</returns>
    internal AnalysisProblem Clone(string msg = null)
    {
        msg ??= Message;

        var problem = new AnalysisProblem(Status, msg, Object);

        AnalysisProblem[] problems = null;
        lock (_lockRoot)
        {
            if (_problems?.Count > 0)
            {
                problems = _problems.ToArray();
            }
        }

        if (problems != null)
        {
            problem._problems = problems.Select(o => o.Clone()).ToList();
        }

        return problem;
    }

    /// <summary>
    /// Returns the message of this problem.
    /// </summary>
    /// <returns>The message string.</returns>
    public override string ToString() => Message;

    /// <summary>
    /// Creates an <see cref="AnalysisProblem"/> from an item that supports analysis.
    /// </summary>
    /// <param name="childItem">The item to analyze.</param>
    /// <param name="prefix">Optional prefix for the message.</param>
    /// <returns>An <see cref="AnalysisProblem"/> representing the analysis result, or null if no problems exist.</returns>
    public static AnalysisProblem FromAnalysis(ISupportAnalysis childItem, string prefix = null)
    {
        if (childItem is null)
        {
            return null;
        }

        var analysis = childItem.Analysis;

        if (analysis?.Problems is { } current)
        {
            string s = EditorUtility.GetBriefStringL(childItem);
            string msg = $"{prefix}{s}";

            var problem = current.Clone(msg);

            return problem;
        }
        else
        {
            string s = EditorUtility.GetBriefStringL(childItem);
            string msg = $"{prefix}{s}";
            var status = analysis?.Status ?? TextStatus.Normal;

            var problem = new AnalysisProblem(status, msg, childItem);

            return problem;
        }

    }
}