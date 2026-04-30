using Suity.Editor.Services;

namespace Suity.Views.PathTree;

/// <summary>
/// A delayed action that triggers a deep refresh of a populate path node and all its descendants.
/// </summary>
public class DelayRefreshNodeDeepAction : DelayedAction<PopulatePathNode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelayRefreshNodeDeepAction"/> class.
    /// </summary>
    /// <param name="value">The populate path node to refresh.</param>
    public DelayRefreshNodeDeepAction(PopulatePathNode value) : base(value)
    {
    }

    /// <summary>
    /// Executes the deep refresh action on the target node.
    /// </summary>
    public override void DoAction()
    {
        if (Value.Parent != null)
        {
            Value.PopulateUpdateDeep();
        }
    }
}

/// <summary>
/// A delayed action that triggers a refresh of a populate path node.
/// </summary>
public class DelayRefreshNodeAction : DelayedAction<PopulatePathNode>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelayRefreshNodeAction"/> class.
    /// </summary>
    /// <param name="value">The populate path node to refresh.</param>
    public DelayRefreshNodeAction(PopulatePathNode value) : base(value)
    {
    }

    /// <summary>
    /// Executes the refresh action on the target node.
    /// </summary>
    public override void DoAction()
    {
        if (Value.Parent != null)
        {
            Value.PopulateUpdateDeep();
        }
    }
}
