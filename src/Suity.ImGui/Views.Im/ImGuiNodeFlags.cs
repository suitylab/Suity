using System;

namespace Suity.Views.Im;

/// <summary>
/// Flags that represent the various states and behaviors of an ImGui node.
/// </summary>
[Flags]
public enum ImGuiNodeFlags
{
    /// <summary>
    /// No flags set.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// The node construction is completed and the first Fit operation is completed
    /// </summary>
    NodeInitialized = 0x1,

    /// <summary>
    /// Node is synchronizing
    /// </summary>
    NodeSync = 0x2,

    /// <summary>
    /// Style information construction completed
    /// </summary>
    StyleCreated = 0x4,

    /// <summary>
    /// Has input
    /// </summary>
    HasInput = 0x8,

    /// <summary>
    /// Mouse in node rectangle
    /// </summary>
    MouseInRect = 0x10,

    /// <summary>
    /// Mouse in node padding rectangle
    /// </summary>
    MouseInInnerRect = 0x20,

    /// <summary>
    /// Mouse in click area
    /// </summary>
    MouseInClickRect = 0x40,

    /// <summary>
    /// The node is dirty and needs to be redrawn
    /// </summary>
    IsRenderDirty = 0x80,

    /// <summary>
    /// Partial dirty area, need to continue collecting child nodes
    /// </summary>
    PartialDirtyRect = 0x100,

    /// <summary>
    /// Contains dirty child nodes
    /// </summary>
    IsChildrenRenderDirty = 0x200,

    /// <summary>
    /// Node has been drawn successfully
    /// </summary>
    IsRendered = 0x400,

    /// <summary>
    /// Support Pseudo to influence child nodes
    /// </summary>
    PseudoAffectsChildren = 0x800,

    /// <summary>
    /// Disabled
    /// </summary>
    Disabled = 0x1000,

    /// <summary>
    /// Disable inheritance at the parent hierarchy
    /// </summary>
    DisabledFromParent = 0x2000,

    /// <summary>
    /// Read only
    /// </summary>
    ReadOnly = 0x4000,

    /// <summary>
    /// Read only inheritance at the parent hierarchy
    /// </summary>
    ReadOnlyFromParent = 0x8000,

    /// <summary>
    /// Indicates that this node has exceeded the boundary of the parent node
    /// </summary>
    OutOfBound = 0x10000,

    /// <summary>
    /// Edited
    /// </summary>
    Edited = 0x20000,

    /// <summary>
    /// Reverse style order
    /// </summary>
    OverrideDisabled = 0x40000,

    /// <summary>
    /// Floating layout, unaffected by layout functions
    /// </summary>
    Floating = 0x80000,

    /// <summary>
    /// Reverse input order
    /// </summary>
    RevertInputOrder = 0x100000,

    /// <summary>
    /// Compact mode, mainly used for horizontal layout of attribute editors
    /// </summary>
    Compact = 0x200000,

    /// <summary>
    /// The marked sub objects can overlap in layout. During rendering, the optimization of ClipRect for individual objects will be canceled
    /// </summary>
    Overlapped = 0x400000,

    /// <summary>
    /// Support dragging and dropping events outside the node area with the mouse, mainly used for Viewport views.
    /// </summary>
    MouseDragOutSideEvent = 0x800000,

    /// <summary>
    /// Not performing its own Transform operation, but executing the parent node without affecting the child nodes, mainly used for Viewport views.
    /// </summary>
    NoTransform = 0x1000000,

    /// <summary>
    /// Dual layout, used for certain UIs with dual constraints such as minimum and maximum values, requiring two layouts to be executed in the ImGui environment.
    /// </summary>
    DoubleLayout = 0x2000000,

    /// <summary>
    /// Mark as deleted. The node will be deleted after the next sync operation and will be create new to replace it, useful when the node contains legacy data.
    /// </summary>
    MarkDeleted = 0x4000000,
}