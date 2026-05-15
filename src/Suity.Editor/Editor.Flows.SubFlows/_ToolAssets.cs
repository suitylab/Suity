using Suity.Drawing;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.SubFlows;

#region IToolAsset

[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Tool Asset", Color = FlowColors.Page, Icon = "*CoreIcon|Tool")]
public interface IToolAsset : IPageAsset
{
    Task<bool> RunTask(IToolInstance toolInstance, CancellationToken cancellation);
}


#endregion

#region IToolInstance

public interface IToolInstance : IPageInstance
{
}

#endregion

#region ToolAsset

public abstract class ToolAsset : StandaloneAsset<IToolAsset>, IToolAsset
{
    protected ToolAsset()
    {
    }

    public override ImageDef DefaultIcon => CoreIconCache.Tool;

    public override ImageDef GetIcon() => CoreIconCache.Tool;

    public abstract IPageInstance CreatePageInstance(PageCreateOption option);
    public abstract Task<bool> RunTask(IToolInstance toolInstance, CancellationToken cancellation);
}

#endregion

#region ToolInstance

public abstract class ToolInstance : IToolInstance
{
    public PageCreateOption Option { get; }
    public ToolAsset Tool { get; }

    protected ToolInstance(PageCreateOption option, ToolAsset tool)
    {
        Option = option ?? throw new ArgumentNullException(nameof(option));
        Tool = tool ?? throw new ArgumentNullException(nameof(tool));
    }

    #region IToolInstance

    public object Owner => Option.Owner;
    public string Name => Tool.Name;

    public abstract SimpleType ToSimpleType();

    public abstract bool? GetAllDone();
    public abstract bool? GetIsDone();
    public abstract bool? GetIsDoneInputs();
    public abstract bool? GetIsDoneOutputs();

    public abstract bool SetParameter(string name, object value);

    public abstract HistoryText GetTaskCommit();
    public abstract TaskCommitInfo GetTaskCommitInfo();

    #endregion
}

#endregion

#region ToolAsset<TInput, TOutput>

public abstract class ToolAsset<TInput, TOutput> : ToolAsset
    where TInput : IViewObject
    where TOutput : IViewObject
{
}

#endregion

#region ToolInstance<TInput, TOutput>

public class ToolInstance<TInput, TOutput> : ToolInstance
    where TInput : IViewObject
    where TOutput : IViewObject
{
    public ToolInstance(PageCreateOption option, ToolAsset tool) : base(option, tool)
    {
    }

    public override SimpleType ToSimpleType()
    {
        throw new NotImplementedException();
    }

    public override bool? GetAllDone() => GetIsDone();

    public override bool? GetIsDone()
    {
        throw new NotImplementedException();
    }

    public override bool? GetIsDoneInputs()
    {
        throw new NotImplementedException();
    }

    public override bool? GetIsDoneOutputs()
    {
        throw new NotImplementedException();
    }

    public override bool SetParameter(string name, object value)
    {
        throw new NotImplementedException();
    }


    public override HistoryText GetTaskCommit()
    {
        throw new NotImplementedException();
    }

    public override TaskCommitInfo GetTaskCommitInfo()
    {
        throw new NotImplementedException();
    }

}

#endregion