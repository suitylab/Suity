using Suity.Drawing;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows.Pages;

/// <summary>
/// Represents a flow node that references a page definition asset and enables page execution within a flow.
/// </summary>
[NotAvailable]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageFunctionNode")]
public class PageFunctionNode : AssetRefFlowNode<SubFlowDefinitionAsset>, IFlowNodeComputeAsync
{
    private SubFlowInstance _instance;

    /// <summary>
    /// Gets or sets the page instance associated with this node.
    /// </summary>
    public SubFlowInstance Instance
    {
        get => _instance;
        set
        {
            if (ReferenceEquals(_instance, value))
            {
                return;
            }

            if (_instance != null)
            {
                _instance.ResultOutput -= _rootElement_ResultOutput;
                _instance.RefreshRequesting -= _rootElement_RefreshRequesting;
            }

            _instance = value;

            if (_instance != null)
            {
                _instance.ResultOutput += _rootElement_ResultOutput;
                _instance.RefreshRequesting += _rootElement_RefreshRequesting;
            }
        }
    }

    private void _rootElement_ResultOutput(object sender, EventArgs e)
    {
        QueueRefreshView();
    }

    private void _rootElement_RefreshRequesting(object sender, EventArgs e)
    {
        QueueRefreshView();
    }

    /// <summary>
    /// Gets the page definition diagram item associated with this node.
    /// </summary>
    public SubFlowDefinitionDiagramItem Page => GetTargetObject() as SubFlowDefinitionDiagramItem;

