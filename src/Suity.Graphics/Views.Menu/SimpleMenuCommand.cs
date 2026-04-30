using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Menu;

/// <summary>
/// Delegate for checking popup state of a menu command.
/// </summary>
/// <param name="cmd">The menu command.</param>
/// <param name="selectionCount">The number of selected items.</param>
/// <param name="types">The collection of types.</param>
/// <param name="commonNodeType">The common node type.</param>
public delegate void CheckPopStateAction(MenuCommand cmd, int selectionCount, ICollection<Type> types, Type commonNodeType);

/// <summary>
/// A simple menu command implementation with action callbacks.
/// </summary>
public class SimpleMenuCommand : MenuCommand
{
    /// <summary>
    /// Gets or sets the action to execute when the command is invoked.
    /// </summary>
    public Action<MenuCommand> CommandAction { get; set; }

    /// <summary>
    /// Gets or sets the action to check popup state.
    /// </summary>
    public CheckPopStateAction CheckPopStateAction { get; set; }

    /// <summary>
    /// Creates a new instance with text and optional icon.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The optional icon.</param>
    public SimpleMenuCommand(string text, Image icon = null)
        : base(text, icon)
    {
    }

    /// <summary>
    /// Creates a new instance with text, icon, and actions.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    public SimpleMenuCommand(string text, Image icon, Action<MenuCommand> action, CheckPopStateAction checkPopState = null)
        : base(text, icon)
    {
        CommandAction = action;
        CheckPopStateAction = checkPopState;
    }

    /// <summary>
    /// Creates a new instance with key, text, icon, and actions.
    /// </summary>
    /// <param name="key">The unique identifier.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    public SimpleMenuCommand(string key, string text, Image icon, Action<MenuCommand> action, CheckPopStateAction checkPopState = null)
        : base(key, text, icon)
    {
        CommandAction = action;
        CheckPopStateAction = checkPopState;
    }

    /// <summary>
    /// Creates a new instance with key, text, hotkey, icon, and actions.
    /// </summary>
    /// <param name="key">The unique identifier.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="hotKey">The hot key text.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    public SimpleMenuCommand(string key, string text, string hotKey, Image icon, Action<MenuCommand> action, CheckPopStateAction checkPopState = null)
        : base(key, text, icon)
    {
        HotKey = hotKey;
        CommandAction = action;
        CheckPopStateAction = checkPopState;
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
        if (!Visible)
        {
            return;
        }

        CheckPopStateAction?.Invoke(this, selectionCount, types, commonNodeType);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (ChildCommandCount == 0)
        {
            CommandAction?.Invoke(this);
        }
    }
}