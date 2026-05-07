using Suity.Drawing;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.AIGC;

#region GetStructDefinition

/// <summary>
/// Node that retrieves the JSON Schema definition text for a struct.
/// </summary>
[DisplayText("Get Struct Definition Text", "*CoreIcon|Structure")]
[ToolTipsText("Get standard Json Schema description text for the struct.")]
[NativeAlias("Suity.Editor.AIGC.Flows.GetStructDefinition")]
public class GetStructDefinition : AigcFlowNode
{
    private readonly FlowNodeConnector _out;

    private readonly AssetSelection<DStruct> _struct = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetStructDefinition"/> class.
    /// </summary>
    public GetStructDefinition()
    {
        _out = AddDataOutputConnector("Out", "string", "Text");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Structs", _struct, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_struct, new ViewProperty("Structs", "Structure").WithWriteBack());
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var type = _struct.Target;
        if (type is null)
        {
            compute.SetValue(_out, string.Empty);
            return;
        }

        string str = type.ToSchemaOverview(compute.Context);

        compute.SetValue(_out, str);
    }
}

#endregion

#region GetStructsDefinition

/// <summary>
/// Node that retrieves the JSON Schema definition text for multiple structs.
/// </summary>
[DisplayText("Get Multiple Struct Definitions Text", "*CoreIcon|Structure")]
[ToolTipsText("Get standard Json Schema description text for multiple structs")]
[NativeAlias("Suity.Editor.AIGC.Flows.GetStructsDefinition")]
public class GetStructsDefinition : AigcFlowNode
{
    private readonly FlowNodeConnector _out;

    private readonly List<AssetSelection<DStruct>> _structs = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetStructsDefinition"/> class.
    /// </summary>
    public GetStructsDefinition()
    {
        _out = AddDataOutputConnector("Out", "string", "Text");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Structs", _structs, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_structs, new ViewProperty("Structs", "Structure").WithWriteBack());
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        var types = _structs.Select(o => o.Target);
        string str = types.ToSchemaOverview(compute.Context);

        compute.SetValue(_out, str);
    }
}

#endregion

#region ManualSelectAsset

/// <summary>
/// Node that allows manual selection of an asset through the UI.
/// </summary>
[DisplayText("Manual Select Asset", "*CoreIcon|Asset")]
[NativeAlias("Suity.Editor.AIGC.Flows.ManualSelectAssetNode", false)]
[NativeAlias("Suity.Editor.AIGC.Flows.ManualSelectAsset")]
//TODO: UI dialog now uses async mode, implementing with Coroutine will cause issues.
[NotAvailable]
public class ManualSelectAsset : AigcFlowNode
{
    private readonly Selection _typeSelection;

    private FlowNodeConnector _in;
    private readonly ConnectorTextBlockProperty _message = new("Message", "Message");

    private FlowNodeConnector _opened;
    private FlowNodeConnector _out;
    private FlowNodeConnector _creating;
    private FlowNodeConnector _failed;

    private readonly ValueProperty<bool> _createEnabled
        = new("CreateEnabled", "Allow Create", true);

    private readonly ValueProperty<bool> _openEnabled
        = new("OpenEnabled", "Allow Open", true);

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualSelectAsset"/> class.
    /// </summary>
    public ManualSelectAsset()
    {
        _typeSelection = new(AssetTypeSelectionList.Instance);

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("AssetType", _typeSelection, SyncFlag.GetOnly);

        _message.Sync(sync);
        _createEnabled.Sync(sync);
        _openEnabled.Sync(sync);

        if (sync.IsSetter("AssetType"))
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_typeSelection, new ViewProperty("AssetType", "Asset Type"));

        _message.InspectorField(setup, this);
        _createEnabled.InspectorField(setup);
        _openEnabled.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var assetLink = _typeSelection.SelectedItem as DAssetLink;

        var typeDef = TypeDefinition.FromNative(assetLink?.AssetType);

        _in = AddActionInputConnector("In", "Input");
        _message.AddConnector(this);
        _opened = AddActionOutputConnector("Opened", "Opened");
        _out = AddDataOutputConnector("Out", typeDef ?? TypeDefinition.Unknown, "Opened Asset");
        _creating = AddActionOutputConnector("Created", "Request Create");
        _failed = AddActionOutputConnector("Failed", "Failed");
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var conversation = compute.Context.GetArgument<IConversationHandler>()
            ?? throw new NullReferenceException($"{nameof(IConversationHandler)} not found.");

        var assetLink = _typeSelection.SelectedItem as DAssetLink;
        var type = (assetLink?.AssetType)
            ?? throw new NullReferenceException($"Asset type not found.");

        string msg = _message.GetText(compute, this) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = "Please select an asset";
        }

        cancel.ThrowIfCancellationRequested();

        // Create passive task
        var source = new TaskCompletionSource<Tuple<object, bool>>();
        // Register for cancellation
        var cancelReg = cancel.Register(() => source.TrySetCanceled());

        conversation.StartCoroutine(DialogCoroutine(conversation, type, msg, (p, n) => source.SetResult(new(p, n))));

        var result = await source.Task;

        cancelReg.Dispose();

        compute.SetValue(_out, result.Item1);
        if (result.Item2)
        {
            return _creating;
        }
        else
        {
            if (result.Item1 != null)
            {
                return _opened;
            }
            else
            {
                return _failed;
            }
        }
    }

    /// <summary>
    /// Coroutine that displays a dialog for selecting or creating an asset.
    /// </summary>
    /// <param name="conversation">The conversation handler.</param>
    /// <param name="assetType">The type of asset to select.</param>
    /// <param name="msg">The message to display.</param>
    /// <param name="setter">Action to set the result.</param>
    private IEnumerator DialogCoroutine(IConversationHandler conversation, Type assetType, string msg, Action<object, bool> setter)
    {
        var dialogItem = conversation.AddDebugMessage(msg, o =>
        {
            List<ConversationButton> btns = [];

            if (_createEnabled.Value)
            {
                btns.Add(new ConversationButton { Key = "Create", Text = "Create" });
            }

            if (_openEnabled.Value)
            {
                btns.Add(new ConversationButton { Key = "Select", Text = "Select" });
            }

            o.AddButtons(string.Empty, [.. btns]);
        });

    label_dialog_01:
        {
            yield return null;

            switch (conversation.InputButton)
            {
                case "Select":
                    goto label_select_item;

                case "Create":
                    goto label_create_doc;

                default:
                    yield break;
            }
        }

    label_select_item:
        {
            var list = AssetManager.Instance.GetAssetSelectionList(assetType);
            var result = list.ShowSelectionGUI("Please select a plan");
            if (!result.IsSuccess)
            {
                goto label_dialog_01;
            }

            // Remove this dialog, user cannot continue clicking to select.
            if (dialogItem != null)
            {
                dialogItem?.Dispose();
                conversation.AddDebugMessage(msg);
                conversation.AddUserMessage("Selected: " + result.Item?.ToDisplayText() ?? "None");
                dialogItem = null;
            }

            setter(result.Item, false);

            yield break;
        }

    label_create_doc:
        {
            setter(null, true);
        }
    }

    /// <inheritdoc/>
    public override ImageDef Icon => _typeSelection.SelectedItem?.ToDisplayIcon() ?? base.Icon;

    /// <inheritdoc/>
    public override string ToString()
    {
        var typeName = _typeSelection.SelectedItem?.ToDisplayText();
        if (!string.IsNullOrWhiteSpace(typeName))
        {
            return $"Manual Select {typeName}";
        }
        else
        {
            return base.ToString();
        }
    }
}

#endregion
