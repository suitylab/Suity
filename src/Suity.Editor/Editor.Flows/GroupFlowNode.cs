using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System.Drawing;

namespace Suity.Editor.Flows;

/// <summary>
/// Interface for flow nodes that represent a group.
/// </summary>
public interface IGroupFlowNode
{
    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    string GroupName { get; }
}

/// <summary>
/// A flow node that represents a visual group/container for organizing other nodes.
/// </summary>
[DisplayText("Group", "*CoreIcon|Group")]
//[ToolTipsText("AIGC workflow start node")]
public class GroupFlowNode : FlowNode, IGroupFlowNode
{
    /// <summary>
    /// Gets the default header color for group nodes.
    /// </summary>
    public static Color GroupHeaderColor { get; } = Color.FromArgb(30, 0, 0, 0);


    private readonly StringProperty _groupName = new("GroupName", "Group Name");
    private readonly AssetProperty<ImageAsset> _icon = new("Icon", "Icon");
    private readonly ColorProperty _color = new("Color", "Color");

    /// <summary>
    /// Gets the title color for the node.
    /// </summary>
    public override Color? TitleColor => GroupHeaderColor;

    /// <summary>
    /// Gets the background color for the node.
    /// </summary>
    public override Color? BackgroundColor => _color;

    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string GroupName => _groupName;

    /// <summary>
    /// Gets the icon for the node.
    /// </summary>
    public override Image Icon => _icon.Target?.GetIconSmall();

    /// <summary>
    /// Initializes a new instance of the GroupFlowNode.
    /// </summary>
    public GroupFlowNode()
    {
        _groupName.Text = "Group";
    }

    /// <summary>
    /// Synchronizes the properties of the node.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _groupName.Sync(sync);
        _icon.Sync(sync);
        _color.Sync(sync);
    }

    /// <summary>
    /// Sets up the view properties for the inspector.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _groupName.InspectorField(setup);
        _icon.InspectorField(setup);
        _color.InspectorField(setup);
    }

}
