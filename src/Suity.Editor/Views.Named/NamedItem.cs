using Suity.Drawing;
using Suity.Editor;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System.Drawing;

namespace Suity.Views.Named;

/// <summary>
/// Represents a named item in the editor view hierarchy, supporting synchronization, display, and validation.
/// </summary>
public class NamedItem :
    INamed,
    IViewObject,
    ITextDisplay,
    ITextEdit,
    ISyncPathObject,
    IViewColor,
    IValidate
{
    private NamedRootCollection _root;
    internal INamedItemList _parentList;
    private string _name;

    /// <summary>
    /// Gets the root collection this item belongs to.
    /// </summary>
    public NamedRootCollection Root
    {
        get => _root;
        internal set
        {
            if (_root != value)
            {
                if (_root != null)
                {
                    var root = _root;
                    _root = null;
                    OnInternalRemoved(root);
                }

                _root = value;
                if (_root != null)
                {
                    OnInternalAdded();
                }
            }
        }
    }

    /// <summary>
    /// Gets the parent list that contains this item.
    /// </summary>
    public INamedItemList ParentList => _parentList;

    /// <summary>
    /// Gets the parent node of this item.
    /// </summary>
    public INamedNode ParentNode => _parentList?.ParentNode ?? (INamedNode)this.Root;

    /// <summary>
    /// Gets a value indicating whether this item has a parent.
    /// </summary>
    public bool HasParent => _parentList != null;

    /// <summary>
    /// Gets or sets the name of this item.
    /// </summary>
    public string Name
    {
        get => _name;
        set => ChangeName(value, true);
    }

    /// <summary>
    /// Gets the icon associated with this item.
    /// </summary>
    public ImageDef Icon => OnGetIcon();

    /// <summary>
    /// Gets the zero-based index of this item in its parent list.
    /// </summary>
    /// <returns>The index of the item, or -1 if not in a list.</returns>
    public int GetIndex() => _parentList?.IndexOf(this) ?? -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItem"/> class.
    /// </summary>
    public NamedItem()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedItem"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name for the item.</param>
    public NamedItem(string name) => _name = name;

    /// <summary>
    /// Gets the full path of this item from the root, using the specified splitter.
    /// </summary>
    /// <param name="splitter">The string used to join path segments (default is ".").</param>
    /// <returns>The full path string.</returns>
    public string GetFullPath(string splitter = ".")
    {
        string path = Name;
        var node = ParentNode;

        while (node != null)
        {
            path = $"{node.Name}{splitter}{path}";
            node = node.ParentNode;
        }

        return path;
    }

    /// <summary>
    /// Gets the group path of this item, traversing up through parent groups.
    /// </summary>
    /// <returns>The group path string.</returns>
    public string GetGroupPath()
    {
        string path = string.Empty;
        var group = ParentNode as NamedGroup;

        while (group != null)
        {
            path = group.GroupName + "/" + path;
            group = group.ParentNode as NamedGroup;
        }

        return path.Trim('/').Trim();
    }

    /// <summary>
    /// Changes the name of this item, optionally logging errors on failure.
    /// </summary>
    /// <param name="newName">The new name to set.</param>
    /// <param name="logError">Whether to log a warning if name verification fails.</param>
    /// <returns>True if the name was changed; otherwise, false.</returns>
    protected bool ChangeName(string newName, bool logError = true)
    {
        if (_name == newName)
        {
            return false;
        }

        if (OnVerifyName(newName))
        {
            if (_root?.ContainsItem(this, true) == true)
            {
                _root.Rename(this, newName);
            }
            else
            {
                UpdateName(newName);
            }

            return true;
        }
        else
        {
            if (logError)
            {
                Logs.LogWarning($"Name verify failed : {newName}.");
            }
            
            return false;
        }
    }

    /// <summary>
    /// Updates the name of this item without triggering rename operations.
    /// </summary>
    /// <param name="name">The new name.</param>
    internal void UpdateName(string name)
    {
        if (_name == name)
        {
            return;
        }

        string oldName = _name;
        _name = name;

        OnNameUpdated(oldName, name);
    }

    #region ISyncObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        Name = sync.Sync("Name", Name, SyncFlag.NotNull);
        OnSync(sync, context);
    }

    #endregion

    #region IViewObject

    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        OnSetupView(setup);
        OnSetupViewAppearance(setup);
        OnSetupViewContent(setup);
    }

    #endregion

    #region ITextDisplay

    string ITextDisplay.DisplayText => OnGetDisplayText();

    TextStatus ITextDisplay.DisplayStatus => OnGetTextStatus();

    object ITextDisplay.DisplayIcon => OnGetIcon();

    #endregion

    #region ITextEdit

    bool ITextEdit.CanEditText => OnCanEditText();

    void ITextEdit.SetText(string text, ISyncContext setup)
    {
        OnSetText(text, setup, true);
    }

    #endregion

    #region ISyncPathObject

    /// <summary>
    /// Gets the synchronization path for this item.
    /// </summary>
    public virtual SyncPath GetPath()
    {
        if (_parentList is null)
        {
            return SyncPath.Empty;
        }

        return _parentList.GetPath(this);
    }

    #endregion

    #region IValidate

    /// <summary>
    /// Searches for the specified string within this item and reports matches.
    /// </summary>
    /// <param name="context">The validation context to report findings.</param>
    /// <param name="findStr">The string to search for.</param>
    /// <param name="findOption">The search options to use.</param>
    public virtual void Find(ValidationContext context, string findStr, SearchOption findOption)
    {
        if (Validator.Compare(_name, findStr, findOption))
        {
            context.Report(_name, this);
        }
    }

    /// <summary>
    /// Validates this item and reports any issues to the context.
    /// </summary>
    /// <param name="context">The validation context to report issues.</param>
    public virtual void Validate(ValidationContext context)
    {
    }

    #endregion

    #region IViewColor

    Color? IViewColor.ViewColor => OnGetColor();

    #endregion

    #region Virtual

    /// <summary>
    /// Gets the suggested prefix for naming new items of this type.
    /// </summary>
    protected internal virtual string OnGetSuggestedPrefix() => "Item";

    /// <summary>
    /// Called when this item is internally added to a collection.
    /// </summary>
    internal virtual void OnInternalAdded() => OnAdded();

    /// <summary>
    /// Called when this item is internally removed from a collection.
    /// </summary>
    /// <param name="root">The root collection the item was removed from.</param>
    internal virtual void OnInternalRemoved(NamedRootCollection root) => OnRemoved(root);

    /// <summary>
    /// Called after this item is added to a collection.
    /// </summary>
    protected virtual void OnAdded()
    { }

    /// <summary>
    /// Called after this item is removed from a collection.
    /// </summary>
    /// <param name="root">The root collection the item was removed from.</param>
    protected virtual void OnRemoved(NamedRootCollection root)
    { }

    /// <summary>
    /// Called during synchronization to allow custom sync behavior.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The sync context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    /// <summary>
    /// Called to set up the view for this item.
    /// </summary>
    /// <param name="setup">The view setup object.</param>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    { }

    /// <summary>
    /// Called to set up the visual appearance of the view.
    /// </summary>
    /// <param name="setup">The view setup object.</param>
    protected virtual void OnSetupViewAppearance(IViewObjectSetup setup)
    { }

    /// <summary>
    /// Called to set up the content of the view.
    /// </summary>
    /// <param name="setup">The view setup object.</param>
    protected virtual void OnSetupViewContent(IViewObjectSetup setup)
    { }

    /// <summary>
    /// Called after the name of this item has been updated.
    /// </summary>
    /// <param name="oldName">The previous name.</param>
    /// <param name="newName">The new name.</param>
    protected virtual void OnNameUpdated(string oldName, string newName)
    { }

    /// <summary>
    /// Gets the icon to display for this item.
    /// </summary>
    protected virtual ImageDef OnGetIcon() => this.GetType().ToDisplayIcon();

    /// <summary>
    /// Gets the text status for display purposes.
    /// </summary>
    protected virtual TextStatus OnGetTextStatus() => TextStatus.Normal;

    /// <summary>
    /// Gets a value indicating whether the text of this item can be edited.
    /// </summary>
    protected virtual bool OnCanEditText() => true;

    /// <summary>
    /// Gets the display text for this item.
    /// </summary>
    protected virtual string OnGetDisplayText() => Name;

    /// <summary>
    /// Sets the text of this item.
    /// </summary>
    /// <param name="text">The new text value.</param>
    /// <param name="setup">The sync context.</param>
    /// <param name="showNotice">Whether to show a notice.</param>
    protected virtual void OnSetText(string text, ISyncContext setup, bool showNotice)
        => NamedExternal._external.SetText(this, text, setup, showNotice);

    /// <summary>
    /// Verifies if the specified name is valid for this item.
    /// </summary>
    /// <param name="name">The name to verify.</param>
    /// <returns>True if the name is valid; otherwise, false.</returns>
    protected internal virtual bool OnVerifyName(string name) => NamingVerifier.VerifyIdentifier(name);

    /// <summary>
    /// Gets the color to use for displaying this item.
    /// </summary>
    protected virtual Color? OnGetColor() => null;

    /// <summary>
    /// Returns a string representation of this item.
    /// </summary>
    public override string ToString() => $"{GetType().Name} {Name}";

    #endregion

    #region Virtual - Field list

    /// <summary>
    /// Called when a field is added to a field list associated with this item.
    /// </summary>
    /// <param name="list">The field list.</param>
    /// <param name="item">The field that was added.</param>
    /// <param name="isNew">Whether the item is newly created.</param>
    protected internal virtual void OnFieldListItemAdded(NamedFieldList list, NamedField item, bool isNew)
    {
    }

    /// <summary>
    /// Called when a field is removed from a field list associated with this item.
    /// </summary>
    /// <param name="list">The field list.</param>
    /// <param name="item">The field that was removed.</param>
    protected internal virtual void OnFieldListItemRemoved(NamedFieldList list, NamedField item)
    {
    }

    /// <summary>
    /// Called when a field in a field list associated with this item is updated.
    /// </summary>
    /// <param name="list">The field list.</param>
    /// <param name="item">The field that was updated.</param>
    /// <param name="forceUpdate">Whether to force a full update.</param>
    protected internal virtual void OnFieldListItemUpdated(NamedFieldList list, NamedField item, bool forceUpdate)
    {
    }

    /// <summary>
    /// Called when a field in a field list associated with this item is renamed.
    /// </summary>
    /// <param name="list">The field list.</param>
    /// <param name="item">The field that was renamed.</param>
    /// <param name="oldName">The previous name of the field.</param>
    protected internal virtual void OnFieldListItemRenamed(NamedFieldList list, NamedField item, string oldName)
    {
    }

    /// <summary>
    /// Called to arrange items in a field list associated with this item.
    /// </summary>
    /// <param name="list">The field list to arrange.</param>
    protected internal virtual void OnFieldListArrageItem(NamedFieldList list)
    {
    }

    #endregion
}