    /// <summary>
    /// Initializes a new instance of <see cref="PageFunctionNode"/>.
    /// </summary>
    public PageFunctionNode()
    {        
        // Does not occupy document editing, only for display.
        base.OccupyDocumentUsage = false;
        UpdateConnectorQueued();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PageFunctionNode"/> with the specified asset.
    /// </summary>
    /// <param name="asset">The page definition asset to associate with this node.</param>
    public PageFunctionNode(SubFlowDefinitionAsset asset)
        : base(asset)
    {
        // Does not occupy document editing, only for display.
        base.OccupyDocumentUsage = false;
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    public override ImageDef Icon => Target?.Icon ?? CoreIconCache.Function;

    /// <inheritdoc/>
    public override string DisplayText
    {
        get
        {
            var target = this.Target;
            return target?.DisplayText ?? base.DisplayText;
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        EnsureInstance()?.UpdateConnector(this);
    }

    /// <inheritdoc/>
    protected override void OnAssetTargetUpdated()
    {
        base.OnAssetTargetUpdated();

        QueuedAction.Do(() => 
        {
            BuildInstance();
            UpdateConnectorQueued();
        });
    }


    /// <inheritdoc/>
    protected internal override void OnLoaded()
    {
        base.OnLoaded();

        // After the document is finally loaded and relationships are fully established, rebuild a Page here
        BuildInstance();
        UpdateConnectorQueued();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        //sync.Sync("Page", RootElement, SyncFlag.GetOnly);
        EnsureInstance()?.Sync(sync, context);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        EnsureInstance()?.SetupView(setup);
    }

    /// <inheritdoc/>
    protected override void OnNavigationButtonClicked(IHasSubDocumentView subDocView)
    {
        do
        {
            if (Page?.GetDocument() is not { } pageDoc)
            {
                break;
            }

            if (this.GetFlowDocument() is not { } myDoc)
            {
                break;
            }

            subDocView ??= myDoc.View?.GetService<IHasSubDocumentView>();
            if (subDocView is null)
            {
                break;
            }

            if (subDocView.CurrentSubView is not IFlowView currentView)
            {
                break;
            }

            if (currentView.GetViewNode(this.Name) is not { } viewNode || viewNode.Node != this)
            {
                break;
            }

            if (subDocView.OpenSubView(pageDoc) is not { } view)
            {
                break;
            }

            if (view is IFlowView flowView)
            {
                var outerComputation = currentView.Computation;
                var state = outerComputation?.GetNodeRunningState(this);
                var innerComputation = state?.Tag as IFlowComputation;

                flowView.Computation = innerComputation;
            }

            if ((view as IServiceProvider)?.GetService<IViewSelectable>() is { } sel && Page is { } page)
            {
                QueuedAction.Do(() =>
                {
                    sel.SetSelection(new ViewSelection(page));
                });
            }

            return;

        } while (false);
    }


    /// <summary>
    /// Executes the page flow asynchronously within the given computation context.
    /// </summary>
    /// <param name="compute">The flow computation context.</param>
    /// <param name="cancel">A token to cancel the computation.</param>
    /// <returns>The result of the computation, or null if execution could not proceed.</returns>
    public virtual async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        if (compute.GetNodeRunningState(this) is not { } state)
        {
            return null;
        }

        // Get the calling port
        if (state.Begin is not { } begin)
        {
            return null;
        }

        var instance = EnsureInstance();
        if (instance is null)
        {
            return null;
        }

        // Get the starting element from the port
        if (instance.GetElement(begin.Name) is not SubFlowBeginElement beginElement)
        {
            return null;
        }
        
        // Get the starting node
        if (beginElement.DiagramItem?.Node is not { } beginNode)
        {
            return null;
        }

        var conversation = compute.Context.GetArgument<IConversationHandler>();
        var context = new FunctionContext(compute.Context);

        // Create internal computation
        /*using */var computeInner = new RunnerFlowComputation(conversation, context);
        // Do not use using here, because after completion, the internal state can still be visualized.

        // Create caller context
        var callerContext = new PageFunctionCallerContext(instance, compute);
        computeInner.Context.SetArgument<IFlowCallerContext>(callerContext);

        state.Tag = computeInner;

        // Run internal computation
        await computeInner.RunStarterNode(beginNode, null, null, cancel);

        // Set output values
        var outputs = instance.GetAllChildElements(false)
            .OfType<IPageParameterOutput>()
            .ToArray();

        foreach (var output in outputs)
        {
            if ((output as SubFlowElement)?.DiagramItem?.Node is not { } node)
            {
                continue;
            }

            var value = computeInner.GetResult(node, true);
            callerContext.SetParameter(compute, output.Name, value);
        }

        // Get end port
        if (instance.GetElement(callerContext.EndActionName) is SubFlowEndElement endElement)
        {
            if (endElement.OuterConnector is { } connector)
            {
                compute.SetValue(connector, callerContext.EndActionValue);
                return connector;
            }
        }

        return null;
    }


    /// <summary>
    /// Ensures that a valid page instance exists and is associated with this node, building one if necessary.
    /// </summary>
    /// <returns>The current <see cref="SubFlowInstance"/>, or null if it could not be created.</returns>
    public SubFlowInstance EnsureInstance()
    {
        if (_instance != null && _instance.IsInDiagram)
        {
            return _instance;
        }
        else
        {
            return BuildInstance();
        }
    }


    private SubFlowInstance BuildInstance()
    {
        if (Page is { } page)
        {
            var instance = _instance;

            if (instance != null && instance.IsInDiagram && instance.DiagramItem == page)
            {
                instance.Build();
            }
            else
            {
                var option = new PageElementOption
                {
                    Mode = PageElementMode.Function,
                    Owner = this,
                };

                instance = new SubFlowInstance(page, option);
                instance.UpdateFromOther(instance);

                this.Instance = instance;
            }

            return instance;
        }
        else
        {
            Instance = null;
            return null;
        }
    }
}

/// <summary>
/// Provides a context for a page function caller to interact with an outer flow computation.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageFunctionCallerContext")]
public class PageFunctionCallerContext : IFlowCallerContext
{
    private readonly SubFlowInstance _rootElement;
    private readonly IFlowComputationAsync _outer;

    /// <summary>
    /// Gets the name of the action that initiated the flow execution.
    /// </summary>
    public string BeginActionName { get; private set; }

    /// <summary>
    /// Gets the name of the action that ended the flow execution.
    /// </summary>
    public string EndActionName { get; private set; }

    /// <summary>
    /// Gets the value returned when the flow execution ended.
    /// </summary>
    public object EndActionValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="PageFunctionCallerContext"/>.
    /// </summary>
    /// <param name="rootElement">The page instance that this context operates on.</param>
    /// <param name="outer">The outer flow computation that invoked this page function.</param>
    public PageFunctionCallerContext(SubFlowInstance rootElement, IFlowComputationAsync outer)
    {
        _rootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
        _outer = outer ?? throw new ArgumentNullException(nameof(outer));
    }

    /// <summary>
    /// Gets or sets the title of this caller context.
    /// </summary>
    public string Title { get; set; }

    /// <inheritdoc/>
    public void OnBeginFlow(IFlowComputation computation, string name)
    {
        BeginActionName = name;
    }

    /// <inheritdoc/>
    public string[] GetDatasToCompute(IFlowComputation computation, string name)
    {
        return _rootElement.GetDatasToCompute(computation, name);
    }

    /// <inheritdoc/>
    public void OnEndFlow(IFlowComputation computation, string name, object value)
    {
        EndActionName = name;
        EndActionValue = value;
    }

    /// <inheritdoc/>
    public bool TryGetParameter(IFlowComputation computation, string name, out object value)
    {
        return _rootElement.TryGetOuterParameter(_outer, name, out value);
    }

    /// <inheritdoc/>
    public void SetParameter(IFlowComputation computation, string name, object value)
    {
        _rootElement.SetOuterParameter(_outer, name, value);
    }

    /// <inheritdoc/>
    public async Task<object> CallFunction(IFlowComputation computation, string name, object value, CancellationToken cancel)
    {
        var endElement = _rootElement.GetElement(name) as SubFlowEndElement;
        if (endElement?.OuterConnector is { } connector)
        {
            _outer.SetValue(connector, value);
            return await _outer.RunAction(connector, cancel);
        }

        return null;
    }

    /// <inheritdoc/>
    public ISubFlowAsset GetDefinitionPage() => _rootElement?.GetDefinitionPage();
}
