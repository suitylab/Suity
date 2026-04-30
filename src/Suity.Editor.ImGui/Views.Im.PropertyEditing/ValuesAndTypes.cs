using Suity;
using Suity.Collections;
using Suity.NodeQuery;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Represents the columns in a property grid view.
/// </summary>
public enum PropertyGridColumn
{
    /// <summary>
    /// The prefix column, typically used for expand/collapse indicators or icons.
    /// </summary>
    Prefix,
    /// <summary>
    /// The name column displaying the property or field name.
    /// </summary>
    Name,
    /// <summary>
    /// The main column containing the value editor.
    /// </summary>
    Main,
    /// <summary>
    /// The option column for additional actions or context menus.
    /// </summary>
    Option,
}

/// <summary>
/// Represents operations that can be performed on array elements in a property grid.
/// </summary>
public enum ArrayElementOp
{
    /// <summary>
    /// Delete the array element.
    /// </summary>
    Delete,
    /// <summary>
    /// Clone the array element.
    /// </summary>
    Clone,
    /// <summary>
    /// Move the array element up in the collection.
    /// </summary>
    MoveUp,
    /// <summary>
    /// Move the array element down in the collection.
    /// </summary>
    MoveDown,
}

/// <summary>
/// Defines a handler for value actions in the property grid system.
/// </summary>
public interface IValueActionHandler
{
    /// <summary>
    /// Executes the specified value action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void DoAction(IValueAction action);
}

/// <summary>
/// Default implementation of <see cref="IValueActionHandler"/> that simply executes actions directly.
/// </summary>
internal sealed class DefaultValueActionHandler : IValueActionHandler
{
    /// <summary>
    /// Gets the singleton default instance.
    /// </summary>
    public static readonly DefaultValueActionHandler Default = new();

    private DefaultValueActionHandler()
    {
    }

    /// <inheritdoc/>
    public void DoAction(IValueAction action)
    {
        action.DoAction();
    }
}

/// <summary>
/// Represents a target in a hierarchical structure that can be navigated via parent relationships.
/// </summary>
public interface ITarget
{
    /// <summary>
    /// Gets the parent target in the hierarchy, or null if this is a root target.
    /// </summary>
    ITarget? Parent { get; }
    /// <summary>
    /// Gets the sequence of parent objects in the hierarchy.
    /// </summary>
    IEnumerable<object?> GetParentObjects();
}

/// <summary>
/// Represents a target that exposes a property value which can be read and modified in a property grid.
/// </summary>
public interface IValueTarget : ITarget
{
    /// <summary>
    /// Gets the actual name of the property.
    /// </summary>
    string PropertyName { get; }
    /// <summary>
    /// Gets the display name shown in the property grid.
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// Gets a unique identifier for this target.
    /// </summary>
    string Id { get; }
    /// <summary>
    /// Gets the preset type used for type hinting, or null if not applicable.
    /// </summary>
    Type? PresetType { get; }
    /// <summary>
    /// Gets the actual type being edited, or null if not applicable.
    /// </summary>
    Type? EditedType { get; }
    /// <summary>
    /// Gets a value indicating whether this property is read-only.
    /// </summary>
    bool ReadOnly { get; }
    /// <summary>
    /// Gets a value indicating whether this property is optional (can be null).
    /// </summary>
    bool Optional { get; }
    /// <summary>
    /// Gets or sets a value indicating whether the property has multiple different values across selected objects.
    /// </summary>
    bool ValueMultiple { get; set; }
    /// <summary>
    /// Gets or sets the text status for display purposes.
    /// </summary>
    TextStatus Status { get; set; }
    /// <summary>
    /// Gets the display color for this property, or null for default coloring.
    /// </summary>
    Color? Color { get; }
    /// <summary>
    /// Gets the attributes associated with this property, or null if none.
    /// </summary>
    IAttributeGetter? Attributes { get; }

    /// <summary>
    /// Gets the current values of this property across all selected objects.
    /// </summary>
    IEnumerable<object?> GetValues();

    /// <summary>
    /// Sets the values of this property across all selected objects.
    /// </summary>
    /// <param name="objects">The values to set.</param>
    /// <param name="context">Optional setter context.</param>
    void SetValues(IEnumerable<object?> objects, ISetterContext? context = null);

    /// <summary>
    /// Gets the style node reader for this property, or null if not applicable.
    /// </summary>
    public INodeReader? Styles { get; }


    /// <summary>
    /// Gets or sets a value indicating whether this target itself has an error.
    /// </summary>
    bool ErrorSelf { get; set; }
    /// <summary>
    /// Gets a value indicating whether any target in the hierarchy has an error.
    /// </summary>
    bool ErrorInHierarchy { get; }
}

