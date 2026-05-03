using Suity.Drawing;
using Suity.Editor.AIGC.TaskPages;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System;

namespace Suity.Editor.AIGC.Flows.Pages;

/// <summary>
/// Represents a canvas node for a page definition asset, providing an expanded view
/// with property grid inspection and navigation capabilities.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.PageDefinitionCanvasNode")]
public class PageCanvasNode : ExpandedCanvasAssetNode<PageDefinitionAsset>
{
    private readonly ImSubPropertyGrid _propGrid = new("AigcPage", true);

    private AigcPageInstance _rootElement;
    private IInspectorContext _inpectorContext;

    /// <summary>
    /// Gets or sets the root page element instance for this node.
    /// Manages event subscriptions for result output and refresh requests.
    /// </summary>
    public AigcPageInstance RootElement
    {
        get => _rootElement;
        set
        {
            if (ReferenceEquals(_rootElement, value))
            {
                return;
            }

            if (_rootElement != null)
            {
                _rootElement.ResultOutput -= _rootElement_ResultOutput;
                _rootElement.RefreshRequesting -= _rootElement_RefreshRequesting;
            }

            _rootElement = value;

            if (_rootElement != null)
            {
                _rootElement.ResultOutput += _rootElement_ResultOutput;
                _rootElement.RefreshRequesting += _rootElement_RefreshRequesting;
            }
        }
    }

    private void _rootElement_ResultOutput(object sender, EventArgs e)
    {
        this.GetFlowDocument()?.MarkDirty(this);

        QueueRefreshView();
    }

    private void _rootElement_RefreshRequesting(object sender, EventArgs e)
    {
        QueueRefreshView();
    }

    /// <summary>
    /// Gets the page definition diagram item associated with this node.
    /// </summary>
    public PageDefinitionDiagramItem Page => GetTargetObject() as PageDefinitionDiagramItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageCanvasNode"/> class.
    /// </summary>
    public PageCanvasNode()
    {
        // Not occupying document editing, only for display.
        base.OccupyDocumentUsage = false;
    }

    /// <inheritdoc/>
    public override bool ResizableOnExpand => true;

    /// <inheritdoc/>
    public override ImageDef Icon => base.Icon;

    /// <inheritdoc/>
    public override void EnterExpandedView(object target, IInspectorContext context = null)
    {
        base.EnterExpandedView(target, context);

        _inpectorContext = context;

        if (RootElement is { } root)
        {
            _propGrid.InspectObjects([root], context: context);
        }
        else
        {
            _propGrid.InspectObjects([]);
        }
    }

    /// <inheritdoc/>
    public override void ExitExpandedView()
    {
        base.ExitExpandedView();

        _inpectorContext = null;
        _propGrid.InspectObjects([]);
    }

    /// <inheritdoc/>
    public override ImGuiNode OnExpandedGui(ImGui gui)
    {
        if (RootElement != null)
        {
            return gui.VerticalLayout("##property_grid_group")
            .InitFullWidth()
            .InitHeightRest()
            .OnContent(() => 
            {
                _propGrid.OnExpandedGui(gui).InitFullSize();
            });
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    protected override void OnAssetTargetUpdated()
    {
        base.OnAssetTargetUpdated();

        BuildPropertyGrid();
    }

    /// <inheritdoc/>
    protected internal override void OnAdded()
    {
        base.OnAdded();

        // Purpose is to refresh IInspectorContext
        BuildPropertyGrid();
    }

    /// <inheritdoc/>
    protected internal override void OnLoaded()
    {
        base.OnLoaded();

        var context = (this.Canvas as Document)?.View?.GetService<IInspectorContext>();

    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Page", RootElement, SyncFlag.GetOnly);
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

            if (Canvas is not Document canvasDoc)
            {
                break;
            }

            subDocView ??= canvasDoc.View?.GetService<IHasSubDocumentView>();
            if (subDocView is null)
            {
                break;
            }

            var currentView = subDocView.CurrentSubView as IFlowView;

            if (subDocView.OpenSubView(pageDoc) is { } view)
            {
                if (view is IFlowView flowView)
                {
                    if (currentView?.GetViewNode(this.Name) is { } viewNode && viewNode.NodeComputation is { } nodeCompute)
                    {
                        flowView.Computation = nodeCompute;
                    }
                    else if (RootElement?.LastComputation is { } lastCompute)
                    {
                        flowView.Computation = lastCompute;
                    }
                }

                if ((view as IServiceProvider)?.GetService<IViewSelectable>() is { } sel && Page is { } page)
                {
                    QueuedAction.Do(() => 
                    {
                        sel.SetSelection(new ViewSelection(page));
                    });
                }
            }

            return;

        } while (false);

        base.OnNavigationButtonClicked(subDocView);
    }

    /// <inheritdoc/>
    public override TextStatus DisplayStatus => _rootElement?.GetAllStatus() ?? TextStatus.Normal;

    private void BuildPropertyGrid()
    {
        if (Page is { } page)
        {
            var rootPage = RootElement;

            if (rootPage?.DiagramItem == page)
            {
                rootPage.Build();
            }
            else
            {
                var option = new PageElementOption
                {
                    Mode = PageElementMode.Page,
                    Owner = this,
                };

                RootElement = new AigcPageInstance(page, option);
                if (rootPage != null)
                {
                    RootElement.UpdateFromOther(rootPage);
                }
            }
        }
        else
        {
            RootElement = null;
        }
    }
}
