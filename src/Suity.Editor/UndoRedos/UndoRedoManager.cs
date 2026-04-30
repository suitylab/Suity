using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.UndoRedos;

/// <summary>
/// Manages undo and redo operations by maintaining a stack of actions.
/// </summary>
public class UndoRedoManager : IViewUndo
{
    /// <summary>
    /// Specifies how to derive the name for a macro action.
    /// </summary>
    public enum MacroDeriveName
    {
        /// <summary>
        /// Uses a generic name for the macro.
        /// </summary>
        UseGeneric,
        /// <summary>
        /// Derives the name from the first action in the macro.
        /// </summary>
        FromFirst,
        /// <summary>
        /// Derives the name from the last action in the macro.
        /// </summary>
        FromLast
    }

    private readonly List<UndoRedoAction> _actionStack = [];
    private int _actionIndex = -1;
    private string _macroName = null;
    private int _macroBeginCount = 0;
    private readonly List<UndoRedoAction> _macroList = [];
    private int _maxActions = 50;
    private bool _lastActionFinished = false;

    private bool _working;

    /// <summary>
    /// Occurs when the undo/redo stack changes.
    /// </summary>
    public event EventHandler StackChanged = null;

    /// <summary>
    /// Occurs when a modifying action is performed.
    /// </summary>
    public event EventHandler Modified = null;

    /// <summary>
    /// Gets or sets the maximum number of actions that can be stored for undo.
    /// </summary>
    public int MaxUndoActions
    {
        get { return _maxActions; }
        set { _maxActions = Math.Max(value, 1); }
    }

    /// <summary>
    /// Gets a value indicating whether an undo operation can be performed.
    /// </summary>
    public bool CanUndo => PrevAction != null;
    /// <summary>
    /// Gets a value indicating whether a redo operation can be performed.
    /// </summary>
    public bool CanRedo => NextAction != null;
    /// <summary>
    /// Gets the display text for the next undo operation.
    /// </summary>
    public string UndoText => PrevActionInfo?.Name;
    /// <summary>
    /// Gets the display text for the next redo operation.
    /// </summary>
    public string RedoText => NextActionInfo?.Name;
    /// <summary>
    /// Gets the information about the previous action that can be undone.
    /// </summary>
    public IUndoRedoActionInfo PrevActionInfo => PrevAction;
    /// <summary>
    /// Gets the information about the next action that can be redone.
    /// </summary>
    public IUndoRedoActionInfo NextActionInfo => NextAction;
    private UndoRedoAction PrevAction => _actionIndex < _actionStack.Count && _actionIndex >= 0 ? _actionStack[_actionIndex] : null;
    private UndoRedoAction NextAction => _actionIndex + 1 < _actionStack.Count && _actionIndex + 1 >= 0 ? _actionStack[_actionIndex + 1] : null;

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoRedoManager"/> class.
    /// </summary>
    public UndoRedoManager()
    {
    }

    /// <summary>
    /// Clears all actions from the undo/redo stack.
    /// </summary>
    public void Clear()
    {
        _actionStack.Clear();
        _actionIndex = -1;
        _lastActionFinished = false;
        OnStackChanged();
    }

    /// <summary>
    /// Executes a macro action with the specified name.
    /// </summary>
    /// <param name="macroName">The name of the macro.</param>
    /// <param name="macro">The actions to include in the macro.</param>
    public void Do(string macroName, params UndoRedoAction[] macro)
    {
        Do(new UndoRedoMacroAction(macroName, macro));
    }

    /// <summary>
    /// Executes the specified action and adds it to the undo stack.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void Do(UndoRedoAction action)
    {
        //Adding new actions is prohibited while Undo Redo is working
        if (_working)
        {
            return;
        }

        AppendAction(action, true);

        //Debug.WriteLine($"DoAction : {action}");
    }

    /// <summary>
    /// Marks the last action as finished, allowing subsequent actions to not be appended to it.
    /// </summary>
    public void Finish()
    {
        if (_macroBeginCount != 0) return;
        _lastActionFinished = true;
    }