/// <summary>
/// Wraps an <see cref="IValueTarget"/> and converts values between two types (<typeparamref name="TFrom"/> and <typeparamref name="TTo"/>).
/// This allows editing a property through a different type representation.
/// </summary>
/// <typeparam name="TFrom">The source type from the inner target.</typeparam>
/// <typeparam name="TTo">The target type exposed to the editor.</typeparam>
public class ConvertedValueTarget<TFrom, TTo> : IValueTarget
    where TFrom : class
{
    private readonly IValueTarget _inner;

    private readonly Func<TFrom, TTo> _convert;
    private readonly Func<TTo, TFrom> _convertReverse;

    /// <summary>
    /// Initializes a new instance of <see cref="ConvertedValueTarget{TFrom, TTo}"/> with the specified inner target and conversion functions.
    /// </summary>
    /// <param name="inner">The inner value target to wrap.</param>
    /// <param name="convert">The function to convert from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.</param>
    /// <param name="convertReverse">The function to convert from <typeparamref name="TTo"/> back to <typeparamref name="TFrom"/>.</param>
    public ConvertedValueTarget(IValueTarget inner, Func<TFrom, TTo> convert, Func<TTo, TFrom> convertReverse)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _convert = convert ?? throw new ArgumentNullException(nameof(convert));
        _convertReverse = convertReverse ?? throw new ArgumentNullException(nameof(convertReverse));
    }

    /// <inheritdoc/>
    public ITarget? Parent => _inner.Parent;

    /// <inheritdoc/>
    public string PropertyName => _inner.PropertyName;

    /// <inheritdoc/>
    public string DisplayName => _inner.DisplayName;

    /// <inheritdoc/>
    public string Id => _inner.Id;

    /// <inheritdoc/>
    public Type? PresetType => _inner.PresetType;
    /// <inheritdoc/>
    public Type? EditedType => _inner.EditedType;

    /// <inheritdoc/>
    public bool ReadOnly => _inner.ReadOnly;

    /// <inheritdoc/>
    public bool Optional => _inner.Optional;

    /// <inheritdoc/>
    public bool ValueMultiple
    {
        get => _inner.ValueMultiple;
        set => _inner.ValueMultiple = value;
    }

    /// <inheritdoc/>
    public Color? Color => _inner.Color;
    /// <inheritdoc/>
    public IAttributeGetter? Attributes => _inner.Attributes;

    /// <inheritdoc/>
    public INodeReader? Styles => _inner.Styles;


    /// <inheritdoc/>
    public bool ErrorSelf
    {
        get => _inner.ErrorSelf;
        set => _inner.ErrorSelf = value;
    }

    /// <inheritdoc/>
    public bool ErrorInHierarchy => _inner.ErrorInHierarchy;

    /// <inheritdoc/>
    public TextStatus Status
    {
        get => _inner.Status;
        set => _inner.Status = value;
    }

    /// <inheritdoc/>
    public IEnumerable<object?> GetParentObjects() => _inner.GetParentObjects();

    /// <inheritdoc/>
    public IEnumerable<object?> GetValues() 
        => _inner.GetValues().As<TFrom>().Select(o => _convert(o)).OfType<object?>();

    /// <inheritdoc/>
    public void SetValues(IEnumerable<object?> objects, ISetterContext? context = null) 
        => _inner.SetValues(objects.Cast<TTo>().Select(o => _convertReverse(o)), context);
}

