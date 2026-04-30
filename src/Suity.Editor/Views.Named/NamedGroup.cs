using Suity.Editor;
using Suity.Helpers;
using Suity.Synchonizing;
using System.Drawing;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Named;

/// <summary>
/// Represents a named group that organizes named items into logical folders.
/// </summary>
public abstract class NamedGroup : NamedNode
{
    /// <summary>
    /// Gets or sets the display name of this group.
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedGroup"/> class with a default localized name.
    /// </summary>
    public NamedGroup()
    {
        GroupName = L("Group");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedGroup"/> class with the specified group name.
    /// </summary>
    /// <param name="groupName">The display name for the group.</param>
    public NamedGroup(string groupName)
    {
        GroupName = groupName;
    }

    /// <summary>
    /// Gets the suggested prefix for naming new groups.
    /// </summary>
    protected internal override string OnGetSuggestedPrefix() => "###Group";

    /// <summary>
    /// Verifies if the specified name is valid. Groups always return true.
    /// </summary>
    /// <param name="name">The name to verify.</param>
    /// <returns>Always true.</returns>
    protected internal override bool OnVerifyName(string name) => true;

    /// <summary>
    /// Gets the icon to display for this group.
    /// </summary>
    protected override Image OnGetIcon() => CoreIconCache.FolderDesign;

    /// <summary>
    /// Gets the display text for this group, which is the group name.
    /// </summary>
    protected override string OnGetDisplayText() => GroupName;

    /// <summary>
    /// Sets the group name through the view service.
    /// </summary>
    /// <param name="text">The new group name.</param>
    /// <param name="setup">The sync context.</param>
    /// <param name="showNotice">Whether to show a notice.</param>
    protected override void OnSetText(string text, ISyncContext setup, bool showNotice)
    {
        if (GroupName == text)
        {
            return;
        }

        setup.DoServiceAction<IViewSetValue>(v => v.SetValue("GroupName", text));
    }

    /// <summary>
    /// Called during synchronization to sync the group name.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The sync context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        GroupName = sync.Sync("GroupName", GroupName);
    }

    /// <summary>
    /// Returns a string representation of this group.
    /// </summary>
    public override string ToString() => GroupName;
}
