using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views;

/// <summary>
/// Enumerates the available features for advanced view editing.
/// </summary>
/// <summary>
/// Represents the available features for advanced view editing.
/// </summary>
public enum ViewAdvancedEditFeatures
{
    /// <summary>
    /// XML editing feature.
    /// </summary>
    XML,
    /// <summary>
    /// JSON editing feature.
    /// </summary>
    Json,
    /// <summary>
    /// Repair functionality.
    /// </summary>
    Repair,
    /// <summary>
    /// Dynamic action feature.
    /// </summary>
    DynamicAction,
}

/// <summary>
/// Provides advanced editing capabilities for views, including XML, JSON, repair, and dynamic action features.
/// </summary>
/// <summary>
/// Provides advanced editing capabilities for views.
/// </summary>
public interface IViewAdvancedEdit
{
    /// <summary>
    /// Gets the target object for field navigation.
    /// </summary>
    object FieldNavigationTarget { get; }

    /// <summary>
    /// Determines whether the specified advanced edit feature is available.
    /// </summary>
    /// <param name="feature">The feature to check.</param>
    /// <returns>True if the feature is available; otherwise, false.</returns>
    bool GetHasFeature(ViewAdvancedEditFeatures feature);

    /// <summary>
    /// Repairs the current view.
    /// </summary>
    void Repair();

    /// <summary>
    /// Relocates the current view.
    /// </summary>
    void Relocate();

    /// <summary>
    /// Sets the dynamic action type for the view.
    /// </summary>
    /// <param name="type">The type to set as the dynamic action.</param>
    void SetDynamicAction(Type type);

    /// <summary>
    /// Gets the text content for the specified feature.
    /// </summary>
    /// <param name="feature">The feature to get text for.</param>
    /// <returns>The text content.</returns>
    string GetText(ViewAdvancedEditFeatures feature);

    /// <summary>
    /// Sets the text content for the specified feature.
    /// </summary>
    /// <param name="feature">The feature to set text for.</param>
    /// <param name="text">The text content to set.</param>
    void SetText(ViewAdvancedEditFeatures feature, string text);
}


/// <summary>
/// Represents a view that displays a target object.
/// </summary>
/// <summary>
/// Represents a view that displays a target object.
/// </summary>
public interface IObjectView
{
    /// <summary>
    /// Gets the target object being displayed in the view.
    /// </summary>
    object TargetObject { get; }
}

/// <summary>
/// Provides editing capabilities for array-based views, including item removal, cloning, and reordering.
/// </summary>
/// <summary>
/// Provides editing capabilities for array-based views.
/// </summary>
public interface IViewArrayEdit
{
    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Removes the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    void RemoveAt(int index);

    /// <summary>
    /// Clones the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to clone.</param>
    void CloneAt(int index);

    /// <summary>
    /// Moves the element at the specified index up by one position.
    /// </summary>
    /// <param name="index">The zero-based index of the element to move.</param>
    void MoveUp(int index);

    /// <summary>
    /// Moves the element at the specified index down by one position.
    /// </summary>
    /// <param name="index">The zero-based index of the element to move.</param>
    void MoveDown(int index);
}

/// <summary>
/// Provides clipboard operations for views, including copy, cut, and paste.
/// </summary>
/// <summary>
/// Provides clipboard operations for views.
/// </summary>
public interface IViewClipboard
{
    /// <summary>
    /// Copies the current selection to the clipboard.
    /// </summary>
    void ClipboardCopy();

    /// <summary>
    /// Cuts the current selection and copies it to the clipboard.
    /// </summary>
    void ClipboardCut();

    /// <summary>
    /// Pastes content from the clipboard.
    /// </summary>
    void ClipboardPaste();
}

/// <summary>
/// Represents a view that has an associated color.
/// </summary>
/// <summary>
/// Represents a view that has an associated color.
/// </summary>
public interface IViewColor
{
    /// <summary>
    /// Gets the color associated with the view, or null if no color is set.
    /// </summary>
    Color? ViewColor { get; }
}

/// <summary>
/// Represents a view that supports commenting functionality.
/// </summary>
/// <summary>
/// Represents a view that supports commenting functionality.
/// </summary>
public interface IViewComment
{
    /// <summary>
    /// Gets a value indicating whether the view can be commented.
    /// </summary>
    bool CanComment { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the view is a comment.
    /// </summary>
    bool IsComment { get; set; }
}

/// <summary>
/// Represents a view that responds to double-click actions.
/// </summary>
/// <summary>
/// Represents a view that responds to double-click actions.
/// </summary>
public interface IViewDoubleClickAction
{
    /// <summary>
    /// Handles the double-click action on the view.
    /// </summary>
    void DoubleClick();
}

/// <summary>
/// Represents a listener that receives notifications when views are entered or exited, in addition to edit notifications.
/// </summary>
/// <summary>
/// Represents a listener for view enter and exit events.
/// </summary>
public interface IViewListener : IViewEditNotify
{
    /// <summary>
    /// Notifies that the user has entered the specified view.
    /// </summary>
    /// <param name="viewId">The ID of the view being entered.</param>
    void NotifyViewEnter(int viewId);

