namespace Suity.Views.Menu;

/// <summary>
/// Represents a separator line in a menu.
/// </summary>
public sealed class MenuSeparator : MenuBase
{
    internal IMenuItemView _separator;

    private bool _visible = true;

    /// <inheritdoc/>
    public override void SetupView(IMenuItemView parent)
    {
        _separator = parent.CreateSeparator();
    }

    /// <inheritdoc/>
    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            if (_separator != null)
            {
                _separator.Visible = value;
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return "---";
    }
}