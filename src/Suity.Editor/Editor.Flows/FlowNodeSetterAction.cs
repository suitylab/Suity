using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.UndoRedos;
using System;

namespace Suity.Editor.Flows;

/// <summary>
/// An undo/redo action for setting a property on a flow node.
/// </summary>
public class FlowNodeSetterAction(FlowNode flowNode, string name, object value, bool update = false)
    : UndoRedoAction, ISyncContext
{
    private readonly object _oldValue = flowNode.GetProperty(name);

    /// <summary>
    /// Gets the name of the action.
    /// </summary>
    public override string Name => name;

    /// <summary>
    /// Executes the action.
    /// </summary>
    public override void Do()
    {
        flowNode.SetProperty(name, value, this);

        if (update)
        {
            flowNode.UpdateQueued();
        }
    }

    /// <summary>
    /// Undoes the action.
    /// </summary>
    public override void Undo()
    {
        flowNode.SetProperty(name, _oldValue, this);

        if (update)
        {
            flowNode.UpdateQueued();
        }
    }

    /// <summary>
    /// Gets the parent sync context.
    /// </summary>
    object ISyncContext.Parent => null;

    /// <summary>
    /// Gets a service of the specified type.
    /// </summary>
    object IServiceProvider.GetService(Type serviceType) => null;
}