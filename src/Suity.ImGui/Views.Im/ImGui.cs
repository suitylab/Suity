using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Suity.ImGui.BuildIn")]

namespace Suity.Views.Im;

/// <summary>
/// Contains caller information for refresh operations.
/// </summary>
public struct RefreshCallerInfo
{
    /// <summary>
    /// The line number where the refresh was requested.
    /// </summary>
    public int Line;
    /// <summary>
    /// The member name where the refresh was requested.
    /// </summary>
    public string? Member;
    /// <summary>
    /// The file path where the refresh was requested.
    /// </summary>
    public string? Path;
}

/// <summary>
/// Representing an immediate mode graphical user interface
/// </summary>
public abstract class ImGui : IFloatTime
{
    #region System

    /// <summary>
    /// Gets the name identifier of this ImGui instance.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Drawing context device
    /// </summary>
    public abstract IGraphicContext Context { get; }

    /// <summary>
    /// Input device
    /// </summary>
    public abstract IGraphicInput Input { get; }

    /// <summary>
    /// Output device
    /// </summary>
    public abstract IGraphicOutput Output { get; }

    /// <summary>
    /// Global theme
    /// </summary>
    public abstract ImGuiTheme Theme { get; set; }

    /// <summary>
    /// Input system
    /// </summary>
    public abstract ImGuiInputSystem InputSystem { get; }

    /// <summary>
    /// Layout system
    /// </summary>
    public abstract ImGuiLayoutSystem LayoutSystem { get; }

    /// <summary>
    /// Fit system
    /// </summary>
    public abstract ImGuiFitSystem FitSystem { get; }

    /// <summary>
    /// Render system
    /// </summary>
    public abstract ImGuiRenderSystem RenderSystem { get; }

    /// <summary>
    /// Gets a system of the specified type if registered.
    /// </summary>
    /// <typeparam name="T">The system type to retrieve.</typeparam>
    /// <returns>The system instance, or null if not found.</returns>
    public abstract T? GetSystem<T>() where T : class;

    /// <summary>
    /// Gets or creates a system of the specified type.
    /// </summary>
    /// <typeparam name="T">The system type to retrieve or create.</typeparam>
    /// <returns>The system instance.</returns>
    public abstract T GetOrAddSystem<T>() where T : class, new();

    /// <summary>
    /// User inputs event version number
    /// </summary>
    public abstract long InputVersion { get; }

    /// <summary>
    /// Latest input status
    /// </summary>
    public abstract GuiInputState LastInputState { get; }

    /// <summary>
    /// Time
    /// </summary>
    public abstract float Time { get; }

    /// <summary>
    /// Time interval
    /// </summary>
    public abstract float DeltaTime { get; }

    /// <summary>
    /// Tooltip display duration
    /// </summary>
    public abstract float ToolTipDuration { get; set; }

    /// <summary>
    /// Set cursor
    /// </summary>
    /// <param name="cursor">The cursor type to set.</param>
    public abstract void SetCursor(GuiCursorTypes cursor);

    /// <summary>
    /// Debugging rendering
    /// </summary>
    public abstract bool DebugDraw { get; set; }

    /// <summary>
    /// Whether processing input or output
    /// </summary>
    public abstract bool IsProcessing { get; }

    /// <summary>
    /// Does it constitute a click event
    /// </summary>
    public abstract bool IsClick { get; }

    /// <summary>
    /// Does it constitute a double-click event
    /// </summary>
    public abstract bool IsDoubleClick { get; }

    /// <summary>
    /// Last mouse press position
    /// </summary>
    public abstract Point LastMouseDownLocation { get; }

    /// <summary>
    /// Last mouse click time
    /// </summary>
    public abstract float LastMouseClickTime { get; }

    /// <summary>
    /// Last mouse click location
    /// </summary>
    public abstract Point LastMouseClickLocation { get; }

    /// <summary>
    /// Double click delay duration in seconds.
    /// </summary>
    public float DoubleClickDuration { get; set; } = 0.3f;

    /// <summary>
    /// Gets or sets the background color for the ImGui canvas.
    /// </summary>
    public Color? BackgroundColor { get; set; } = Color.Black;

    #endregion

    #region Node

    /// <summary>
    /// The node currently being filled with content
    /// </summary>
    public abstract ImGuiNode CurrentNode { get; }

    /// <summary>
    /// Node hovering over with the mouse
    /// </summary>
    public abstract ImGuiNode? HoverNode { get; }

    /// <summary>
    /// Gets the node that is currently being refreshed.
    /// </summary>
    public abstract ImGuiNode? RefreshingNode { get; }

    /// <summary>
    /// Node that gains focus
    /// </summary>
    public abstract ImGuiNode? FocusNode { get; }

    /// <summary>
    /// Node under control
    /// </summary>
    public abstract ImGuiNode? ControllingNode { get; }

    /// <summary>
    /// Determines whether the mouse is currently over the specified node.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if the mouse is over the node; otherwise, false.</returns>
    public abstract bool GetIsMouseIn(ImGuiNode node);

