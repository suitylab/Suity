using static Suity.Helpers.GlobalLocalizer;
using ComputerBeacon.Json;
using Suity.Editor.AIGC.Tools;
using Suity.Editor.Documents;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Suity.Editor.Design;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Abstract base service for AI assistant operations, providing LLM calls, prompt management,
/// classification, task subdivision, canvas operations, assistant management, tool execution,
/// and text generation capabilities.
/// </summary>
public abstract class AIAssistantService
{
    internal static AIAssistantService _external;
    internal static IAIAssistantConfig _config;

    /// <summary>
    /// Gets the singleton instance of the AI assistant service.
    /// </summary>
    public static AIAssistantService Instance => _external;

    /// <summary>
    /// Gets the configuration for the AI assistant service.
    /// </summary>
    public static IAIAssistantConfig Config => _config;

    #region LLm

    /// <summary>
    /// Creates an LLM call with the specified model and optional configuration.
    /// </summary>
    /// <param name="model">The LLM model to use for the call.</param>
    /// <param name="config">Optional model parameter configuration.</param>
    /// <param name="conversation">Optional conversation handler.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new LLM call instance.</returns>
    public abstract ILLmCall CreateLLmCall(ILLmModel model, LLmModelParameter config = null, IConversationHandler conversation = null, FunctionContext context = null);

    /// <summary>
    /// Creates an LLM call using a preset model type and level.
    /// </summary>
    /// <param name="presetType">The preset model type to use.</param>
    /// <param name="level">The AI model quality level, defaults to Default.</param>
    /// <param name="conversation">Optional conversation handler.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new LLM call instance.</returns>
    public abstract ILLmCall CreateLLmCall(LLmModelPreset presetType, AigcModelLevel level = AigcModelLevel.Default, IConversationHandler conversation = null, FunctionContext context = null);

    /// <summary>
    /// Creates an LLM call with a prompt builder and specified model.
    /// </summary>
    /// <param name="builder">The prompt builder containing the prompt content.</param>
    /// <param name="model">The LLM model to use for the call.</param>
    /// <param name="config">Optional model parameter configuration.</param>
    /// <param name="conversation">Optional conversation handler.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new LLM call instance.</returns>
    public abstract ILLmCall CreateLLmCall(PromptBuilder builder, ILLmModel model, LLmModelParameter config = null, IConversationHandler conversation = null, FunctionContext context = null);

    /// <summary>
    /// Creates an LLM call with a prompt builder using the default model.
    /// </summary>
    /// <param name="builder">The prompt builder containing the prompt content.</param>
    /// <param name="conversation">Optional conversation handler.</param>
    /// <param name="context">Optional function context.</param>
    /// <returns>A new LLM call instance.</returns>
    public abstract ILLmCall CreateLLmCall(PromptBuilder builder, IConversationHandler conversation = null, FunctionContext context = null);

    /// <summary>
    /// Creates an assistant chat session for the specified AI assistant.
    /// </summary>
    /// <param name="assistant">The AI assistant to create a chat session for.</param>
    /// <param name="context">The function context for the chat session.</param>
    /// <returns>A new LLM chat instance.</returns>
    public abstract ILLmChat CreateAssistantChat(AIAssistant assistant, FunctionContext context);

    /// <summary>
    /// Gets the current speech language setting.
    /// </summary>
    /// <returns>The current speech language as a string.</returns>
    public abstract string GetSpeechLanguage();

    #endregion

    #region Prompt

    /// <summary>
    /// Gets the prompt record associated with the specified prompt ID.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <returns>The prompt record, or null if not found.</returns>
    public abstract AIPromptRecord GetPromptRecord(string promptId);

    /// <summary>
    /// Gets the prompt record for the specified ID, or throws an exception if not found.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <returns>The prompt record.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the prompt template does not exist.</exception>
    public AIPromptRecord GetPromptRecordOrThrow(string promptId) => GetPromptRecord(promptId) 
        ?? throw new KeyNotFoundException(L("Prompt template does not exist: ") + promptId);

    /// <summary>
    /// Gets the prompt template string for the specified prompt ID.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <returns>The prompt template string, or null if not found.</returns>
    public string GetPromptTemplate(string promptId) => GetPromptRecord(promptId)?.Prompt;

