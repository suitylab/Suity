using Suity.Editor.AIGC.RAG;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Flows;

/// <summary>
/// Defines the operational mode for a switchable canvas node.
/// </summary>
public enum CanvasSwitchableMode
{
    /// <summary>
    /// No special mode is active.
    /// </summary>
    [DisplayText("None")]
    None,

    /// <summary>
    /// Enables data transport functionality.
    /// </summary>
    [DisplayText("Data transport")]
    DataTransport,

    /// <summary>
    /// Enables knowledge base functionality.
    /// </summary>
    [DisplayText("Knowledge base")]
    KnowledgeBase,
}


/// <summary>
/// A canvas node that can switch between different operational modes, such as data transport or knowledge base.
/// </summary>
/// <typeparam name="TAsset">The type of asset this node represents.</typeparam>
public class CanvasSwitchableNode<TAsset> : CanvasAssetNode<TAsset>, 
    ICanvasSwitchableNode, 
    IKnowledgeBase,
    IVectorKnowledge,
    IFeatureKnowledge
     where TAsset : Asset
{
    /// <summary>
    /// The default title displayed for an unconnected data transport input.
    /// </summary>
    public const string DEFAULT_TRANSPORT_TITLE = "---";


    static readonly GuiDropDownValue _modeDropDown
        = GuiDropDownValue.FromEnumType<CanvasSwitchableMode>();

    readonly ValueProperty<CanvasSwitchableMode> _mode = new(nameof(Mode), "Mode", CanvasSwitchableMode.None);


    /// <summary>
    /// Gets the current operational mode of this node.
    /// </summary>
    public CanvasSwitchableMode Mode => _mode.Value;


    /// <summary>
    /// Gets the connector for data transport input.
    /// </summary>
    protected FixedNodeConnector DataTransportConnector { get; }

    /// <summary>
    /// Gets the connector for knowledge base input.
    /// </summary>
    protected FixedNodeConnector KnowledgeInConnector { get; }

    /// <summary>
    /// Gets the connector for knowledge base output.
    /// </summary>
    protected FixedNodeConnector KnowledgeOutConnector { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasSwitchableNode{TAsset}"/> class.
    /// </summary>
    public CanvasSwitchableNode()
    {
        base.EditorGui = DrawEditorGui;

        DataTransportConnector = FixedNodeConnector.CreateControlInput("DataIn", TypeDefinition.FromNative<IDataTransport>(), DEFAULT_TRANSPORT_TITLE);
        KnowledgeInConnector = FixedNodeConnector.CreateDataInput("KnowledgeIn", TypeDefinition.FromNative<IKnowledgeBase>(), "Knowledge Input");
        KnowledgeOutConnector = FixedNodeConnector.CreateDataOutput("KnowledgeOut", TypeDefinition.FromNative<IKnowledgeBase>(), "Knowledge Output");

        _mode.ValueChanged += (s, e) => UpdateConnectorQueued();

        KnowledgeInputs = [];
    }

    #region Sync & Connector

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _mode.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(Views.IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _mode.InspectorField(setup);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        switch (_mode.Value)
        {
            case CanvasSwitchableMode.DataTransport:
                AddConnector(DataTransportConnector);
                break;

            case CanvasSwitchableMode.KnowledgeBase:
                AddConnector(KnowledgeInConnector);
                AddConnector(KnowledgeOutConnector);
                break;
        }
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        if (DataTransportConnector.ParentNode != null)
        {
            DataTransport = compute.GetValue<IDataTransport>(DataTransportConnector);
        }

        if (KnowledgeInConnector.ParentNode != null)
        {
            KnowledgeInputs = compute.GetValues<IKnowledgeBase>(KnowledgeInConnector, true);
        }
        
        if (KnowledgeOutConnector.ParentNode != null)
        {
            compute.SetValue(KnowledgeOutConnector, this);
        }
    }
    #endregion

    /// <summary>
    /// Gets the currently connected data transport instance.
    /// </summary>
    protected IDataTransport? DataTransport { get; private set; }

    /// <summary>
    /// Gets the array of knowledge base inputs connected to this node.
    /// </summary>
    protected IKnowledgeBase[] KnowledgeInputs { get; private set; }


    /// <summary>
    /// Gets the data transport object connected to this node, or null if none is connected.
    /// </summary>
    /// <returns>The connected <see cref="IDataTransport"/>, or null.</returns>
    public IDataTransport? GetDataTransport() => DataTransport;

    /// <summary>
    /// Gets the knowledge base inputs connected to this node.
    /// </summary>
    /// <returns>An array of <see cref="IKnowledgeBase"/> instances, or an empty array if none are connected.</returns>
    public IKnowledgeBase[] GetKnowledgeInputs() => KnowledgeInputs?.ToArray() ?? [];

    
    /// <summary>
    /// Override to provide the object to transport.
    /// </summary>
    /// <returns>The object to transport, or null.</returns>
    protected virtual object? GetTransportObject() => null;

    /// <summary>
    /// Override to provide the knowledge base implementation for this node.
    /// </summary>
    /// <returns>The attached <see cref="IKnowledgeBase"/>, or null.</returns>
    protected virtual IKnowledgeBase? GetAttachedKnowledge()
    {
        if (TargetAsset is not { } asset)
        {
            return null;
        }

        if (asset is IKnowledgeBase knowledgeBase)
        {
            return knowledgeBase;
        }

        return asset.GetAttachedAssets().OfType<IKnowledgeBase>().FirstOrDefault();
    }

    #region ImGui

    /// <inheritdoc/>
    protected override bool DrawEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        base.DrawEditorGui(gui, pipeline, context);

        switch (pipeline)
        {
            case EditorImGuiPipeline.Preview:
                {
                    gui.Button("#config", ImGuiIcons.Switch)
                    .InitClass("configBtn")
                    .InitInputFunctionChain(ConfigInput);
                }
                return true;

            case EditorImGuiPipeline.Input:
                {
                    var connnectors = this.Connectors.Where(o => o.Direction == FlowDirections.Input);
                    foreach (var connector in connnectors)
                    {
                        if (connector == DataTransportConnector)
                        {
                            NodeGraphExtensions.FlowConnectorRow(gui, connector, context, true, _ => DrawPlayButton(gui, connector));
                        }
                        else
                        {
                            NodeGraphExtensions.FlowConnectorRow(gui, connector, context, true);
                        }
                    }
                }
                return true;

            case EditorImGuiPipeline.Output:
                {
                    var connnectors = this.Connectors.Where(o => o.Direction == FlowDirections.Output);
                    foreach (var connector in connnectors)
                    {
                        NodeGraphExtensions.FlowConnectorRow(gui, connector, context, true);
                    }
                }
                return true;
        }

        return false;
    }

    /// <summary>
    /// Draws the play button for the data transport connector, allowing manual data push.
    /// </summary>
    /// <param name="gui">The ImGui instance for rendering.</param>
    /// <param name="connector">The flow node connector associated with this button.</param>
    protected virtual void DrawPlayButton(ImGui gui, FlowNodeConnector connector)
    {
        var obj = GetTransportObject();

        gui.Button("#active", ImGuiIcons.Play)
        .InitClass("configBtn")
        .SetToolTipsL("Push data")
        .OverrideSize(8, 8)
        .SetEnabled(obj != null && IsExpanded)
        .OnClick(() =>
        {
            if (obj is null)
            {
                return;
            }

            try
            {
                if (DataTransport is { } dataTransport)
                {
                    dataTransport.SendData(obj, 0);
                    connector.FlashingOnce();
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }

            //gui.QueueRefresh();
            //gui.RequestOutput();
        });
    }

    /// <summary>
    /// Handles input for the mode configuration dropdown button.
    /// </summary>
    /// <param name="pipeline">The current GUI pipeline stage.</param>
    /// <param name="node">The ImGui node receiving input.</param>
    /// <param name="input">The graphic input event.</param>
    /// <param name="baseAction">The base input function to call.</param>
    /// <returns>The resulting input state.</returns>
    protected virtual GuiInputState ConfigInput(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var state = baseAction(pipeline);

        if (node.IsDisabled)
        {
            return GuiInputState.None;
        }

        switch (input.EventType)
        {
            case GuiEventTypes.MouseUp:
                if (node.GetIsClicked() && !node.IsReadOnly && node.Gui.Context is IGraphicDropDownEdit dropDownEdit)
                {
                    var rect = node.GlobalRect;
                    var dropDownRect = new RectangleF(rect.X, rect.Bottom, 150, 100);

                    var currentValue = _modeDropDown.Items.FirstOrDefault(o => (CanvasSwitchableMode)o.Value == _mode.Value);

                    dropDownEdit.ShowComboBoxDropDown(dropDownRect.ToInt(), _modeDropDown.Items.OfType<object>(), currentValue, obj =>
                    {
                        var item = (GuiDropDownItem)obj;
                        var value = (CanvasSwitchableMode)item.Value;

                        var action = new FlowNodeSetterAction(this, nameof(Mode), value);
                        if (this.GetInspectorContext() is { } context)
                        {
                            context.InspectorDoAction(action);
                            context.InspectorEditFinish();

                            NodeGraphExtensions.QueueRefresh(this);
                        }
                    });
                }

                ImGui.MergeState(ref state, GuiInputState.None);
                break;

            default:
                ImGui.MergeState(ref state, GuiInputState.None);
                break;
        }

        return state;
    }


    #endregion

    #region IKnowledgeBase

    /// <inheritdoc/>
    Task<string[]> IKnowledgeBase.GetDBInfos(CancellationToken cancel)
    {
        return GetAttachedKnowledge()?.GetDBInfos(cancel)
            ?? Task.FromResult<string[]>([]);
    }


    /// <inheritdoc/>
    Task IKnowledgeBase.ClearKnowledge(KnowledgeTypes type, CancellationToken cancel)
    {
        return GetAttachedKnowledge()?.ClearKnowledge(type)
            ?? Task.CompletedTask;
    }

    #endregion

    #region IVectorKnowledge

    /// <inheritdoc/>
    Task<RagQueryResult[]> IVectorKnowledge.GetAllVectorDocuments(CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IVectorKnowledge)?.GetAllVectorDocuments(cancel)
            ?? Task.FromResult(new RagQueryResult[0]);
    }


    // The reason for implementing IVectorRAG is that changes to knowledge base documents cannot be passed to nodes,
    // resulting in failure to notify other nodes to recalculate. Therefore, IVectorRAG is always implemented,
    // and judgment is made based on the actual interface return value obtained.
    /// <inheritdoc/>
    Task<RagQueryResult[]> IVectorKnowledge.QueryVectorDocuments(string query, int? topk, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IVectorKnowledge)?.QueryVectorDocuments(query, topk, cancel)
            ?? Task.FromResult(new RagQueryResult[0]);
    }

    /// <inheritdoc/>
    Task<RagQueryResult[]> IVectorKnowledge.QueryVectorDocuments(IEnumerable<string> ids, string query, int? topk, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IVectorKnowledge)?.QueryVectorDocuments(ids, query, topk, cancel)
            ?? Task.FromResult(new RagQueryResult[0]);
    }



    #endregion

    #region IFeatureKnowledge

    /// <inheritdoc/>
    IVectorKnowledge? IFeatureKnowledge.GetBaseVectorRAG(CancellationToken cancel)
    {
        if (GetAttachedKnowledge() is IVectorKnowledge v)
        {
            return v;
        }

        if (GetAttachedKnowledge() is IFeatureKnowledge)
        {
            return GetKnowledgeInputs().OfType<IVectorKnowledge>().FirstOrDefault();
        }

        return null;
    }

    /// <inheritdoc/>
    Task<FeatureQueryResult?> IFeatureKnowledge.GetFeature(string tag, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.GetFeature(tag)
            ?? Task.FromResult<FeatureQueryResult?>(null);
    }

    /// <inheritdoc/>
    Task<string[]> IFeatureKnowledge.GetEntitiesByType(string type, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.GetEntitiesByType(type, cancel)
            ?? Task.FromResult<string[]>([]);
    }

    /// <inheritdoc/>
    Task<int> IFeatureKnowledge.GetEntityCountByType(string type, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.GetEntityCountByType(type, cancel)
            ?? Task.FromResult<int>(0);
    }

    /// <inheritdoc/>
    Task<string[]> IFeatureKnowledge.GetAllEntityTypes(CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.GetAllEntityTypes(cancel)
            ?? Task.FromResult<string[]>([]);
    }

    /// <inheritdoc/>
    Task<FeatureQueryResult[]> IFeatureKnowledge.EnumerateFeature(string query, int? topk, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.EnumerateFeature(query, topk, cancel)
            ?? Task.FromResult(new FeatureQueryResult[0]);
    }

    /// <inheritdoc/>
    Task<FeatureQueryResult[]> IFeatureKnowledge.GetFeatureByTag(string tag, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.GetFeatureByTag(tag, cancel)
            ?? Task.FromResult(new FeatureQueryResult[0]);
    }

    /// <inheritdoc/>
    Task<string[]> IFeatureKnowledge.GetFeatureSourceRefIds(string tag, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.GetFeatureSourceRefIds(tag, cancel)
            ?? Task.FromResult(new string[0]);
    }

    /// <inheritdoc/>
    Task<EntityQueryResult> IFeatureKnowledge.GetEntity(string tag, CancellationToken cancel)
    {
        return (GetAttachedKnowledge() as IFeatureKnowledge)?.GetEntity(tag, cancel)
          ?? Task.FromResult<EntityQueryResult>(null);
    }


    #endregion
}