/// <summary>
/// Provides data for the <see cref="PropertyGridData.ValueActionRequest"/> event.
/// </summary>
public class ValueActionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the value action associated with this event.
    /// </summary>
    public IValueAction Action { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the action has been handled by an event subscriber.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="ValueActionEventArgs"/> with the specified action.
    /// </summary>
    /// <param name="action">The value action to associate with this event.</param>
    public ValueActionEventArgs(IValueAction action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}

/// <summary>
/// Manages a stack-based system for tracking the current ImGui editor node during property grid rendering.
/// </summary>
internal class PropertyGridSystem
{
    private readonly Stack<ImGuiNode> m_stack = new();

    /// <summary>
    /// Pushes an editor node onto the stack, making it the current active node.
    /// </summary>
    /// <param name="node">The editor node to push. Must not be null.</param>
    public void PushEditorNode(ImGuiNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        m_stack.Push(node);
    }

    /// <summary>
    /// Pops the top editor node from the stack and returns it.
    /// </summary>
    /// <returns>The popped editor node, or null if the stack is empty.</returns>
    public ImGuiNode? PopEditorNode()
    {
        if (m_stack.Count > 0)
        {
            return m_stack.Pop();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the current editor node at the top of the stack without removing it.
    /// </summary>
    public ImGuiNode? CurrentEditorNode => m_stack.Count > 0 ? m_stack.Peek() : null;
}

/// <summary>
/// Represents the data model for a property grid control that handles value actions and selections.
/// Implements the IValueActionHandler interface to process actions on property values.
/// </summary>
public class PropertyGridData : IValueActionHandler
{
    /// <summary>
    /// Gets or sets the name of the grid.
    /// </summary>
    public string GridName { get; set; }

    /// <summary>
    /// Gets or sets the minimum width for the name column.
    /// Default value is 40.
    /// </summary>
    public int NameColumnWidthMin { get; set; } = 40;
    
    /// <summary>
    /// Gets or sets the maximum width for the name column.
    /// Nullable to indicate no maximum limit.
    /// </summary>
    public int? NameColumnWidthMax { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum width for the editor column.
    /// Default value is 40.
    /// </summary>
    public int EditorColumnWidthMin { get; set; } = 40;
    
    /// <summary>
    /// Gets or sets the maximum width for the editor column.
    /// Nullable to indicate no maximum limit.
    /// </summary>
    public int? EditorColumnWidthMax { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple columns are supported.
    /// Default value is true.
    /// </summary>
    public bool SupportMultipleColumn { get; set; } = true;

    /// <summary>
    /// Gets or sets the path to the grid node.
    /// Internal set to restrict modification to the same assembly.
    /// </summary>
    public ImGuiPath? GridNodePath { get; internal set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether field selection is enabled.
    /// Default value is true.
    /// </summary>
    public bool CanSelectField { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the currently selected field data.
    /// Internal set to restrict modification to the same assembly.
    /// </summary>
    public PropertyRowData? SelectedField { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether a delete request has been made.
    /// </summary>
    public bool RequestDelete { get; set; }

    /// <summary>
    /// Event raised when a value action is requested.
    /// </summary>
    public event EventHandler<ValueActionEventArgs>? ValueActionRequest;

    /// <summary>
    /// Event raised when the selection changes.
    /// </summary>
    public event EventHandler<PropertyRowData?>? SelectionChanged;

    /// <summary>
    /// Initializes a new instance of the PropertyGridData class with an empty grid name.
    /// </summary>
    public PropertyGridData()
    {
        GridName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the PropertyGridData class with the specified grid name.
    /// </summary>
    /// <param name="gridName">The name to assign to the grid.</param>
    public PropertyGridData(string gridName)
    {
        GridName = gridName;
    }

    /// <summary>
    /// Executes the specified value action, raising the ValueActionRequest event if registered.
    /// The action is only performed if it's not handled by the event subscribers.
    /// </summary>
    /// <param name="action">The action to be performed.</param>
    public void DoAction(IValueAction action)
    {
        if (ValueActionRequest != null)
        {
            var args = new ValueActionEventArgs(action);
            ValueActionRequest(this, args);

            if (!args.Handled)
            {
                action.DoAction();
            }
        }
        else
        {
            action.DoAction();
        }
    }

    /// <summary>
    /// Sets the currently selected field, raising the SelectionChanged event if the selection changes.
    /// Returns true if the selection was successfully changed, false otherwise.
    /// </summary>
    /// <param name="field">The field data to select, or null to deselect.</param>
    /// <returns>True if the selection was changed, false otherwise.</returns>
    public bool SetSelection(PropertyRowData? field)
    {
        // Return false if field selection is disabled
        if (!CanSelectField)
        {
            return false;
        }

        // Return false if the new field is the same as the current selection
        if (ReferenceEquals(field, SelectedField))
        {
            return false;
        }

        // Return false if the field doesn't belong to this grid
        if (field != null && field.GridData != this)
        {
            return false;
        }

        // Return false if field selection is disabled for the specified field
        if (field is { SelectEnabled: false })
        {
            return false;
        }

        // Deselect the current field if one is selected
        if (SelectedField is { } currentField)
        {
            currentField.IsSelected = false;
        }

        // Set the new selection
        SelectedField = field;

        // Select the new field if not null
        if (field != null)
        {
            field.IsSelected = true;
        }

        // Raise the selection changed event if there are subscribers
        SelectionChanged?.Invoke(this, field);

        return true;
    }
}

/// <summary>
/// Represents a row in a property grid, containing various properties and metadata
/// about a specific node or element in the hierarchy.
/// </summary>
public class PropertyRowData
{
    /// <summary>
    /// Gets the grid data associated with this property row.
    /// </summary>
    public PropertyGridData? GridData { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the root element is missing.
    /// </summary>
    public bool IsRootMissing { get; internal set; }

    /// <summary>
    /// Gets the parent row of this property row in the hierarchy.
    /// </summary>
    public PropertyRowData? ParentRow { get; internal set; }

    /// <summary>
    /// Gets the path to this node in the ImGui hierarchy.
    /// </summary>
    public ImGuiPath? NodePath { get; internal set; }

    /// <summary>
    /// Gets the target object that this property row represents.
    /// </summary>
    public PropertyTarget? Target { get; internal set; }

    /// <summary>
    /// Gets the indentation level of this row in the property grid.
    /// </summary>
    public int Indent { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this row is currently selected.
    /// </summary>
    public bool IsSelected { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether selection is enabled for this row.
    /// Default value is true.
    /// </summary>
    public bool SelectEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the key down request for this row.
    /// </summary>
    public string? KeyDownRequest { get; set; }

    /// <summary>
    /// Initializes a new instance of the PropertyRowData class.
    /// </summary>
    public PropertyRowData()
    {
    }
}
