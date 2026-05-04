using Suity.Drawing;
using Suity.Views.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// ImGui drawing node
/// </summary>
public abstract class ImGuiNode
{
    //TODO: Implement recycling

    /// <summary>
    /// Pseudo name of mouse in
    /// </summary>
    public const string PseudoMouseIn = "mouse-in";

    /// <summary>
    /// Pseudo name of mouse down
    /// </summary>
    public const string PseudoMouseDown = "mouse-down";

    /// <summary>
    /// Pseudo name of active
    /// </summary>
    public const string PseudoActive = "active";

    /// <summary>
    /// Pseudo name of active mouse in
    /// </summary>
    public const string PseudoActiveMouseIn = "active-mouse-in";

    /// <summary>
    /// Pseudo name of active mouse down
    /// </summary>
    public const string PseudoActiveMouseDown = "active-mouse-down";

    #region System

    /// <summary>
    /// Main ImGui drawing
    /// </summary>
    public abstract ImGui Gui { get; }

    /// <summary>
    /// Get the local ID of this node
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Get the index of this node on the parent node
    /// </summary>
    public abstract int Index { get; }

    /// <summary>
    /// Get the full name of this node path
    /// </summary>
    public abstract ImGuiPath FullPath { get; }

    /// <summary>
    /// Get or set the type name of this node. 
    /// Note that the Gui system will use this type name to parse Gui functions
    /// </summary>
    public abstract string? TypeName { get; set; }

    /// <summary>
    /// Get parent node
    /// </summary>
    public abstract ImGuiNode? Parent { get; }

    /// <summary>
    /// Obtain the feature flags of this node
    /// </summary>
    public abstract ImGuiNodeFlags Flags { get; }

    /// <summary>
    /// Get the previous node of this node on the parent node
    /// </summary>
    public virtual ImGuiNode? Previous => Parent?.GetNodeAt(Index - 1);

    /// <summary>
    /// Get the next node of this node on the parent node
    /// </summary>
    public virtual ImGuiNode? Next => Parent?.GetNodeAt(Index + 1);

    /// <summary>
    /// Gets whether the specified node is included in the parent node hierarchy.
    /// </summary>
    /// <param name="parent">The parent node to check for in the hierarchy.</param>
    /// <returns>True if the specified node is found in the parent hierarchy; otherwise, false.</returns>
    public virtual bool ContainsParent(ImGuiNode parent)
    {
        var myParent = Parent;
        while (myParent is { })
        {
            if (myParent.Equals(parent))
            {
                return true;
            }

            myParent = myParent.Parent;
        }

        return false;
    }

    /// <summary>
    /// Get whether it is in use or not
    /// </summary>
    public virtual bool IsInUsage => Parent is { };

    /// <summary>
    /// Get whether initialization has been completed
    /// </summary>
    public virtual bool IsInitializing => !Flags.HasFlag(ImGuiNodeFlags.NodeInitialized);

    /// <summary>
    /// Queues a refresh request for this node, optionally capturing caller information for debugging.
    /// </summary>
    /// <param name="line">The line number where the refresh was requested.</param>
    /// <param name="member">The member name where the refresh was requested.</param>
    /// <param name="path">The file path where the refresh was requested.</param>
    public abstract void QueueRefresh([CallerLineNumber] int line = 0, [CallerMemberName] string? member = null, [CallerFilePath] string? path = null);

    #endregion

    #region Transforms

    /// <summary>
    /// The core drawing area is set to absolute coordinates through the Layout function
    /// </summary>
    public abstract RectangleF Rect { get; set; }

    /// <summary>
    /// Gets the bounding rectangle of the element in its local coordinate space.
    /// </summary>
    public abstract RectangleF LocalRect { get; }

    /// <summary>
    /// Gets the bounding rectangle of the element in global (screen) coordinates.
    /// </summary>
    public abstract RectangleF GlobalRect { get; }

    /// <summary>
    /// The area within the padding
    /// </summary>
    public abstract RectangleF InnerRect { get; }

    /// <summary>
    /// Gets the inner rectangle in local coordinates.
    /// </summary>
    public abstract RectangleF LocalInnerRect { get; }

