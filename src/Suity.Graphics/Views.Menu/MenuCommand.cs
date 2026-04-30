using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Views.Menu;

/// <summary>
/// Abstract base class representing a menu command in the UI
/// </summary>
public abstract class MenuCommand : MenuBase
{
    // Private fields for menu properties
    /// <summary>
    /// Text displayed for the menu command.
    /// </summary>
    private string _text;
    /// <summary>
    /// Icon associated with the menu command.
    /// </summary>
    private Image _icon;
    /// <summary>
    /// Hot key for the menu command.
    /// </summary>
    private string _hotKey;
    /// <summary>
    /// Visibility flag for the menu command.
    /// </summary>
    private bool _visible = true;
    /// <summary>
    /// Enabled state of the menu command.
    /// </summary>
    private bool _enabled = true;
    /// <summary>
    /// Selected state of the menu command.
    /// </summary>
    private bool _selected;

    /// <summary>
    /// Collection of child menu commands.
    /// </summary>
    private readonly List<MenuBase> _childCommands = [];
    /// <summary>
    /// Types accepted by this menu command.
    /// </summary>
    private HashSet<Type> _acceptedTypes;

    /// <summary>
    /// Internal reference to the menu item view.
    /// </summary>
    internal IMenuItemView _view;

    /// <summary>
    /// Default constructor for MenuCommand
    /// </summary>
    public MenuCommand()
    {
        _text = string.Empty;
        SetupDefaultKey();
    }

    /// <summary>
    /// Constructor for MenuCommand with text and optional icon
    /// </summary>
    /// <param name="text">Text to display for the menu command</param>
    /// <param name="icon">Optional icon to display with the menu command</param>
    public MenuCommand(string text, Image icon = null)
    {
        _text = text ?? string.Empty;
        _icon = icon;
        SetupDefaultKey();
    }

    /// <summary>
    /// Constructor for MenuCommand with key, text, and optional icon
    /// </summary>
    /// <param name="key">Unique identifier for the menu command</param>
    /// <param name="text">Text to display for the menu command</param>
    /// <param name="icon">Optional icon to display with the menu command</param>
    public MenuCommand(string key, string text, Image icon = null)
    {
        Id = key ?? throw new ArgumentNullException(nameof(key));
        _text = text ?? string.Empty;
        _icon = icon;
    }

    // Sets up default key based on the command type name
    private void SetupDefaultKey()
    {
        Id = this.GetType().Name.RemoveFromLast("Command");
    }

    /// <summary>
    /// Gets the unique identifier for the menu command.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Gets or sets the text displayed for the menu command
    /// </summary>
    public string Text
    {
        get => _text;
        protected set
        {
            _text = value;
            _view?.Text = value;
        }
    }

    /// <summary>
    /// Gets or sets the icon associated with the menu command
    /// </summary>
    public Image Icon
    {
        get => _icon;
        protected set
        {
            _icon = value;
            _view?.Image = value;
        }
    }

