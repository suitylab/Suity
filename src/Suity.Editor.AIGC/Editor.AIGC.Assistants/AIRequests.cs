using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Rex;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;


#region AIRequest
/// <summary>
/// Base request context for AI assistant operations.
/// </summary>
public class AIRequest
{
    /// <summary>
    /// Gets or sets the default speech language for all AI requests.
    /// </summary>
    public static string DefaultSpeechLanguage { get; set; } = string.Empty;

    /// <summary>
    /// User message
    /// </summary>
    public string UserMessage { get; init; }

    /// <summary>
    /// Knowledge
    /// </summary>
    public string Knowledge { get; init; }

    /// <summary>
    /// Gets or sets the number of retry attempts for failed operations.
    /// </summary>
    public int? RetryCount { get; init; }

    /// <summary>
    /// Gets or sets the top-K value for knowledge base queries.
    /// </summary>
    public int? TopK { get; init; }

    /// <summary>
    /// Whether to generate complex fields. Useed in generating data with complex fields.
    /// </summary>
    public bool? ComplexField { get; init; }


    /// <summary>
    /// Conversation
    /// </summary>
    public IConversationHandler Conversation { get; init; }

    /// <summary>
    /// Cancellation token
    /// </summary>
    public CancellationToken Cancel { get; init; }

    /// <summary>
    /// Action to request cancellation of the ongoing operation. This can be invoked by the assistant to signal that it wants to stop processing, for example when a user clicks a cancel button in the conversation interface.
    /// </summary>
    public Action RequestCancel { get; init; }

    /// <summary>
    /// Context to pass additional arguments.
    /// </summary>
    public FunctionContext FuncContext { get; init; }

    /// <summary>
    /// The default item name. Used in generating data item or tree graph with root node.
    /// </summary>
    public string ItemName { get; init; }

    /// <summary>
    /// Controlling the depth of generation.
    /// </summary>
    public int Depth { get; init; }

    /// <summary>
    /// Gets or sets the speech language for AI responses.
    /// </summary>
    public string SpeechLanguage { get; init; } = DefaultSpeechLanguage;

    /// <summary>
    /// Gets the resolved speech language, falling back to the editor's localized language if not set.
    /// </summary>
    /// <returns>The speech language to use for AI responses.</returns>
    public string GetSpeechLanguage()
    {
        string lang = SpeechLanguage;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            return lang;
        }

