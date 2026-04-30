using Suity.Collections;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Helpers;
using Suity.UndoRedos;
using Suity.Views.Im;
using System;
using System.Linq;

namespace Suity.Editor.Documents.Articles;

[DocumentViewUsage(typeof(ArticleDocument))]
/// <summary>
/// Provides the document view for editing and displaying articles in a tree-inspector layout.
/// </summary>
public class ArticleDocumentView : IDocumentView, IDrawImGui, IDrawContext
{
    private readonly ImGuiNodeRef _guiRef = new();

    private readonly IUndoableViewObjectImGui _treeView;
    private readonly IInspectorContext _inspectorContext;
    private readonly ImGuiTheme _theme;

    private UndoRedoManager _undoManager;
    private ArticleDocument _document;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleDocumentView"/> class.
    /// </summary>
    public ArticleDocumentView()
    {
        //_treeView = EditorUtility.CreateColumnTreeImGui(":Article");
        _treeView = EditorUtility.CreateSimpleTreeImGui(":Article");

        _treeView.SelectionChanged += View_SelectionChanged;
        _treeView.Dirty += View_Dirty;
        _treeView.Edited += View_Edited;

        _inspectorContext = _treeView.GetService<IInspectorContext>();

        _theme = EditorUtility.GetEditorImGuiTheme();
    }

    #region IServiceProvider

    /// <inheritdoc/>
    public object GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        return _treeView.GetService(serviceType);
    }

    #endregion

    #region IDocumentView

    /// <summary>
    /// Gets the target document object being viewed.
    /// </summary>
    public object TargetObject => _document;

    /// <inheritdoc/>
    public void StartView(Document document, IDocumentViewHost host)
    {
        if (document == null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (document is not AssetDocument)
        {
            throw new ArgumentException("Document must be a LinkedDocument", nameof(document));
        }

        if (_document != null)
        {
            throw new InvalidOperationException();
        }

        _document = (ArticleDocument)document;
        _undoManager = host.GetService<UndoRedoManager>() ?? new UndoRedoManager();

        _treeView.UndoManager = _undoManager;
        _treeView.Target = _document;
        _treeView.ExpandAll();

        string formatName = _document.Entry?.Format?.FormatName;
        if (!string.IsNullOrWhiteSpace(formatName))
        {
            _treeView.CreateMenu("#" + formatName);
        }

        RestoreDocumentViewState(_document);
    }

    /// <inheritdoc/>
    public void StopView()
    {
        SaveDocumentViewState(_document);

        _treeView.Target = null;
        _document = null;
        _undoManager = null;
        _treeView.UndoManager = null;
        _guiRef.Node = null;
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
        _treeView.Target = _document;
    }

    /// <inheritdoc/>
    public void ActivateView(bool focus)
    {
        if (focus)
        {
            _treeView.FocusView(false);
        }

        _treeView.UpdateAnalysis();
    }

    /// <inheritdoc/>
    public void RefreshView()
    {
        _treeView?.UpdateDisplayedObject();
    }

    #endregion

    #region IDrawImGui

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        _guiRef.Node = gui.HorizontalFrame("main_ui")
        .InitTheme(_theme)
        .InitClass("editorBg")
        .InitFullSize()
        .OnContent(() =>
        {
            _treeView.OnNodeGui(gui)
            .InitFullHeight()
            .InitWidthPercentage(30);

            gui.HorizontalResizer(40, null)
            .InitFullHeight()
            .InitClass("resizer_h");

            var frame = gui.VerticalLayout("content")
            .InitFullHeight()
            .InitWidthRest()
            .OnContent(() =>
            {
                if (_treeView.SelectedObjects.CountOne() && _treeView.SelectedObjects.FirstOrDefault() is IArticle article)
                {
                    OnArticleGui(gui, article);
                }
            });

        });
    }

    private void OnArticleGui(ImGui gui, IArticle article)
    {
        bool draw = EditorServices.ImGuiService.DrawItem(gui, article, EditorImGuiPipeline.Content, this, false);

        if (!draw)
        {
            gui.ScrollableFrame("scrollbox")
            .InitFullWidth()
            .InitFullHeight()
            .OnContent(() =>
            {
                var node = gui.TextArea("#textArea", article.Content)
                .InitFullWidth();
            });
        }
    }

    #endregion

    #region Events

    private void View_SelectionChanged(object sender, EventArgs e)
    {
        //if (_view.SelectedObjects.CountOne() && _view.SelectedObjects.FirstOrDefault() is LaunchGrid grid)
        //{
        //    _edit.Grid = grid;
        //}
        //else
        //{
        //    _edit.Grid = null;
        //}

        //_edit.QueueRefresh();

        _guiRef.QueueRefresh();
    }

    private void View_Dirty(object sender, EventArgs e)
    {
        _document?.MarkDirty(this);
    }

    private void View_Edited(object sender, object[] objs)
    {
        //_edit.QueueRefresh();
        _guiRef.QueueRefresh();
    }

    #endregion

    #region State

    /// <summary>
    /// Restores the saved GUI state for the document view.
    /// </summary>
    /// <param name="document">The document to restore state for.</param>
    public void RestoreDocumentViewState(Document document)
    {
        var asset = document?.GetAsset();
        if (asset != null)
        {
            object config = EditorServices.PluginService.GetPlugin<GuiStatePlugin>().GetGuiState<object>(asset);

            _treeView.RestoreViewState(config);
        }
    }

    /// <summary>
    /// Saves the current GUI state for the document view.
    /// </summary>
    /// <param name="document">The document to save state for.</param>
    public void SaveDocumentViewState(Document document)
    {
        var asset = document?.GetAsset();
        if (asset != null)
        {
            var config = _treeView.SaveViewState();
            if (config != null)
            {
                EditorServices.PluginService.GetPlugin<GuiStatePlugin>().SetGuiState<object>(asset, config);
            }
        }
    }

    #endregion
}
