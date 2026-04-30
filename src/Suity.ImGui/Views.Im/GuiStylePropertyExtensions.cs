using Suity.Views.Graphics;
using System;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for setting style properties on ImGui nodes and themes, including fit, size, margin, padding, color, font, border, and alignment.
/// </summary>
public static class GuiStylePropertyExtensions
{
    #region Fit

    /// <summary>
    /// Sets the fit orientation of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="orientation">The fit orientation to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFit(this ImGuiNode node, GuiOrientation orientation)
    {
        node.FitOrientation = orientation;
        return node;
    }

    /// <summary>
    /// Sets the node to fit both horizontally and vertically.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFitBoth(this ImGuiNode node)
    {
        node.FitOrientation = GuiOrientation.Both;
        node.Width = null;
        node.Height = null;
        return node;
    }

    /// <summary>
    /// Sets the node to fit horizontally.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFitHorizontal(this ImGuiNode node)
    {
        node.FitOrientation = GuiOrientation.Horizontal;
        return node;
    }

    /// <summary>
    /// Sets the node to fit vertically.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFitVertical(this ImGuiNode node)
    {
        node.FitOrientation = GuiOrientation.Vertical;
        return node;
    }

    /// <summary>
    /// Sets the fit orientation only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="orientation">The fit orientation to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFit(this ImGuiNode node, GuiOrientation orientation)
    {
        if (node.IsInitializing)
        {
            node.FitOrientation = orientation;
        }
        return node;
    }

