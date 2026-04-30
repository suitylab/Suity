using Suity.Editor;
using Suity.Editor.Services;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Defines a property grid UI component that inspects and edits object properties via ImGui.
/// </summary>
public interface IPropertyGrid : IDrawImGui, IServiceProvider
{
    /// <summary>
    /// Gets the current inspector context associated with this property grid, if any.
    /// </summary>
    IInspectorContext? Context { get; }

    /// <summary>
    /// Gets the underlying data model for the property grid.
    /// </summary>
    PropertyGridData GridData { get; }

    /// <summary>
    /// Raised when a refresh of the property grid is requested.
    /// </summary>
    public event EventHandler? RequestRefresh;

    /// <summary>
    /// Raised when an undo/redo action should be executed.
    /// </summary>
    public event EventHandler<UndoRedoActionEventArgs>? RequestDoAction;

    /// <summary>
    /// Raised when an object property has been edited.
    /// </summary>
    public event EventHandler<ObjectPropertyEventArgs>? Edited;


    /// <summary>
    /// Gets or sets whether the context menu is shown in the property grid.
    /// </summary>
    bool ShowContextMenu { get; set; }

    /// <summary>
    /// Gets or sets whether the toolbar is shown in the property grid.
    /// </summary>
    bool ShowToolBar { get; set; }

    /// <summary>
    /// Gets a value indicating whether the property grid currently has a target object to inspect.
    /// </summary>
    bool HasTarget { get; }

    /// <summary>
    /// Gets a value indicating whether the property grid is in read-only mode.
    /// </summary>
    bool ReadOnly { get; }


    /// <summary>
    /// Adds a service to the property grid's service container.
    /// </summary>
    /// <typeparam name="T">The type of service to add.</typeparam>
    /// <param name="service">The service instance.</param>
    void AddService<T>(T service) where T : class;


    /// <summary>
    /// Begins inspecting the specified objects, optionally in read-only mode.
    /// </summary>
    /// <param name="objs">The objects to inspect.</param>
    /// <param name="readOnly">Whether the properties should be read-only.</param>
    /// <param name="context">Optional inspector context.</param>
    /// <param name="styles">Optional node reader for styling.</param>
    void InspectObjects(IEnumerable<object> objs, bool readOnly = false, IInspectorContext? context = null, INodeReader? styles = null);

    /// <summary>
    /// Gets the collection of objects currently being inspected.
    /// </summary>
    IEnumerable<object> InspectedObjects { get; }

    /// <summary>
    /// Sets the selection to the specified synchronization path.
    /// </summary>
    /// <param name="path">The path to select.</param>
    /// <param name="rest">The remaining path that could not be resolved.</param>
    void SetSelection(SyncPath path, out SyncPath rest);


    /// <summary>
    /// Executes a value action (such as a property change) through the grid.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void DoAction(IValueAction action);
}