        //return $"As same as the user message";
        return EditorServices.LocalizationService.LanguageName;
    }

    /// <summary>
    /// Gets a memory object of the specified type from the function context.
    /// </summary>
    /// <typeparam name="T">The type of memory to retrieve.</typeparam>
    /// <returns>The memory object, or null if not found.</returns>
    public T GetMemory<T>() where T : class => FuncContext.GetArgument<T>();

    /// <summary>
    /// Gets or creates a memory object of the specified type in the function context.
    /// </summary>
    /// <typeparam name="T">The type of memory to get or create.</typeparam>
    /// <returns>The existing or newly created memory object.</returns>
    public T GetOrAddMemory<T>() where T : class, new()
    {
        var memory = FuncContext.GetArgument<T>();
        if (memory is null)
        {
            memory = new T();
            FuncContext.SetArgument(memory);
        }
        return memory;
    }

    /// <summary>
    /// Gets or sets the disposable collector for tracking resources.
    /// </summary>
    public DisposeCollector Disposes { get; set; }

    /// <summary>
    /// Gets or sets custom LLM model settings to override the default model.
    /// </summary>
    public CustomLLmModelSetting CustomLLmModel { get; set; }

    /// <summary>
    /// Gets or sets an option value associated with the current instance.
    /// </summary>
    public object Option { get; set; }

    /// <summary>
    /// Gets a value indicating whether the operation should be performed in the background.
    /// No interuptions or user interactions are expected in this mode.
    /// </summary>
    public bool BackgroundMode { get; set; }


    /// <summary>
    /// Initializes a new instance with default values.
    /// </summary>
    public AIRequest()
    {
        FuncContext = new();
    }

    /// <summary>
    /// Initializes a new instance by copying values from a source request.
    /// </summary>
    /// <param name="source">The source request to copy from.</param>
    /// <param name="increaseDepth">Whether to increment the generation depth.</param>
    public AIRequest(AIRequest source, bool increaseDepth = false)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        UserMessage = source.UserMessage;
        Knowledge = source.Knowledge;
        RetryCount = source.RetryCount;
        TopK = source.TopK;
        ComplexField = source.ComplexField;
        Conversation = source.Conversation;
        Cancel = source.Cancel;
        FuncContext = source.FuncContext;
        ItemName = source.ItemName;
        Depth = increaseDepth ? source.Depth + 1 : source.Depth;
        SpeechLanguage = source.SpeechLanguage;
        CustomLLmModel = source.CustomLLmModel;
        Option = source.Option;
        BackgroundMode = source.BackgroundMode;
    }

    /// <summary>
    /// Checks if the current generation depth is within the allowed limit.
    /// </summary>
    /// <returns>True if depth is within limit; otherwise, false and adds a warning to the conversation.</returns>
    public bool CheckDepth()
    {
        int maxDepth = AIAssistantService.Config.MaxGenerateDepth;
        if (Depth < maxDepth)
        {
            return true;
        }

        Conversation?.AddWarningMessage("AI assistant generation depth reached maximum: " + maxDepth);
        return false;
    }

    /// <summary>
    /// Adds a system message to the conversation if prompt display is enabled.
    /// </summary>
    /// <param name="msg">The message to add.</param>
    /// <returns>A disposable handle for the added message.</returns>
    public IDisposable AddPromptMessage(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            return EmptyDisposable.Empty;
        }

        if (!AIAssistantService.Config.ShowPromptInConverasation)
        {
            return EmptyDisposable.Empty;
        }

        return Conversation?.AddSystemMessage(msg) as IDisposable ?? EmptyDisposable.Empty;
    }

    /// <summary>
    /// Handles a button click event from the conversation interface.
    /// </summary>
    /// <param name="button">The button key that was clicked.</param>
    /// <returns>A task representing the async button handling operation.</returns>
    public Task HandleButtonClick(string button)
        => FuncContext?.GetArgument<IConversationHostAsync>()?.HandleButtonClickAsync(button, Cancel);


    /// <summary>
    /// Fills the prompt builder with request-specific values.
    /// </summary>
    /// <param name="builder">The prompt builder to fill.</param>
    public virtual void FillPrompt(PromptBuilder builder)
    {
        builder.Replace(TAG.PROMPT, FormatGuidingText(UserMessage));
        builder.Replace(TAG.KNOWLEDGE, FormatGuidingText(Knowledge));
        builder.Replace(TAG.SPEECH_LANGUAGE, GetSpeechLanguage());
    }

    /// <summary>
    /// Formats a guiding text value, returning a placeholder if empty.
    /// </summary>
    /// <param name="guiding">The guiding text value.</param>
    /// <param name="noGuiding">The placeholder to use when guiding is empty.</param>
    /// <returns>The guiding text or the placeholder.</returns>
    public static string FormatGuidingText(string guiding, string noGuiding = "---")
    {
        if (!string.IsNullOrWhiteSpace(guiding))
        {
            return guiding;
        }

        return noGuiding;
    }
}
#endregion

#region AIJsonRequest
/// <summary>
/// Request context for AI-driven JSON generation or editing.
/// </summary>
public class AIJsonRequest : AIRequest
{
    /// <summary>
    /// Gets or sets the compound type to generate or edit JSON for.
    /// </summary>
    public DCompond Type { get; init; }

    /// <summary>
    /// Gets or sets the existing JSON string to edit, if performing a modification.
    /// </summary>
    public string JsonToEdit { get; init; }

    /// <summary>
    /// Gets or sets the fields to exclude from generation or editing.
    /// </summary>
    public ICollection<DStructField> ExcludedFields { get; init; }

    /// <summary>
    /// Initializes a new instance of the AI JSON request.
    /// </summary>
    public AIJsonRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance by copying values from a source request.
    /// </summary>
    /// <param name="origin">The source request to copy from.</param>
    /// <param name="increaseDepth">Whether to increment the generation depth.</param>
    public AIJsonRequest(AIRequest origin, bool increaseDepth = false)
        : base(origin, increaseDepth)
    {
    }
}
#endregion

#region AISummaryRequest
/// <summary>
/// Request context for AI-driven summary generation.
/// </summary>
public class AISummaryRequest : AIRequest
{
    /// <summary>
    /// Gets or sets the result text to summarize.
    /// </summary>
    public string Result { get; init; }

    /// <summary>
    /// Gets or sets the before-state text for comparison summaries.
    /// </summary>
    public string Before { get; init; }

    /// <summary>
    /// Initializes a new instance of the AI summary request.
    /// </summary>
    public AISummaryRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance by copying values from a source request.
    /// </summary>
    /// <param name="origin">The source request to copy from.</param>
    /// <param name="increaseDepth">Whether to increment the generation depth.</param>
    public AISummaryRequest(AIRequest origin, bool increaseDepth = false)
        : base(origin, increaseDepth)
    {
    }
} 
#endregion