using System;

namespace Suity.Views.Im;

/// <summary>
/// Delegate for drawing ImGui content.
/// </summary>
/// <param name="gui">The ImGui instance to draw with.</param>
public delegate void DrawImGui(ImGui gui);

/// <summary>
/// Delegate for drawing an ImGui node and returning it.
/// </summary>
/// <param name="gui">The ImGui instance to draw with.</param>
/// <returns>The created ImGuiNode.</returns>
public delegate ImGuiNode DrawImGuiNode(ImGui gui);

/// <summary>
/// Interface for objects that can draw ImGui content.
/// </summary>
public interface IDrawImGui
{
    /// <summary>
    /// Called to draw ImGui content.
    /// </summary>
    /// <param name="gui">The ImGui instance to draw with.</param>
    void OnGui(ImGui gui);
}

/// <summary>
/// Interface for objects that can draw and return an ImGui node.
/// </summary>
public interface IDrawImGuiNode
{
    /// <summary>
    /// Called to draw ImGui content and return the resulting node.
    /// </summary>
    /// <param name="gui">The ImGui instance to draw with.</param>
    /// <returns>The created ImGuiNode.</returns>
    ImGuiNode OnNodeGui(ImGui gui);
}

/// <summary>
/// Wrapper class that adapts a DrawImGui delegate to the IDrawImGui interface.
/// </summary>
public class DrawImguiWrapper : IDrawImGui
{
    private readonly DrawImGui _onGui;

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawImguiWrapper"/> class.
    /// </summary>
    /// <param name="onGui">The draw delegate to wrap.</param>
    public DrawImguiWrapper(DrawImGui onGui)
    {
        _onGui = onGui ?? throw new ArgumentNullException(nameof(onGui));
    }

    /// <inheritdoc/>
    public void OnGui(ImGui gui)
    {
        _onGui(gui);
    }
}
