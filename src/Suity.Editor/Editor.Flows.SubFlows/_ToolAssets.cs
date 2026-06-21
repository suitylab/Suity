using Suity.Drawing;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Synchonizing.Preset;
using Suity.Views;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.SubFlows;

#region ToolCallContext

public record ToolCallContext
{
    public IToolInstance ToolInstance { get; init; }
    public WorkSpace WorkSpace { get; init; }
    public string RootDirectory { get; init; }
    public FunctionContext FuncContext { get; init; }
    public IConversationHandler Conversation { get; init; }
    public CancellationToken Cancellation { get; init; }
}
#endregion

#region IToolAsset

[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Tool Asset", Color = FlowColors.PageGroup, Icon = "*CoreIcon|Tool")]
public interface IToolAsset : IPageAsset
{
    Task<bool> Run(ToolCallContext context);
}


#endregion

#region IToolInstance

public interface IToolInstance : IPageInstance
{
    IToolAsset GetToolAsset();

    IViewObject InputObject { get; }
    IViewObject OutputObject { get; }
    Exception ErrorInfo { get; }
}

#endregion

#region ToolAsset

public abstract class ToolAsset : StandaloneAsset, IToolAsset
{
    protected ToolAsset(bool resolveId = true)
        : base([typeof(IToolAsset), typeof(IPageAsset)], resolveId)
    {
    }

    public override ImageDef DefaultIcon => CoreIconCache.Tool;

    public override ImageDef GetIcon() => CoreIconCache.Tool;

    public abstract IPageInstance CreatePageInstance(PageCreateOption option);
    public abstract Task<bool> Run(ToolCallContext context);
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
    public string FullName => Tool.FullName; // equivalent to Tool.AssetKey
    public ImageDef Icon => Tool.Icon;
    public IToolAsset GetToolAsset() => Tool;

    public abstract IViewObject InputObject { get; }
    public abstract IViewObject OutputObject { get; }
    public abstract Exception ErrorInfo { get; }
    public abstract IConversationHandler Conversation { get; }

    public abstract SimpleType ToSimpleType(FlowDirections direction);

    public abstract bool GetError();
    public abstract string GetErrorMessage();
    public abstract bool? GetIsDone();
    public abstract bool? GetIsDoneInputs();
    public abstract bool? GetIsDoneOutputs();

    public abstract object GetParameter(string name);
    public abstract bool SetParameter(string name, object value);

    public abstract HistoryText GetTaskCommit(ResolveChatIntents intent);
    public abstract TaskCommitParameter GetTaskCommitParameter();

    #endregion

    #region IViewObject

    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
    }

    public virtual void SetupView(IViewObjectSetup setup)
    {
    }


    #endregion

    public abstract void UpdateFromOther(IToolInstance other);
}

#endregion

#region ToolAsset<TInput, TOutput>

public abstract class ToolAsset<TInput, TOutput> : ToolAsset, IHasCategory, IPreviewDisplay
    where TInput : class, IViewObject
    where TOutput : class, IViewObject
{
    private readonly string _category;

    protected ToolAsset(bool resolveId = true)
        : base(resolveId)
    {
        _category = typeof(TInput).GetAttributeCached<NativeTypeAttribute>()?.Category;

        this.PreviewText = typeof(TInput).ToToolTipsText() ?? string.Empty;
    }

    public string Category => _category;

    public object PreviewIcon => null;

    public sealed override IPageInstance CreatePageInstance(PageCreateOption option)
    {
        return new ToolInstance<TInput, TOutput>(option, this);
    }

    public sealed override async Task<bool> Run(ToolCallContext context)
    {
        if (context?.ToolInstance is not ToolInstance<TInput, TOutput> myInstance)
        {
            return false;
        }

        if (myInstance.Tool != this)
        {
            return false;
        }

        try
        {
            var output = await RunTask(myInstance.Input, context);
            if (output != null)
            {
                myInstance.SetOutput(output);
                return true;
            }
            else
            {
                myInstance.SetError(new NullReferenceException("Output is null"));
                return true;
            }
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception error)
        {
            myInstance.SetError(error);

            return true;
        }
    }

    protected abstract Task<TOutput> RunTask(TInput input, ToolCallContext context);
}

#endregion

#region ToolInstance<TInput, TOutput>

