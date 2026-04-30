using Suity.Editor.Documents.Linked;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System.Collections.Generic;

namespace Suity.Editor.Design;

/// <summary>
/// Represents a group in the editor.
/// </summary>
[DisplayText("Group", "*CoreIcon|Group")]
public class DesignGroup : SNamedGroup,
    IHasAttributeDesign,
    IAttributeGetter,
    IDesignObject
{
    private readonly SArrayAttributeDesign _attributes = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="DesignGroup"/> class.
    /// </summary>
    public DesignGroup()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignGroup"/> class with the specified group name.
    /// </summary>
    public DesignGroup(string groupName)
        : base(groupName)
    {
    }

    /// <summary>
    /// Gets the type order for sorting.
    /// </summary>
    public virtual int TypeOrder => 0;

    /// <summary>
    /// Gets the display name for this type.
    /// </summary>
    public virtual string TypeDisplayName => "Group";

    #region IAttributeDesign

    /// <summary>
    /// Gets the attribute design for this group.
    /// </summary>
    public IAttributeDesign Attributes => _attributes;

    #endregion

    #region IHasAttribute

    public IEnumerable<object> GetAttributes() => _attributes.GetAttributes();

    public IEnumerable<object> GetAttributes(string typeName) => _attributes.GetAttributes(typeName);

    public IEnumerable<T> GetAttributes<T>() where T : class => _attributes.GetAttributes<T>();

    #endregion

    #region IViewDesignObject

    SArray IDesignObject.DesignItems => _attributes.Array;
    string IDesignObject.DesignPropertyName => "Attributes";
    string IDesignObject.DesignPropertyDescription => "Property";

    #endregion

    #region Override

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Attributes", _attributes.Array, SyncFlag.GetOnly);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportExtended())
        {
            setup.ExtendedField(_attributes.Array, new ViewProperty("Attributes", "Property"));
        }
    }

    #endregion
}