using Suity.Editor.Types;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Tree modes for the inspector view.
/// </summary>
public enum InspectorTreeModes
{
    /// <summary>
    /// No tree view.
    /// </summary>
    None,

    /// <summary>
    /// Main tree view.
    /// </summary>
    MainTree,

    /// <summary>
    /// Detail tree view.
    /// </summary>
    DetailTree,
}

/// <summary>
/// Represents a view that displays inspector content in a split layout.
/// Inspecting object that implements this interface will show its detail view in a separate pane.
/// </summary>
public interface IInspectorSplittedView
{
}


/// <summary>
/// Service interface for inspecting and editing objects.
/// </summary>
public interface IInspector
{
    /// <summary>
    /// Inspects a single object.
    /// </summary>
    /// <param name="obj">The object to inspect.</param>
    /// <param name="context">Optional inspector context.</param>
    /// <param name="readOnly">Whether the inspector is read-only.</param>
    /// <param name="supportStyle">Optional style support.</param>
    /// <param name="treeMode">The tree mode.</param>
    void InspectObject(object obj, IInspectorContext context = null, bool readOnly = false,
        ISupportStyle supportStyle = null, InspectorTreeModes treeMode = InspectorTreeModes.None);

    /// <summary>
    /// Inspects multiple objects.
    /// </summary>
    /// <param name="objs">The objects to inspect.</param>
    /// <param name="context">Optional inspector context.</param>
    /// <param name="readOnly">Whether the inspector is read-only.</param>
    /// <param name="supportStyle">Optional style support.</param>
    /// <param name="treeMode">The tree mode.</param>
    void InspectObjects(IEnumerable<object> objs, IInspectorContext context = null, bool readOnly = false,
        ISupportStyle supportStyle = null, InspectorTreeModes treeMode = InspectorTreeModes.None);

    /// <summary>
    /// Checks if an object is selected.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if selected.</returns>
    bool IsObjectSelected(object obj);

    /// <summary>
    /// Updates the inspector display.
    /// </summary>
    void UpdateInspector();

    /// <summary>
    /// Sets the selection in the inspector.
    /// </summary>
    /// <param name="path">The sync path.</param>
    /// <param name="rest">The remaining path.</param>
    /// <param name="skipDetailView">Whether to skip the detail view.</param>
    void SetSelection(SyncPath path, out SyncPath rest, bool skipDetailView = false);

    /// <summary>
    /// Gets or sets the detail tree selection.
    /// </summary>
    IEnumerable<object> DetailTreeSelection { get; set; }

    /// <summary>
    /// Gets or sets the splitter position.
    /// </summary>
    float SplitterPosition { get; set; }

    /// <summary>
    /// Gets the selected field.
    /// </summary>
    DField SelectedField { get; }

    /// <summary>
    /// Gets the current target object.
    /// </summary>
    /// <returns>The current target.</returns>
    object GetCurrentTarget();

    /// <summary>
    /// Performs an action on the inspected object.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    void DoAction(object action);
}

/// <summary>
/// Empty implementation of the inspector service.
/// </summary>
internal class EmptyInspector : IInspector
{
    /// <summary>
    /// Gets the singleton instance of EmptyInspector.
    /// </summary>
    public static EmptyInspector Empty { get; } = new EmptyInspector();

    private EmptyInspector()
    { }

    /// <inheritdoc/>
    public void InspectObject(object obj, IInspectorContext context, bool readOnly, ISupportStyle style, InspectorTreeModes treeMode)
    { }

    /// <inheritdoc/>
    public void InspectObjects(IEnumerable<object> objs, IInspectorContext context, bool readOnly, ISupportStyle style, InspectorTreeModes treeMode)
    { }

    /// <inheritdoc/>
    public bool IsObjectSelected(object obj) => false;

    /// <inheritdoc/>
    public void UpdateInspector()
    { }

    /// <inheritdoc/>
    public void SetSelection(SyncPath path, out SyncPath rest, bool skipDetailView)
    {
        rest = path;
    }

    /// <inheritdoc/>
    public IEnumerable<object> DetailTreeSelection { get; set; }

    /// <inheritdoc/>
    public float SplitterPosition { get; set; }

    /// <inheritdoc/>
    public DField SelectedField => null;

    /// <inheritdoc/>
    public object GetCurrentTarget() => null;

    /// <inheritdoc/>
    public void DoAction(object action)
    {
        return;
    }
}

/// <summary>
/// Context interface for inspector operations.
/// </summary>
public interface IInspectorContext : IServiceProvider
{
    /// <summary>
    /// Called when entering the inspector.
    /// </summary>
    void InspectorEnter();

    /// <summary>
    /// Called when exiting the inspector.
    /// </summary>
    void InspectorExit();

    /// <summary>
    /// Begins a macro operation.
    /// </summary>
    /// <param name="name">Optional macro name.</param>
    void InspectorBeginMacro(string name = null);

    /// <summary>
    /// Performs an undo/redo action.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>True if successful.</returns>
    bool InspectorDoAction(UndoRedoAction action);

    /// <summary>
    /// Ends a macro operation.
    /// </summary>
    /// <param name="name">Optional macro name.</param>
    void InspectorEndMarco(string name = null);

    /// <summary>
    /// Called when an object is edited.
    /// </summary>
    /// <param name="objs">The edited objects.</param>
    /// <param name="propertyName">The property name.</param>
    void InspectorObjectEdited(IEnumerable<object> objs, string propertyName);

    /// <summary>
    /// Called when editing is finished.
    /// </summary>
    void InspectorEditFinish();

    /// <summary>
    /// Gets or sets user data for the inspector.
    /// </summary>
    object InspectorUserData { get; set; }
}

/// <summary>
/// Route interface for inspector navigation.
/// </summary>
public interface IInspectorRoute : IViewRedirect
{
    /// <summary>
    /// Gets the routed tree mode.
    /// </summary>
    /// <returns>The tree mode, or null if not routed.</returns>
    InspectorTreeModes? GetRoutedTreeMode();

    /// <summary>
    /// Gets whether the route is read-only.
    /// </summary>
    /// <returns>True if read-only.</returns>
    bool GetRoutedReadonly();

    /// <summary>
    /// Gets the routed styles.
    /// </summary>
    /// <returns>The node reader for styles.</returns>
    INodeReader GetRoutedStyles();
}

/// <summary>
/// Interface for notifying inspector edits.
/// </summary>
public interface IInspectorEditNotify
{
    /// <summary>
    /// Notifies that the inspector has been edited.
    /// </summary>
    void NotifyInspectorEdited();
}