    /// <summary>
    /// Sets the node to fit both directions only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFit(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.FitOrientation = GuiOrientation.Both;
            node.Width = null;
            node.Height = null;
        }
        return node;
    }

    /// <summary>
    /// Sets the node to fit horizontally only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFitHorizontal(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.FitOrientation = GuiOrientation.Horizontal;
        }
        return node;
    }

    /// <summary>
    /// Sets the node to fit vertically only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFitVertical(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.FitOrientation = GuiOrientation.Vertical;
        }
        return node;
    }

    /// <summary>
    /// Removes horizontal fit from a node's fit orientation.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    internal static ImGuiNode UnsetFitHorizontal(this ImGuiNode node)
    {
        if (node.FitOrientation == GuiOrientation.Both)
        {
            node.FitOrientation = GuiOrientation.Vertical;
        }
        else if (node.FitOrientation == GuiOrientation.Horizontal)
        {
            node.FitOrientation = GuiOrientation.None;
        }
        return node;
    }

    /// <summary>
    /// Removes vertical fit from a node's fit orientation.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    internal static ImGuiNode UnsetFitVertical(this ImGuiNode node)
    {
        if (node.FitOrientation == GuiOrientation.Both)
        {
            node.FitOrientation = GuiOrientation.Horizontal;
        }
        else if (node.FitOrientation == GuiOrientation.Vertical)
        {
            node.FitOrientation = GuiOrientation.None;
        }
        return node;
    }

    #endregion

    #region Size

    /// <summary>
    /// Sets the width and height of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The width to set.</param>
    /// <param name="height">The height to set.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetSize(this ImGuiNode node, float width, float height)
    {
        node.Width = width;
        node.Height = height;
        node.FitOrientation = GuiOrientation.None;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width and height of a node with specific modes.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="wValue">The width value.</param>
    /// <param name="wMode">The width mode.</param>
    /// <param name="hValue">The height value.</param>
    /// <param name="hMode">The height mode.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetSize(this ImGuiNode node, float wValue, GuiLengthMode wMode, float hValue, GuiLengthMode hMode)
    {
        node.Width = new GuiLength(wValue, wMode);
        node.Height = new GuiLength(hValue, hMode);
        node.FitOrientation = GuiOrientation.None;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width and height with scaled fixed mode.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The scaled width value.</param>
    /// <param name="height">The scaled height value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetScaledSize(this ImGuiNode node, float width, float height)
    {
        node.Width = new GuiLength(width, GuiLengthMode.ScaledFixed);
        node.Height = new GuiLength(height, GuiLengthMode.ScaledFixed);
        node.FitOrientation = GuiOrientation.None;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the node to fill 100% of available width and height.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFullSize(this ImGuiNode node)
    {
        node.Width = new GuiLength(100, GuiLengthMode.Percentage);
        node.Height = new GuiLength(100, GuiLengthMode.Percentage);
        node.FitOrientation = GuiOrientation.None;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the node to take remaining available space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetSizeRest(this ImGuiNode node)
    {
        node.Width = new GuiLength(0, GuiLengthMode.RestExcept);
        node.Height = new GuiLength(0, GuiLengthMode.RestExcept);
        node.FitOrientation = GuiOrientation.None;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the node size as a percentage of remaining space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="wPercentage">The width percentage of remaining space.</param>
    /// <param name="hPercentage">The height percentage of remaining space.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetSizePercentageRest(this ImGuiNode node, float wPercentage, float hPercentage)
    {
        node.Width = new GuiLength(wPercentage, GuiLengthMode.RestPercentage);
        node.Height = new GuiLength(hPercentage, GuiLengthMode.RestPercentage);
        node.FitOrientation = GuiOrientation.None;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the node size as a percentage of total space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="wPercentage">The width percentage of total space.</param>
    /// <param name="hPercentage">The height percentage of total space.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetSizePercentage(this ImGuiNode node, float wPercentage, float hPercentage)
    {
        node.Width = new GuiLength(wPercentage, GuiLengthMode.Percentage);
        node.Height = new GuiLength(hPercentage, GuiLengthMode.Percentage);
        node.FitOrientation = GuiOrientation.None;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width with scaled fixed mode.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The scaled width value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetScaledWidth(this ImGuiNode node, float width)
    {
        node.Width = new GuiLength(width, GuiLengthMode.ScaledFixed);
        node.UnsetFitHorizontal();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The width to set.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetWidth(this ImGuiNode node, float width)
    {
        node.Width = width;
        node.UnsetFitHorizontal();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width of a node with a specific mode.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="value">The width value.</param>
    /// <param name="mode">The length mode to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetWidth(this ImGuiNode node, float value, GuiLengthMode mode)
    {
        node.Width = new GuiLength(value, mode);
        node.UnsetFitHorizontal();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width as a percentage of remaining space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="percentage">The width percentage of remaining space.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetWidthPercentageRest(this ImGuiNode node, float percentage)
    {
        node.Width = new GuiLength(percentage, GuiLengthMode.RestPercentage);
        node.UnsetFitHorizontal();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width as a percentage of total space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="value">The width percentage of total space.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetWidthPercentage(this ImGuiNode node, float value)
    {
        node.Width = new GuiLength(value, GuiLengthMode.Percentage);
        node.UnsetFitHorizontal();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the width to take remaining available space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetWidthRest(this ImGuiNode node) => node.SetWidth(0, GuiLengthMode.RestExcept);

    /// <summary>
    /// Sets the width to take remaining available space with a specific rest value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="rest">The rest value to use.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetWidthRest(this ImGuiNode node, float rest) => node.SetWidth(rest, GuiLengthMode.RestExcept);

    /// <summary>
    /// Sets the width to 100% of available space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFullWidth(this ImGuiNode node) => node.SetWidth(100, GuiLengthMode.Percentage);

    /// <summary>
    /// Sets the height with scaled fixed mode.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="height">The scaled height value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetScaledHeight(this ImGuiNode node, float height)
    {
        node.Height = new GuiLength(height, GuiLengthMode.ScaledFixed);
        node.UnsetFitHorizontal();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the height of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="height">The height to set.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeight(this ImGuiNode node, float height)
    {
        node.Height = height;
        node.UnsetFitVertical();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the height of a node with a specific mode.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="value">The height value.</param>
    /// <param name="mode">The length mode to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeight(this ImGuiNode node, float value, GuiLengthMode mode)
    {
        node.Height = new GuiLength(value, mode);
        node.UnsetFitVertical();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the height as a percentage of remaining space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="percentage">The height percentage of remaining space.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeightPercentageRest(this ImGuiNode node, float percentage)
    {
        node.Height = new GuiLength(percentage, GuiLengthMode.RestPercentage);
        node.UnsetFitVertical();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the height as a percentage of total space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="value">The height percentage of total space.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeightPercentage(this ImGuiNode node, float value)
    {
        node.Height = new GuiLength(value, GuiLengthMode.Percentage);
        node.UnsetFitVertical();
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the height to take remaining available space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeightRest(this ImGuiNode node) => node.SetHeight(0, GuiLengthMode.RestExcept);

    /// <summary>
    /// Sets the height to take remaining available space with a specific rest value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="rest">The rest value to use.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeightRest(this ImGuiNode node, float rest) => node.SetHeight(rest, GuiLengthMode.RestExcept);

    /// <summary>
    /// Sets the height to 100% of available space.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFullHeight(this ImGuiNode node) => node.SetHeight(100, GuiLengthMode.Percentage);

    /// <summary>
    /// Sets the size only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The width to set.</param>
    /// <param name="height">The height to set.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitSize(this ImGuiNode node, float width, float height)
    {
        if (node.IsInitializing)
        {
            node.Width = width;
            node.Height = height;
            node.FitOrientation = GuiOrientation.None;
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the size with specific modes only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="wValue">The width value.</param>
    /// <param name="wMode">The width mode.</param>
    /// <param name="hValue">The height value.</param>
    /// <param name="hMode">The height mode.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitSize(this ImGuiNode node, float wValue, GuiLengthMode wMode, float hValue, GuiLengthMode hMode)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(wValue, wMode);
            node.Height = new GuiLength(hValue, hMode);
            node.FitOrientation = GuiOrientation.None;
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the scaled size only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The scaled width value.</param>
    /// <param name="height">The scaled height value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitScaledSize(this ImGuiNode node, float width, float height)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(width, GuiLengthMode.ScaledFixed);
            node.Height = new GuiLength(height, GuiLengthMode.ScaledFixed);
            node.FitOrientation = GuiOrientation.None;
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the node to fill 100% of available space only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFullSize(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(100, GuiLengthMode.Percentage);
            node.Height = new GuiLength(100, GuiLengthMode.Percentage);
            node.FitOrientation = GuiOrientation.None;
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the node to take remaining available space only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitSizeRest(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(0, GuiLengthMode.RestExcept);
            node.Height = new GuiLength(0, GuiLengthMode.RestExcept);
            node.FitOrientation = GuiOrientation.None;

            // After testing, ApplyLayout still needs to be executed, because subsequent layouts need to obtain Rect data
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the node to adaptive sizing only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitSizeAdapt(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(0, GuiLengthMode.Adapt);
            node.Height = new GuiLength(0, GuiLengthMode.Adapt);
            node.FitOrientation = GuiOrientation.None;
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the scaled width only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The scaled width value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitScaledWidth(this ImGuiNode node, float width)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(width, GuiLengthMode.ScaledFixed);
            node.UnsetFitHorizontal();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the width only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The width to set.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitWidth(this ImGuiNode node, float width)
    {
        if (node.IsInitializing)
        {
            node.Width = width;
            node.UnsetFitHorizontal();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the width with a specific mode only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="value">The width value.</param>
    /// <param name="mode">The length mode to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitWidth(this ImGuiNode node, float value, GuiLengthMode mode)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(value, mode);
            node.UnsetFitHorizontal();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the width as a percentage only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="percentage">The width percentage.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitWidthPercentage(this ImGuiNode node, float percentage)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(percentage, GuiLengthMode.Percentage);
            node.UnsetFitHorizontal();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the width to 100% only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFullWidth(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(100, GuiLengthMode.Percentage);
            node.UnsetFitHorizontal();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the width to take remaining space only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitWidthRest(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.SetWidth(0, GuiLengthMode.RestExcept);
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the width to take remaining space with a specific rest value only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="rest">The rest value to use.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitWidthRest(this ImGuiNode node, float rest)
    {
        if (node.IsInitializing)
        {
            node.SetWidth(rest, GuiLengthMode.RestExcept);
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the width to adaptive sizing only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitWidthAdapt(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.Width = new GuiLength(0, GuiLengthMode.Adapt);
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the scaled height only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="height">The scaled height value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitScaledHeight(this ImGuiNode node, float height)
    {
        if (node.IsInitializing)
        {
            node.Height = new GuiLength(height, GuiLengthMode.ScaledFixed);
            node.UnsetFitHorizontal();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the height only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="height">The height to set.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeight(this ImGuiNode node, float height)
    {
        if (node.IsInitializing)
        {
            node.Height = height;
            node.UnsetFitVertical();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the height with a specific mode only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="value">The height value.</param>
    /// <param name="mode">The length mode to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeight(this ImGuiNode node, float value, GuiLengthMode mode)
    {
        if (node.IsInitializing)
        {
            node.Height = new GuiLength(value, mode);
            node.UnsetFitVertical();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the height as a percentage only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="percentage">The height percentage.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeightPercentage(this ImGuiNode node, float percentage)
    {
        if (node.IsInitializing)
        {
            node.Height = new GuiLength(percentage, GuiLengthMode.Percentage);
            node.UnsetFitVertical();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the height to 100% only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFullHeight(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.Height = new GuiLength(100, GuiLengthMode.Percentage);
            node.UnsetFitVertical();
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the height to take remaining space only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeightRest(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.SetHeight(0, GuiLengthMode.RestExcept);
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the height to take remaining space with a specific rest value only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="rest">The rest value to use.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeightRest(this ImGuiNode node, float rest)
    {
        if (node.IsInitializing)
        {
            node.SetHeight(rest, GuiLengthMode.RestExcept);
            node.Layout();
        }
        return node;
    }

    /// <summary>
    /// Sets the height to adaptive sizing only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeightAdapt(this ImGuiNode node)
    {
        if (node.IsInitializing)
        {
            node.Height = new GuiLength(0, GuiLengthMode.Adapt);
            node.Layout();
        }
        return node;
    }

    private static ImGuiNode UnsetSize(this ImGuiNode node)
    {
        node.Width = null;
        node.Height = null;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Overrides the size using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The width override value.</param>
    /// <param name="height">The height override value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideSize(this ImGuiNode node, float width, float height)
    {
        var size = node.GetOrCreateValue<GuiSizeStyle>();
        size.Width = width;
        size.Height = height;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Overrides the size using a style value only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The width override value.</param>
    /// <param name="height">The height override value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideSize(this ImGuiNode node, float width, float height)
    {
        if (node.IsInitializing)
        {
            var size = node.GetOrCreateValue<GuiSizeStyle>();
            size.Width = width;
            size.Height = height;
            node.Layout();
        }
        return node;
    }

    #endregion

    #region Size (theme)

    /// <summary>
    /// Sets the default size for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="width">The width to set.</param>
    /// <param name="height">The height to set.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetSize(this ImGuiTheme theme, GuiLength width, GuiLength height)
    {
        GuiSizeStyle size = new() { Width = width, Height = height };
        theme.SetStyle(size);
        return theme;
    }

    /// <summary>
    /// Sets the default size for a theme with specific modes.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="wValue">The width value.</param>
    /// <param name="wMode">The width mode.</param>
    /// <param name="hValue">The height value.</param>
    /// <param name="hMode">The height mode.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetSize(this ImGuiTheme theme, float wValue, GuiLengthMode wMode, float hValue, GuiLengthMode hMode)
    {
        GuiSizeStyle size = new() { Width = new GuiLength(wValue, wMode), Height = new GuiLength(hValue, hMode) };
        theme.SetStyle(size);
        return theme;
    }

    /// <summary>
    /// Sets the theme default size to fill 100% of available space.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetFullSize(this ImGuiTheme theme)
    {
        GuiSizeStyle size = new() { Width = new GuiLength(100, GuiLengthMode.Percentage), Height = new GuiLength(100, GuiLengthMode.Percentage) };
        theme.SetStyle(size);
        return theme;
    }

    /// <summary>
    /// Sets the theme default size to take remaining available space.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetSizeRest(this ImGuiTheme theme)
    {
        GuiSizeStyle size = new() { Width = new GuiLength(0, GuiLengthMode.RestExcept), Height = new GuiLength(0, GuiLengthMode.RestExcept) };
        theme.SetStyle(size);
        return theme;
    }

    /// <summary>
    /// Sets the theme default size as a percentage of remaining space.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="wPercentage">The width percentage of remaining space.</param>
    /// <param name="hPercentage">The height percentage of remaining space.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetSizePercentageRest(this ImGuiTheme theme, float wPercentage, float hPercentage)
    {
        GuiSizeStyle size = new() { Width = new GuiLength(wPercentage, GuiLengthMode.RestPercentage), Height = new GuiLength(hPercentage, GuiLengthMode.RestPercentage) };
        theme.SetStyle(size);
        return theme;
    }

    /// <summary>
    /// Sets the theme default size as a percentage of total space.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="wPercentage">The width percentage of total space.</param>
    /// <param name="hPercentage">The height percentage of total space.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetSizePercentage(this ImGuiTheme theme, float wPercentage, float hPercentage)
    {
        GuiSizeStyle size = new() { Width = new GuiLength(wPercentage, GuiLengthMode.Percentage), Height = new GuiLength(hPercentage, GuiLengthMode.Percentage) };
        theme.SetStyle(size);
        return theme;
    }

    /// <summary>
    /// Sets the theme default width.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="width">The width to set.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetWidth(this ImGuiTheme theme, GuiLength width)
    {
        theme.GetOrCreateStyle<GuiSizeStyle>().Width = width;
        return theme;
    }

    /// <summary>
    /// Sets the theme default width to take remaining space.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetWidthRest(this ImGuiTheme theme)
    {
        theme.GetOrCreateStyle<GuiSizeStyle>().Width = new GuiLength(0, GuiLengthMode.RestExcept);
        return theme;
    }

    /// <summary>
    /// Sets the theme default width to 100%.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetFullWidth(this ImGuiTheme theme)
    {
        theme.GetOrCreateStyle<GuiSizeStyle>().Width = new GuiLength(100, GuiLengthMode.Percentage);
        return theme;
    }

    /// <summary>
    /// Sets the theme default height.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="height">The height to set.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetHeight(this ImGuiTheme theme, GuiLength height)
    {
        theme.GetOrCreateStyle<GuiSizeStyle>().Height = height;
        return theme;
    }

    /// <summary>
    /// Sets the theme default height to take remaining space.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetHeightRest(this ImGuiTheme theme)
    {
        theme.GetOrCreateStyle<GuiSizeStyle>().Height = new GuiLength(0, GuiLengthMode.RestExcept);
        return theme;
    }

    /// <summary>
    /// Sets the theme default height to 100%.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetFullHeight(this ImGuiTheme theme)
    {
        theme.GetOrCreateStyle<GuiSizeStyle>().Height = new GuiLength(100, GuiLengthMode.Percentage);
        return theme;
    }

    #endregion

    #region Margin

    /// <summary>
    /// Sets the margin on all sides of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="margin">The margin value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetMargin(this ImGuiNode node, float margin)
    {
        node.Margin = margin;
        return node;
    }

    /// <summary>
    /// Sets the margin only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="margin">The margin value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitMargin(this ImGuiNode node, float margin)
    {
        if (node.IsInitializing) { node.Margin = margin; }
        return node;
    }

    /// <summary>
    /// Overrides the margin using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="margin">The margin value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideMargin(this ImGuiNode node, float margin)
    {
        node.GetOrCreateValue<GuiMarginStyle>().Margin = margin;
        return node;
    }

    /// <summary>
    /// Overrides the margin with individual side values.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top margin value.</param>
    /// <param name="bottom">The bottom margin value.</param>
    /// <param name="left">The left margin value.</param>
    /// <param name="right">The right margin value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideMargin(this ImGuiNode node, float top, float bottom, float left, float right)
    {
        node.GetOrCreateValue<GuiMarginStyle>().Margin = new GuiThickness { Top = top, Bottom = bottom, Left = left, Right = right };
        return node;
    }

    /// <summary>
    /// Overrides the margin with nullable side values.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top margin value, or null to keep existing.</param>
    /// <param name="bottom">The bottom margin value, or null to keep existing.</param>
    /// <param name="left">The left margin value, or null to keep existing.</param>
    /// <param name="right">The right margin value, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideMargin(this ImGuiNode node, float? top, float? bottom, float? left, float? right)
    {
        var value = node.GetOrCreateValue<GuiMarginStyle>();
        var m = value.Margin;
        if (top.HasValue) m.Top = top.Value;
        if (bottom.HasValue) m.Bottom = bottom.Value;
        if (left.HasValue) m.Left = left.Value;
        if (right.HasValue) m.Right = right.Value;
        value.Margin = m;
        return node;
    }

    /// <summary>
    /// Overrides the margin only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="margin">The margin value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideMargin(this ImGuiNode node, float margin)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiMarginStyle>().Margin = margin; }
        return node;
    }

    /// <summary>
    /// Overrides the margin with individual side values only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top margin value.</param>
    /// <param name="bottom">The bottom margin value.</param>
    /// <param name="left">The left margin value.</param>
    /// <param name="right">The right margin value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideMargin(this ImGuiNode node, float top, float bottom, float left, float right)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiMarginStyle>().Margin = new GuiThickness { Top = top, Bottom = bottom, Left = left, Right = right }; }
        return node;
    }

    /// <summary>
    /// Overrides the margin with nullable side values only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top margin value, or null to keep existing.</param>
    /// <param name="bottom">The bottom margin value, or null to keep existing.</param>
    /// <param name="left">The left margin value, or null to keep existing.</param>
    /// <param name="right">The right margin value, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideMargin(this ImGuiNode node, float? top, float? bottom, float? left, float? right)
    {
        if (node.IsInitializing)
        {
            var value = node.GetOrCreateValue<GuiMarginStyle>();
            var m = value.Margin;
            if (top.HasValue) m.Top = top.Value;
            if (bottom.HasValue) m.Bottom = bottom.Value;
            if (left.HasValue) m.Left = left.Value;
            if (right.HasValue) m.Right = right.Value;
            value.Margin = m;
        }
        return node;
    }

    /// <summary>
    /// Sets the default margin for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="margin">The margin thickness to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetMargin(this ImGuiTheme theme, GuiThickness margin)
    {
        theme.SetStyle(new GuiMarginStyle { Margin = margin });
        return theme;
    }

    /// <summary>
    /// Sets the default margin for a theme with individual side values.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="top">The top margin value.</param>
    /// <param name="bottom">The bottom margin value.</param>
    /// <param name="left">The left margin value.</param>
    /// <param name="right">The right margin value.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetMargin(this ImGuiTheme theme, float top, float bottom, float left, float right)
    {
        var margin = new GuiThickness { Top = top, Bottom = bottom, Left = left, Right = right };
        theme.SetStyle(new GuiMarginStyle { Margin = margin });
        return theme;
    }

    #endregion

    #region Padding

    /// <summary>
    /// Sets the padding on all sides of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="padding">The padding value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetPadding(this ImGuiNode node, float padding)
    {
        node.Padding = padding;
        return node;
    }

    /// <summary>
    /// Sets the padding only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="padding">The padding value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitPadding(this ImGuiNode node, float padding)
    {
        if (node.IsInitializing) { node.Padding = padding; }
        return node;
    }

    /// <summary>
    /// Sets the padding with individual side values only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top padding value.</param>
    /// <param name="bottom">The bottom padding value.</param>
    /// <param name="left">The left padding value.</param>
    /// <param name="right">The right padding value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitPadding(this ImGuiNode node, float top, float bottom, float left, float right)
    {
        if (node.IsInitializing) { node.Padding = new GuiThickness { Top = top, Bottom = bottom, Left = left, Right = right }; }
        return node;
    }

    /// <summary>
    /// Overrides the padding using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="padding">The padding value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverridePadding(this ImGuiNode node, float padding)
    {
        node.GetOrCreateValue<GuiPaddingStyle>().Padding = padding;
        return node;
    }

    /// <summary>
    /// Overrides the padding with individual side values.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top padding value.</param>
    /// <param name="bottom">The bottom padding value.</param>
    /// <param name="left">The left padding value.</param>
    /// <param name="right">The right padding value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverridePadding(this ImGuiNode node, float top, float bottom, float left, float right)
    {
        node.GetOrCreateValue<GuiPaddingStyle>().Padding = new GuiThickness { Top = top, Bottom = bottom, Left = left, Right = right };
        return node;
    }

    /// <summary>
    /// Overrides the padding with nullable side values.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top padding value, or null to keep existing.</param>
    /// <param name="bottom">The bottom padding value, or null to keep existing.</param>
    /// <param name="left">The left padding value, or null to keep existing.</param>
    /// <param name="right">The right padding value, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverridePadding(this ImGuiNode node, float? top, float? bottom, float? left, float? right)
    {
        var value = node.GetOrCreateValue<GuiPaddingStyle>();
        var p = value.Padding;
        if (top.HasValue) p.Top = top.Value;
        if (bottom.HasValue) p.Bottom = bottom.Value;
        if (left.HasValue) p.Left = left.Value;
        if (right.HasValue) p.Right = right.Value;
        value.Padding = p;
        return node;
    }

    /// <summary>
    /// Overrides the padding only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="padding">The padding value to apply to all sides.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverridePadding(this ImGuiNode node, float padding)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiPaddingStyle>().Padding = padding; }
        return node;
    }

    /// <summary>
    /// Overrides the padding with individual side values only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top padding value.</param>
    /// <param name="bottom">The bottom padding value.</param>
    /// <param name="left">The left padding value.</param>
    /// <param name="right">The right padding value.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverridePadding(this ImGuiNode node, float top, float bottom, float left, float right)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiPaddingStyle>().Padding = new GuiThickness { Top = top, Bottom = bottom, Left = left, Right = right }; }
        return node;
    }

    /// <summary>
    /// Overrides the padding with nullable side values only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="top">The top padding value, or null to keep existing.</param>
    /// <param name="bottom">The bottom padding value, or null to keep existing.</param>
    /// <param name="left">The left padding value, or null to keep existing.</param>
    /// <param name="right">The right padding value, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverridePadding(this ImGuiNode node, float? top, float? bottom, float? left, float? right)
    {
        if (node.IsInitializing)
        {
            var value = node.GetOrCreateValue<GuiPaddingStyle>();
            var p = value.Padding;
            if (top.HasValue) p.Top = top.Value;
            if (bottom.HasValue) p.Bottom = bottom.Value;
            if (left.HasValue) p.Left = left.Value;
            if (right.HasValue) p.Right = right.Value;
            value.Padding = p;
        }
        return node;
    }

    /// <summary>
    /// Sets the default padding for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="padding">The padding thickness to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetPadding(this ImGuiTheme theme, GuiThickness padding)
    {
        theme.SetStyle(new GuiPaddingStyle { Padding = padding });
        return theme;
    }

    /// <summary>
    /// Sets the default padding for a theme with individual side values.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="top">The top padding value.</param>
    /// <param name="bottom">The bottom padding value.</param>
    /// <param name="left">The left padding value.</param>
    /// <param name="right">The right padding value.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetPadding(this ImGuiTheme theme, float top, float bottom, float left, float right)
    {
        var padding = new GuiThickness { Top = top, Bottom = bottom, Left = left, Right = right };
        theme.SetStyle(new GuiPaddingStyle { Padding = padding });
        return theme;
    }

    #endregion

    #region ChildSpacing

    /// <summary>
    /// Sets the spacing between child nodes.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetChildSpacing(this ImGuiNode node, float? spacing)
    {
        node.ChildSpacing = spacing;
        return node;
    }

    /// <summary>
    /// Sets the child spacing only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitChildSpacing(this ImGuiNode node, float? spacing)
    {
        if (node.IsInitializing) { node.ChildSpacing = spacing; }
        return node;
    }

    /// <summary>
    /// Overrides the child spacing using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideChildSpacing(this ImGuiNode node, float? spacing)
    {
        node.GetOrCreateValue<GuiChildSpacingStyle>().ChildSpacing = spacing ?? 0;
        return node;
    }

    /// <summary>
    /// Overrides the child spacing only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideChildSpacing(this ImGuiNode node, float? spacing)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiChildSpacingStyle>().ChildSpacing = spacing ?? 0; }
        return node;
    }

    /// <summary>
    /// Sets the default child spacing for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="spacing">The spacing value to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetChildSpacing(this ImGuiTheme theme, float spacing)
    {
        theme.SetStyle(new GuiChildSpacingStyle() { ChildSpacing = spacing });
        return theme;
    }

    #endregion

    #region SiblingSpacing

    /// <summary>
    /// Sets the spacing between sibling nodes.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetSiblingSpacing(this ImGuiNode node, float spacing)
    {
        node.SiblingSpacing = spacing;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the sibling spacing only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitSiblingSpacing(this ImGuiNode node, float spacing)
    {
        if (node.IsInitializing) { node.SiblingSpacing = spacing; }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Overrides the sibling spacing using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideSiblingSpacing(this ImGuiNode node, float spacing)
    {
        node.GetOrCreateValue<GuiSiblingSpacingStyle>().SiblingSpacing = spacing;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Overrides the sibling spacing only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="spacing">The spacing value to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideSiblingSpacing(this ImGuiNode node, float spacing)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiSiblingSpacingStyle>().SiblingSpacing = spacing; }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the default sibling spacing for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="spacing">The spacing value to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetSiblingSpacing(this ImGuiTheme theme, float spacing)
    {
        theme.SetStyle(new GuiSiblingSpacingStyle() { SiblingSpacing = spacing });
        return theme;
    }

    #endregion

    #region Color

    /// <summary>
    /// Sets the background color of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The color to set, or null to use theme default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetColor(this ImGuiNode node, Color? color)
    {
        node.Color = color;
        return node;
    }

    /// <summary>
    /// Sets the background color from a theme color style.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The theme color style to use.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetColor(this ImGuiNode node, ColorStyle color)
    {
        node.Color = node.Theme.Colors.GetColor(color);
        return node;
    }

    /// <summary>
    /// Sets the color only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The color to set, or null to use theme default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitColor(this ImGuiNode node, Color? color)
    {
        if (node.IsInitializing) { node.Color = color; }
        return node;
    }

    /// <summary>
    /// Sets the color from a theme color style only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The theme color style to use.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitColor(this ImGuiNode node, ColorStyle color)
    {
        if (node.IsInitializing) { node.Color = node.Theme.Colors.GetColor(color); }
        return node;
    }

    /// <summary>
    /// Overrides the color using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The color to set, or null to remove the override.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideColor(this ImGuiNode node, Color? color)
    {
        if (color is { } c)
        {
            node.GetOrCreateValue<GuiColorStyle>().Color = c;
        }
        else
        {
            node.RemoveValue<GuiColorStyle>();
        }
        return node;
    }

    /// <summary>
    /// Overrides the color only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The color to set.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideColor(this ImGuiNode node, Color color)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiColorStyle>().Color = color; }
        return node;
    }

    /// <summary>
    /// Sets the default color for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="color">The color to set.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetColor(this ImGuiTheme theme, Color color)
    {
        theme.SetStyle(new GuiColorStyle { Color = color });
        return theme;
    }

    /// <summary>
    /// Sets the default color from a theme color style.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="color">The theme color style to use.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetColor(this ImGuiTheme theme, ColorStyle color)
    {
        var c = theme.Colors.GetColor(color);
        theme.SetStyle(new GuiColorStyle { Color = c });
        return theme;
    }

    /// <summary>
    /// Sets the font color of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The font color to set, or null to use theme default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFontColor(this ImGuiNode node, Color? color)
    {
        node.FontColor = color;
        return node;
    }

    /// <summary>
    /// Sets the font color only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The font color to set, or null to use theme default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFontColor(this ImGuiNode node, Color? color)
    {
        if (node.IsInitializing) { node.FontColor = color; }
        return node;
    }

    /// <summary>
    /// Gets the font color for a node.
    /// </summary>
    /// <param name="node">The node to get the font color from.</param>
    /// <returns>The font color, or the theme default if not set.</returns>
    public static Color GetFontColor(this ImGuiNode node)
    {
        return node.FontColor ?? node.Theme.Colors.GetColor(ColorStyle.Normal);
    }

    /// <summary>
    /// Gets the foreground color for a node.
    /// </summary>
    /// <param name="node">The node to get the foreground color from.</param>
    /// <returns>The foreground color, or the theme default if not set.</returns>
    public static Color GetForeColor(this ImGuiNode node)
    {
        return node.Color ?? node.Theme.Colors.GetColor(ColorStyle.Normal);
    }

    /// <summary>
    /// Gets the background color for a node.
    /// </summary>
    /// <param name="node">The node to get the background color from.</param>
    /// <returns>The background color, or the theme default if not set.</returns>
    public static Color GetBackgroundColor(this ImGuiNode node)
    {
        return node.Color ?? node.Theme.Colors.GetColor(ColorStyle.Background);
    }

    /// <summary>
    /// Gets the progress bar color for a node.
    /// </summary>
    /// <param name="node">The node to get the progress color from.</param>
    /// <returns>The progress bar color, or the theme default if not set.</returns>
    public static Color GetProgressColor(this ImGuiNode node)
    {
        return node.GetStyle<GuiProgressStyle>()?.Color ?? node.Theme.Colors.GetColor(ColorStyle.ProgressBar);
    }

    /// <summary>
    /// Gets the progress bar color with a fallback default.
    /// </summary>
    /// <param name="node">The node to get the progress color from.</param>
    /// <param name="defaultColor">A function that provides a fallback color.</param>
    /// <returns>The progress bar color, the fallback, or the theme default.</returns>
    public static Color GetProgressColor(this ImGuiNode node, Func<Color?> defaultColor)
    {
        return node.GetStyle<GuiProgressStyle>()?.Color ?? defaultColor() ?? node.Theme.Colors.GetColor(ColorStyle.ProgressBar);
    }

    #endregion

    #region Font

    /// <summary>
    /// Sets the font of a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="font">The font to set, or null to use theme default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetFont(this ImGuiNode node, Font? font)
    {
        node.Font = font;
        return node;
    }

    /// <summary>
    /// Sets the font only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="font">The font to set, or null to use theme default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitFont(this ImGuiNode node, Font? font)
    {
        if (node.IsInitializing) { node.Font = font; }
        return node;
    }

    #endregion

    #region Image

    /// <summary>
    /// Sets the image only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="image">The image to set.</param>
    /// <param name="color">Optional filter color to apply to the image.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitImage(this ImGuiNode node, Image image, Color? color = null)
    {
        if (node.IsInitializing)
        {
            node.Image = image;
            node.ImageFilterColor = color;
        }
        return node;
    }

    /// <summary>
    /// Sets the image on a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="image">The image to set.</param>
    /// <param name="color">Optional filter color to apply to the image.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetImage(this ImGuiNode node, Image image, Color? color = null)
    {
        node.Image = image;
        node.ImageFilterColor = color;
        return node;
    }

    /// <summary>
    /// Sets the default image for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="image">The image to set as default.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetImage(this ImGuiTheme theme, Image image)
    {
        theme.SetStyle(image);
        return theme;
    }

    /// <summary>
    /// Sets the default image filter color for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="color">The filter color to apply to images.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetImageFilterColor(this ImGuiTheme theme, Color color)
    {
        GuiImageFilterStyle filterStyle = new() { Color = color };
        theme.SetStyle(filterStyle);
        return theme;
    }

    /// <summary>
    /// Sets the default image filter color from a theme color style.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="color">The theme color style to use for the image filter.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetImageFilterColor(this ImGuiTheme theme, ColorStyle color)
    {
        var c = theme.Colors.GetColor(color);
        GuiImageFilterStyle filterStyle = new() { Color = c };
        theme.SetStyle(filterStyle);
        return theme;
    }

    #endregion

    #region Text

    /// <summary>
    /// Sets the text only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="text">The text to display.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitText(this ImGuiNode node, string text)
    {
        if (node.IsInitializing) { node.Text = text; }
        return node;
    }

    /// <summary>
    /// Sets the text on a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="text">The text to display.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetText(this ImGuiNode node, string text)
    {
        node.Text = text;
        return node;
    }

    /// <summary>
    /// Sets the default text alignment for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="alignment">The text alignment to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetTextAlignment(this ImGuiTheme theme, GuiAlignment alignment)
    {
        var style = theme.GetOrCreateStyle<GuiTextAlignmentStyle>();
        style.Alignment = alignment;
        return theme;
    }

    /// <summary>
    /// Sets the text alignment on a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="alignment">The text alignment to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetTextAlignment(this ImGuiNode node, GuiAlignment alignment)
    {
        node.GetOrCreateValue<GuiTextAlignmentStyle>().Alignment = alignment;
        return node;
    }

    /// <summary>
    /// Sets the text alignment only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="alignment">The text alignment to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitTextAlignment(this ImGuiNode node, GuiAlignment alignment)
    {
        if (node.IsInitializing) { node.GetOrCreateValue<GuiTextAlignmentStyle>().Alignment = alignment; }
        return node;
    }

    #endregion

    #region Font (override)

    /// <summary>
    /// Overrides the font and color using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="font">The font to set, or null to keep existing.</param>
    /// <param name="color">The color to set, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideFont(this ImGuiNode node, Font? font = null, Color? color = null)
    {
        var style = node.GetOrCreateValue<GuiFontStyle>();
        style.Font = font;
        style.Color = color;
        return node;
    }

    /// <summary>
    /// Overrides the font and color only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="font">The font to set, or null to keep existing.</param>
    /// <param name="color">The color to set, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideFont(this ImGuiNode node, Font? font = null, Color? color = null)
    {
        if (node.IsInitializing)
        {
            var style = node.GetOrCreateValue<GuiFontStyle>();
            style.Font = font;
            style.Color = color;
        }
        return node;
    }

    /// <summary>
    /// Sets the default font and color for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="font">The font to set.</param>
    /// <param name="color">The color to set.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetFont(this ImGuiTheme theme, Font font, Color color)
    {
        theme.SetStyle(new GuiFontStyle { Font = font, Color = color });
        return theme;
    }

    /// <summary>
    /// Gets the scaled font for a node.
    /// </summary>
    /// <param name="node">The node to get the scaled font from.</param>
    /// <returns>The font scaled according to the current DPI settings.</returns>
    public static Font GetScaledFont(this ImGuiNode node)
    {
        return ImGuiExternal._external.GetScaledFont(node);
    }

    #endregion

    #region Border

    /// <summary>
    /// Overrides the border properties using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The border width, or null to remove the border.</param>
    /// <param name="color">The border color, or null to use theme default.</param>
    /// <param name="scaled">Whether the border width should be DPI-scaled.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideBorder(this ImGuiNode node, float? width, Color? color = null, bool scaled = true)
    {
        if (width.HasValue)
        {
            var style = node.GetOrCreateValue<GuiBorderStyle>();
            style.Width = width;
            style.Color = color;
            style.Scaled = scaled;
        }
        else
        {
            node.RemoveValue<GuiBorderStyle>();
        }
        return node;
    }

    /// <summary>
    /// Overrides the border only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The border width, or null to remove the border.</param>
    /// <param name="color">The border color, or null to use theme default.</param>
    /// <param name="scaled">Whether the border width should be DPI-scaled.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideBorder(this ImGuiNode node, float? width, Color? color = null, bool scaled = true)
    {
        if (node.IsInitializing)
        {
            var style = node.GetOrCreateValue<GuiBorderStyle>();
            style.Width = width;
            style.Color = color;
            style.Scaled = scaled;
        }
        return node;
    }

    /// <summary>
    /// Sets the default border for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="width">The border width, or null for no border.</param>
    /// <param name="color">The border color, or null to use theme default.</param>
    /// <param name="scaled">Whether the border width should be DPI-scaled.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetBorder(this ImGuiTheme theme, float? width, Color? color = null, bool scaled = true)
    {
        theme.SetStyle(new GuiBorderStyle { Width = width, Color = color, Scaled = scaled });
        return theme;
    }

    #endregion

    #region Corner

    /// <summary>
    /// Overrides the corner roundness using a style value.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="round">The corner roundness value, or null for sharp corners.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode OverrideCorner(this ImGuiNode node, float? round)
    {
        var value = node.GetOrCreateValue<GuiFrameStyle>();
        value.CornerRound = round;
        return node;
    }

    /// <summary>
    /// Overrides the corner roundness only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="round">The corner roundness value, or null for sharp corners.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitOverrideCorner(this ImGuiNode node, float? round)
    {
        if (node.IsInitializing)
        {
            var value = node.GetOrCreateValue<GuiFrameStyle>();
            value.CornerRound = round;
        }
        return node;
    }

    /// <summary>
    /// Sets the default corner roundness for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="round">The corner roundness value.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetCornerRound(this ImGuiTheme theme, float round)
    {
        theme.SetStyle(new GuiFrameStyle { CornerRound = round });
        return theme;
    }

    /// <summary>
    /// Sets the theme to use a rectangular frame style.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="color">The frame color to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetRectangleFrame(this ImGuiTheme theme, Color color)
    {
        theme.SetBorder(0);
        theme.SetCornerRound(0);
        theme.SetColor(color);
        return theme;
    }

    #endregion

    #region Alignment

    /// <summary>
    /// Sets the horizontal alignment only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="alignment">The horizontal alignment to apply.</param>
    /// <param name="stretch">Whether to stretch the node, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHorizontalAlignment(this ImGuiNode node, GuiAlignment alignment, bool? stretch = null)
    {
        if (node.IsInitializing) { SetHorizontalAlignment(node, alignment, stretch); }
        return node;
    }

    /// <summary>
    /// Sets the vertical alignment only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="alignment">The vertical alignment to apply.</param>
    /// <param name="stretch">Whether to stretch the node, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitVerticalAlignment(this ImGuiNode node, GuiAlignment alignment, bool? stretch = null)
    {
        if (node.IsInitializing) { SetVerticalAlignment(node, alignment, stretch); }
        return node;
    }

    /// <summary>
    /// Sets the horizontal alignment.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="alignment">The horizontal alignment to apply.</param>
    /// <param name="stretch">Whether to stretch the node, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHorizontalAlignment(this ImGuiNode node, GuiAlignment alignment, bool? stretch = null)
    {
        node.HorizontalAlignment = alignment;
        if (stretch.HasValue) { node.AlignmentStretch = stretch.Value; }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the vertical alignment.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="alignment">The vertical alignment to apply.</param>
    /// <param name="stretch">Whether to stretch the node, or null to keep existing.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetVerticalAlignment(this ImGuiNode node, GuiAlignment alignment, bool? stretch = null)
    {
        node.VerticalAlignment = alignment;
        if (stretch.HasValue) { node.AlignmentStretch = stretch.Value; }
        node.Layout();
        return node;
    }

    /// <summary>
    /// Centers the node only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="stretch">Whether to stretch the node in both axes.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitCenter(this ImGuiNode node, bool stretch = false)
    {
        if (node.IsInitializing) { node.SetCenter(stretch); }
        return node;
    }

    /// <summary>
    /// Centers the node vertically only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="stretch">Whether to stretch the node vertically.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitCenterVertical(this ImGuiNode node, bool stretch = false)
    {
        if (node.IsInitializing) { node.SetCenterVertical(stretch); }
        return node;
    }

    /// <summary>
    /// Centers the node horizontally only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="stretch">Whether to stretch the node horizontally.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitCenterHorizontal(this ImGuiNode node, bool stretch = false)
    {
        if (node.IsInitializing) { node.SetCenterHorizontal(stretch); }
        return node;
    }

    /// <summary>
    /// Centers the node in both axes.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="stretch">Whether to stretch the node in both axes.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetCenter(this ImGuiNode node, bool stretch = false)
    {
        node.Alignment = new() { HorizontalAlignment = GuiAlignment.Center, VerticalAlignment = GuiAlignment.Center, Stretch = stretch };
        node.Layout();
        return node;
    }

    /// <summary>
    /// Centers the node vertically.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="stretch">Whether to stretch the node vertically.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetCenterVertical(this ImGuiNode node, bool stretch = false)
    {
        node.Alignment = new() { VerticalAlignment = GuiAlignment.Center, Stretch = stretch };
        node.Layout();
        return node;
    }

    /// <summary>
    /// Centers the node horizontally.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="stretch">Whether to stretch the node horizontally.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetCenterHorizontal(this ImGuiNode node, bool stretch = false)
    {
        node.Alignment = new() { HorizontalAlignment = GuiAlignment.Center, Stretch = stretch };
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the default fit orientation for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="orientation">The fit orientation to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetFitOrientation(this ImGuiTheme theme, GuiOrientation orientation)
    {
        theme.SetStyle(new GuiFitOrientationStyle { FitOrientation = orientation });
        return theme;
    }

    /// <summary>
    /// Sets the default alignment to center for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="stretch">Whether to stretch nodes in both axes.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetCenter(this ImGuiTheme theme, bool stretch = false)
    {
        theme.SetStyle(new GuiAlignmentStyle { HorizontalAlignment = GuiAlignment.Center, VerticalAlignment = GuiAlignment.Center, Stretch = stretch });
        return theme;
    }

    /// <summary>
    /// Sets the default vertical alignment to center for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="stretch">Whether to stretch nodes vertically.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetCenterVertical(this ImGuiTheme theme, bool stretch = false)
    {
        theme.SetStyle(new GuiAlignmentStyle { VerticalAlignment = GuiAlignment.Center, Stretch = stretch });
        return theme;
    }

    /// <summary>
    /// Sets the default horizontal alignment to center for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="stretch">Whether to stretch nodes horizontally.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetCenterHorizontal(this ImGuiTheme theme, bool stretch = false)
    {
        theme.SetStyle(new GuiAlignmentStyle { HorizontalAlignment = GuiAlignment.Center, Stretch = stretch });
        return theme;
    }

    /// <summary>
    /// Sets the default horizontal alignment for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="alignment">The horizontal alignment to apply.</param>
    /// <param name="stretch">Whether to stretch nodes horizontally.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetHorizontalAlignment(this ImGuiTheme theme, GuiAlignment alignment, bool? stretch = false)
    {
        var style = theme.GetOrCreateStyle<GuiAlignmentStyle>();
        style.HorizontalAlignment = alignment;
        if (stretch.HasValue) { style.Stretch = stretch.Value; }
        return theme;
    }

    /// <summary>
    /// Sets the default vertical alignment for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="alignment">The vertical alignment to apply.</param>
    /// <param name="stretch">Whether to stretch nodes vertically.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetVerticalAlignment(this ImGuiTheme theme, GuiAlignment alignment, bool? stretch = false)
    {
        var style = theme.GetOrCreateStyle<GuiAlignmentStyle>();
        style.VerticalAlignment = alignment;
        if (stretch.HasValue) { style.Stretch = stretch.Value; }
        return theme;
    }

    #endregion

    #region Header

    /// <summary>
    /// Sets the header width only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The header width, or null to use default.</param>
    /// <param name="padding">The header padding, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeaderWidth(this ImGuiNode node, float? width, float? padding = null)
    {
        if (node.IsInitializing) { node.SetHeaderWidth(width, padding); }
        return node;
    }

    /// <summary>
    /// Sets the header height only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="height">The header height, or null to use default.</param>
    /// <param name="padding">The header padding, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitHeaderHeight(this ImGuiNode node, float? height, float? padding = null)
    {
        if (node.IsInitializing) { node.SetHeaderHeight(height, padding); }
        return node;
    }

    /// <summary>
    /// Sets the header width.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="width">The header width, or null to use default.</param>
    /// <param name="padding">The header padding, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeaderWidth(this ImGuiNode node, float? width, float? padding = null)
    {
        node.HeaderWidth = width;
        node.HeaderPadding = padding;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the header height.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="height">The header height, or null to use default.</param>
    /// <param name="padding">The header padding, or null to use default.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetHeaderHeight(this ImGuiNode node, float? height, float? padding = null)
    {
        node.HeaderHeight = height;
        node.HeaderPadding = padding;
        node.Layout();
        return node;
    }

    /// <summary>
    /// Sets the default header width for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="width">The header width, or null to use default.</param>
    /// <param name="padding">The header padding, or null to use default.</param>
    /// <param name="spacing">The header spacing, or null to use default.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetHeaderWidth(this ImGuiTheme theme, float? width, float? padding = null, float? spacing = null)
    {
        var style = theme.GetOrCreateStyle<GuiHeaderStyle>();
        style.Width = width;
        style.Padding = padding;
        style.Spacing = spacing;
        return theme;
    }

    /// <summary>
    /// Sets the default header height for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="height">The header height, or null to use default.</param>
    /// <param name="padding">The header padding, or null to use default.</param>
    /// <param name="spacing">The header spacing, or null to use default.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetHeaderHeight(this ImGuiTheme theme, float? height, float? padding = null, float? spacing = null)
    {
        var style = theme.GetOrCreateStyle<GuiHeaderStyle>();
        style.Height = height;
        style.Padding = padding;
        style.Spacing = spacing;
        return theme;
    }

    /// <summary>
    /// Sets the default header color for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="color">The header color, or null to use default.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetHeaderColor(this ImGuiTheme theme, Color? color)
    {
        var style = theme.GetOrCreateStyle<GuiHeaderStyle>();
        style.Color = color;
        return theme;
    }

    /// <summary>
    /// Gets the header rectangle for a node.
    /// </summary>
    /// <param name="node">The node to get the header rectangle from.</param>
    /// <returns>The header rectangle adjusted for custom width or height if set.</returns>
    public static RectangleF GetHeaderRect(this ImGuiNode node)
    {
        float? w = node.HeaderWidth;
        float? h = node.HeaderHeight;
        var rect = node.Rect;
        if (w.HasValue) { rect.Width = w.Value; }
        if (h.HasValue) { rect.Height = h.Value; }
        return rect;
    }

    #endregion

    #region Progress

    /// <summary>
    /// Sets the progress bar color on a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="color">The progress bar color to apply.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetProgressColor(this ImGuiNode node, Color color)
    {
        var style = node.GetOrCreateValue<GuiProgressStyle>();
        style.Color = color;
        return node;
    }

    /// <summary>
    /// Sets the default progress bar color for a theme.
    /// </summary>
    /// <param name="theme">The theme to modify.</param>
    /// <param name="color">The progress bar color to apply.</param>
    /// <returns>The same ImGuiTheme for chaining.</returns>
    public static ImGuiTheme SetProgressColor(this ImGuiTheme theme, Color color)
    {
        var style = theme.GetOrCreateStyle<GuiProgressStyle>();
        style.Color = color;
        return theme;
    }

    #endregion

    #region Position

    /// <summary>
    /// Sets the position only during initialization.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode InitPosition(this ImGuiNode node, float x, float y)
    {
        if (node.IsInitializing)
        {
            var pos = node.GetOrCreateValue<GuiPositionValue>();
            pos.Position = new PointF(x, y);
        }
        return node;
    }

    /// <summary>
    /// Sets the position on a node.
    /// </summary>
    /// <param name="node">The node to modify.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <returns>The same ImGuiNode for chaining.</returns>
    public static ImGuiNode SetPosition(this ImGuiNode node, float x, float y)
    {
        var pos = node.GetOrCreateValue<GuiPositionValue>();
        pos.Position = new PointF(x, y);
        node.Layout();
        return node;
    }

    #endregion
}