    /// <summary>
    /// Gets or sets the hot key text for the menu command.
    /// </summary>
    public string HotKey
    {
        get => _hotKey;
        protected set 
        {
            _hotKey = value;
            _view?.HotKey = value;
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the menu command
    /// </summary>
    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            _view?.Visible = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the menu command is enabled
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        protected set
        {
            _enabled = value;
            _view?.Enabled = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the command accepts only one item.
    /// </summary>
    public bool AcceptOneItemOnly { get; set; }

    /// <summary>
    /// Gets or sets the common type accepted by the command.
    /// </summary>
    public Type AcceptedCommonType { get; set; }

    /// <summary>
    /// Gets the object that triggered the menu.
    /// </summary>
    public object Sender { get; private set; }

    /// <summary>
    /// Gets the selected items.
    /// </summary>
    public object[] Selection { get; private set; }

    /// <summary>
    /// Gets the number of selected items.
    /// </summary>
    public int SelectionCount => Selection?.Length ?? 0;

    /// <summary>
    /// Applies the sender object to this command and its children
    /// </summary>
    /// <param name="sender">The sender object</param>
    public void ApplySender(object sender)
    {
        Sender = sender;

        foreach (var childCommand in _childCommands.OfType<MenuCommand>())
        {
            childCommand.ApplySender(sender);
        }
    }

    /// <summary>
    /// Applies the selection to this command and its children
    /// </summary>
    /// <param name="selection">Array of selected objects</param>
    public void ApplySelection(object[] selection)
    {
        Selection = selection ?? [];

        foreach (var childCommand in _childCommands.OfType<MenuCommand>())
        {
            childCommand.ApplySelection(selection);
        }
    }

    /// <summary>
    /// Clears the current selection
    /// </summary>
    public void ClearSelection()
    {
        Selection = [];
    }

    /// <summary>
    /// Gets the collection of child commands.
    /// </summary>
    public IEnumerable<MenuBase> ChildCommands => _childCommands.Select(o => o);

    /// <summary>
    /// Gets the number of child commands.
    /// </summary>
    public int ChildCommandCount => _childCommands.Count;

    /// <summary>
    /// Adds a new command to this menu
    /// </summary>
    /// <param name="command">The menu command to add</param>
    public void AddCommand(MenuBase command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        _childCommands.Add(command);

        if (_view != null)
        {
            command.SetupView(_view);
        }

        (command as MenuCommand)?.ApplySender(this.Sender);
    }

    /// <summary>
    /// Adds a simple command with text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Action<MenuCommand> action = null, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, null, action, checkPopState);
        AddCommand(cmd);

        return cmd;
    }

    /// <summary>
    /// Adds a simple command with text and icon.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Image icon, Action<MenuCommand> action = null, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, icon, action, checkPopState);
        AddCommand(cmd);

        return cmd;
    }

    /// <summary>
    /// Adds a simple command with text, icon, and single-item restriction.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="oneItem">Whether to accept only one item.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Image icon, bool oneItem, Action<MenuCommand> action = null, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, icon, action, checkPopState)
        {
            AcceptOneItemOnly = oneItem
        };
        AddCommand(cmd);

        return cmd;
    }

    /// <summary>
    /// Adds a simple command with text, accepted type, icon, and single-item restriction.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="commentType">The accepted common type.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="oneItem">Whether to accept only one item.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Type commentType, Image icon, bool oneItem, Action<MenuCommand> action = null, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, icon, action, checkPopState)
        {
            AcceptOneItemOnly = oneItem,
            AcceptedCommonType = commentType,
        };
        AddCommand(cmd);