    /// <summary>
    /// Redoes the next action in the stack.
    /// </summary>
    public void Redo()
    {
        if (_working)
        {
            throw new InvalidOperationException();
        }

        UndoRedoAction action = NextAction;
        if (action == null)
        {
            return;
        }

        try
        {
            _working = true;
            _actionIndex++;
            action.Do();
        }
        finally
        {
            _working = false;
        }

        if (action.Modifying)
        {
            OnModified();
        }
        OnStackChanged();
    }

    /// <summary>
    /// Undoes the previous action in the stack.
    /// </summary>
    public void Undo()
    {
        if (_working)
        {
            throw new InvalidOperationException();
        }

        UndoRedoAction action = PrevAction;
        if (action == null)
        {
            return;
        }

        try
        {
            _working = true;
            _actionIndex--;
            action.Undo();
        }
        finally
        {
            _working = false;
        }

        if (action.Modifying)
        {
            OnModified();
        }
        OnStackChanged();
    }

    private void AppendAction(UndoRedoAction action, bool performAction)
    {
        if (action.IsVoid)
        {
            return;
        }

        if (_macroBeginCount > 0)
        {
            UndoRedoAction prev = _macroList.Count > 0 ? _macroList[_macroList.Count - 1] : null;
            if (prev?.CanAppend(action) == true)
            {
                prev.Append(action, performAction);
            }
            else
            {
                _macroList.Add(action);
                if (performAction)
                {
                    action.Do();
                    if (action.Modifying)
                    {
                        OnModified();
                    }
                }
            }
        }
        else
        {
            //if (Sandbox.IsActive)
            //{
            //    if (performAction) action.Do();
            //    return;
            //}

            bool hadNext = false;
            if (_actionStack.Count - _actionIndex - 1 > 0)
            {
                _actionStack.RemoveRange(_actionIndex + 1, _actionStack.Count - _actionIndex - 1);
                hadNext = true;
            }

            UndoRedoAction prev = PrevAction;
            if (!_lastActionFinished && !hadNext && prev?.CanAppend(action) == true)
            {
                prev.Append(action, performAction);
            }
            else
            {
                _lastActionFinished = false;
                _actionStack.Add(action);
                _actionIndex++;
                if (performAction)
                {
                    action.Do();
                    if (action.Modifying)
                    {
                        OnModified();
                    }
                }
            }

            if (_actionStack.Count > _maxActions)
            {
                _actionIndex -= _actionStack.Count - _maxActions;
                _actionStack.RemoveRange(0, _actionStack.Count - _maxActions);
            }

            OnStackChanged();
        }
    }

    /// <summary>
    /// Begins recording a macro action. Actions performed while recording will be grouped together.
    /// </summary>
    /// <param name="name">Optional name for the macro.</param>
    public void BeginMacro(string name = null)
    {
        if (_macroBeginCount == 0 && name != null)
        {
            _macroName = name;
        }
        _macroBeginCount++;

        //Debug.WriteLine($"BeginMacro : {name}");
    }

    /// <summary>
    /// Ends the current macro recording and adds the grouped actions as a single macro action.
    /// </summary>
    /// <param name="name">The name for the macro.</param>
    public void EndMacro(string name)
    {
        if (_macroBeginCount == 0)
        {
            throw new InvalidOperationException("Attempting to end a non-existent macro recording");
        }

        _macroBeginCount--;
        if (_macroBeginCount == 0)
        {
            if (_macroList.Count == 1)
            {
                AppendAction(_macroList[0], false);
            }
            else
            {
                AppendAction(new UndoRedoMacroAction(name ?? _macroName, _macroList), false);
            }
            _macroList.Clear();
        }

        //Debug.WriteLine($"EndMacro : {name}");
    }

    /// <summary>
    /// Ends the current macro recording with a name derived from the macro actions.
    /// </summary>
    /// <param name="name">Specifies how to derive the macro name.</param>
    public void EndMacro(MacroDeriveName name = MacroDeriveName.UseGeneric)
    {
        string nameStr = null;

        if (_macroList.Count > 0)
        {
            if (name == MacroDeriveName.FromFirst)
            {
                nameStr = _macroList.First().Name;
            }
            if (name == MacroDeriveName.FromLast)
            {
                nameStr = _macroList.Last().Name;
            }
        }

        EndMacro(nameStr);
    }

    private void OnStackChanged()
    {
        StackChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnModified()
    {
        Modified?.Invoke(this, EventArgs.Empty);
    }
}