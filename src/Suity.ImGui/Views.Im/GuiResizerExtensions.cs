using System;
using Suity.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Suity.Views.Im;

/// <summary>
/// Stores the state for grouped resizers, tracking the length of each segment.
/// </summary>
public class GroupedResizerState
{
    private readonly List<float> _lengths = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupedResizerState"/> class.
    /// </summary>
    public GroupedResizerState()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified segment lengths.
    /// </summary>
    public GroupedResizerState(params float[] lengths)
    {
        _lengths.AddRange(lengths);
    }

    /// <summary>
    /// Sets all segment lengths, replacing existing values.
    /// </summary>
    public void SetLengths(params float[] lengths)
    {
        _lengths.Clear();
        _lengths.AddRange(lengths);
    }

    /// <summary>
    /// Sets the length of a specific segment.
    /// </summary>
    public void SetLength(int index, float length)
    {
        if (index < 0 || index > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        while (index >= _lengths.Count)
        {
            _lengths.Add(0);
        }
        _lengths[index] = length;
    }

    /// <summary>
    /// Gets the length of a specific segment, or null if not set.
    /// </summary>
    public float? GetLength(int index)
    {
        return index >= 0 && index < _lengths.Count ? _lengths[index] : null;
    }

    /// <summary>
    /// Gets the length of a specific segment, setting a default if not set.
    /// </summary>
    public float GetLength(int index, float defaultLength)
    {
        if (GetLength(index) is { } length)
        {
            return length;
        }
        else
        {
            SetLength(index, defaultLength);
            return defaultLength;
        }
    }

    /// <summary>
    /// Gets whether a length has been set for the specified index.
    /// </summary>
    public bool HasLength(int index)
    {
        return index >= 0 && index < _lengths.Count;
    }

    /// <summary>
    /// Gets or sets the length at the specified index.
    /// </summary>
    public float? this[int index]
    {
        get => GetLength(index);
        set => SetLength(index, value ?? 0);
    }

    /// <summary>
    /// Removes the length at the specified index.
    /// </summary>
    public bool RemoveAt(int index)
    {
        return _lengths.RemoveAtSafe(index);
    }

    /// <summary>
    /// Swaps two segment lengths.
    /// </summary>
    public bool Swap(int index, int indexTo)
    {
        return _lengths.SwapListItem(index, indexTo);
    }

    /// <summary>
    /// Removes a segment from one position and inserts it at another.
    /// </summary>
    public bool RemoveInsert(int indexFrom, int indexInsert)
    {
        return _lengths.RemoveInserListItem(indexFrom, indexInsert);
    }
}

/// <summary>
/// Stores state for an individual grouped resizer item.
/// </summary>
public class GroupedResizerItem
{
    /// <summary>
    /// Gets or sets whether the size has been initialized.
    /// </summary>
    public bool SizeInitialized { get; set; }

    /// <summary>
    /// Gets or sets the index of this resizer in the group.
    /// </summary>
    public int Index { get; set; }
}

/// <summary>
/// Extension methods for creating resizer controls in ImGui.
/// </summary>
public static class GuiResizerExtensions
{
    /// <summary>
    /// Creates a vertical resizer with auto-generated ID.
    /// </summary>
    public static ImGuiNode VerticalResizer(this ImGui gui, float? min = null, float? max = null, bool affectSibling = false, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    {
        string id = $"##v_resizer_{member}#{line}";
        return gui.VerticalResizer(id, min, max, affectSibling);
    }

    /// <summary>
    /// Creates a vertical resizer with the specified ID.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">The unique identifier for the resizer.</param>
    /// <param name="min">The minimum position constraint, or null for no minimum.</param>
    /// <param name="max">The maximum position constraint, or null for no maximum.</param>
    /// <param name="affectSibling">Whether the resizer should affect sibling nodes.</param>
    /// <returns>An ImGuiNode representing the vertical resizer.</returns>
    public static ImGuiNode VerticalResizer(this ImGui gui, string id, float? min = null, float? max = null, bool affectSibling = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Resizer";
            node.SetInputFunction(ImGuiInputSystem.Resizer);
            node.SetRenderFunction(nameof(GuiCommonExtensions.HorizontalLine));
            node.FitOrientation = GuiOrientation.Vertical;
            node.GetOrCreateValue(() => new GuiResizerValue
            {
                Orientation = GuiOrientation.Vertical,
                MinPosition = min,
                MaxPosition = max,
                AffectSibling = affectSibling,
            });
        }
        return node;
    }

