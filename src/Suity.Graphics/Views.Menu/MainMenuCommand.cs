using Suity.Drawing;

namespace Suity.Views.Menu;

/// <summary>
/// Main menu bar, automatically implements expand state checking.
/// </summary>
public class MainMenuCommand : MenuCommand
{
    /// <summary>
    /// Creates a new instance of MainMenuCommand.
    /// </summary>
    public MainMenuCommand() : base()
    {
    }

    /// <summary>
    /// Creates a new instance with text and optional icon.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The optional icon.</param>
    public MainMenuCommand(string text, ImageDef icon = null) : base(text, icon)
    {
    }

    /// <summary>
    /// Creates a new instance with key, text, and optional icon.
    /// </summary>
    /// <param name="key">The unique identifier.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The optional icon.</param>
    public MainMenuCommand(string key, string text, ImageDef icon = null) : base(key, text, icon)
    {
    }

    /// <inheritdoc/>
    protected override void OnDropDown()
    {
        base.OnDropDown();

        // When expanding submenu, also trigger submenu check state, including all sub-level menus.
        PopUp(0, []);
    }
}