    /// <summary>
    /// Gets all nodes that the mouse is currently hovering over.
    /// </summary>
    public abstract IEnumerable<ImGuiNode> MouseInNodes { get; }

    /// <summary>
    /// Adds a node to the timer update list for periodic updates.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <returns>True if the node was added successfully.</returns>
    public abstract bool AddTimerNode(ImGuiNode node);

    /// <summary>
    /// Removes a node from the timer update list.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <returns>True if the node was removed successfully.</returns>
    public abstract bool RemoveTimerNode(ImGuiNode node);

    /// <summary>
    /// Finds a node by its path in the node hierarchy.
    /// </summary>
    /// <param name="path">The path to search for.</param>
    /// <returns>The found node, or null if not found.</returns>
    public abstract ImGuiNode? FindNode(ImGuiPath? path);

    #endregion

    #region Input & Output

    /// <summary>
    /// Processes the given graphical input and executes the ImGui callback.
    /// </summary>
    /// <param name="input">The graphical input to process.</param>
    /// <param name="onGui">The callback to execute for UI construction.</param>
    public abstract void HandleGraphicInput(IGraphicInput input, Action<ImGui> onGui);

    /// <summary>
    /// Renders the ImGui output to the specified graphical output device.
    /// </summary>
    /// <param name="output">The graphical output device.</param>
    public abstract void HandleGraphicOutput(IGraphicOutput output);

    /// <summary>
    /// Request queue action
    /// </summary>
    /// <param name="action">The action to queue.</param>
    public abstract void QueueAction(Action action);

    /// <summary>
    /// Request queue refresh, this refresh method will include the currently synchronized node in the refresh nodes
    /// </summary>
    /// <param name="line">Caller line number.</param>
    /// <param name="member">Caller member name.</param>
    /// <param name="path">Caller file path.</param>
    public abstract void QueueRefresh([CallerLineNumber] int line = 0, [CallerMemberName] string? member = null, [CallerFilePath] string? path = null);

    /// <summary>
    /// Request input state
    /// </summary>
    /// <param name="state">The input state to queue.</param>
    public abstract void QueueInputState(GuiInputState state);

    /// <summary>
    /// Request rendering, usually does not require calling
    /// </summary>
    public abstract void RequestOutput();

    /// <summary>
    /// Request full screen rendering, usually without calling
    /// </summary>
    public abstract void RequestFullOutput();


    /// <summary>
    /// Event raised when node configuration begins.
    /// </summary>
    public event Action<ImGuiNode>? NodeBeginConfig;

    /// <summary>
    /// Event raised when node configuration finishes.
    /// </summary>
    public event Action<ImGuiNode>? NodeFinishConfig;

    /// <inheritdoc/>
    protected virtual void OnNodeBeginConfig(ImGuiNode node)
    {
        NodeBeginConfig?.Invoke(node);
    }

    /// <inheritdoc/>
    protected virtual void OnNodeFinishConfig(ImGuiNode node)
    {
        NodeFinishConfig?.Invoke(node);
    }

    /// <summary>
    /// Indicate that the upper UI needs to be closed
    /// </summary>
    public bool IsClosing { get; set; }

    #endregion

    #region Sync & Content

    /// <summary>
    /// Create Node Process
    /// </summary>
    /// <param name="id">The unique identifier for the node.</param>
    /// <returns>The created or retrieved ImGuiNode.</returns>
    public abstract ImGuiNode BeginCurrentNode(string id);

    /// <summary>
    /// Passes the current node without creating a new one, optionally matching by ID.
    /// </summary>
    /// <param name="id">The optional ID to match.</param>
    /// <returns>The passed node, or null if not available.</returns>
    public abstract ImGuiNode? PassCurrentNode(string? id);

    /// <summary>
    /// Forcefully end the synchronization of the current node, which does not need to be called under normal circumstances, and is only called when the synchronized parameters need to be obtained in a timely manner.
    /// </summary>
    /// <returns>Return to the current node</returns>
    public abstract ImGuiNode? EndCurrentNode();

    /// <summary>
    /// Skips content updates for the current node, used internally to ignore updates.
    /// </summary>
    /// <param name="layout">Whether to still execute layout processing.</param>
    public abstract void PassCurrentContents();

    /// <summary>
    /// Lays out the current node's content with optional fit and alignment.
    /// </summary>
    /// <param name="fit">Whether to perform fitting.</param>
    /// <param name="align">Whether to perform alignment.</param>
    public abstract void LayoutCurrentContents(bool fit = true, bool align = true);

    /// <summary>
    /// Executes the content action within the context of a new node.
    /// </summary>
    /// <param name="contentAction">The action to execute.</param>
    /// <param name="layout">Whether to perform layout after content.</param>
    /// <returns>The created ImGuiNode, or null.</returns>
    public abstract ImGuiNode? OnContent(Action contentAction, bool layout = true);

    /// <summary>
    /// Executes the content action within the context of the specified node.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="contentAction">The action to execute.</param>
    /// <param name="layout">Whether to perform layout after content.</param>
    public abstract void OnContent(ImGuiNode node, Action contentAction, bool layout = true);

