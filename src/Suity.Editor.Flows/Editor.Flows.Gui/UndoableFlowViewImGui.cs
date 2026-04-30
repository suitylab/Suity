using Suity.UndoRedos;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.Gui;

/// <summary>
/// Extends <see cref="FlowViewImGui"/> with undo/redo support via an <see cref="UndoRedoManager"/>.
/// </summary>
public class UndoableFlowViewImGui : FlowViewImGui, IViewUndo
{
    private UndoRedoManager _undoManager;

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
                _undoManager.Modified -= UndoManager_Modified;
            }

            _undoManager = value;
            if (_undoManager != null)
            {
                _undoManager.Modified += UndoManager_Modified;
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

    private void UndoManager_Modified(object sender, EventArgs e)
    {
        OnDirty();
    }

    /// <inheritdoc/>
    protected override void OnRebuild()
    {
        _undoManager?.Clear();
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

    #region Flow Context

    /// <inheritdoc/>
    protected override void OnFlowBeginMacro(string name)
    {
        _undoManager?.BeginMacro(name);
    }

    /// <inheritdoc/>
    protected override void OnFlowEndMacro(string name)
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
    protected override bool OnFlowDoAction(UndoRedoAction action)
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
    protected override void OnFlowEditFinish()
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
    public void Undo() => _undoManager?.Undo();

    /// <inheritdoc/>
    public void Redo() => _undoManager?.Redo();

    #endregion
}