    /// <summary>
    /// Gets the prompt template string for the specified ID, or throws an exception if not found.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt.</param>
    /// <returns>The prompt template string.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the prompt template does not exist.</exception>
    public string GetPromptTemplateOrThrow(string promptId) => GetPromptRecord(promptId)?.Prompt
        ?? throw new KeyNotFoundException(L("Prompt template does not exist: ") + promptId);

    #endregion

    #region Classify

    /// <summary>
    /// Classifies the main input type from an AI request.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <returns>The classified user main input type.</returns>
    public abstract Task<UserMainInputTypes> ClassifyMainInputType(AIRequest request);

    /// <summary>
    /// Classifies the document operation type from an AI request.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <param name="hasSelection">Indicates whether there is a current selection.</param>
    /// <returns>The classified user operation type for documents.</returns>
    public abstract Task<UserOperationTypes> ClassifyDocumentOperation(AIRequest request, bool hasSelection);

    /// <summary>
    /// Classifies the RAG (Retrieval-Augmented Generation) operation type from an AI request.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <param name="hasSelection">Indicates whether there is a current selection.</param>
    /// <returns>The classified user operation type for RAG.</returns>
    public abstract Task<UserOperationTypes> ClassifyRagOperation(AIRequest request, bool hasSelection);

    /// <summary>
    /// Classifies the query scope from an AI request.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <param name="hasSelection">Indicates whether there is a current selection.</param>
    /// <returns>The classified query scope type.</returns>
    public abstract Task<QueryScopeTypes> ClassifyQueryScope(AIRequest request, bool hasSelection);

    /// <summary>
    /// Classifies the generate multiple type from an AI request.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <returns>The classified generate multiple type.</returns>
    public abstract Task<GenerateMultipleTypes> ClassifyGenerateMultipleType(AIRequest request);

    /// <summary>
    /// Classifies the generate source type from an AI request.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <returns>The classified generate source type.</returns>
    public abstract Task<GenerateSourceTypes> ClassifyGenerateSourceType(AIRequest request);

    /// <summary>
    /// Classifies the operation target from an AI request.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <returns>The classified operation target type.</returns>
    [Obsolete]
    public abstract Task<OperationTargetTypes> ClassifyTarget(AIRequest request);

    /// <summary>
    /// Classifies the correlation between an AI request and a source.
    /// </summary>
    /// <param name="request">The AI request to classify.</param>
    /// <param name="source">The source to compare against.</param>
    /// <returns>A float value representing the correlation score.</returns>
    public abstract Task<float> ClassifyCorrelation(AIRequest request, string source);

    #endregion

    #region Subdivide

    /// <summary>
    /// Performs segmentation on an AI request, breaking it into smaller parts.
    /// </summary>
    /// <param name="request">The AI request to segment.</param>
    /// <returns>An array of segmented strings.</returns>
    public abstract Task<string[]> Segmentation(AIRequest request);

    /// <summary>
    /// Subdivides an AI request into multiple tasks.
    /// </summary>
    /// <param name="request">The AI request to subdivide.</param>
    /// <returns>An array of task strings.</returns>
    public abstract Task<string[]> TaskSubdivision(AIRequest request);

    /// <summary>
    /// Performs brainstorming on an AI request to generate ideas.
    /// </summary>
    /// <param name="request">The AI request to brainstorm.</param>
    /// <returns>An array of brainstormed strings.</returns>
    public abstract Task<string[]> BrainStorming(AIRequest request);

    #endregion

    #region Canvas

    /// <summary>
    /// Creates a new canvas document from the specified file path.
    /// </summary>
    /// <param name="rFilePath">The file path for the canvas document.</param>
    /// <param name="showView">Indicates whether to show the view after creation, defaults to true.</param>
    /// <returns>A new canvas document instance.</returns>
    public abstract ICanvasDocument CreateCanvas(string rFilePath, bool showView = true);

    /// <summary>
    /// Resolves the current canvas context.
    /// </summary>
    /// <returns>The current canvas context.</returns>
    public abstract CanvasContext ResolveCanvasContext();

    /// <summary>
    /// Validates that the selection count does not exceed the maximum allowed.
    /// </summary>
    /// <param name="selection">The current selection objects.</param>
    /// <param name="maxSelection">The maximum allowed selection count.</param>
    /// <exception cref="Exception">Thrown when selection count exceeds the maximum.</exception>
    public abstract void ValidateSelectionCount(object[] selection, int maxSelection);

