using Suity.Views;
using System.Collections.Generic;

namespace Suity.Synchonizing.Core;

public class ReportList : ISyncList, INavigable, ITextDisplay
{
    public object Owner { get; }

    public List<object> List { get; } = [];

    public ReportList()
    {
    }

    public ReportList(object owner)
    {
        Owner = owner;
    }

    int ISyncList.Count => List.Count;

    public string DisplayText { get; set; }

    public object DisplayIcon { get; set; }

    public TextStatus DisplayStatus { get; set; }

    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIList(List);
    }

    public override string ToString()
    {
        return Owner?.ToString() ?? string.Empty;
    }

    object INavigable.GetNavigationTarget()
    {
        return Owner;
    }
}

public class ReportItem : ObjectLogItem, ITextDisplay
{
    public string DisplayText => Message;

    public object DisplayIcon { get; set; }

    public TextStatus DisplayStatus { get; set; }

    public ReportItem()
    {
    }

    public ReportItem(string message, object obj)
        : base(message, obj)
    {
    }
}

public class SyncPathReportItem
{
    public readonly object Owner;

    public readonly SyncPath Path;

    public readonly object Info;

    public readonly string Message;

    public SyncPathReportItem(object owner, SyncPath path)
    {
        Owner = owner;
        Path = path ?? SyncPath.Empty;
    }

    public SyncPathReportItem(object owner, SyncPath path, object info, string message)
    {
        Owner = owner;
        Path = path ?? SyncPath.Empty;
        Info = info;
        Message = message;
    }

    public override string ToString()
    {
        if (Path != null)
        {
            if (!string.IsNullOrEmpty(Message))
            {
                //return Message;
                return $"{Message}\r\n{Owner}|{Path}";
            }
            else
            {
                return $"{Owner}|{Path}";
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(Message))
            {
                //return Message;
                return $"{Message}\r\n{Owner}";
            }
            else
            {
                return $"{Owner}";
            }
        }
    }
}