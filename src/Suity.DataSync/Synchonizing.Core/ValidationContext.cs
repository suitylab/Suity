using System.Collections.Generic;

namespace Suity.Synchonizing.Core;

/// <summary>
/// Context for validation operations
/// </summary>
public class ValidationContext
{
    public class ReportItem
    {
        public string Message { get; set; }
        public TextStatus Status { get; set; }
        public object Information { get; set; }

        public ReportItem()
        {
        }

        public ReportItem(string message, object information)
        {
            Message = message;
            Information = information;
        }
    }

    public readonly List<ReportItem> Reports = [];

    public void Report(string message)
    {
        Reports.Add(new ReportItem { Message = message });
    }

    public void Report(string message, TextStatus status)
    {
        Reports.Add(new ReportItem { Message = message, Status = status });
    }

    public void Report(string message, object info)
    {
        Reports.Add(new ReportItem { Message = message, Information = info });
    }
}