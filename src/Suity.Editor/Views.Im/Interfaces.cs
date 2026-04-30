using Suity.Editor.Services;
using Suity.UndoRedos;
using System;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Represents an ImGui view object that supports undo/redo operations.
/// </summary>
public interface IUndoableViewObjectImGui : IViewObjectImGui, IViewUndo
{
    /// <summary>
    /// Gets or sets the undo/redo manager for tracking and managing view state changes.
    /// </summary>
    UndoRedoManager UndoManager { get; set; }
}

/// <summary>
/// Represents the base interface for ImGui view objects that support drawing, selection, and state management.
/// </summary>
public interface IViewObjectImGui : IDrawImGui, IDrawImGuiNode, IServiceProvider
{
    /// <summary>
    /// Occurs when the selection in the view changes.
    /// </summary>
    event EventHandler SelectionChanged;

    /// <summary>
    /// Occurs when the view content becomes dirty (modified).
    /// </summary>
    event EventHandler Dirty;

    /// <summary>
    /// Occurs when objects are edited in the view.
    /// </summary>
    event EventHandler<object[]> Edited;

    /// <summary>
    /// Occurs when an inspect request is made for objects in the view.
    /// </summary>
    event EventHandler<object[]> RequestInspect;

    /// <summary>
    /// Gets or sets the target object being displayed or edited in the view.
    /// </summary>
    object Target { get; set; }

    /// <summary>
    /// Gets the collection of currently selected objects in the view.
    /// </summary>
    IEnumerable<object> SelectedObjects { get; }

    /// <summary>
    /// Focuses the view, optionally triggering an inspect action.
    /// </summary>
    /// <param name="inspect">Whether to trigger an inspect action when focusing.</param>
    void FocusView(bool inspect);

    /// <summary>
    /// Queues a refresh of the view, optionally redrawing all content.
    /// </summary>
    /// <param name="redrawAll">Whether to redraw all content or only modified parts.</param>
    void QueueRefresh(bool redrawAll = false);

    /// <summary>
    /// Expands the root node in the view hierarchy.
    /// </summary>
    void ExpandRoot();

    /// <summary>
    /// Expands all nodes in the view hierarchy.
    /// </summary>
    void ExpandAll();

    /// <summary>
    /// Updates the displayed object in the view to reflect current state.
    /// </summary>
    void UpdateDisplayedObject();

    /// <summary>
    /// Updates the analysis data displayed in the view.
    /// </summary>
    void UpdateAnalysis();

    /// <summary>
    /// Restores the view state from a previously saved configuration object.
    /// </summary>
    /// <param name="configObj">Optional configuration object containing saved view state.</param>
    void RestoreViewState(object configObj = null);

    /// <summary>
    /// Saves the current view state to an object that can be restored later.
    /// </summary>
    /// <returns>An object containing the current view state.</returns>
    object SaveViewState();

    /// <summary>
    /// Creates a context menu with the specified menu name.
    /// </summary>
    /// <param name="menuName">The name of the menu to create.</param>
    void CreateMenu(string menuName);
}

/// <summary>
/// Represents an ImGui view that supports expanded display mode with resizable content.
/// </summary>
public interface IDrawExpandedImGui
{
    /// <summary>
    /// Gets a value indicating whether the expanded view is resizable.
    /// </summary>
    bool ResizableOnExpand { get; }

    /// <summary>
    /// Gets the content scale factor for the expanded view, or null if using default scaling.
    /// </summary>
    float? ContentScale { get; }

    /// <summary>
    /// Enters expanded view mode for the specified target object.
    /// </summary>
    /// <param name="target">The target object to display in expanded view.</param>
    /// <param name="context">Optional inspector context for additional configuration.</param>
    void EnterExpandedView(object target, IInspectorContext context = null);

    /// <summary>
    /// Exits expanded view mode and returns to the normal view.
    /// </summary>
    void ExitExpandedView();

    /// <summary>
    /// Updates the target object displayed in the expanded view.
    /// </summary>
    void UpdateExpandedTarget();

    /// <summary>
    /// Builds and returns the ImGuiNode for the expanded GUI representation.
    /// </summary>
    /// <param name="gui">The ImGui instance to build the node with.</param>
    /// <returns>The ImGuiNode representing the expanded GUI.</returns>
    ImGuiNode OnExpandedGui(ImGui gui);

    /// <summary>
    /// Clears the current selection in the expanded view.
    /// </summary>
    void ClearSelection();
}
