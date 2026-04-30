using Suity.Editor.Documents.Linked;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor;

/// <summary>
/// Base class for design items used in the editor.
/// </summary>
public abstract class BaseDesignItem : SNamedItem, IViewRedirect, IDescriptionDisplay, IPreviewDisplay
{
    private string _description = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDesignItem"/> class.
    /// </summary>
    public BaseDesignItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDesignItem"/> class with the specified name.
    /// </summary>
    public BaseDesignItem(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDesignItem"/> class with the specified builder.
    /// </summary>
    public BaseDesignItem(AssetBuilder builder)
        : base(builder)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDesignItem"/> class with the specified name and builder.
    /// </summary>
    public BaseDesignItem(string name, AssetBuilder builder)
        : base(builder)
    {
        this.Name = name;
    }


    /// <summary>
    /// Gets or sets the description of this design item.
    /// </summary>
    public virtual string Description
    {
        get => _description;
        set
        {
            _description = value;
            AssetBuilder?.SetDescription(value);
        }
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        Description = sync.Sync("Description", Description);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);
        setup.InspectorField(Description, new ViewProperty("Description", "Description"));
    }

    protected virtual object OnGetShowingObject(int viewIndex)
    {
        return this;
    }

    protected override string OnGetDisplayText()
    {
        if (!string.IsNullOrWhiteSpace(_description))
        {
            return _description;
        }

        return Name;
    }

    #region IShowOption

    object IViewRedirect.GetRedirectedObject(int viewId)
    {
        return OnGetShowingObject(viewId);
    }

    #endregion

    #region IPreviewDisplay

    string IPreviewDisplay.PreviewText => string.Empty;

    object IPreviewDisplay.PreviewIcon => null;

    #endregion
}