        return cmd;
    }


    /// <summary>
    /// Adds a simple command with async action.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="action">The async action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Func<MenuCommand, Task> action, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, null, ConvertAsyncAction(action), checkPopState);
        AddCommand(cmd);

        return cmd;
    }

    /// <summary>
    /// Adds a simple command with async action and icon.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="action">The async action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Image icon, Func<MenuCommand, Task> action, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, icon, ConvertAsyncAction(action), checkPopState);
        AddCommand(cmd);

        return cmd;
    }

    /// <summary>
    /// Adds a simple command with async action, icon, and single-item restriction.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="oneItem">Whether to accept only one item.</param>
    /// <param name="action">The async action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Image icon, bool oneItem, Func<MenuCommand, Task> action, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, icon, ConvertAsyncAction(action), checkPopState)
        {
            AcceptOneItemOnly = oneItem
        };
        AddCommand(cmd);

        return cmd;
    }

    /// <summary>
    /// Adds a simple command with async action, accepted type, icon, and single-item restriction.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="commentType">The accepted common type.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="oneItem">Whether to accept only one item.</param>
    /// <param name="action">The async action to execute.</param>
    /// <param name="checkPopState">The optional popup state check action.</param>
    /// <returns>The added command.</returns>
    public MenuCommand AddCommand(string text, Type commentType, Image icon, bool oneItem, Func<MenuCommand, Task> action, CheckPopStateAction checkPopState = null)
    {
        var cmd = new SimpleMenuCommand(text, icon, ConvertAsyncAction(action), checkPopState)
        {
            AcceptOneItemOnly = oneItem,
            AcceptedCommonType = commentType,
        };
        AddCommand(cmd);

        return cmd;
    }



    /// <summary>
    /// Adds a separator to the menu
    /// </summary>
    public void AddSeparator()
    {
        AddCommand(new MenuSeparator());
    }

    /// <summary>
    /// Executes the menu command
    /// </summary>
    public virtual void DoCommand()
    {
    }

    /// <summary>
    /// Called when the dropdown menu is opening
    /// </summary>
    protected virtual void OnDropDown()
    {
    }

    /// <summary>
    /// Sets up the view for the menu command
    /// </summary>
    /// <param name="container">The container view</param>
    public override void SetupView(IMenuItemView container)
    {
        if (container is null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        if (_view != null)
        {
            //throw new InvalidOperationException("View is already set.");
        }

        _view = container.CreateChildItemView();
        _view.Text = _text;
        _view.Image = _icon;
        _view.HotKey = _hotKey;
        _view.Visible = _visible;
        _view.Enabled = _enabled;
        _view.Click += (sender, e) => DoCommand();
        _view.DropDownOpening += (sender, e) => OnDropDown();

        foreach (var command in _childCommands)
        {
            command.SetupView(_view);
        }
    }

    /// <summary>
    /// Updates the view of the menu command
    /// </summary>
    public override void UpdateView()
    {
        if (_view != null)
        {
            _view.Text = _text;
            _view.Image = _icon;
            _view.Visible = _visible;
            _view.Enabled = _enabled;
        }

        foreach (var command in _childCommands)
        {
            command.UpdateView();
        }
    }

    /// <summary>
    /// Sets up the container view for the menu
    /// </summary>
    /// <param name="view">The menu item view</param>
    public void SetupContainerView(IMenuItemView view)
    {
        // Because of errors, temporarily allow duplicate view settings
        //if (_view != null)
        //{
        //    throw new InvalidOperationException("View is already set.");
        //}

        _view = view ?? throw new ArgumentNullException(nameof(view));

        foreach (var command in ChildCommands)
        {
            command.SetupView(_view);
        }
    }

    /// <summary>
    /// Automatically hides separators that are consecutive or at the beginning/end.
    /// </summary>
    /// <returns>The number of visible items after hiding separators.</returns>
    private int AutoHideSeparators()
    {
        int num = 0;
        MenuBase last = null;

        for (int i = 0; i < _childCommands.Count; i++)
        {
            if (_childCommands[i] is MenuSeparator s)
            {
                if (num == 0 || last is MenuSeparator)
                {
                    s.Visible = false;
                }
                else
                {
                    s.Visible = true;
                    last = _childCommands[i];
                    num++;
                }
            }
            else
            {
                if (_childCommands[i].Visible)
                {
                    last = _childCommands[i];
                    num++;
                }
            }
        }

        if (last is MenuSeparator menuSeparator)
        {
            menuSeparator.Visible = false;
            num--;
        }

        return num;
    }

    /// <summary>
    /// Finds a command by its key
    /// </summary>
    /// <param name="key">The key of the command to find</param>
    /// <returns>The found command or null</returns>
    public MenuCommand FindCommand(string key)
    {
        return _childCommands.OfType<MenuCommand>().FirstOrDefault(o => o.Id == key);
    }

    /// <summary>
    /// Finds a command of a specific type by its key
    /// </summary>
    /// <typeparam name="T">The type of command to find</typeparam>
    /// <param name="key">The key of the command to find</param>
    /// <returns>The found command or null</returns>
    public T FindCommand<T>(string key)
        where T : MenuCommand
    {
        return _childCommands.OfType<T>().FirstOrDefault(o => o.Id == key);
    }


    /// <summary>
    /// Sorts the child commands using the specified comparison.
    /// </summary>
    /// <param name="comparison">The comparison function.</param>
    public void SortCommands(Comparison<MenuBase> comparison)
    {
        List<MenuBase> cmds = [.. _childCommands];

        Clear();

        cmds.Sort(comparison);
        foreach (var command in cmds)
        {
            AddCommand(command);
        }
    }

    /// <summary>
    /// Clears all child commands and their views
    /// </summary>
    public void Clear()
    {
        _childCommands.Clear();
        _view?.Clear();
    }

    #region Accepted Type

    /// <summary>
    /// Sets up acceptance for a specific type
    /// </summary>
    /// <typeparam name="T">The type to accept</typeparam>
    /// <param name="acceptDerivedTypes">Whether to accept derived types</param>
    protected void AcceptType<T>(bool acceptDerivedTypes = true)
    {
        if (typeof(T) == typeof(object))
        {
            return;
        }

        (_acceptedTypes ??= []).Add(typeof(T));
        if (acceptDerivedTypes)
        {
            foreach (var type in typeof(T).GetDerivedTypes())
            {
                _acceptedTypes.Add(type);
            }
        }
    }

    /// <summary>
    /// Checks if a type is accepted by this menu command
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is accepted</returns>
    public bool IsTypeAccepted(Type type)
    {
        return _acceptedTypes is null || _acceptedTypes.Contains(type);
    }

    /// <summary>
    /// Pops up the menu with specified selection and types
    /// </summary>
    /// <param name="selectionCount">Number of selected items</param>
    /// <param name="types">Collection of types</param>
    /// <param name="commonNodeType">Common node type</param>
    /// <param name="selection">Selected objects</param>
    /// <returns>Number of visible items after hiding separators</returns>
    public int PopUp(int selectionCount, ICollection<Type> types, Type commonNodeType = null, object[] selection = null)
    {
        commonNodeType ??= types.GetCommonType();

        if (selection != null)
        {
            ApplySelection(selection);
        }
        else
        {
            ClearSelection();
        }

        OnPopUp(selectionCount, types, commonNodeType);

        foreach (var subCommand in ChildCommands.OfType<MenuCommand>())
        {
            subCommand.PopUp(selectionCount, types, commonNodeType, selection);
        }

        return AutoHideSeparators();
    }

    /// <summary>
    /// Called when the menu is popping up
    /// </summary>
    /// <param name="selectionCount">Number of selected items</param>
    /// <param name="types">Collection of types</param>
    /// <param name="commonNodeType">Common node type</param>
    protected virtual void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        if (AcceptOneItemOnly && selectionCount != 1)
        {
            Visible = false;

            return;
        }

        if (AcceptedCommonType != null)
        {
            Visible = AcceptedCommonType.IsAssignableFrom(commonNodeType);

            return;
        }

        Visible = types.Count == 0 || types.All(o => IsTypeAccepted(o));
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
        return Text ?? base.ToString();
    }


    /// <summary>
    /// Converts an async function to an async action.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <param name="taskAction">The async function.</param>
    /// <returns>An async action.</returns>
    private static Action<T> ConvertAsyncAction<T>(Func<T, Task> taskAction)
    {
        return async o => await taskAction(o);
    }
}

/// <summary>
/// Generic abstract class for menu commands that work with a specific target type.
/// </summary>
/// <typeparam name="T">The target type.</typeparam>
public abstract class MenuCommand<T> : MenuCommand where T : class
{
    /// <summary>
    /// Gets the target object for this command.
    /// </summary>
    public T Target { get; }

    /// <summary>
    /// Constructor for MenuCommand with target.
    /// </summary>
    /// <param name="target">The target object.</param>
    public MenuCommand(T target)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    /// <summary>
    /// Constructor for MenuCommand with target, text, and optional icon.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="text">Text to display for the menu command.</param>
    /// <param name="icon">Optional icon to display with the menu command.</param>
    public MenuCommand(T target, string text, Image icon = null)
        : base(text, icon)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }
}
