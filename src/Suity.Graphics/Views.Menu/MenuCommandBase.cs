namespace Suity.Views.Menu;

/// <summary>
/// Abstract base class for all menu items.
/// </summary>
public abstract class MenuBase
{
    /// <summary>
    /// Sets up the view for this menu item.
    /// </summary>
    /// <param name="parent">The parent menu item view.</param>
    public virtual void SetupView(IMenuItemView parent)
    {
    }

    /// <summary>
    /// Updates the view to reflect current state.
    /// </summary>
    public virtual void UpdateView()
    {
    }

    /// <summary>
    /// Gets or sets the visibility of the menu item.
    /// </summary>
    public virtual bool Visible { get; set; }
}