    /// <summary>
    /// Gets the inner rectangle in global coordinates.
    /// </summary>
    public abstract RectangleF GlobalInnerRect { get; }

    /// <summary>
    /// The area outside the padding
    /// </summary>
    public abstract RectangleF OuterRect { get; }

    /// <summary>
    /// Gets the outer rectangle in local coordinates.
    /// </summary>
    public abstract RectangleF LocalOuterRect { get; }

    /// <summary>
    /// Gets the outer rectangle in global coordinates.
    /// </summary>
    public abstract RectangleF GlobalOuterRect { get; }

    /// <summary>
    /// Area that need to be redrawn
    /// </summary>
    public abstract RectangleF? DirtyRect { get; }

    /// <summary>
    /// Gets the dirty rectangle in global coordinates.
    /// </summary>
    public abstract RectangleF? GlobalDirtyRect { get; }

    /// <summary>
    /// Get or set the mouse click area, default is <see cref="Rect"/>
    /// </summary>
    public abstract RectangleF? MouseClickRect { get; set; }

    /// <summary>
    /// Gets the mouse click rectangle in global coordinates.
    /// </summary>
    public abstract RectangleF? GlobalMouseClickRect { get; }

    /// <summary>
    /// Offsets the coordinates of this node and all its child nodes.
    /// </summary>
    /// <param name="x">The horizontal offset amount.</param>
    /// <param name="y">The vertical offset amount.</param>
    public abstract void OffsetPositionDeep(float x, float y);


    /// <summary>
    /// Gets or sets the transformation applied to this node.
    /// </summary>
    public abstract GuiTransform? Transform { get; set; }

    /// <summary>
    /// Gets the local scale factor of this node.
    /// </summary>
    public abstract float? LocalScale { get; }

    /// <summary>
    /// Gets the global scale factor of this node.
    /// </summary>
    public abstract float? GlobalScale { get; }

    /// <summary>
    /// Scales a value by the local scale factor.
    /// </summary>
    /// <param name="value">The value to scale.</param>
    /// <returns>The scaled value.</returns>
    public abstract float LocalScaleValue(float value);

    /// <summary>
    /// Reverses a value scaled by the local scale factor.
    /// </summary>
    /// <param name="value">The value to reverse scale.</param>
    /// <returns>The reverse scaled value.</returns>
    public abstract float LocalReverseScaleValue(float value);

    /// <summary>
    /// Scales a value by the global scale factor.
    /// </summary>
    /// <param name="value">The value to scale.</param>
    /// <returns>The scaled value.</returns>
    public abstract float GlobalScaleValue(float value);

    /// <summary>
    /// Reverses a value scaled by the global scale factor.
    /// </summary>
    /// <param name="value">The value to reverse scale.</param>
    /// <returns>The reverse scaled value.</returns>
    public abstract float GlobalReverseScaleValue(float value);

    /// <summary>
    /// Transforms a point to global coordinates using the scale factor.
    /// </summary>
    /// <param name="point">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public abstract PointF GlobalScalePoint(PointF point);

    /// <summary>
    /// Reverts a point from global coordinates using the scale factor.
    /// </summary>
    /// <param name="point">The point to revert.</param>
    /// <returns>The reverted point.</returns>
    public abstract PointF GlobalRevertScalePoint(PointF point);

    /// <summary>
    /// Transforms a rectangle to global coordinates using the scale factor.
    /// </summary>
    /// <param name="rect">The rectangle to transform.</param>
    /// <returns>The transformed rectangle.</returns>
    public abstract RectangleF GlobalScaleRect(RectangleF rect);

    /// <summary>
    /// Reverts a rectangle from global coordinates using the scale factor.
    /// </summary>
    /// <param name="rect">The rectangle to revert.</param>
    /// <returns>The reverted rectangle.</returns>
    public abstract RectangleF GlobalRevertScaleRect(RectangleF rect);

    /// <summary>
    /// Transforms a child rectangle relative to this node.
    /// </summary>
    /// <param name="rect">The child rectangle to transform.</param>
    /// <returns>The transformed rectangle.</returns>
    public abstract RectangleF TransformChildRect(RectangleF rect);

