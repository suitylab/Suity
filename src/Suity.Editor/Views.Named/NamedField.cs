using Suity.Editor;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System.Drawing;

namespace Suity.Views.Named;

/// <summary>
/// Abstract base class for a named field, representing a configurable property within a named item.
/// </summary>
public abstract class NamedField :
    INamed,
    IViewObject,
    ITextEdit,
    ISyncPathObject,
    IValidate
{
    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedField"/> class.
    /// </summary>
    public NamedField()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedField"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name for the field.</param>
    public NamedField(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Gets the field list that contains this field.
    /// </summary>
    public NamedFieldList List { get; internal set; }

    /// <summary>
    /// Gets or sets the name of this field.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            if (OnVerifyName(value))
            {
                if (List?.Contains(this) == true)
                {
                    List.ChangeName(this, value);
                }
                else
                {
                    UpdateName(value);
                }
            }
            else
            {
                Logs.LogWarning($"Field name verify failed : {value}");
            }
        }
    }

    /// <summary>
    /// Gets the display icon for this field.
    /// </summary>
    public object DisplayIcon => OnGetIcon();

    /// <summary>
    /// Gets the suggested prefix for naming new fields of this type.
    /// </summary>
    public string SuggestedPrefix => OnGetSuggestedPrefix();

    /// <summary>
    /// Updates the name of this field without triggering rename operations.
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
        OnRenamed(oldName, name);
    }

    /// <summary>
    /// Sets a field value and notifies the parent list of the update.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">Reference to the field.</param>
    /// <param name="value">The new value.</param>
    protected void Set<T>(ref T field, T value)
    {
        if (!Equals(field, value))
        {
            field = value;
            List?.OnItemUpdated(this, false);
        }
    }

    #region ISyncObject IInspectorObject IVisionTreeObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        Name = sync.Sync("Name", Name);
        OnSync(sync, context);
    }

    void IViewObject.SetupView(IViewObjectSetup setup) => OnSetupView(setup);

    #endregion

    #region ITextEdit ITextDisplay

    string ITextDisplay.DisplayText => OnGetDisplayText();
    TextStatus ITextDisplay.DisplayStatus => OnGetTextStatus();
    bool ITextEdit.CanEditText => OnCanEditText();

    void ITextEdit.SetText(string text, ISyncContext setup) 
        => OnSetText(text, setup, true);

    #endregion

    #region ISyncPathObject

    /// <summary>
    /// Gets the synchronization path for this field.
    /// </summary>
    public SyncPath GetPath() => List?.GetPath(this) ?? SyncPath.Empty;

    #endregion

    #region IValidate

    /// <summary>
    /// Searches for the specified string within this field and reports matches.
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
    /// Validates this field and reports any issues to the context.
    /// </summary>
    /// <param name="context">The validation context to report issues.</param>
    public virtual void Validate(ValidationContext context)
    {
    }

    #endregion

    #region Virtual

    /// <summary>
    /// Called during synchronization to allow custom sync behavior.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The sync context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    /// <summary>
    /// Called to set up the view for this field.
    /// </summary>
    /// <param name="setup">The view setup object.</param>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    { }

    /// <summary>
    /// Gets the suggested prefix for naming new fields of this type.
    /// </summary>
    protected internal virtual string OnGetSuggestedPrefix() => "Item";

    /// <summary>
    /// Gets the display text for this field.
    /// </summary>
    protected virtual string OnGetDisplayText() => Name;

    /// <summary>
    /// Gets the icon to display for this field.
    /// </summary>
    protected virtual Image OnGetIcon() => CoreIconCache.Field;

    /// <summary>
    /// Gets the text status for display purposes.
    /// </summary>
    protected virtual TextStatus OnGetTextStatus() => TextStatus.Normal;

    /// <summary>
    /// Gets a value indicating whether the text of this field can be edited.
    /// </summary>
    protected virtual bool OnCanEditText() => true;

    /// <summary>
    /// Sets the text of this field.
    /// </summary>
    /// <param name="text">The new text value.</param>
    /// <param name="setup">The sync context.</param>
    /// <param name="showNotice">Whether to show a notice.</param>
    protected virtual void OnSetText(string text, ISyncContext setup, bool showNotice)
        => NamedExternal._external.SetText(this, text, setup, showNotice);

    /// <summary>
    /// Called after the name of this field has been renamed.
    /// </summary>
    /// <param name="oldName">The previous name.</param>
    /// <param name="newName">The new name.</param>
    protected virtual void OnRenamed(string oldName, string newName)
    { }

    /// <summary>
    /// Verifies if the specified name is valid for this field.
    /// </summary>
    /// <param name="name">The name to verify.</param>
    /// <returns>True if the name is valid; otherwise, false.</returns>
    internal protected virtual bool OnVerifyName(string name) 
        => NamingVerifier.VerifyIdentifier(name);

    /// <summary>
    /// Returns a string representation of this field.
    /// </summary>
    public override string ToString() => Name;

    #endregion

    /// <summary>
    /// Notifies the parent list that this field has been updated.
    /// </summary>
    /// <param name="forceUpdate">Whether to force a full update.</param>
    protected void NotifyFieldUpdated(bool forceUpdate = false) 
        => List?.OnItemUpdated(this, forceUpdate);
}