    /// <summary>
    /// Notifies that the user has exited the specified view.
    /// </summary>
    /// <param name="viewId">The ID of the view being exited.</param>
    void NotifyViewExit(int viewId);
}

/// <summary>
/// Object editing notification flow
/// </summary>
public interface IViewEditNotify
{
    void NotifyViewEdited(object obj, string propertyName);
}

/// <summary>
/// Provides notifications when view elements are edited.
/// </summary>
/// <summary>
/// Provides notification when view elements are edited.
/// </summary>
public interface IViewElementEditNotify
{
    /// <summary>
    /// Notifies that the specified view elements have been edited.
    /// </summary>
    /// <param name="objs">The collection of edited objects.</param>
    void NotifyViewElementEdited(IEnumerable<object> objs);
}


/// <summary>
/// Represents an owner that can retrieve view elements by name.
/// </summary>
/// <summary>
/// Represents an owner of view elements.
/// </summary>
public interface IViewElementOwner
{
    /// <summary>
    /// Gets the view element with the specified name.
    /// </summary>
    /// <param name="name">The name of the element to retrieve.</param>
    /// <returns>The element with the specified name, or null if not found.</returns>
    object GetElement(string name);
}


/// <summary>
/// Provides the ability to navigate to a definition based on a sync path.
/// </summary>
/// <summary>
/// Provides functionality to navigate to a definition.
/// </summary>
public interface IViewGotoDefinitionAction
{
    /// <summary>
    /// Navigates to the definition at the specified path.
    /// </summary>
    /// <param name="path">The path to navigate to.</param>
    /// <param name="rest">The remaining path that could not be resolved.</param>
    void GotoDefinition(SyncPath path, out SyncPath rest);
}


/// <summary>
/// Represents a view that can be located within a project.
/// </summary>
/// <summary>
/// Represents a view that can be located within a project.
/// </summary>
public interface IViewLocateInProject
{
}


/// <summary>
/// Represents a view that can be marked as optional.
/// </summary>
/// <summary>
/// Represents a view that can be marked as optional.
/// </summary>
public interface IViewOptional
{
    /// <summary>
    /// Gets or sets a value indicating whether the view is optional.
    /// </summary>
    bool IsOptional { get; set; }
}


/// <summary>
/// Display separately in a new pane
/// </summary>
public interface IViewRedirect
{
    object GetRedirectedObject(int viewId);
}

/// <summary>
/// Represents a view that can be refreshed.
/// </summary>
/// <summary>
/// Represents a view that can be refreshed.
/// </summary>
public interface IViewRefresh
{
    /// <summary>
    /// Queues a refresh operation for the view.
    /// </summary>
    void QueueRefreshView();
}

/// <summary>
/// Represents a view that supports saving.
/// </summary>
/// <summary>
/// Represents a view that can be saved.
/// </summary>
public interface IViewSave
{
    /// <summary>
    /// Saves the current state of the view.
    /// </summary>
    void SaveView();
}

/// <summary>
/// Represents a view that supports search functionality.
/// </summary>
/// <summary>
/// Represents a view that supports search functionality.
/// </summary>
public interface IViewSearch
{
    /// <summary>
    /// Opens the search interface with optional initial text.
    /// </summary>
    /// <param name="text">The initial search text, or null to start empty.</param>
    void OpenSearch(string text = null);

    /// <summary>
    /// Closes the search interface.
    /// </summary>
    void CloseSearch();
}


/// <summary>
/// View selection interface. This interface is used to implement view selection functionality, but the selected object may not necessarily be the object itself, possibly just the object's path information.
/// To get the selected object, use <see cref="IViewSelectionInfo"/>
/// </summary>
public interface IViewSelectable
{
    /// <summary>
    /// Get current selection
    /// </summary>
    /// <returns></returns>
    ViewSelection GetSelection();

    /// <summary>
    /// Set current selection
    /// </summary>
    /// <param name="selection"></param>
    /// <returns></returns>
    bool SetSelection(ViewSelection selection);
}


/// <summary>
/// View selection object info. This interface is used to get the currently selected object in the view, but does not provide object selection functionality.
/// To use object selection functionality, use the <see cref="IViewSelectable"/> interface.
/// </summary>
public interface IViewSelectionInfo
{
    /// <summary>
    /// Currently selected objects
    /// </summary>
    IEnumerable<object> SelectedObjects { get; }

    /// <summary>
    /// Find the currently selected object or its parent by specified type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="distinct"></param>
    /// <returns></returns>
    IEnumerable<T> FindSelectionOrParent<T>(bool distinct = true) where T : class;
}

/// <summary>
/// Represents a view that allows setting values by name.
/// </summary>
/// <summary>
/// Represents a view that supports setting values by name.
/// </summary>
public interface IViewSetValue
{
    /// <summary>
    /// Sets the value for the specified property name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value to set.</param>
    void SetValue(string name, object value);
}

/// <summary>
/// Represents a view that supports undo and redo operations.
/// </summary>
public interface IViewUndo
{
    /// <summary>
    /// Gets a value indicating whether an undo operation is available.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Gets a value indicating whether a redo operation is available.
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Gets the display text describing the next undo operation.
    /// </summary>
    string UndoText { get; }

    /// <summary>
    /// Gets the display text describing the next redo operation.
    /// </summary>
    string RedoText { get; }

    /// <summary>
    /// Performs the undo operation.
    /// </summary>
    void Undo();

    /// <summary>
    /// Performs the redo operation.
    /// </summary>
    void Redo();
}