    /// <summary>
    /// Reverts a child rectangle transformation relative to this node.
    /// </summary>
    /// <param name="rect">The child rectangle to revert.</param>
    /// <returns>The reverted rectangle.</returns>
    public abstract RectangleF RevertTransformChildRect(RectangleF rect);

    #endregion

    #region Functions

    /// <summary>
    /// Final input function
    /// </summary>
    public abstract InputFunction? InputFunction { get; }

    /// <summary>
    /// Final layout function
    /// </summary>
    public abstract LayoutFunction? LayoutFunction { get; }

    /// <summary>
    /// Final fit function
    /// </summary>
    public abstract FitFunction? FitFunction { get; }

    /// <summary>
    /// Final render function
    /// </summary>
    public abstract RenderFunction? RenderFunction { get; }

    /// <summary>
    /// Base input function
    /// </summary>
    public abstract InputFunction? BaseInputFunction { get; set; }

    /// <summary>
    /// Base layout function
    /// </summary>
    public abstract LayoutFunction? BaseLayoutFunction { get; set; }

    /// <summary>
    /// Base fit function
    /// </summary>
    public abstract FitFunction? BaseFitFunction { get; set; }

    /// <summary>
    /// Base render function
    /// </summary>
    public abstract RenderFunction? BaseRenderFunction { get; set; }

    #endregion

    #region Style

    /// <summary>
    /// Get or set the style class of this node
    /// </summary>
    public abstract string[]? Classes { get; set; }

    /// <summary>
    /// Will obtaining or setting the pseudo style changes of this node affect the changes of all its child nodes.。
    /// </summary>
    public abstract bool PseudoAffectsChildren { get; set; }

    /// <summary>
    /// Adds a style class to this node.
    /// </summary>
    /// <param name="class">The style class name to add.</param>
    /// <returns>True if the class was added successfully; otherwise, false.</returns>
    public abstract bool AddClass(string @class);

    /// <summary>
    /// Removes a style class from this node.
    /// </summary>
    /// <param name="class">The style class name to remove.</param>
    /// <returns>True if the class was removed successfully; otherwise, false.</returns>
    public abstract bool RemoveClass(string @class);

    /// <summary>
    /// Obtain whether there is a style definition
    /// </summary>
    public abstract bool HasStyle { get; }

    /// <summary>
    /// Obtain whether there is a pseudo style definition
    /// </summary>
    public abstract bool HasPseudoStyle { get; }

    /// <summary>
    /// Gets or sets the current pseudo style
    /// </summary>
    public abstract string? Pseudo { get; set; }

    /// <summary>
    /// Get or set a theme
    /// </summary>
    public abstract ImGuiTheme Theme { get; set; }

    /// <summary>
    /// Gets a style value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of style to retrieve.</typeparam>
    /// <returns>The style value, or null if not found.</returns>
    public abstract T? GetStyle<T>() where T : class;

    /// <summary>
    /// Sets a style value for the specified name.
    /// </summary>
    /// <typeparam name="T">The type of style to set.</typeparam>
    /// <param name="name">The style property name.</param>
    /// <param name="style">The style value to set.</param>
    public abstract void SetStyle<T>(string name, T style) where T : class;

    /// <summary>
    /// Sets a style value for the specified name and pseudo state.
    /// </summary>
    /// <typeparam name="T">The type of style to set.</typeparam>
    /// <param name="name">The style property name.</param>
    /// <param name="pseudo">The pseudo state name (e.g., "hover", "active").</param>
    /// <param name="style">The style value to set.</param>
    public abstract void SetStyle<T>(string name, string pseudo, T style) where T : class;

    /// <summary>
    /// Aplly all change of styles.
    /// </summary>
    public abstract void ApplyStyles();

    /// <summary>
    /// Gets the current animation applied to this node.
    /// </summary>
    public abstract IGuiAnimation? Animation { get; }

    /// <summary>
    /// Gets the start time of the current animation.
    /// </summary>
    public abstract float AnimationStartTime { get; }

    /// <summary>
    /// Starts an animation on this node.
    /// </summary>
    /// <param name="animation">The animation to start.</param>
    /// <param name="forceRestart">If true, restarts the animation even if it is already running.</param>
    public abstract void StartAnimation(IGuiAnimation animation, bool forceRestart = false);

