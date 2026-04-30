using System;
using System.Collections.Generic;

namespace Suity.Views.Menu;

/// <summary>
/// Represents the root menu command that serves as the top-level container.
/// </summary>
public class RootMenuCommand : MenuCommand
{
    /// <summary>
    /// Creates a new instance of RootMenuCommand.
    /// </summary>
    public RootMenuCommand()
        : base()
    {
    }

    /// <summary>
    /// Creates a new instance with the specified key.
    /// </summary>
    /// <param name="key">The unique identifier.</param>
    public RootMenuCommand(string key)
        : base(key, null, null)
    {
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        Visible = true;
    }

    /// <summary>
    /// Creates a root menu command using a builder action.
    /// </summary>
    /// <param name="creation">The action to configure the menu.</param>
    /// <returns>The configured root menu command.</returns>
    public static RootMenuCommand Create(Action<MenuCommand> creation)
    {
        RootMenuCommand cmd = new();
        creation(cmd);

        return cmd;
    }
}