public class ToolInstance<TInput, TOutput> : ToolInstance
    where TInput : class, IViewObject
    where TOutput : class, IViewObject
{
    private readonly IConversationImGui _conversation;

    private readonly TInput _input;
    private readonly SimpleType _inputType;

    private TOutput _output;
    private readonly TextBlockProperty _errorMessage = new("Error", "Error Message");

    private Exception _lastError;

    public ToolInstance(PageCreateOption option, ToolAsset tool) : base(option, tool)
    {
        string name = SubFlowExtensions.UseFullName ? tool.FullName : tool.Name;

        _input = Activator.CreateInstance<TInput>();
        _inputType = DTypeManager.Instance.GetViewObjectSimpleType(_input, name, tool.FullName);

        _errorMessage.Text = null;
        _errorMessage.Property.WithOptional();

        _conversation = EditorServices.ImGuiService.CreateConversationImGui(typeof(TInput).Name, false);
    }

    public TInput Input => _input;
    public TOutput Output => _output;

    #region Virtual / Override

    public override IViewObject InputObject => _input;

    public override IViewObject OutputObject => _output;

    public override Exception ErrorInfo => _lastError;

    public override IConversationHandler Conversation => _conversation;


    public override SimpleType ToSimpleType(FlowDirections direction)
    {
        if (direction == FlowDirections.Input)
        {
            return _inputType;
        }
        else
        {
            var output = _output;
            output ??= Activator.CreateInstance<TOutput>();

            if (output != null)
            {
                var outputType = DTypeManager.Instance.GetViewObjectSimpleType(output, output.GetType().Name, output.GetType().FullName);
                return outputType;
            }
            else
            {
                return null;
            }
        }
    }

    public override bool GetError() => _errorMessage.Value?.Text != null;

    public override string GetErrorMessage() => _errorMessage.Text;

    public override bool? GetIsDone() => _output != null;

    public override bool? GetIsDoneInputs() => _input != null;

    public override bool? GetIsDoneOutputs() => _output != null;

    public override object GetParameter(string name)
    {
        return _input.GetProperty(name) ?? _output?.GetProperty(name);
    }

    public override bool SetParameter(string name, object value)
    {
        var field = _inputType.Fields.FirstOrDefault(o => o.Name == name);
        if (field is null)
        {
            return false;
        }

        if (value != null)
        {
            var valueType = TypeDefinition.ResolveNative(value);
            if (!TypeDefinition.IsNullOrEmpty(valueType))
            {
                var result = EditorServices.TypeConvertService.TryConvert(valueType, field.Type, false, value);
                if (result.State == TypeConvertState.Unconvertible)
                {
                    return false;
                }

                value = result.To;
            }
        }

        var v = SItem.ResolveObject(value);
        if (v is Array ary)
        {
            v = SyncList.CreateReadonly(ary);
        }

        Cloner.CloneOneProperty(name, v, _input);

        return true;
    }

    public override HistoryText GetTaskCommit(ResolveChatIntents intent)
    {
        var writer = new XmlNodeWriter("ToolCall", false);
        writer.SetAttribute("name", Tool.Name);
        if (!string.IsNullOrWhiteSpace(_errorMessage.Text))
        {
            writer.SetElement("Error", w => w.SetValue(_errorMessage.Text));
        }

        if (_output is { } output)
        {
            Serializer.Serialize(output, writer);
        }

        XmlNodeWriter.ConvertEscapedTextToCData(writer.GetDocument());

        return writer.ToString();
    }

    public override TaskCommitParameter GetTaskCommitParameter()
    {
        if (GetError())
        {
            return new(TaskCommitStatus.TaskFailed, _errorMessage.Text);
        }
        else if (_output != null)
        {
            return new(TaskCommitStatus.TaskFinished, _output);
        }
        else
        {
            return new(TaskCommitStatus.None, null);
        }
    }

    #endregion

    #region Virtual (Sync)

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        sync.Sync("Input", _input, SyncFlag.GetOnly);
        _output = sync.Sync("Output", _output);
        _errorMessage.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        base.SetupView(setup);

        setup.InspectorField(_input, new ViewProperty("Input").WithExpand());
        setup.InspectorField(_output, new ViewProperty("Output").WithExpand().WithOptional());
        _errorMessage.InspectorField(setup);
    }

    #endregion

    public override void UpdateFromOther(IToolInstance other)
    {
        Cloner.CloneProperty(other.InputObject, _input);
        _output = Cloner.Clone(other.OutputObject) as TOutput;
    }

    public void SetOutput(TOutput output)
    {
        _output = output;
        _lastError = null;
        _errorMessage.Text = null;
    }

    public void SetError(Exception error)
    {
        _lastError = error ?? new Exception("Unknown error");
        _errorMessage.Text = _lastError.ToString() ?? "Unknown error";
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
        if (!string.IsNullOrWhiteSpace(tooltips))
        {
            desc = $" description='{tooltips}'";
        }

        return desc;
    }
}

#endregion

#region ToolCommand

public abstract class ToolCommand<TOutput> : IViewObject
    where TOutput : class, IViewObject
{
    public virtual void Sync(IPropertySync sync, ISyncContext context) { }
    public virtual void SetupView(IViewObjectSetup setup) { }

    public abstract Task<TOutput> Run(ToolCallContext context);
}

#endregion