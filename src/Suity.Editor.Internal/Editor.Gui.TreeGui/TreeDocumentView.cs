using Suity.Collections;
using Suity.Editor;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Gui.TreeGui;

/// <summary>
/// A document view that displays a tree-based representation of a named document with undo/redo support.
/// </summary>
[DocumentViewUsage(typeof(SNamedDocument))]
[DefaultDocumentFormat]
public class TreeDocumentView : UndoableTreeImGui, IDocumentView
{
    private AssetDocument _document;

    /// <summary>
    /// Initializes a new instance with default column tree options.
    /// </summary>
    public TreeDocumentView()
        : base(default(ColumnTreeOptions))
    {
    }

    #region IDocumentView

    /// <inheritdoc/>
    public void StartView(Document document, IDocumentViewHost host)
    {
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (document is not AssetDocument assetDocument)
        {
            throw new ArgumentException("Document must be a LinkedDocument", nameof(document));
        }

        if (_document != null)
        {
            throw new InvalidOperationException("Document already set.");
        }

        _document = assetDocument;
        UndoManager = host.GetService<UndoRedoManager>() ?? new UndoRedoManager();

        Target = _document;

        string formatName = _document.Entry?.Format?.FormatName;
        if (!string.IsNullOrWhiteSpace(formatName))
        {
            CreateMenu("#" + formatName);
        }

        RestoreDocumentViewState();
    }

    /// <inheritdoc/>
    public void StopView()
    {
        SaveDocumentViewState();

        Target = null;
        _document = null;
        UndoManager = null;
    }

    /// <inheritdoc/>
    public object GetUIObject()
    {
        return this;
    }

    /// <inheritdoc/>
    public void SetDataToDocument()
    {
    }

    /// <inheritdoc/>
    public void GetDataFromDocument()
    {
        Target = _document;
    }

    /// <inheritdoc/>
    public void ActivateView(bool focus)
    {
        if (focus)
        {
            FocusView(false);
        }

        UpdateAnalysis();
    }

    /// <inheritdoc/>
    public void RefreshView()
    {
        UpdateDisplayedObject();
        QueueRefresh(true);
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnSelectionChanged()
    {
        base.OnSelectionChanged();

        NavigationService.Current.AddRecord(_document);
    }

    /// <inheritdoc/>
    protected override void OnDirty()
    {
        base.OnDirty();

        _document?.MarkDirty(this);
    }

    /// <inheritdoc/>
    protected override bool OnInspectObjects(IEnumerable<object> objs)
    {
        return (_document as SNamedDocument)?.HandleInspect(objs, this) == true;
    }

    #region ViewState

    /// <summary>
    /// Restores the view state from the document's stored GUI state.
    /// </summary>
    private void RestoreDocumentViewState()
    {
        var asset = _document?.GetAsset();
        if (asset != null)
        {
            object config = EditorServices.PluginService.GetPlugin<GuiStatePlugin>().GetGuiState<object>(asset);

            RestoreViewState(config);
        }
    }

    /// <summary>
    /// Saves the current view state to the document's GUI state storage.
    /// </summary>
    private void SaveDocumentViewState()
    {
        var asset = _document?.GetAsset();
        if (asset != null)
        {
            var config = SaveViewState();
            if (config != null)
            {
                EditorServices.PluginService.GetPlugin<GuiStatePlugin>().SetGuiState(asset, config);
            }
        }
    }

    #endregion
}
