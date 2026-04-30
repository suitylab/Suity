using Suity.Editor.Services;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Provides extension methods for ImGui nodes with text status coloring.
/// </summary>
public static class ImGuiTreeExtensions
{
    /// <summary>
    /// Sets the background color of the node based on the specified text status.
    /// </summary>
    /// <param name="node">The ImGui node to modify.</param>
    /// <param name="textStatus">The text status used to determine the color.</param>
    /// <returns>The modified ImGui node for chaining.</returns>
    public static ImGuiNode SetColor(this ImGuiNode node, TextStatus textStatus)
    {
        if (textStatus == TextStatus.Normal)
        {
            node.Color = null;
        }
        else
        {
            node.Color = EditorServices.ColorConfig.GetStatusColor(textStatus);
        }

        return node;
    }

    /// <summary>
    /// Sets the font color of the node based on the specified text status.
    /// </summary>
    /// <param name="node">The ImGui node to modify.</param>
    /// <param name="textStatus">The text status used to determine the font color.</param>
    /// <returns>The modified ImGui node for chaining.</returns>
    public static ImGuiNode SetFontColor(this ImGuiNode node, TextStatus textStatus)
    {
        if (textStatus == TextStatus.Normal)
        {
            node.FontColor = null;
        }
        else
        {
            node.FontColor = EditorServices.ColorConfig.GetStatusColor(textStatus);
        }

        return node;
    }
}