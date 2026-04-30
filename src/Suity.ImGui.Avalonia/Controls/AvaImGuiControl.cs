using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using System.Drawing;

namespace Suity.Controls;

/// <summary>
/// Avalonia control for rendering ImGui-based user interfaces.
/// </summary>
public class AvaImGuiControl : AvaSKGraphicControl
{
    private ImGuiGraphicObject? _guiObject;
    private ImGuiTheme? _theme;
    private Color _bgColor = Color.Black;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaImGuiControl"/> class.
    /// </summary>
    public AvaImGuiControl()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaImGuiControl"/> class with a specified ImGui drawer.
    /// </summary>
    /// <param name="drawImGui">The ImGui drawing implementation.</param>
    public AvaImGuiControl(IDrawImGui drawImGui)
        : this()
    {
        this.DrawImGui = drawImGui;
    }

    /// <summary>
    /// Gets or sets the ImGui theme for styling.
    /// </summary>
    public ImGuiTheme? GuiTheme
    {
        get => _theme;
        set
        {
            _theme = value;
            _guiObject?.GuiTheme = value;
        }
    }

    /// <summary>
    /// Gets or sets the background color of the ImGui control.
    /// </summary>
    public Color BackgroundColor
    {
        get => _bgColor;
        set
        {
            _bgColor = value;
            _guiObject?.BackgroundColor = value;
        }
    }

    /// <summary>
    /// Gets or sets the ImGui drawing implementation.
    /// </summary>
    public IDrawImGui? DrawImGui
    {
        get => _guiObject?.DrawImGui;
        set
        {
            if (_guiObject?.DrawImGui == value)
            {
                return;
            }

            if (value != null)
            {
                _guiObject = new(value)
                {
                    GuiTheme = _theme,
                    BackgroundColor = _bgColor,
                };
            }
            else
            {
                _guiObject = null;
            }

            base.GraphicObject = _guiObject;
        }
    }

}

/// <summary>
/// Internal graphic object that bridges ImGui rendering with the Avalonia graphics context.
/// </summary>
internal class ImGuiGraphicObject : IGraphicObject, IDropTarget
{
    private readonly IDrawImGui _drawImGui;

    private ImGuiTheme? _theme;
    private Color _bgColor = Color.Black;

    private ImGui? _gui;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiGraphicObject"/> class.
    /// </summary>
    /// <param name="drawImGui">The ImGui drawing implementation.</param>
    public ImGuiGraphicObject(IDrawImGui drawImGui)
    {
        _drawImGui = drawImGui ?? throw new ArgumentNullException(nameof(drawImGui));
    }

    /// <summary>
    /// Gets the ImGui drawing implementation.
    /// </summary>
    public IDrawImGui DrawImGui => _drawImGui;

    /// <summary>
    /// Gets or sets the ImGui theme for styling.
    /// </summary>
    public ImGuiTheme? GuiTheme
    {
        get => _theme;
        set
        {
            _theme = value;
            _gui?.Theme = value;
        }
    }

    /// <summary>
    /// Gets or sets the background color of the ImGui control.
    /// </summary>
    public Color BackgroundColor
    {
        get => _bgColor; 
        set
        {
            _bgColor = value;
            _gui?.BackgroundColor = value;
        }
    }

    /// <inheritdoc/>
    public IGraphicContext? GraphicContext
    {
        get => _gui?.Context;
        set
        {
            if (value != null)
            {
                ImGuiServices.Initialize();

                var config = new ImGuiConfig()
                {
                    InputSystem = ImGuiInputSystemBK.Instance,
                    LayoutSystem = ImGuiLayoutSystemBK.Instance,
                    FitSystem = ImGuiFitSystemBK.Instance,
                    RenderSystem = ImGuiRenderSystemBK.Instance,
                    Theme = _theme,
                };

                _gui = ImGuiServices.CreateImGui(value, config);
                _gui.BackgroundColor = _bgColor;
            }
            else
            {
                _gui = null;
            }
        }
    }

    #region IDropTarget

    /// <inheritdoc/>
    public void DragOver(IDragEvent e)
    {
        (_drawImGui as IDropTarget)?.DragOver(e);
    }

    /// <inheritdoc/>
    public void DragDrop(IDragEvent e)
    {
        (_drawImGui as IDropTarget)?.DragDrop(e);
    }

    #endregion

    /// <inheritdoc/>
    public void HandleGraphicInput(IGraphicInput input) => _gui?.HandleGraphicInput(input, OnGui);

    /// <inheritdoc/>
    public void HandleGraphicOutput(IGraphicOutput output) => _gui?.HandleGraphicOutput(output);

    /// <summary>
    /// Callback invoked during ImGui GUI rendering.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    private void OnGui(ImGui gui) => _drawImGui?.OnGui(gui);
}