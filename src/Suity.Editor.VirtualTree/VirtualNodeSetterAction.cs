using Suity.Synchonizing;
using Suity.UndoRedos;
using System;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Represents an action that sets values on virtual nodes, supporting undo/redo operations.
/// </summary>
public abstract class VirtualNodeSetterAction : UndoRedoAction, ISyncContext
{
    private readonly VirtualTreeModel _model;
    private readonly object _value;
    private readonly string _propertyName;

    /// <summary>
    /// Gets or sets a value indicating whether this is the first execution of the action.
    /// When true, value refresh is skipped on first execution.
    /// </summary>
    public bool FirstDo { get; set; } = true;

    /// <summary>
    /// Initializes a new instance with a model reference.
    /// </summary>
    /// <param name="model">The tree model to operate on.</param>
    public VirtualNodeSetterAction(VirtualTreeModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    /// Initializes a new instance with a node reference.
    /// </summary>
    /// <param name="node">The node to get the model and displayed value from.</param>
    public VirtualNodeSetterAction(VirtualNode node)
        : this(node.FindModel())
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        _value = node.DisplayedValue;
    }

    /// <summary>
    /// Initializes a new instance with a node and property name.
    /// </summary>
    /// <param name="node">The node to get the model and displayed value from.</param>
    /// <param name="propertyName">The name of the property being modified.</param>
    public VirtualNodeSetterAction(VirtualNode node, string propertyName)
        : this(node.FindModel())
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        _value = node.DisplayedValue;
        _propertyName = propertyName;
    }

    /// <summary>
    /// Initializes a new instance with explicit model, value, and property name.
    /// </summary>
    /// <param name="model">The tree model to operate on.</param>
    /// <param name="value">The value being set.</param>
    /// <param name="propertyName">The name of the property being modified.</param>
    public VirtualNodeSetterAction(VirtualTreeModel model, object value, string propertyName)
        : this(model)
    {
        _value = value;
        _propertyName = propertyName;
    }

    /// <summary>
    /// Gets the tree model this action operates on.
    /// </summary>
    public VirtualTreeModel Model => _model;

    /// <inheritdoc/>
    public override void Do()
    {
        //No need to refresh on first execution
        if (!FirstDo)
        {
            _model.PerformGetValue();
        }
        FirstDo = false;

        _model.NotifyNodeEdited(_value, _propertyName);
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        _model.PerformGetValue();
        _model.NotifyNodeEdited(_value, _propertyName);
    }

    #region ISyncContext

    object ISyncContext.Parent => null;

    object IServiceProvider.GetService(Type serviceType)
    {
        return _model.ServiceProvider?.GetService(serviceType);
    }

    #endregion
}
