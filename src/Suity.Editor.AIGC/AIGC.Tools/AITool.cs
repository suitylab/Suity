using Suity.Editor.AIGC.Assistants;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Contains metadata information about an AI tool, including its type, parameter type, and display information.
/// </summary>
public sealed record AIToolInfo
{
    /// <summary>
    /// Gets or sets the type of the AI tool.
    /// </summary>
    public Type ToolType { get; init; }

    /// <summary>
    /// Gets or sets the type of the parameter expected by the tool.
    /// </summary>
    public Type ParameterType { get; init; }

    /// <summary>
    /// Gets or sets the type of document the tool operates on.
    /// </summary>
    public Type DocumentType { get; init; }

    /// <summary>
    /// Gets or sets the display text for the tool.
    /// </summary>
    public string DisplayText { get; init; }

    /// <summary>
    /// Gets or sets the tooltip description for the tool.
    /// </summary>
    public string ToolTips { get; init; }
}

/// <summary>
/// Abstract base class for all AI tools that can be invoked with object parameters.
/// </summary>
public abstract class AITool
{
    /// <summary>
    /// Invokes the tool with the specified request, canvas context, and parameter object.
    /// </summary>
    /// <param name="request">The AI request containing conversation and context.</param>
    /// <param name="canvasContext">The canvas context for the current session.</param>
    /// <param name="parameter">The parameter object for the tool.</param>
    /// <returns>A task representing the asynchronous operation, returning the call result.</returns>
    public abstract Task<AICallResult> CallObject(AIRequest request, CanvasContext canvasContext, object parameter);
}

/// <summary>
/// Generic abstract base class for AI tools with a strongly-typed parameter.
/// </summary>
/// <typeparam name="T">The type of parameter expected by the tool.</typeparam>
public abstract class AITool<T> : AITool
{
    /// <summary>
    /// Sealed implementation that casts the parameter to the expected type and calls the typed overload.
    /// </summary>
    /// <param name="request">The AI request containing conversation and context.</param>
    /// <param name="canvasContext">The canvas context for the current session.</param>
    /// <param name="parameter">The parameter object to cast and pass to the typed method.</param>
    /// <returns>A task representing the asynchronous operation, returning the call result.</returns>
    public override sealed Task<AICallResult> CallObject(AIRequest request, CanvasContext canvasContext, object parameter)
        => Call(request, canvasContext, (T)parameter);

    /// <summary>
    /// Invokes the tool with the specified request, canvas context, and strongly-typed parameter.
    /// </summary>
    /// <param name="request">The AI request containing conversation and context.</param>
    /// <param name="canvasContext">The canvas context for the current session.</param>
    /// <param name="parameter">The strongly-typed parameter for the tool.</param>
    /// <returns>A task representing the asynchronous operation, returning the call result.</returns>
    public abstract Task<AICallResult> Call(AIRequest request, CanvasContext canvasContext, T parameter);
}

/// <summary>
/// Obsolete generic abstract base class for AI tools with typed parameters and target types.
/// </summary>
/// <typeparam name="T">The type of parameter expected by the tool.</typeparam>
/// <typeparam name="TTarget">The target type the tool operates on.</typeparam>
[Obsolete]
public abstract class AITool<T, TTarget> : AITool<T>
{
}

/// <summary>
/// Used to mark tool parameters rather than the tool itself, to specify the tool's return type.
/// </summary>
[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class ToolReturnTypeAttribute : Attribute
{
    /// <summary>
    /// Gets the return type specified for the tool.
    /// </summary>
    public Type ReturnType { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolReturnTypeAttribute"/> class with the specified return type.
    /// </summary>
    /// <param name="returnType">The type that the tool returns.</param>
    public ToolReturnTypeAttribute(Type returnType)
    {
        ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
    }
}
