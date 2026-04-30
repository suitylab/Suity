namespace Suity.Views.Graphics;

/// <summary>
/// Interface for objects that can handle graphic input and output operations.
/// </summary>
public interface IGraphicObject
{
    /// <summary>
    /// Gets or sets the graphic context associated with this object.
    /// </summary>
    IGraphicContext GraphicContext { get; set; }

    /// <summary>
    /// Handles graphic input events such as mouse and keyboard.
    /// </summary>
    /// <param name="input">The graphic input to process.</param>
    void HandleGraphicInput(IGraphicInput input);

    /// <summary>
    /// Handles graphic output rendering operations.
    /// </summary>
    /// <param name="output">The graphic output to render to.</param>
    void HandleGraphicOutput(IGraphicOutput output);
}