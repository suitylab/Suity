using Suity.Collections;
using System;
using System.Collections.Generic;

namespace Suity.Views;

/// <summary>
/// Represents a button value that can be clicked or not clicked.
/// </summary>
[Serializable]
public sealed class ButtonValue : IViewValue
{
    /// <summary>
    /// Gets an empty ButtonValue (not clicked).
    /// </summary>
    public static ButtonValue Empty { get; } = new(false);
    /// <summary>
    /// Gets a clicked ButtonValue.
    /// </summary>
    public static ButtonValue Clicked { get; } = new(true);

    private ButtonValue(bool isClicked)
    {
        IsClicked = isClicked;
    }

    /// <summary>
    /// Gets a value indicating whether the button is clicked.
    /// </summary>
    public bool IsClicked { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return IsClicked.ToString();
    }

    /// <summary>
    /// Tries to parse a string into a ButtonValue.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="value">The parsed ButtonValue if successful.</param>
    /// <returns>True if parsing was successful; otherwise, false.</returns>
    public static bool TryParse(string s, out ButtonValue value)
    {
        if (bool.TryParse(s, out bool b))
        {
            value = b ? Clicked : Empty;

            return true;
        }
        else
        {
            value = ButtonValue.Empty;

            return false;
        }
    }
}

/// <summary>
/// Represents a collection of buttons with a clicked button tracking.
/// </summary>
public class MultipleButtonValue
{
    /// <summary>
    /// Represents a single button item.
    /// </summary>
    public class ButtonItem
    {
        /// <summary>
        /// Gets the button key.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Gets the button title.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Gets the button image.
        /// </summary>
        public object Image { get; }
        /// <summary>
        /// Gets the button tooltip.
        /// </summary>
        public string ToolTips { get; }

        /// <summary>
        /// Initializes a new instance of the ButtonItem class.
        /// </summary>
        /// <param name="key">The button key.</param>
        /// <param name="title">The button title.</param>
        /// <param name="image">The button image.</param>
        /// <param name="toolTips">The button tooltip.</param>
        public ButtonItem(string key, string title, object image, string toolTips)
        {
            Key = key;
            Title = title;
            Image = image;
            ToolTips = toolTips;
        }
    }

    private readonly Dictionary<string, ButtonItem> _buttons;

    public MultipleButtonValue()
    {
        _buttons = [];
    }

    private MultipleButtonValue(Dictionary<string, ButtonItem> buttons, string clicked)
    {
        _buttons = buttons;
        ClickedButton = clicked;
    }

    /// <summary>
    /// Gets the key of the clicked button.
    /// </summary>
    public string ClickedButton { get; private set; }

    /// <summary>
    /// Gets all buttons.
    /// </summary>
    public IEnumerable<ButtonItem> Buttons => _buttons.Values.Pass();

    /// <summary>
    /// Gets a button by key.
    /// </summary>
    /// <param name="key">The button key.</param>
    /// <returns>The button item, or null if not found.</returns>
    public ButtonItem GetButton(string key) => _buttons.GetValueSafe(key);

    /// <summary>
    /// Adds a button to the collection.
    /// </summary>
    /// <param name="key">The button key.</param>
    /// <param name="title">The button title.</param>
    /// <param name="image">The button image.</param>
    /// <param name="tooltips">The button tooltip.</param>
    /// <returns>This instance for chaining.</returns>
    public MultipleButtonValue Add(string key, string title = null, object image = null, string tooltips = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"'${nameof(key)}' cannot be null or whitespace.", nameof(key));
        }

        if (_buttons.ContainsKey(key))
        {
            throw new ArgumentException($"Button with key '{key}' already exists.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = key;
        }

        var item = new ButtonItem(key, title, image, tooltips);
        _buttons[key] = item;

        return this;
    }

    /// <summary>
    /// Creates a new instance with a clicked button.
    /// </summary>
    /// <param name="clicked">The key of the clicked button.</param>
    /// <returns>A new instance with the clicked button set, or null if the button doesn't exist.</returns>
    public MultipleButtonValue CreateClicked(string clicked)
    {
        if (_buttons.ContainsKey(clicked))
        {
            return new MultipleButtonValue(_buttons, clicked);
        }
        else
        {
            return null;
        }
    }
}
