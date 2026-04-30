using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.WorkSpaces;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Views.Named;

/// <summary>
/// Represents a linked render target item that provides navigation, display, and preview capabilities for render targets.
/// </summary>
internal class LinkedRenderTargetItem : INavigable, ITextDisplay, IPreviewDisplay, IViewDoubleClickAction, IViewObject
{
    private readonly ISupportAnalysis _supportAnalysis;
    private readonly RenderTarget _renderTarget;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedRenderTargetItem"/> class.
    /// </summary>
    /// <param name="supportAnalysis">The analysis support provider.</param>
    /// <param name="renderTarget">The render target to wrap.</param>
    public LinkedRenderTargetItem(ISupportAnalysis supportAnalysis, RenderTarget renderTarget)
    {
        _supportAnalysis = supportAnalysis;
        _renderTarget = renderTarget;
    }

    /// <inheritdoc/>
    public object GetNavigationTarget()
    {
        return _renderTarget?.FileName;
    }

    /// <inheritdoc/>
    public string DisplayText => _renderTarget != null ? Path.GetFileName(_renderTarget.FileName.PhysicRelativePath) : string.Empty;
    /// <inheritdoc/>
    public object DisplayIcon => _renderTarget != null ? EditorUtility.GetIconForFileExact(_renderTarget.FileName.PhysicFullPath) : null;
    /// <inheritdoc/>
    public TextStatus DisplayStatus => _renderTarget != null ? (_renderTarget.UserCodeCount > 0 ? TextStatus.UserCode : TextStatus.Tag) : TextStatus.Error;

    /// <inheritdoc/>
    string IPreviewDisplay.PreviewText
    {
        get
        {
            if (_renderTarget?.Tag is IWorkSpaceRefItem refItem)
            {
                return refItem.WorkSpace?.Name ?? string.Empty;
            }
            else
            {
                return _renderTarget?.ToString() ?? string.Empty;
            }
        }
    }

    /// <inheritdoc/>
    object IPreviewDisplay.PreviewIcon
    {
        get
        {
            return (_renderTarget?.Tag as IWorkSpaceRefItem)?.WorkSpace?.Icon;
        }
    }

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick()
    {
        if (_renderTarget != null)
        {
            if (DocumentManager.Instance.ShowDocument(_renderTarget.FileName.PhysicFullPath) is IViewSearch search)
            {
                Guid id = (_supportAnalysis as IHasId)?.Id ?? _renderTarget.RenderItemId;

                search.OpenSearch(id.ToString());
            }
        }
    }

    /// <inheritdoc/>
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
    }

    /// <inheritdoc/>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
    }
}

/// <summary>
/// Provides a list view for named render targets, enabling synchronization and display of render target analysis results.
/// </summary>
internal class NamedRenderTargetList : INamedRenderTargetList
{
    /// <summary>
    /// Gets the analysis support provider associated with this list.
    /// </summary>
    public ISupportAnalysis SupportAnalysis { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedRenderTargetList"/> class.
    /// </summary>
    /// <param name="supportAnalysis">The analysis support provider. Must not be null.</param>
    public NamedRenderTargetList(ISupportAnalysis supportAnalysis)
    {
        SupportAnalysis = supportAnalysis ?? throw new ArgumentNullException(nameof(supportAnalysis));
    }

    /// <inheritdoc/>
    int IViewList.ListViewId => ViewIds.TreeView;
    /// <inheritdoc/>
    int ISyncList.Count => SupportAnalysis.Analysis?.RenderTargets.Count ?? 0;

    /// <inheritdoc/>
    void ISyncList.Sync(IIndexSync sync, ISyncContext context)
    {
        List<RenderTarget> list = SupportAnalysis.Analysis?.RenderTargets;
        if (list is null)
        {
            return;
        }

        switch (sync.Mode)
        {
            case SyncMode.RequestElementType:
                sync.Sync(0, typeof(LinkedRenderTargetItem));
                break;

            case SyncMode.Get:
                if (sync.Index >= 0 && sync.Index < list.Count)
                {
                    sync.Sync(sync.Index, new LinkedRenderTargetItem(SupportAnalysis, list[sync.Index]));
                }
                break;

            case SyncMode.GetAll:
                for (int i = 0; i < list.Count; i++)
                {
                    sync.Sync(i, new LinkedRenderTargetItem(SupportAnalysis, list[i]));
                }
                break;
        }
    }

    /// <inheritdoc/>
    bool IDropInCheck.DropInCheck(object value) => false;

    /// <inheritdoc/>
    object IDropInCheck.DropInConvert(object value) => null;

    /// <inheritdoc/>
    public string DisplayText => "Render";
    /// <inheritdoc/>
    public object DisplayIcon => CoreIconCache.Tag;
    /// <inheritdoc/>
    public TextStatus DisplayStatus => TextStatus.Reference;
}