    /// <summary>
    /// Stop animation
    /// </summary>
    public abstract void StopAnimation();

    #endregion

    #region Value

    /// <summary>
    /// Gets a cached value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <returns>The cached value, or null if not found.</returns>
    public abstract T? GetValue<T>() where T : class;

    /// <summary>
    /// Gets a cached value of the specified type.
    /// </summary>
    /// <param name="type">The type of value to retrieve.</param>
    /// <returns>The cached value, or null if not found.</returns>
    public abstract object? GetValue(Type type);

    /// <summary>
    /// Retrieves the first value of the specified reference type found in the current object or its parent hierarchy.
    /// </summary>
    /// <remarks>This method searches the current object and ascends through its parent hierarchy until a
    /// value of the specified type is found. If no such value exists, the method returns null.</remarks>
    /// <typeparam name="T">The reference type of the value to retrieve.</typeparam>
    /// <returns>An instance of type T if a value is found in the hierarchy; otherwise, null.</returns>
    public abstract T? GetValueInHierarchy<T>() where T : class;

    /// <summary>
    /// Retrieves the first value of the specified type found in the current object or its parent hierarchy.
    /// </summary>
    /// <param name="type">The type of value to retrieve.</param>
    /// <returns>The value if found in the hierarchy; otherwise, null.</returns>
    public abstract object? GetValueInHierarchy(Type type);

    /// <summary>
    /// Gets or creates a cached value of the specified type using the default constructor.
    /// </summary>
    /// <typeparam name="T">The type of value to get or create.</typeparam>
    /// <returns>The existing cached value, or a newly created instance.</returns>
    public abstract T GetOrCreateValue<T>() where T : class, new();

    /// <summary>
    /// Gets or creates a cached value using the specified factory function.
    /// </summary>
    /// <typeparam name="T">The type of value to get or create.</typeparam>
    /// <param name="creation">The factory function to create a new instance if none exists.</param>
    /// <returns>The existing cached value, or a newly created instance.</returns>
    public abstract T GetOrCreateValue<T>(Func<T> creation) where T : class;

    /// <summary>
    /// Gets or creates a cached value, indicating whether a new instance was created.
    /// </summary>
    /// <typeparam name="T">The type of value to get or create.</typeparam>
    /// <param name="created">When this method returns, contains true if a new value was created; otherwise, false.</param>
    /// <returns>The existing cached value, or a newly created instance.</returns>
    public abstract T GetOrCreateValue<T>(out bool created) where T : class, new();

    /// <summary>
    /// Gets or creates a cached value using the specified factory function, indicating whether a new instance was created.
    /// </summary>
    /// <typeparam name="T">The type of value to get or create.</typeparam>
    /// <param name="creation">The factory function to create a new instance if none exists.</param>
    /// <param name="created">When this method returns, contains true if a new value was created; otherwise, false.</param>
    /// <returns>The existing cached value, or a newly created instance.</returns>
    public abstract T GetOrCreateValue<T>(Func<T> creation, out bool created) where T : class;

    /// <summary>
    /// Sets a cached value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of value to set.</typeparam>
    /// <param name="value">The value to cache.</param>
    /// <returns>True if the value was set successfully; otherwise, false.</returns>
    public abstract bool SetValue<T>(T value) where T : class;

    /// <summary>
    /// Removes a cached value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of value to remove.</typeparam>
    /// <returns>True if the value was removed successfully; otherwise, false.</returns>
    public abstract bool RemoveValue<T>() where T : class;

    #endregion

    #region Values

    /// <summary>
    /// Get or set text
    /// </summary>
    public abstract string? Text { get; set; }

    /// <summary>
    /// Get text alignment mode
    /// </summary>
    public abstract GuiAlignment? TextAlignment { get; }

    /// <summary>
    /// Get or set color
    /// </summary>
    public abstract Color? Color { get; set; }

    /// <summary>
    /// Get or set the height.
    /// Note: not setting the height in ImGui mode will result in the upper level height only increasing and not decreasing
    /// </summary>
    public abstract GuiLength? Width { get; set; }

    /// <summary>
    /// Get or set the base width
    /// </summary>
    public abstract GuiLength? BaseWidth { get; set; }

