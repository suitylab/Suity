using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.UndoRedos;

/// <summary>
/// An undo/redo action that groups multiple actions into a single macro action.
/// </summary>
public class UndoRedoMacroAction : UndoRedoAction
{
    private readonly UndoRedoAction[] macro = null;
    private readonly string name = null;

    /// <summary>
    /// Gets the name of this macro action.
    /// </summary>
    public override string Name => this.name;

    /// <summary>
    /// Gets a value indicating whether this macro action contains no actions.
    /// </summary>
    public override bool IsVoid => this.macro == null || this.macro.Length == 0;

    /// <summary>
    /// Gets a value indicating whether any action in this macro modifies the document.
    /// </summary>
    public override bool Modifying => this.macro?.Any(o => o.Modifying) == true;

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoRedoMacroAction"/> class.
    /// </summary>
    /// <param name="name">The name of the macro.</param>
    /// <param name="actions">The collection of actions to include in the macro.</param>
    public UndoRedoMacroAction(string name, IEnumerable<UndoRedoAction> actions)
    {
        if (actions == null) throw new ArgumentNullException(nameof(actions));
        this.macro = actions.Where(o => o != null && !o.IsVoid).ToArray();

        if (this.macro.Length == 1)
        {
            this.name = this.macro[0].Name;
        }
        else
        {
            this.name = name ?? string.Format("Macro: {0} Actions", this.macro.Length);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoRedoMacroAction"/> class.
    /// </summary>
    /// <param name="name">The name of the macro.</param>
    /// <param name="actions">The array of actions to include in the macro.</param>
    public UndoRedoMacroAction(string name, params UndoRedoAction[] actions) : this(name, actions as IEnumerable<UndoRedoAction>)
    {
    }

    /// <summary>
    /// Determines whether the specified action can be appended to this macro action.
    /// </summary>
    /// <param name="action">The action to check.</param>
    /// <returns>True if the action can be appended; otherwise, false.</returns>
    public override bool CanAppend(UndoRedoAction action)
    {
        UndoRedoMacroAction castAction = action as UndoRedoMacroAction;

        if (castAction == null) return false;
        if (castAction.macro.Length != this.macro.Length) return false;
        for (int i = 0; i < this.macro.Length; i++)
        {
            if (!this.macro[i].CanAppend(castAction.macro[i])) return false;
        }

        return true;
    }

    /// <summary>
    /// Appends the specified action to this macro action.
    /// </summary>
    /// <param name="action">The action to append.</param>
    /// <param name="performAction">Whether to perform the action immediately.</param>
    public override void Append(UndoRedoAction action, bool performAction)
    {
        base.Append(action, performAction);
        UndoRedoMacroAction castAction = action as UndoRedoMacroAction;

        for (int i = 0; i < this.macro.Length; i++)
        {
            this.macro[i].Append(castAction.macro[i], performAction);
        }
    }

    /// <summary>
    /// Executes all actions in the macro.
    /// </summary>
    public override void Do()
    {
        foreach (UndoRedoAction action in this.macro)
        {
            action.Do();
        }
    }

    /// <summary>
    /// Undoes all actions in the macro in reverse order.
    /// </summary>
    public override void Undo()
    {
        foreach (UndoRedoAction action in this.macro.ReverseEnumerable())
        {
            action.Undo();
        }
    }
}