    /// <summary>
    /// Creates a horizontal resizer with auto-generated ID.
    /// </summary>
    public static ImGuiNode HorizontalResizer(this ImGui gui, float? min = null, float? max = null, bool affectSibling = false, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    {
        string id = $"##h_resizer_{member}#{line}";
        return gui.HorizontalResizer(id, min, max, affectSibling);
    }

    /// <summary>
    /// Creates a horizontal resizer with the specified ID.
    /// </summary>
    public static ImGuiNode HorizontalResizer(this ImGui gui, string id, float? min = null, float? max = null, bool affectSibling = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "Resizer";
            node.SetInputFunction(ImGuiInputSystem.Resizer);
            node.SetRenderFunction(nameof(GuiCommonExtensions.VerticalLine));
            node.FitOrientation = GuiOrientation.Horizontal;
            node.GetOrCreateValue(() => new GuiResizerValue
            {
                Orientation = GuiOrientation.Horizontal,
                MinPosition = min,
                MaxPosition = max,
                AffectSibling = affectSibling,
            });
        }
        return node;
    }

    /// <summary>
    /// Gets the current position of a vertical resizer.
    /// </summary>
    public static float? GetVerticalResizerPosition(this ImGuiNode node)
    {
        var value = node.GetValue<GuiResizerValue>();
        if (value != null)
        {
            return value.ContentSize.Height;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the current position of a horizontal resizer.
    /// </summary>
    public static float? GetHorizontalResizerPosition(this ImGuiNode node)
    {
        var value = node.GetValue<GuiResizerValue>();
        if (value != null)
        {
            return value.ContentSize.Width;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a vertical resizer fitter with auto-generated ID.
    /// </summary>
    public static ImGuiNode VerticalResizerFitter(this ImGui gui, float? min = null, float? max = null, bool affectSibling = false, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    {
        string id = $"##v_resizer_fitter_{member}#{line}";
        return gui.VerticalResizerFitter(id, min, max, affectSibling);
    }

    /// <summary>
    /// Creates a vertical resizer fitter with the specified ID.
    /// </summary>
    public static ImGuiNode VerticalResizerFitter(this ImGui gui, string id, float? min = null, float? max = null, bool affectSibling = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "ResizerFitter";
            node.SetInputFunction(ImGuiInputSystem.ResizerFitter);
            node.SetRenderFunction(nameof(GuiCommonExtensions.HorizontalLine));
            node.FitOrientation = GuiOrientation.Vertical;
            node.GetOrCreateValue(() => new GuiResizerValue
            {
                Orientation = GuiOrientation.Vertical,
                MinPosition = min,
                MaxPosition = max,
                AffectSibling = affectSibling,
            });
        }
        return node;
    }

    /// <summary>
    /// Creates a horizontal resizer fitter with auto-generated ID.
    /// </summary>
    public static ImGuiNode HorizontalResizerFitter(this ImGui gui, float? min = null, float? max = null, bool affectSibling = false, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
    {
        string id = $"##h_resizer_fitter_{member}#{line}";
        return gui.HorizontalResizerFitter(id, min, max, affectSibling);
    }

    /// <summary>
    /// Creates a horizontal resizer fitter with the specified ID.
    /// </summary>
    public static ImGuiNode HorizontalResizerFitter(this ImGui gui, string id, float? min = null, float? max = null, bool affectSibling = false)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);
        if (node.IsInitializing)
        {
            node.TypeName = "ResizerFitter";
            node.SetInputFunction(ImGuiInputSystem.ResizerFitter);
            node.SetRenderFunction(nameof(GuiCommonExtensions.VerticalLine));
            node.FitOrientation = GuiOrientation.Horizontal;
            node.GetOrCreateValue(() => new GuiResizerValue
            {
                Orientation = GuiOrientation.Horizontal,
                MinPosition = min,
                MaxPosition = max,
                AffectSibling = affectSibling,
            });
        }
        return node;
    }

    /// <summary>
    /// Initializes a grouped resizer state on a node.
    /// </summary>
    /// <param name="node">The ImGuiNode to initialize the state on.</param>
    /// <returns>The same ImGuiNode for method chaining.</returns>
    public static ImGuiNode InitGroupedResizerState(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.GetOrCreateValue<GroupedResizerState>();
        }
        return node;
    }

    /// <summary>
    /// Initializes a grouped resizer state with the specified state.
    /// </summary>
    public static ImGuiNode InitGroupedResizerState(this ImGuiNode node, GroupedResizerState state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (node.IsInitializing)
        {
            node.SetValue(state);
        }
        return node;
    }

    /// <summary>
    /// Initializes a node as part of a grouped resizer at the specified index.
    /// </summary>
    public static ImGuiNode InitGroupedResizer(this ImGuiNode node, int index)
    {
        if (node.IsInitializing)
        {
            node.InitInputFunctionChain(ImGuiInputSystem.GroupedResizer);
            node.GetOrCreateValue<GroupedResizerItem>().Index = index;
        }
        return node;
    }
}
