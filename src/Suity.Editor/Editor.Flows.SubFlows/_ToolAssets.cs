using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Text;
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
    IViewObject InputObject { get; }
    IViewObject OutputObject { get; }
    Exception ErrorInfo { get; }
}

#endregion

#region ToolAsset

public abstract class ToolAsset : StandaloneAsset<IToolAsset>, IToolAsset
{
    protected ToolAsset()
        : base(true, [typeof(IPageAsset)])
    {
    }

    public override ImageDef DefaultIcon => CoreIconCache.Tool;

    public override ImageDef GetIcon() => CoreIconCache.Tool;

    public abstract IPageInstance CreatePageInstance(PageCreateOption option);
    public abstract Task<bool> RunTask(IToolInstance toolInstance, CancellationToken cancellation);
}

#endregion

#region ToolInstance

public abstract class ToolInstance : IToolInstance, IViewObject
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

    public abstract IViewObject InputObject { get; }
    public abstract IViewObject OutputObject { get; }
    public abstract Exception ErrorInfo { get; }

    public abstract SimpleType ToSimpleType();

    public abstract bool? GetAllDone();
    public abstract bool? GetIsDone();
    public abstract bool? GetIsDoneInputs();
    public abstract bool? GetIsDoneOutputs();

    public abstract bool SetParameter(string name, object value);

    public abstract HistoryText GetTaskCommit();
    public abstract TaskCommitInfo GetTaskCommitInfo();

    #endregion

    #region IViewObject

    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
    }

    public virtual void SetupView(IViewObjectSetup setup)
    {
    }


    #endregion
}

#endregion

#region ToolAsset<TInput, TOutput>

public abstract class ToolAsset<TInput, TOutput> : ToolAsset
    where TInput : class, IViewObject, new()
    where TOutput : class, IViewObject
{
    public sealed override IPageInstance CreatePageInstance(PageCreateOption option)
    {
        return new ToolInstance<TInput, TOutput>(option, this);
    }

    public sealed override async Task<bool> RunTask(IToolInstance toolInstance, CancellationToken cancellation)
    {
        if (toolInstance is not ToolInstance<TInput, TOutput> myInstance)
        {
            return false;
        }

        if (myInstance.Tool != this)
        {
            return false;
        }

        try
        {
            var output = await RunTask(myInstance.Input, cancellation);
            if (output != null)
            {
                myInstance.SetOutput(output);
                return true;
            }
            else
            {
                myInstance.SetError(new NullReferenceException("Output is null"));
                return false;
            }
        }
        catch (Exception error)
        {
            myInstance.SetError(error);

            return false;
        }
    }

    protected abstract Task<TOutput> RunTask(TInput input, CancellationToken cancellation);
}

#endregion

#region ToolInstance<TInput, TOutput>

public class ToolInstance<TInput, TOutput> : ToolInstance
    where TInput : class, IViewObject, new()
    where TOutput : class, IViewObject
{
    private readonly TInput _input;
    private TOutput _output;
    private Exception _errorInfo;

    public ToolInstance(PageCreateOption option, ToolAsset tool) : base(option, tool)
    {
        _input = new();
    }

    public TInput Input => _input;
    public TOutput Output => _output;

    #region Virtual / Override

    public override IViewObject InputObject => _input;

    public override IViewObject OutputObject => _output;

    public override Exception ErrorInfo => _errorInfo;

    
    public override SimpleType ToSimpleType()
    {
        return EditorServices.JsonSchemaService.GetViewObjectSimpleType(_input);
    }

    public override bool? GetAllDone() => GetIsDone();

    public override bool? GetIsDone() => _output != null;

    public override bool? GetIsDoneInputs() => _input != null;

    public override bool? GetIsDoneOutputs() => _output != null;

    public override bool SetParameter(string name, object value)
    {
        _input.SetProperty(name, value);
        return true;
    }


    public override HistoryText GetTaskCommit()
    {
        if (_output is not { } output)
        {
            return HistoryText.Empty;
        }

        var simpleType = EditorServices.JsonSchemaService.GetViewObjectSimpleType(output);

        var sync = new GetAllPropertySync(SyncIntent.Serialize, false);
        output.Sync(sync, SyncContext.Empty);

        var builder = new StringBuilder();

        foreach (var field in simpleType.Fields)
        {
            if (!sync.Values.TryGetValue(field.Name, out var value))
            {
                continue;
            }

            string attr = ResolveElementXmlAttr(value, field);
            builder.AppendLine($"<{simpleType.Name}{attr}>");

            try
            {
                var text = SubFlowExtensions.ConvertChatHistoryText(field.Type, value, true);
                builder.AppendLine(text.Text);
            }
            catch (Exception)
            {
                builder.AppendLine("---");
            }
            builder.AppendLine($"</{simpleType.Name}>");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    public override TaskCommitInfo GetTaskCommitInfo()
    {
        if (_output != null)
        {
            return new(TaskCommitTypes.TaskFinished, _output);
        }
        else
        {
            return new(TaskCommitTypes.None, null);
        }
    }
    #endregion

    #region Virtual (Sync)

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        sync.Sync("Input", _input, SyncFlag.GetOnly);
        _output = sync.Sync("Output", _output);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        setup.InspectorField(_input, new ViewProperty("Input").WithExpand());
        setup.InspectorField(_output, new ViewProperty("Output").WithExpand());
    }

    #endregion

    public void SetOutput(TOutput output)
    {
        _output = output;
        _errorInfo = null;
    }

    public void SetError(Exception error)
    {
        _errorInfo = error;
        _output = null;
    }

    private string ResolveElementXmlAttr(object value, SimpleField field)
    {
        if (value is null)
        {
            return string.Empty;
        }

        string tooltips = field.Tooltips;
        if (string.IsNullOrWhiteSpace(tooltips))
        {
            tooltips = field.Description;
        }

        string desc = string.Empty;
        if (string.IsNullOrWhiteSpace(tooltips))
        {
            desc = $" description='{tooltips}'";
        }

        return desc;
    }
}

#endregion