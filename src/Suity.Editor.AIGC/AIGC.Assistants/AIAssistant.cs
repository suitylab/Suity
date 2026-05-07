using Suity.Editor.AIGC.Tools;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Contains metadata information about an AI assistant.
/// </summary>
public sealed record AIAssistantInfo
{
    /// <summary>
    /// Gets the type of the assistant.
    /// </summary>
    public Type AssistantType { get; init; }

    /// <summary>
    /// Gets the display text for the assistant.
    /// </summary>
    public string DisplayText { get; init; }

    /// <summary>
    /// Gets the tooltip text for the assistant.
    /// </summary>
    public string ToolTips { get; init; }
}

/// <summary>
/// AI intelligent assistant.
/// Please use <see cref="DisplayTextAttribute"/> and <see cref="ToolTipsTextAttribute"/> to mark display text and tooltips.
/// </summary>
public abstract class AIAssistant
{
    /// <summary>
    /// Error message constant indicating that the requested assistant was not found.
    /// </summary>
    public const string ERROR_MSG_ASSISTANT_NOT_FOUND = "ERROR_MSG_ASSISTANT_NOT_FOUND";

    /// <summary>
    /// Handles an AI request and returns the result.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <returns>A task representing the asynchronous operation, containing the AI call result.</returns>
    public abstract Task<AICallResult> HandleRequest(AIRequest request);

    /// <summary>
    /// Handles a conversation interaction.
    /// </summary>
    /// <param name="conversasion">The conversation handler to interact with.</param>
    public virtual void HandleConversation(IConversationHandler conversasion) { }

    /// <summary>
    /// Gets the introduction text for this assistant. By default, returns the tooltip text or a generated display string.
    /// </summary>
    public virtual string IntroductionText
    {
        get
        {
            var toolTips = this.GetType().GetAttributeCached<ToolTipsTextAttribute>()?.ToolTips;
            if (!string.IsNullOrWhiteSpace(toolTips))
            {
                return toolTips;
            }

            return $"Assistant for {this.ToDisplayText()}";
        }
    }

    #region Tool

    /// <summary>
    /// Creates an AI tool of the specified parameter type.
    /// </summary>
    /// <param name="parameterType">The type of the tool parameter.</param>
    /// <returns>The created AI tool instance.</returns>
    protected AITool CreateTool(Type parameterType) 
        => AIAssistantService.Instance.CreateTool(parameterType);

    /// <summary>
    /// Creates a generic AI tool of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the tool.</typeparam>
    /// <returns>The created AI tool instance.</returns>
    protected AITool<T> CreateTool<T>()
        => AIAssistantService.Instance.CreateTool<T>();

    /// <summary>
    /// Calls an AI tool with the specified request, canvas context, and parameter.
    /// </summary>
    /// <typeparam name="T">The type of the tool parameter.</typeparam>
    /// <param name="request">The AI request to process.</param>
    /// <param name="canvasContext">The canvas context for the tool call.</param>
    /// <param name="parameter">The parameter to pass to the tool.</param>
    /// <returns>A task representing the asynchronous operation, containing the AI call result.</returns>
    protected Task<AICallResult> CallTool<T>(AIRequest request, CanvasContext canvasContext, T parameter)
        => AIAssistantService.Instance.CallTool<T>(request, canvasContext, parameter);

    #endregion
}

/// <summary>
/// An empty assistant that returns empty results for all requests.
/// </summary>
public sealed class EmptyAssistant : AIAssistant
{
    /// <summary>
    /// Gets the singleton instance of the empty assistant.
    /// </summary>
    public static EmptyAssistant Instance { get; } = new EmptyAssistant();

    /// <summary>
    /// Gets the introduction text for this empty assistant.
    /// </summary>
    public override string IntroductionText => "Empty Assistant";

    private EmptyAssistant()
    {
    }

    /// <summary>
    /// Handles the AI request by returning an empty result.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <returns>A task representing the asynchronous operation, containing an empty AI call result.</returns>
    public override Task<AICallResult> HandleRequest(AIRequest request)
    {
        return Task.FromResult(AICallResult.Empty);
    }
}
