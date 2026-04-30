using Suity.Editor.Documents;
using Suity.Helpers;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using System;

namespace Suity.Editor.Flows.Gui;


/// <summary>
/// A document view for visual flow editing with undo/redo support, sub-document navigation,
/// and ImGui-based graphical rendering.
/// </summary>
[DocumentViewUsage(typeof(FlowDocument))]
public class FlowDocumentView : UndoableFlowViewImGui,
    IHasSubDocumentView,
    IGraphicObject,
    IDocumentView,
    IViewRefresh
{
    private readonly SubDocumentView _subView;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowDocumentView"/> class.
    /// </summary>
    public FlowDocumentView()
    {
        _subView = new SubDocumentView(this);   
    }

    /// <inheritdoc/>
    public override object GetService(Type serviceType)
    {
        if (serviceType == typeof(IHasSubDocumentView))
        {
            return this;
        }

        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        if (serviceType.IsInstanceOfType(_subView))
        {
            return _subView;
        }

        return base.GetService(serviceType);
    }

    #region IHasSubDocumentView

    /// <summary>
    /// Gets the current active sub-view.
    /// </summary>
    public object CurrentSubView => _subView.CurrentSubView;

    /// <summary>
    /// Opens a sub-view for the specified document.
    /// </summary>
    /// <param name="document">The document to open in a sub-view.</param>
    /// <returns>The opened sub-view object.</returns>
    public object OpenSubView(Document document) => _subView.OpenSubView(document);

    #endregion

    #region IGraphicObject

    private ImGui _gui;

    /// <inheritdoc/>
    public IGraphicContext GraphicContext
    {
        get => _gui?.Context;
        set
        {
            var context = _gui?.Context;

            if (ReferenceEquals(context, value))
            {
                return;
            }

            GraphPanel.GraphicContext = value;

            if (value != null)
            {
                var config = new ImGuiConfig
                {
                    Name = "Flow",
                    Theme = NodeGraphTheme.Default,
                };
                _gui = ImGuiServices.CreateImGui(value, config);
            }
            else
            {
                _gui = null;
            }
        }
    }


    /// <summary>
    /// Handles graphic input events and routes them to the GUI handler.
    /// </summary>
    /// <param name="input">The graphic input event data.</param>
    /// <inheritdoc/>
    public void HandleGraphicInput(IGraphicInput input) 
        => _gui?.HandleGraphicInput(input, OnGui);

    /// <inheritdoc/>
    public void HandleGraphicOutput(IGraphicOutput output) 
        => _gui.HandleGraphicOutput(output);

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        _subView.OnGui(gui, n => { }, MainGui);
    }

    /// <summary>
    /// Renders the main graph panel layout.
    /// </summary>
    /// <param name="gui">The ImGui instance to render with.</param>
    private void MainGui(ImGui gui)
    {
        gui.VerticalLayout("#root")
        .InitValue<IFlowView>(this)
        .InitFullSize()
        .OnContent(() =>
        {
            GraphPanel.OnNodeGui(gui);
        });
    }

    #endregion

    #region IDocumentView

    /// <summary>
    /// Gets the target object this view represents (the document).
    /// </summary>
    public object TargetObject => Document;

    /// <summary>
    /// Activates the view and optionally focuses on the current selection.
    /// </summary>
    /// <param name="focus">Whether to focus on the current selection.</param>
    /// <inheritdoc/>
    public void ActivateView(bool focus)
    {
        if (focus)
        {
            InspectSelection();
            // Bottom layer changed, reset graphics device
            GraphicContext?.ResetContext();
            GraphicContext?.RequestFocus();
        }

        //nodeGraphPanel1?.ClearCache();

        QueueAnalysis();
    }

    /// <summary>
    /// Reloads data from the document into the view by rebuilding it.
    /// </summary>
    public void GetDataFromDocument()
    {
        RebuildView();
    }

    /// <summary>
    /// Returns this view as the UI object for embedding.
    /// </summary>
    /// <returns>The view instance.</returns>
    public object GetUIObject()
    {
        return this;
    }

    /// <summary>
    /// Pushes any changes from the view back to the document (no-op for this view).
    /// </summary>
    public void SetDataToDocument()
    {
    }

    /// <summary>
    /// Starts the view by initializing it with a document and host services.
    /// </summary>
    /// <param name="document">The flow document to display.</param>
    /// <param name="host">The document view host providing services.</param>
    public void StartView(Document document, IDocumentViewHost host)
    {
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }

        if (document is not FlowDocument flowDocument)
        {
            throw new ArgumentException(nameof(document));
        }

        if (this.Document != null)
        {
            throw new InvalidOperationException("Document already set.");
        }

        this.Document = flowDocument;
        _subView.Document = flowDocument;

        UndoManager = host.GetService<UndoRedoManager>() ?? new UndoRedoManager();

        if (_gui != null)
        {
            QueuedAction.Do(() => RestoreViewState(_gui));
        }

        if (document?.Entry is { } docEntry)
        {
            docEntry.Saved += DocumentEntry_Saved;
        }
    }

    /// <summary>
    /// Stops the view, saving state and cleaning up resources.
    /// </summary>
    public void StopView()
    {
        if (_gui != null)
        {
            SaveViewState(_gui);
        }

        if (this.Document?.Entry is { } docEntry)
        {
            docEntry.Saved -= DocumentEntry_Saved;
        }

        // Need to set base class Document=null to execute StopView
        this.Document = null;
        _subView.Document = null;

        _subView.ExitAll();
        _subView.ClearGui();
    }

    #endregion

    #region IDropTarget

    /// <inheritdoc/>
    public override void DragOver(IDragEvent e)
    {
        // DragDrop event is initiated by GraphicViewControl, can only route
        if (_subView.CurrentSubView is IDropTarget dropTarget && dropTarget != this)
        {
            dropTarget.DragOver(e);
        }
        else
        {
            base.DragOver(e);
        }
    }

    /// <inheritdoc/>
    public override void DragDrop(IDragEvent e)
    {
        // DragDrop event is initiated by GraphicViewControl, can only route
        if (_subView.CurrentSubView is IDropTarget dropTarget && dropTarget != this)
        {
            dropTarget.DragDrop(e);
        }
        else
        {
            base.DragDrop(e);
        }
    }

    #endregion

    #region IViewRefresh

    /// <summary>
    /// Queues a refresh of the GUI to update the view.
    /// </summary>
    public void QueueRefreshView() => _gui?.QueueRefresh();

    #endregion

    /// <summary>
    /// Handles the document saved event, saving all sub-documents in the stack.
    /// </summary>
    private void DocumentEntry_Saved(object sender, EventArgs e)
    {
        foreach (var item in _subView.StackItems)
        {
            item.Document.Save();
        }
    }
}