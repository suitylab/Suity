using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.AIGC.Assistants;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Abstract base class for AI assistants that handle tool invocation requests within a canvas context.
/// </summary>
public abstract class ToolingAssistant : AICanvasAssistant
{
    private readonly Dictionary<string, Type> _parameterTypes = [];

    /// <summary>
    /// Registers a tool parameter type for tool resolution.
    /// </summary>
    /// <param name="toolParameterType">The type of the tool parameter to register.</param>
    public void AddParameterType(Type toolParameterType)
    {
        if (toolParameterType is null)
        {
            throw new ArgumentNullException(nameof(toolParameterType));
        }

        _parameterTypes[toolParameterType.FullName] = toolParameterType;
    }

    /// <summary>
    /// Registers a tool parameter type for tool resolution using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type of the tool parameter to register.</typeparam>
    public void AddParameterType<T>() where T : class
    {
        _parameterTypes[typeof(T).FullName] = typeof(T);
    }


    /// <summary>
    /// Handles an incoming AI request by routing it to the tool input handler.
    /// </summary>
    /// <param name="request">The AI request to handle.</param>
    /// <returns>A task representing the asynchronous operation, returning the call result.</returns>
    public override Task<AICallResult> HandleRequest(AIRequest request)
    {
        return HandleToolInput(request);
    }

    /// <summary>
    /// Processes the tool input from an AI request, selects the appropriate tool, and executes it.
    /// </summary>
    /// <param name="request">The AI request containing tool input.</param>
    /// <returns>A task representing the asynchronous operation, returning the call result.</returns>
    public virtual async Task<AICallResult> HandleToolInput(AIRequest request)
    {
        //using var resolveMsg = request.Conversation.AddSystemMessage("Parsing tool..");

        object toolParam = await AIAssistantService.Instance.SelectToolParameter(_parameterTypes.Values, request);
        if (toolParam is null || toolParam is ToolNotFound)
        {
            throw new AigcException(L("Unable to get tool parameter."));
            //return AICallResult.FromFailed("Tool parameter parsing failed.");
        }

        var tool = AIAssistantService.Instance.CreateTool(toolParam.GetType());
        if (tool is null)
        {
            throw new AigcException(L("Unable to get tool: ") + toolParam.GetType().Name);
            //return AICallResult.FromFailed("Unable to get tool: " + toolParam.GetType().Name);
        }

        //resolveMsg?.Dispose();

        using var callingMsg = request.Conversation.AddRunningMessage(L($"Executing tool: {tool.ToDisplayText()}..."));

        var result = await tool.CallObject(request, this.Context, toolParam);

        request.Disposes?.Dispose();

        return result;
    }
}