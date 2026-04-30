using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Represents a stack item for sub-document views, managing the lifecycle and inspector context of a sub-document.
/// </summary>
public class SubDocumentStackItem : IInspectorContext
{
    /// <summary>
    /// Gets the parent service provider.
    /// </summary>
    private IServiceProvider _parent;

    /// <summary>
    /// Gets the cached file name of the document entry.
    /// </summary>
    public StorageLocation CachedFileName { get; private set; }
    private string _cachedName;
    private readonly DocumentUsageToken _docUseToken = new(nameof(SubDocumentStackItem));

    /// <summary>
    /// Gets the document entry for this stack item.
    /// </summary>
    public DocumentEntry Entry { get; }
    /// <summary>
    /// Gets the document content.
    /// </summary>
    public Document Document => Entry.Content;
    /// <summary>
    /// Gets the expanded view drawer for this document.
    /// </summary>
    public IDrawExpandedImGui Draw { get; }
    /// <summary>
    /// Gets or sets the unique identifier for this stack item.
    /// </summary>
    public string Id { get; set; } = "???";
    /// <summary>
    /// Gets the undo/redo manager for this sub-document.
    /// </summary>
    public UndoRedoManager UndoManager { get; } = new();

    /// <summary>
    /// Gets a value indicating whether this item is currently active in the view.
    /// </summary>
    public bool IsInView { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubDocumentStackItem"/> class.
    /// </summary>
    /// <param name="parent">The parent service provider.</param>
    /// <param name="entry">The document entry.</param>
    /// <param name="draw">The expanded view drawer.</param>
    public SubDocumentStackItem(IServiceProvider parent, DocumentEntry entry, IDrawExpandedImGui draw)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        Draw = draw ?? throw new ArgumentNullException(nameof(draw));
    }

    /// <summary>
    /// Gets the display name of the document (cached file name without extension).
    /// </summary>
    public string Name
    {
        get
        {
            if (CachedFileName != Entry.FileName)
            {
                CachedFileName = Entry.FileName;
                _cachedName = Path.GetFileNameWithoutExtension(CachedFileName.PhysicFileName);
            }

            return _cachedName;
        }
    }

    /// <summary>
    /// Starts the view by marking the document entry as in use and entering the expanded view.
    /// </summary>
    public void StartView()
    {
        if (IsInView)
        {
            return;
        }

        IsInView = true;

        var entry = Entry;
        entry.MarkUsage(_docUseToken);
        entry.DirtyChanged += Entry_DirtyChanged;

        if (entry.Content is { } doc)
        {
            Draw.EnterExpandedView(doc, this);
        }
    }

    /// <summary>
    /// Stops the view by unmarking the document entry and exiting the expanded view.
    /// </summary>
    public void StopView()
    {
        if (!IsInView)
        {
            return;
        }

        IsInView = false;

        var entry = Entry;

        entry.UnmarkUsage(_docUseToken);
        entry.DirtyChanged -= Entry_DirtyChanged;

        Draw.ExitExpandedView();
    }

    private void Entry_DirtyChanged(object sender, EventArgs e)
    {
        (_parent as IViewRefresh)?.QueueRefreshView();
    }


    #region IInspectorContext

    /// <summary>
    /// Gets or sets the user data associated with the inspector context.
    /// </summary>
    public object InspectorUserData { get; set; }

    /// <inheritdoc/>
    public void InspectorEnter()
    {
    }

    /// <inheritdoc/>
    public void InspectorExit()
    {
    }

    /// <inheritdoc/>
    public void InspectorBeginMacro(string name = null)
    {
        UndoManager.BeginMacro(name);
    }

    /// <inheritdoc/>
    public bool InspectorDoAction(UndoRedoAction action)
    {
        UndoManager.Do(action);
        return true;
    }

    /// <inheritdoc/>
    public void InspectorEndMarco(string name = null)
    {
        UndoManager.EndMacro(name);
    }

    /// <inheritdoc/>
    public void InspectorObjectEdited(IEnumerable<object> objs, string propertyName)
    {
    }

    /// <inheritdoc/>
    public void InspectorEditFinish()
    {
    }