    #endregion

    #region Assistant

    /// <summary>
    /// Selects an appropriate assistant based on the AI request.
    /// </summary>
    /// <typeparam name="T">The type of assistant to select.</typeparam>
    /// <param name="request">The AI request to base the selection on.</param>
    /// <param name="context">Optional canvas context.</param>
    /// <returns>The selected assistant instance.</returns>
    public abstract Task<T> SelectAssistant<T>(AIRequest request, CanvasContext context = null)
        where T : class;

    /// <summary>
    /// Selects multiple assistants based on the AI request.
    /// </summary>
    /// <typeparam name="T">The type of assistants to select.</typeparam>
    /// <param name="request">The AI request to base the selection on.</param>
    /// <param name="context">Optional canvas context.</param>
    /// <returns>A chain of assistant calls.</returns>
    public abstract Task<AssistantCallChain<T>> SelectAssistants<T>(AIRequest request, CanvasContext context = null)
        where T : class;

    /// <summary>
    /// Creates a canvas assistant for the specified canvas context.
    /// </summary>
    /// <typeparam name="T">The type of canvas assistant to create.</typeparam>
    /// <param name="context">The canvas context for the assistant.</param>
    /// <returns>A new canvas assistant instance.</returns>
    public abstract T CreateCanvasAssistant<T>(CanvasContext context)
        where T : AICanvasAssistant, new();

    /// <summary>
    /// Creates a document assistant for the specified canvas context.
    /// </summary>
    /// <param name="context">The canvas context for the assistant.</param>
    /// <returns>A new document assistant instance.</returns>
    public abstract AIDocumentAssistant CreateDocumentAssistant(CanvasContext context);

    /// <summary>
    /// Creates multiple document assistants for the specified canvas context.
    /// </summary>
    /// <param name="context">The canvas context for the assistants.</param>
    /// <returns>An array of document assistant instances.</returns>
    public abstract AIDocumentAssistant[] CreateDocumentAssistants(CanvasContext context);

    /// <summary>
    /// Creates a RAG (Retrieval-Augmented Generation) assistant for the specified canvas context.
    /// </summary>
    /// <param name="context">The canvas context for the assistant.</param>
    /// <returns>A new RAG assistant instance.</returns>
    public abstract AIDocumentAssistant CreateRAGAssistant(CanvasContext context);

    /// <summary>
    /// Handles resume operation for an AI request within a canvas context.
    /// </summary>
    /// <param name="request">The AI request to handle.</param>
    /// <param name="context">The canvas context for the operation.</param>
    /// <returns>The result of the AI call.</returns>
    public abstract Task<AICallResult> HandleResume(AIRequest request, CanvasContext context);

    #endregion

    #region Tool

    /// <summary>
    /// Selects a tool parameter from the given types based on the AI request.
    /// </summary>
    /// <param name="toolParameterTypes">The collection of tool parameter types to choose from.</param>
    /// <param name="request">The AI request to base the selection on.</param>
    /// <returns>The selected tool parameter object.</returns>
    public abstract Task<object> SelectToolParameter(IEnumerable<Type> toolParameterTypes, AIRequest request);

    /// <summary>
    /// Creates a tool instance of the specified parameter type.
    /// </summary>
    /// <param name="parameterType">The type of parameter for the tool.</param>
    /// <returns>A new AI tool instance.</returns>
    public abstract AITool CreateTool(Type parameterType);

    /// <summary>
    /// Creates a tool instance of the specified generic type.
    /// </summary>
    /// <typeparam name="T">The type of parameter for the tool.</typeparam>
    /// <returns>A new AI tool instance, or null if not found.</returns>
    public abstract AITool<T> CreateTool<T>();

    /// <summary>
    /// Creates and calls a tool with the specified parameter.
    /// </summary>
    /// <typeparam name="T">The type of tool parameter.</typeparam>
    /// <param name="request">The AI request for the tool call.</param>
    /// <param name="canvasContext">The canvas context for the tool call.</param>
    /// <param name="parameter">The parameter to pass to the tool.</param>
    /// <returns>The result of the AI tool call.</returns>
    /// <exception cref="AigcException">Thrown when the tool is not found.</exception>
    public Task<AICallResult> CallTool<T>(AIRequest request, CanvasContext canvasContext, T parameter)
    {
        var tool = CreateTool<T>()
            ?? throw new AigcException(L("Tool not found: ") + typeof(T).FullName);

        return tool.Call(request, canvasContext, parameter);
    }