    /// <summary>
    /// Executes the content action with the node as a parameter.
    /// </summary>
    /// <param name="node">The target node.</param>
    /// <param name="contentAction">The action to execute with the node.</param>
    /// <param name="layout">Whether to perform layout after content.</param>
    public abstract void OnContent(ImGuiNode node, Action<ImGuiNode> contentAction, bool layout = true);

    /// <summary>
    /// Begins a content block for the current node.
    /// </summary>
    /// <returns>The current ImGuiNode, or null.</returns>
    public abstract ImGuiNode? BeginContent();

    /// <summary>
    /// Begins a content block for the specified node.
    /// </summary>
    /// <param name="node">The target node.</param>
    public abstract void BeginContent(ImGuiNode node);

    /// <summary>
    /// Ends the current content block.
    /// </summary>
    /// <returns>The ended ImGuiNode, or null.</returns>
    public abstract ImGuiNode? EndContent();

    /// <summary>
    /// Lays out the content of the specified node.
    /// </summary>
    /// <param name="node">The node whose content to layout.</param>
    public abstract void LayoutNodeContent(ImGuiNode node);

    #endregion

    #region Style

    /// <summary>
    /// Sets the current theme for subsequent UI operations.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public abstract void SetCurrentTheme(ImGuiTheme theme);

    /// <summary>
    /// Sets a style value for the current node by name.
    /// </summary>
    /// <typeparam name="T">The style type.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="style">The style value.</param>
    public abstract void SetCurrentStyle<T>(string name, T style) where T : class;

    /// <summary>
    /// Sets a style value for the current node by name and pseudo state.
    /// </summary>
    /// <typeparam name="T">The style type.</typeparam>
    /// <param name="name">The style name.</param>
    /// <param name="pseudo">The pseudo state.</param>
    /// <param name="style">The style value.</param>
    public abstract void SetCurrentStyle<T>(string name, string pseudo, T style) where T : class;

    /// <summary>
    /// Sets a transition for the current node between states.
    /// </summary>
    /// <param name="name">The transition name.</param>
    /// <param name="state">The source state.</param>
    /// <param name="targetState">The target state.</param>
    /// <param name="transition">The transition factory.</param>
    public abstract void SetCurrentTransition(string name, string? state, string? targetState, ITransitionFactory transition);

    #endregion

    #region Value

    /// <summary>
    /// Gets a value of the specified type from the current node.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>The value, or null if not found.</returns>
    public abstract T? GetValue<T>() where T : class;

    /// <summary>
    /// Gets a value of the specified type from the current node.
    /// </summary>
    /// <param name="type">The value type.</param>
    /// <returns>The value, or null if not found.</returns>
    public abstract object? GetValue(Type type);

    /// <summary>
    /// Gets or creates a value of the specified type on the current node.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>The existing or newly created value.</returns>
    public abstract T GetOrCreateValue<T>() where T : class, new();

    /// <summary>
    /// Gets or creates a value using the provided creation function.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="creation">The function to create the value if not found.</param>
    /// <returns>The existing or newly created value.</returns>
    public abstract T GetOrCreateValue<T>(Func<T> creation) where T : class;

    /// <summary>
    /// Gets or creates a value, indicating whether it was newly created.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="created">True if the value was newly created; otherwise, false.</param>
    /// <returns>The existing or newly created value.</returns>
    public abstract T GetOrCreateValue<T>(out bool created) where T : class, new();

    /// <summary>
    /// Gets or creates a value using the provided creation function, indicating whether it was newly created.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="creation">The function to create the value if not found.</param>
    /// <param name="created">True if the value was newly created; otherwise, false.</param>
    /// <returns>The existing or newly created value.</returns>
    public abstract T GetOrCreateValue<T>(Func<T> creation, out bool created) where T : class;

    /// <summary>
    /// Sets a value on the current node.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the value was set successfully.</returns>
    public abstract bool SetValue<T>(T value) where T : class;

    /// <summary>
    /// Removes a value of the specified type from the current node.
    /// </summary>
    /// <typeparam name="T">The value type to remove.</typeparam>
    /// <returns>True if the value was removed successfully.</returns>
    public abstract bool RemoveValue<T>() where T : class;

    #endregion

    #region State

    /// <summary>
    /// Backs up the current GUI state to the provided state object.
    /// </summary>
    /// <param name="state">The state object to backup to.</param>
    public abstract void BackupState(IImGuiGuiBackupState state);

    /// <summary>
    /// Restores the GUI state from the provided state object.
    /// </summary>
    /// <param name="state">The state object to restore from.</param>
    public abstract void RestoreState(IImGuiGuiBackupState state);

    /// <summary>
    /// Restores the GUI state from the provided node's state.
    /// </summary>
    /// <param name="node">The node to restore state from.</param>
    public abstract void RestoreState(ImGuiNode node);

    #endregion

    /// <summary>
    /// Merges two input states, keeping the higher priority state.
    /// </summary>
    /// <param name="state">The current state to update.</param>
    /// <param name="other">The other state to merge.</param>
    public static void MergeState(ref GuiInputState state, GuiInputState other)
    {
        if (state < other)
        {
            state = other;
        }
    }
}