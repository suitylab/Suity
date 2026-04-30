using System;

namespace Suity.Views.Graphics;

/// <summary>
/// Event arguments for graphic context events.
/// </summary>
public class GraphicContextEventArgs : EventArgs
{
    /// <summary>
    /// Gets the graphic context associated with the event.
    /// </summary>
    public IGraphicContext GraphicContext { get; }

    /// <summary>
    /// Creates a new instance with the specified graphic context.
    /// </summary>
    /// <param name="graphicContext">The graphic context.</param>
    public GraphicContextEventArgs(IGraphicContext graphicContext)
    {
        GraphicContext = graphicContext;
    }
}

/// <summary>
/// Event arguments for graphic output events.
/// </summary>
public class GraphicOutputEventArgs : EventArgs
{
    /// <summary>
    /// Gets the graphic output associated with the event.
    /// </summary>
    public IGraphicOutput Output { get; }

    /// <summary>
    /// Creates a new instance with the specified graphic output.
    /// </summary>
    /// <param name="output">The graphic output.</param>
    public GraphicOutputEventArgs(IGraphicOutput output)
    {
        Output = output;
    }
}

/// <summary>
/// Event arguments for graphic input events.
/// </summary>
public class GraphicInputEventArgs : EventArgs
{
    /// <summary>
    /// Gets the graphic input associated with the event.
    /// </summary>
    public IGraphicInput Input { get; }

    /// <summary>
    /// Creates a new instance with the specified graphic input.
    /// </summary>
    /// <param name="input">The graphic input.</param>
    public GraphicInputEventArgs(IGraphicInput input)
    {
        Input = input;
    }
}