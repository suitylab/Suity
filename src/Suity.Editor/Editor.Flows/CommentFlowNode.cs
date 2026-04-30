using Suity.Synchonizing;
using Suity.Views;
using System.Drawing;

namespace Suity.Editor.Flows;

/// <summary>
/// A flow node that displays a comment with customizable background color.
/// </summary>
public class CommentFlowNode : FlowNode
{
    private readonly TextBlockProperty _comment = new("Comment", "Comment");
    private readonly ColorProperty _color = new("Color", "Color");

    /// <summary>
    /// Gets the background color of the comment node.
    /// </summary>
    public override Color? BackgroundColor => _color;


    /// <summary>
    /// Gets or sets the comment text.
    /// </summary>
    public string CommentText
    {
        get => _comment;
        set => _comment.Text = value;
    }

    /// <summary>
    /// Initializes a new instance of the CommentFlowNode.
    /// </summary>
    public CommentFlowNode()
    {
        _comment.Text = "Comment";
    }

    /// <summary>
    /// Synchronizes the properties of the node.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _comment.Sync(sync);
        _color.Sync(sync);
    }

    /// <summary>
    /// Sets up the view properties for the inspector.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _comment.InspectorField(setup);
        _color.InspectorField(setup);
    }
}