    #endregion

    #region Text

    /// <summary>
    /// Creates a unique identifier based on the AI request.
    /// </summary>
    /// <param name="request">The AI request to base the identifier on.</param>
    /// <returns>A generated identifier string.</returns>
    public abstract Task<string> CreateIdentifier(AIRequest request);

    /// <summary>
    /// Creates a summary from the AI summary request.
    /// </summary>
    /// <param name="request">The AI summary request.</param>
    /// <returns>A generated summary string.</returns>
    public abstract Task<string> CreateSummary(AISummaryRequest request);

    /// <summary>
    /// Creates a summary that compares multiple items from the AI summary request.
    /// </summary>
    /// <param name="request">The AI summary request.</param>
    /// <returns>A generated comparison summary string.</returns>
    public abstract Task<string> CreateSummaryCompare(AISummaryRequest request);

    /// <summary>
    /// Creates a summary for a partial update from the AI summary request.
    /// </summary>
    /// <param name="request">The AI summary request.</param>
    /// <returns>A generated partial update summary string.</returns>
    public abstract Task<string> CreateSummaryPartialUpdate(AISummaryRequest request);

    #endregion

    #region Try

    /// <summary>
    /// Try to execute the task action multiple times until a non-null object is returned or the retry count is exceeded.
    /// The action will try to catch <see cref="AigcException"/> exceptions and automatically retry.
    /// Other exceptions will interrupt execution.
    /// </summary>
    /// <typeparam name="T">The return type of the task.</typeparam>
    /// <param name="title">The title for the retry operation.</param>
    /// <param name="task">The task function to execute.</param>
    /// <param name="acceptNull">Indicates whether null results are acceptable, defaults to false.</param>
    /// <param name="retry">Optional retry count override. If null, uses the default.</param>
    /// <param name="conversation">Optional conversation handler.</param>
    /// <param name="cancel">Optional cancellation token.</param>
    /// <returns>The result from the task execution.</returns>
    public abstract Task<T> DoRetryAction<T>(string title, Func<Task<T>> task, bool acceptNull = false, int? retry = null, 
        IConversationHandler conversation = null, CancellationToken cancel = default) where T : class;

    /// <summary>
    /// Executes a retry action using configuration from an AI request.
    /// </summary>
    /// <typeparam name="T">The return type of the task.</typeparam>
    /// <param name="request">The AI request providing retry configuration.</param>
    /// <param name="title">The title for the retry operation.</param>
    /// <param name="task">The task function to execute.</param>
    /// <param name="acceptNull">Indicates whether null results are acceptable, defaults to false.</param>
    /// <returns>The result from the task execution.</returns>
    public Task<T> DoRetryAction<T>(AIRequest request, string title, Func<Task<T>> task, bool acceptNull = false) where T : class
    {
        return DoRetryAction(title, task, acceptNull, request.RetryCount, request.Conversation, request.Cancellation);
    }

    #endregion

    /// <summary>
    /// Attempts to repair malformed JSON using AI assistance.
    /// </summary>
    /// <param name="request">The AI request for the repair operation.</param>
    /// <param name="json">The malformed JSON string to repair.</param>
    /// <returns>The repaired JSON string.</returns>
    public abstract Task<string> RepairJson(AIRequest request, string json);

    /// <summary>
    /// Resolves and repairs malformed JSON into a JsonObject using AI assistance.
    /// </summary>
    /// <param name="request">The AI request for the repair operation.</param>
    /// <param name="json">The malformed JSON string to resolve and repair.</param>
    /// <returns>The resolved and repaired JsonObject.</returns>
    public abstract Task<JsonObject> ResolveAndRepairJson(AIRequest request, string json);

    /// <summary>
    /// Applies the target data to the specified document view.
    /// </summary>
    /// <param name="request">The AI request for the apply operation.</param>
    /// <param name="docView">The document view to apply the target to.</param>
    /// <param name="doc">The document to modify.</param>
    /// <param name="json">The JSON data to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task ApplyTarget(AIRequest request, IDocumentView docView, Document doc, object json);
}