    /// <inheritdoc/>
    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(UndoRedoManager))
        {
            return UndoManager;
        }

        return _parent.GetService(serviceType);
    }

    #endregion
}

/// <summary>
/// Manages a stack of sub-document views with navigation, drag-and-drop support, and save capabilities.
/// </summary>
public class SubDocumentView : IHasSubDocumentView, IViewSave, IDropTarget
{
    private readonly IServiceProvider _owner;
    private readonly ImGuiNodeRef _guiRef = new();

    private ImGuiTheme _theme;
    private Document _document;

    private readonly Stack<SubDocumentStackItem> _docViewStack = [];

    int _subIdAlloc = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubDocumentView"/> class.
    /// </summary>
    /// <param name="owner">The owner service provider.</param>
    /// <param name="document">Optional initial document.</param>
    public SubDocumentView(IServiceProvider owner, Document document = null)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));

        _theme = EditorUtility.GetEditorImGuiTheme();
    }

    /// <inheritdoc/>
    public Document Document
    {
        get => _document;
        set => _document = value;
    }

    /// <summary>
    /// Gets or sets the ImGui theme for this view.
    /// </summary>
    public ImGuiTheme Theme
    {
        get => _theme;
        set => _theme = value;
    }

    #region IHasSubDocumentView

    /// <inheritdoc/>
    public object OpenSubView(Document document)
    {
        if (document is null)
        {
            return null;
        }

        if (document.Entry is not { } entry)
        {
            return null;
        }

        var view = EditorServices.ImGuiService.CreateExpandedView(document.GetType());
        if (view is null)
        {
            return null;
        }

        var stackItem = new SubDocumentStackItem(_owner, entry, view)
        {
            Id = "#sub-view-" + (++_subIdAlloc).ToString(),
        };

        PushSubDocumentView(stackItem);

        _guiRef.QueueRefresh();

        return view;
    }

    /// <summary>
    /// Gets the currently active sub-view, either the expanded drawer or the owner service provider.
    /// </summary>
    public object CurrentSubView => CurrentStackItem?.Draw ?? (object)_owner;

    #endregion

    #region IViewSave

    /// <inheritdoc/>
    public void SaveView()
    {
        if (CurrentStackItem?.Document is { } subDoc)
        {
            subDoc.Save();
        }
        else if (_document is { } doc)
        {
            doc.Save();
        }
    }

    #endregion

    #region IDropTarget

    /// <inheritdoc/>
    public virtual void DragOver(IDragEvent e)
    {
        // DragDrop event is initiated by GraphicViewControl, can only route.
        if (CurrentSubView is IDropTarget dropTarget && dropTarget != _owner)
        {
            dropTarget.DragOver(e);
        }
    }

    /// <inheritdoc/>
    public virtual void DragDrop(IDragEvent e)
    {
        // DragDrop event is initiated by GraphicViewControl, can only route.
        if (CurrentSubView is IDropTarget dropTarget && dropTarget != _owner)
        {
            dropTarget.DragDrop(e);
        }
    }

    #endregion

    /// <summary>
    /// Gets the current stack item at the top of the sub-document view stack.
    /// </summary>
    public SubDocumentStackItem CurrentStackItem => _docViewStack.Count > 0 ? _docViewStack.Peek() : null;

    /// <summary>
    /// Gets all stack items in the sub-document view stack.
    /// </summary>
    public IEnumerable<SubDocumentStackItem> StackItems => _docViewStack;

    /// <summary>
    /// Renders the GUI for the sub-document view with navigation support.
    /// </summary>
    /// <param name="gui">The ImGui instance to render with.</param>
    /// <param name="init">Optional initialization action for the root node.</param>
    /// <param name="mainGui">The main GUI drawing callback.</param>
    public void OnGui(ImGui gui, Action<ImGuiNode> init, DrawImGui mainGui)
    {
        _guiRef.Node = gui.OverlayLayout("#main")
        .OnInitialize(n =>
        {
            gui.SetValue<IHasSubDocumentView>(this);
            n.InitTheme(_theme);
            n.InitFullSize();
            init?.Invoke(n);
        })
        .OnContent(() =>
        {
            if (_document is null)
            {
                return;
            }

            if (_docViewStack.Count == 0)
            {
                mainGui?.Invoke(gui);
                return;
            }

            var stackItem = _docViewStack.Peek();

            gui.VerticalLayout(stackItem.Id)
            .InitFullSize()
            .OnContent(() =>
            {
                stackItem.Draw.OnExpandedGui(gui);
            });

            gui.VerticalLayout("#head-up")
            .InitTheme(_theme)
            .InitFullSize()
            .OnContent(() =>
            {
                gui.HorizontalLayout("#navigation")
                .InitFullWidth()
                .InitFitVertical()
                .OnContent(() =>
                {
                    string dirtyStr = (_document?.IsDirty == true) ? "*" : "";

                    gui.Button("#tag-main", dirtyStr + L("Root"))
                    .InitClass("simpleFrame")
                    .InitCenterVertical()
                    .OnClick(() =>
                    {
                        GuiPopSubDocumentView();
                    });

                    int i = 0;
                    foreach (var item in _docViewStack.Reverse())
                    {
                        gui.Frame("splitter-" + i)
                        .InitClass("simpleFrame")
                        .InitFit()
                        .InitCenterVertical()
                        .OnContent(() =>
                        {
                            gui.Text(">");
                        });

                        DrawSubDocumentHeader(gui, i, item);

                        i++;
                    }
                });

            });
        });
    }

    /// <summary>
    /// Clears the GUI reference, releasing the current GUI state.
    /// </summary>
    public void ClearGui()
    {
        _guiRef.Node = null;
    }

    /// <summary>
    /// Exits all sub-document views, popping the entire stack.
    /// </summary>
    public void ExitAll()
    {
        while (PopSubDocumentView() != null)
        {
        }
    }


    private void PushSubDocumentView(SubDocumentStackItem stackItem)
    {
        stackItem.StartView();
        _docViewStack.Push(stackItem);
        //this.UndoManager = stackItem.UndoManager;

        _guiRef.QueueRefresh();
    }


    private async void GuiPopSubDocumentView(SubDocumentStackItem stopAt = null)
    {
        // Navigate to project when clicking the last one
        if (_docViewStack.Count > 0 && _docViewStack.Peek() == stopAt)
        {
            EditorUtility.LocateInProject(stopAt.Document);
            return;
        }

        while (_docViewStack.Count > 0)
        {
            var current = _docViewStack.Peek();
            if (current == stopAt)
            {
                break;
            }

            bool abandon = false;

            if (current.Document.IsDirty)
            {
                var check = await DialogUtility.ShowYesNoCancelDialogAsync($"Do you want to save {current.Name}?");
                if (!check.HasValue)
                {
                    break;
                }

                if (check is { } aCheck)
                {
                    if (aCheck)
                    {
                        current.Document.Save();
                    }
                    else
                    {
                        abandon = true;
                    }
                }
            }

            var popped = PopSubDocumentView();
            if (abandon && popped.Document?.Entry is { } entry)
            {
                // Force close after abandoning save
                DocumentManager.Instance.CloseDocument(entry);
            }
        }
    }

    private SubDocumentStackItem PopSubDocumentView()
    {
        if (_docViewStack.Count == 0)
        {
            return null;
        }

        var stackItem = _docViewStack.Pop();
        stackItem.StopView();
        //if (_docViewStack.Count > 0)
        //{
        //    this.UndoManager = _docViewStack.Peek().UndoManager;
        //}
        //else
        //{
        //    this.UndoManager = _rootUndoMananger;
        //}

        _guiRef.QueueRefresh();

        return stackItem;
    }

    private void DrawSubDocumentHeader(ImGui gui, int i, SubDocumentStackItem item)
    {
        string dirtyStr = item.Document.IsDirty ? "*" : "";

        gui.Button("#tag-" + i, dirtyStr + item.Name, item.Document?.Icon)
        .InitClass("simpleFrame")
        .InitCenterVertical()
        .OnClick(() =>
        {
            GuiPopSubDocumentView(item);
            CurrentStackItem?.StartView();
            _guiRef.QueueRefresh();
        });
    }

    private void DocumentEntry_Saved(object sender, EventArgs e)
    {
        foreach (var item in _docViewStack)
        {
            item.Document.Save();
        }
    }
}