    /// <summary>
    /// Get or set the height.
    /// Note: not setting the height in ImGui mode will result in the upper level height only increasing and not decreasing
    /// </summary>
    public abstract GuiLength? Height { get; set; }

    /// <summary>
    /// Get or set the base height
    /// </summary>
    public abstract GuiLength? BaseHeight { get; set; }

    /// <summary>
    /// Get or set the margin
    /// </summary>
    public abstract GuiThickness? Margin { get; set; }

    /// <summary>
    /// Get or set the padding
    /// </summary>
    public abstract GuiThickness? Padding { get; set; }

    /// <summary>
    /// Get or set the fit orientation
    /// </summary>
    public abstract GuiOrientation FitOrientation { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Get or set the alignment
    /// </summary>
    public abstract GuiAlignmentStyle? Alignment { get; set; }

    /// <summary>
    /// Get or set whether to disable
    /// </summary>
    public abstract bool IsDisabled { get; set; }

    /// <summary>
    /// Get or set whether it is read-only
    /// </summary>
    public abstract bool IsReadOnly { get; set; }

    /// <summary>
    /// Get or set whether to reverse the style reading order
    /// </summary>
    public abstract bool OverrideDisabled { get; set; }

    /// <summary>
    /// Get whether it is floating layout
    /// </summary>
    public abstract bool IsFloating { get; set; }

    /// <summary>
    /// Get or set edited
    /// </summary>
    public abstract bool IsEdited { get; set; }

    /// <summary>
    /// Indicates that this node has exceeded the boundary of the parent node
    /// </summary>
    public abstract bool IsOutOfBound { get; set; }

    /// <summary>
    /// Get or set the spacing between child nodes
    /// </summary>
    public abstract float? ChildSpacing { get; set; }

    /// <summary>
    /// Get or set sibling spacing
    /// </summary>
    public abstract float SiblingSpacing { get; set; }

    /// <summary>
    /// Gets or sets whether to revert the input order of child nodes.
    /// </summary>
    public abstract bool RevertInputOrder { get; set; }

    /// <summary>
    /// Get or set whether it is in compact mode
    /// </summary>
    public abstract bool IsCompact { get; set; }

    /// <summary>
    /// Marking sub objects can overlap in layout
    /// </summary>
    public abstract bool IsOverlapped { get; set; }

    /// <summary>
    /// Support dragging and dropping events outside the node area with the mouse, mainly used for Viewport views.
    /// </summary>
    public abstract bool IsMouseDragOutSideEvent { get; set; }

    /// <summary>
    /// Not performing its own Transform operation, but executing the parent node without affecting the child nodes, mainly used for Viewport views.
    /// </summary>
    public abstract bool IsNoTransform { get; set; }

    /// <summary>
    /// Dual layout, used for certain UIs with dual constraints such as minimum and maximum values, requiring two layouts to be executed in the ImGui environment.
    /// </summary>
    public abstract bool IsDoubleLayout { get; set; }

    /// <summary>
    /// Mark as deleted. The node will be deleted after the next sync operation and will be create new to replace it, useful when the node contains legacy data.
    /// </summary>
    public abstract bool MarkDeleted { get; set; }

    #endregion

    #region Values extended

    /// <summary>
    /// Gets the margin value for the specified side.
    /// </summary>
    /// <param name="side">The side direction to get the margin for.</param>
    /// <returns>The margin value for the specified side, or 0 if no margin is set.</returns>
    public float GetMargin(GuiSides side) => Margin?.GetValue(side) ?? 0;

    /// <summary>
    /// Gets the padding value for the specified side.
    /// </summary>
    /// <param name="side">The side direction to get the padding for.</param>
    /// <returns>The padding value for the specified side, or 0 if no padding is set.</returns>
    public float GetPadding(GuiSides side) => Padding?.GetValue(side) ?? 0;

    /// <summary>
    /// Get or set horizontal alignment mode
    /// </summary>
    public abstract GuiAlignment? HorizontalAlignment { get; set; }

    /// <summary>
    /// Get or set vertical alignment mode
    /// </summary>
    public abstract GuiAlignment? VerticalAlignment { get; set; }

    /// <summary>
    /// Stretch during alignment
    /// </summary>
    public abstract bool AlignmentStretch { get; set; }

    /// <summary>
    /// Get header style
    /// </summary>
    public abstract GuiHeaderStyle? HeaderStyle { get; }

    /// <summary>
    /// Get or set the header width
    /// </summary>
    public abstract float? HeaderWidth { get; set; }

    /// <summary>
    /// Get or set the header height
    /// </summary>
    public abstract float? HeaderHeight { get; set; }

    /// <summary>
    /// Get or set the inner edge of the header
    /// </summary>
    public abstract float? HeaderPadding { get; set; }

    /// <summary>
    /// Get or set the header color
    /// </summary>
    public abstract Color? HeaderColor { get; set; }

    /// <summary>
    /// Get or set border width
    /// </summary>
    public abstract float? BorderWidth { get; set; }

    /// <summary>
    /// Gets the border width scaled by the current scale factor.
    /// </summary>
    public abstract float ScaledBorderWidth { get; }

    /// <summary>
    /// Get or set border color
    /// </summary>
    public abstract Color? BorderColor { get; set; }

    /// <summary>
    /// Get or set the color of the scrollbar
    /// </summary>
    public abstract Color? ScrollBarColor { get; set; }

    /// <summary>
    /// Get or set font
    /// </summary>
    public abstract FontDef? Font { get; set; }

    /// <summary>
    /// Get or set font color
    /// </summary>
    public abstract Color? FontColor { get; set; }

    /// <summary>
    /// Get or set image
    /// </summary>
    public abstract ImageDef? Image { get; set; }

    /// <summary>
    /// Get or set image filter color
    /// </summary>
    public abstract Color? ImageFilterColor { get; set; }

    /// <summary>
    /// Get or set the fillet width
    /// </summary>
    public abstract float? CornerRound { get; set; }

    /// <summary>
    /// Get or set whether is expanded
    /// </summary>
    public abstract bool? Expanded { get; set; }

    #endregion

    #region Input

    /// <summary>
    /// Input version
    /// </summary>
    public abstract long InputVersion { get; }

    /// <summary>
    /// Get whether the mouse pointer is within the area
    /// </summary>
    public abstract bool IsMouseInRect { get; }

    /// <summary>
    /// Get whether the mouse pointer is within the click area
    /// </summary>
    public abstract bool IsMouseInClickRect { get; }

    /// <summary>
    /// Retrieve whether the mouse pointer is within the inner edge box
    /// </summary>
    public abstract bool IsMouseInInnerRect { get; }

    /// <summary>
    /// Get the current mouse status
    /// </summary>
    public abstract GuiMouseState MouseState { get; }

    #endregion

    #region Update & child nodes

    /// <summary>
    /// Begins defining a new child node with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the child node.</param>
    /// <returns>The node that is currently being defined.</returns>
    public abstract ImGuiNode BeginNode(string id);

    /// <summary>
    /// Retrieves a child node by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the child node to retrieve.</param>
    /// <returns>The child node with the specified identifier, or null if not found.</returns>
    public abstract ImGuiNode? GetChildNode(string? id);

    /// <summary>
    /// Retrieves a child node by its index.
    /// </summary>
    /// <param name="index">The zero-based index of the child node.</param>
    /// <returns>The child node at the specified index, or null if not found.</returns>
    public abstract ImGuiNode? GetChildNode(int index);

    /// <summary>
    /// Searches for a node with the specified identifier among all descendant nodes.
    /// </summary>
    /// <param name="id">The identifier of the node to find.</param>
    /// <returns>The node with the specified identifier, or null if not found in the hierarchy.</returns>
    public abstract ImGuiNode? FindNodeInChildren(string id);

    /// <summary>
    /// Get the number of child nodes contained
    /// </summary>
    public abstract int ChildNodeCount { get; }

    /// <summary>
    /// Get all child nodes
    /// </summary>
    public abstract IEnumerable<ImGuiNode> ChildNodes { get; }

    /// <summary>
    /// Retrieves a child node by its zero-based index.
    /// </summary>
    /// <param name="index">The zero-based index of the child node.</param>
    /// <returns>The child node at the specified index, or null if the index is out of range.</returns>
    public abstract ImGuiNode? GetNodeAt(int index);

    /// <summary>
    /// Clear all contents
    /// </summary>
    public abstract void ClearContents();

    #endregion

    #region Layout

    /// <summary>
    /// Current layout index. 
    /// Do not use for calculations, this value is used to record the index of the current layout for layout recovery, but it may also not be set to a value.
    /// </summary>
    public abstract int CurrentLayoutIndex { get; }

    /// <summary>
    /// Initial layout coordinates
    /// </summary>
    public abstract PointF InitialLayoutPosition { get; }

    /// <summary>
    /// Layout coordinates
    /// </summary>
    public abstract PointF LayoutPosition { get; }

    /// <summary>
    /// Current layout coordinates
    /// </summary>
    public abstract PointF CurrentLayoutPosition { get; }

    /// <summary>
    /// Sets the initial layout position of this node.
    /// </summary>
    /// <param name="pos">The initial layout position to set.</param>
    public abstract void SetInitialLayoutPosition(PointF pos);

    /// <summary>
    /// Apply layout. By applying for a layout from the higher-level node, calculate your own Rect.
    /// </summary>
    public abstract void Layout();

    #endregion

    #region Fit

    /// <summary>
    /// Calculates the fit size of this node based on its content and constraints.
    /// </summary>
    public abstract void Fit();

    #endregion

    #region Render

    /// <summary>
    /// Obtain whether rendering is required
    /// </summary>
    public abstract bool NeedRender { get; }

    /// <summary>
    /// Get whether it has been rendered or not
    /// </summary>
    public abstract bool IsRendered { get; }

    /// <summary>
    /// Renders this node to the specified pipeline and graphics output.
    /// </summary>
    /// <param name="pipeline">The rendering pipeline stage.</param>
    /// <param name="output">The graphics output target.</param>
    public abstract void Render(GuiPipeline pipeline, IGraphicOutput output);

    #endregion

    #region Dirty

    /// <summary>
    /// Obtain the dirty flag for rendering
    /// </summary>
    public virtual bool IsRenderDirty => Flags.HasFlag(ImGuiNodeFlags.IsRenderDirty);// || (Flags.HasFlag(ImGuiNodeFlags.IsChildrenRenderDirty) && Flags.HasFlag(ImGuiNodeFlags.Overlapped)) ;

    /// <summary>
    /// Retrieve whether the child node contains a dirty rendering flag
    /// </summary>
    public virtual bool IsChildrenRenderDirty => Flags.HasFlag(ImGuiNodeFlags.IsChildrenRenderDirty);

    /// <summary>
    /// Marking and rendering dirty flags
    /// </summary>
    public abstract void MarkRenderDirty();

    /// <summary>
    /// Marks a specific area as needing re-rendering.
    /// </summary>
    /// <param name="dirtyRect">The rectangle area to mark as dirty.</param>
    public abstract void MarkRenderDirty(RectangleF dirtyRect);

    /// <summary>
    /// Marking and rendering dirty flags
    /// </summary>
    /// <param name="mouseClickRectDirty">Mark the mouse click area as dirty</param>
    public abstract void MarkRenderDirty(bool mouseClickRectDirty);

    #endregion

    #region Global state

    /// <summary>
    /// Get whether the mouse is in the area
    /// </summary>
    public virtual bool IsMouseIn => Gui.GetIsMouseIn(this);

    /// <summary>
    /// Get whether it is the current mouse hover node
    /// </summary>
    public virtual bool IsHover => Gui.HoverNode == this;

    /// <summary>
    /// Get whether it is the current focus node
    /// </summary>
    public virtual bool IsFocused => Gui.FocusNode == this;

    /// <summary>
    /// Get whether it is the current control node
    /// </summary>
    public virtual bool IsControlling => Gui.ControllingNode == this;

    /// <summary>
    /// Sets the focus state of this node.
    /// </summary>
    /// <param name="isFocused">True to set this node as focused; false to remove focus.</param>
    public abstract void SetIsFocused(bool isFocused);

    /// <summary>
    /// Sets the controlling state of this node.
    /// </summary>
    /// <param name="isControlling">True to set this node as controlling; false to remove control.</param>
    public abstract void SetIsControlling(bool isControlling);

    #endregion
}