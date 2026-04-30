using Suity.Helpers;
using Suity.Views.Graphics;
using System;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Extension methods for creating and managing external (native) controls within ImGui.
/// </summary>
public static class ExternalControlExtensions
{
    /// <summary>
    /// Creates an external control node that hosts a native control.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="initType">The type of the external control.</param>
    /// <param name="initName">The name of the external control.</param>
    /// <returns>The created ImGuiNode.</returns>
    public static ImGuiNode ExternalControl(this ImGui gui, string id, string initType, string initName)
    {
        ImGuiNode node = gui.BeginCurrentNode(id);

        if (node.IsInitializing)
        {
            node.TypeName = nameof(ExternalControl);
            node.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
            node.SetFitFunction(ImGuiFitSystem.Auto);
            node.FitOrientation = GuiOrientation.Vertical;

            node.Layout();

            //var layoutFunc = CreateLayoutFunc(node, initType, initName);
            //if (layoutFunc is { })
            //{
            //    node.InitLayoutFunctionChain(layoutFunc);
            //}

            var fitFunc = CreateFitFunc(node, initType, initName);
            if (fitFunc is { })
            {
                node.InitFitFunctionChain(fitFunc);
            }

            var renderFunc = CreateRenderFunc(node, initType, initName);
            if (renderFunc is { })
            {
                node.InitRenderFunctionChain(renderFunc);
            }
        }

        return node;
    }

    private static FitFunction? CreateFitFunc(ImGuiNode node, string initType, string initName)
    {
        var value = node.GetOrCreateValue<ExternalControlValue>(() => new());

        ICustomControl? control = value.Control;
        control ??= value.Control = (node.Gui.Context as IGraphicCustomControl)?.CreateControl(initType, initName, node.Rect.ToInt());

        if (control is null)
        {
            return null;
        }

        node.SetValue(value);

        return new FitFunction((pipeline, node, baseAction) =>
        {
            if (node.Rect != value.LastRect)
            {
                value.LastRect = node.Rect;
                control.Move(node.Rect.ToInt());
            }
        });
    }

    private static LayoutFunction? CreateLayoutFunc(ImGuiNode node, string initType, string initName)
    {
        var value = node.GetOrCreateValue<ExternalControlValue>(() => new());

        ICustomControl? control = value.Control;
        control ??= value.Control = (node.Gui.Context as IGraphicCustomControl)?.CreateControl(initType, initName, node.Rect.ToInt());

        if (control is null)
        {
            return null;
        }

        node.SetValue(value);

        return new LayoutFunction((pipeline, childNode, pos, baseAction) =>
        {
            var prentNode = childNode.Parent!;

            if (prentNode.Rect != value.LastRect)
            {
                value.LastRect = prentNode.Rect;
                control.Move(prentNode.Rect.ToInt());
            }
        });
    }

    private static RenderFunction? CreateRenderFunc(ImGuiNode node, string initType, string initName)
    {
        var value = node.GetOrCreateValue<ExternalControlValue>(() => new());

        ICustomControl? control = value.Control;
        control ??= value.Control = (node.Gui.Context as IGraphicCustomControl)?.CreateControl(initType, initName, node.Rect.ToInt());

        if (control is null)
        {
            return null;
        }

        node.SetValue(value);

        return new RenderFunction((pipeline, node, output, dirtyMode, baseAction) =>
        {
            if (node.Rect != value.LastRect)
            {
                value.LastRect = node.Rect;
                control.Move(node.Rect.ToInt());
            }
        });
    }
}

/// <summary>
/// Stores the state for an external control, including the control reference and last known rectangle.
/// </summary>
public class ExternalControlValue : IViewValue, IDisposable
{
    /// <summary>
    /// Gets or sets the last known rectangle of the control.
    /// </summary>
    public RectangleF LastRect { get; set; }

    /// <summary>
    /// Gets or sets the external control instance.
    /// </summary>
    public ICustomControl? Control { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        var control = this.Control;
        this.Control = null;

        control?.Dispose();
    }
}
