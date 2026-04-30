using Suity.Editor.Services;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.TreeEditing;
using System;

namespace Suity.Editor.Gui.TreeGui;

/// <summary>
/// A tree ImGui view that supports undo/redo operations and synchronization state recording.
/// </summary>
public class UndoableTreeImGui : TreeImGui,
    IUndoableViewObjectImGui,
    ISyncStateRecord
{
    private UndoRedoManager _undoManager;

    /// <summary>
    /// Initializes a new instance with headerless tree options.
    /// </summary>
    /// <param name="option">The headerless tree configuration options.</param>
    public UndoableTreeImGui(HeaderlessTreeOptions option)
        : base(option)
    {
    }

    /// <summary>
    /// Initializes a new instance with column tree options.
    /// </summary>
    /// <param name="option">The column tree configuration options.</param>
    public UndoableTreeImGui(ColumnTreeOptions option)
        : base(option)
    {
    }

    /// <summary>
    /// Initializes a new instance with an existing virtual tree view.
    /// </summary>
    /// <param name="treeView">The virtual tree view to use.</param>
    protected UndoableTreeImGui(ImGuiVirtualTreeView treeView)
        : base(treeView)
    {
    }

    /// <summary>
    /// Gets or sets the undo/redo manager for this view.
    /// </summary>
    public UndoRedoManager UndoManager
    {
        get => _undoManager;
        set
        {
            if (ReferenceEquals(value, _undoManager))
            {
                return;
            }

            if (_undoManager != null)
            {
                _undoManager.Modified -= _undoManager_Modified;
            }

            _undoManager = value;
            if (_undoManager != null)
            {
                _undoManager.Modified += _undoManager_Modified;
            }
        }
    }

    /// <inheritdoc/>
    public override object GetService(Type serviceType)
    {
        if (serviceType == typeof(UndoRedoManager))
        {
            return _undoManager;
        }

        return base.GetService(serviceType);
    }

    /// <summary>
    /// Handles the modified event from the undo manager.
    /// </summary>
    private void _undoManager_Modified(object sender, EventArgs e)
    {
        OnDirty();
        QueueRefresh();
    }

    #region IInspectorContext

    /// <inheritdoc/>
    public override void InspectorBeginMacro(string name)
    {
        _undoManager?.BeginMacro(name);
    }

    /// <inheritdoc/>
    public override void InspectorEndMarco(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            _undoManager?.EndMacro(name);
        }
        else
        {
            _undoManager?.EndMacro(UndoRedoManager.MacroDeriveName.FromFirst);
        }
    }

    /// <inheritdoc/>
    public override bool InspectorDoAction(UndoRedoAction action)
    {
        if (_undoManager != null)
        {
            _undoManager.Do(action);
        }
        else
        {
            action.Do();
        }

        return true;
    }

    /// <inheritdoc/>
    public override void InspectorEditFinish()
    {
        _undoManager?.Finish();
    }

    #endregion

    #region Tree Context

    /// <inheritdoc/>
    protected override void OnTreeBeginMacro(string name)
    {
        _undoManager?.BeginMacro(name);
    }

    /// <inheritdoc/>
    protected override void OnTreeEndMacro(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            _undoManager?.EndMacro(name);
        }
        else
        {
            _undoManager?.EndMacro(UndoRedoManager.MacroDeriveName.FromFirst);
        }
    }

    /// <inheritdoc/>
    protected override bool OnTreeDoAction(UndoRedoAction action)
    {
        if (_undoManager != null)
        {
            _undoManager.Do(action);
        }
        else
        {
            action.Do();
        }

        return true;
    }

    /// <inheritdoc/>
    protected override void OnTreeEditFinish()
    {
        _undoManager?.Finish();
    }

    #endregion

    #region IViewUndo

    /// <inheritdoc/>
    public bool CanUndo => _undoManager?.CanUndo ?? false;

    /// <inheritdoc/>
    public bool CanRedo => _undoManager?.CanRedo ?? false;

    /// <inheritdoc/>
    public string UndoText => _undoManager?.PrevActionInfo?.Name;

    /// <inheritdoc/>
    public string RedoText => _undoManager?.NextActionInfo?.Name;

    /// <inheritdoc/>
    public void Undo()
    {
        _undoManager?.Undo();
    }

    /// <inheritdoc/>
    public void Redo()
    {
        _undoManager?.Redo();
    }

    #endregion

    #region ISyncStateRecord

    /// <inheritdoc/>
    void ISyncStateRecord.Record(ISyncObject obj)
    {
        _undoManager?.Do(new SnapshotObjectUndoAction(obj, null, this));
    }

    /// <inheritdoc/>
    void ISyncStateRecord.Record(ISyncList list)
    {
        _undoManager?.Do(new SnapshotListUndoAction(list, null, this));
    }

    #endregion
}
