using Suity.Editor.AIGC.Assistants;
using Suity.Editor.AIGC.Flows;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Abstract base class providing core LLM (Large Language Model) service operations including chat, workflow execution, model retrieval, and function calling.
/// </summary>
public abstract class LLmService
{
    internal static LLmService _external;

    /// <summary>
    /// Gets the singleton instance of the external LLM service.
    /// </summary>
    public static LLmService Instance => _external;


    /// <summary>
    /// Checks whether the current model configuration is valid and ready for use.
    /// </summary>
    /// <returns>True if the current model configuration is valid; otherwise, false.</returns>
    public abstract Task<bool> CheckCurrentModelConfig();



    /// <summary>
    /// Starts a main chat session with the specified AI assistant.
    /// </summary>
    /// <param name="assistant">The AI assistant to use for the chat.</param>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public Task<object> StartMainChat(AIAssistant assistant, object option = null)
    {
        var aOption = new AIAssistantOption { Assistant = assistant, Option = option };
        return StartMainChat(aOption);
    }

    /// <summary>
    /// Starts a chat session with the specified provider and AI assistant.
    /// </summary>
    /// <param name="provider">The LLM chat provider to use.</param>
    /// <param name="assistant">The AI assistant to use for the chat.</param>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public Task<object> StartChat(ILLmChatProvider provider, AIAssistant assistant, object option = null)
    {
        var aOption = new AIAssistantOption { Assistant = assistant, Option = option };
        return StartChat(provider, aOption);
    }

    /// <summary>
    /// Sends an input message to the main chat session with the specified AI assistant.
    /// </summary>
    /// <param name="input">The input message to send.</param>
    /// <param name="assistant">The AI assistant to use for the chat.</param>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public Task<object> InputMainChat(string input, AIAssistant assistant, object option = null)
    {
        var aOption = new AIAssistantOption { Assistant = assistant, Option = option };
        return InputMainChat(input, aOption);
    }

    /// <summary>
    /// Sends an input message to a chat session with the specified provider and AI assistant.
    /// </summary>
    /// <param name="provider">The LLM chat provider to use.</param>
    /// <param name="input">The input message to send.</param>
    /// <param name="assistant">The AI assistant to use for the chat.</param>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public Task<object> InputChat(ILLmChatProvider provider, string input, AIAssistant assistant, object option = null)
    {
        var aOption = new AIAssistantOption { Assistant = assistant, Option = option };
        return InputChat(provider, input, aOption);
    }

    /// <summary>
    /// Starts a main chat session with the specified option.
    /// </summary>
    /// <param name="option">The configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public abstract Task<object> StartMainChat(object option);

    /// <summary>
    /// Starts a chat session with the specified provider and option.
    /// </summary>
    /// <param name="provider">The LLM chat provider to use.</param>
    /// <param name="option">The configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public abstract Task<object> StartChat(ILLmChatProvider provider, object option);

    /// <summary>
    /// Sends an input message to the main chat session.
    /// </summary>
    /// <param name="input">The input message to send.</param>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public abstract Task<object> InputMainChat(string input, object option = null);

    /// <summary>
    /// Sends an input message to a chat session with the specified provider.
    /// </summary>
    /// <param name="provider">The LLM chat provider to use.</param>
    /// <param name="input">The input message to send.</param>
    /// <param name="option">Optional configuration for the chat session.</param>
    /// <returns>A task representing the asynchronous operation, returning the chat result.</returns>
    public abstract Task<object> InputChat(ILLmChatProvider provider, string input, object option = null);

    /// <summary>
    /// Starts a workflow-based chat session with the specified runnable workflow.
    /// </summary>
    /// <param name="runnable">The workflow to execute.</param>
    /// <param name="view">Optional flow view for UI integration.</param>
    /// <param name="config">Optional action to configure the flow computation.</param>
    /// <returns>A task representing the asynchronous operation, returning the workflow result.</returns>
    public abstract Task<object> StartWorkflowChat(IAigcRunWorkflow runnable, IFlowView view = null, Action<IFlowComputation> config = null);

    /// <summary>
    /// Starts a specific workflow task with the given AI request.
    /// </summary>
    /// <param name="request">The AI request to process.</param>
    /// <param name="runnable">The workflow to execute.</param>
    /// <param name="view">Optional flow view for UI integration.</param>
    /// <param name="config">Optional action to configure the flow computation.</param>
    /// <returns>A task representing the asynchronous operation, returning the task result.</returns>
    public abstract Task<object> StartWorkflowTask(AIRequest request, IAigcRunWorkflow runnable, IFlowView view = null, Action<IFlowComputation> config = null);

    /// <summary>
    /// Gets an LLM model based on the specified level and type.
    /// </summary>
    /// <param name="level">The model performance level.</param>
    /// <param name="type">The type of LLM model required.</param>
    /// <returns>The configured LLM model instance.</returns>
    public abstract ILLmModel GetLLmModel(AigcModelLevel level, LLmModelType type);

    /// <summary>
    /// Gets an image generation model based on the specified level.
    /// </summary>
    /// <param name="level">The model performance level.</param>
    /// <returns>The configured image generation model instance.</returns>
    public abstract IImageGenModel GetImageGenModel(AigcModelLevel level);

    /// <summary>
    /// Gets the embedding model for vector representations.
    /// </summary>
    /// <returns>The configured embedding model instance.</returns>
    public abstract IEmbeddingModel GetEmbedding();


    /// <summary>
    /// Sets the input message and optional attachments for the current chat context.
    /// </summary>
    /// <param name="msg">The message text to set.</param>
    /// <param name="attachments">Optional collection of attachments to include.</param>
    public abstract void SetInput(string msg, IEnumerable<AttachmentSet> attachments = null);


    /// <summary>
    /// Executes an LLM call with the specified request and returns the raw string response.
    /// </summary>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <returns>A task representing the asynchronous operation, returning the response string.</returns>
    public abstract Task<string> Call(ILLmCall call, LLmCallRequest callRequest);

    /// <summary>
    /// Executes an LLM call and deserializes the response into the specified compound type.
    /// </summary>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <param name="type">The compound type definition for deserialization.</param>
    /// <returns>A task representing the asynchronous operation, returning the deserialized response string.</returns>
    public abstract Task<string> Call(ILLmCall call, LLmCallRequest callRequest, DCompond type);

    /// <summary>
    /// Executes an LLM call and processes the response as a function call result.
    /// </summary>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <param name="type">The function call type definition.</param>
    /// <returns>A task representing the asynchronous operation, returning the response string.</returns>
    public abstract Task<string> Call(ILLmCall call, LLmCallRequest callRequest, IFunctionCallType type);

    /// <summary>
    /// Executes an LLM call and deserializes the response into the specified .NET type.
    /// </summary>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <param name="type">The .NET type to deserialize the response into.</param>
    /// <returns>A task representing the asynchronous operation, returning the response string.</returns>
    public abstract Task<string> Call(ILLmCall call, LLmCallRequest callRequest, Type type);

    /// <summary>
    /// Executes an LLM call and converts the response using the specified converter function.
    /// </summary>
    /// <typeparam name="TConvert">The target type for the converted result.</typeparam>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <param name="converter">The function to convert the raw response to the target type.</param>
    /// <returns>A task representing the asynchronous operation, returning the converted result.</returns>
    public abstract Task<TConvert> CallConvert<TConvert>(ILLmCall call, LLmCallRequest callRequest, Func<LLmCallRequest, string, TConvert> converter)
        where TConvert : class;

    /// <summary>
    /// Executes an LLM call as a function call and deserializes the result into the specified type.
    /// </summary>
    /// <typeparam name="T">The target type for the function call result.</typeparam>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <param name="verifier">Optional predicate to verify the deserialized result.</param>
    /// <returns>A task representing the asynchronous operation, returning the function call result.</returns>
    public abstract Task<T> CallFunction<T>(ILLmCall call, LLmCallRequest callRequest, Predicate<T> verifier = null) 
        where T : class;

    /// <summary>
    /// Executes an LLM function call and converts the result using the specified converter function.
    /// </summary>
    /// <typeparam name="T">The intermediate type for the function call result.</typeparam>
    /// <typeparam name="TConvert">The final target type after conversion.</typeparam>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <param name="converter">The function to convert the intermediate result to the final type.</param>
    /// <returns>A task representing the asynchronous operation, returning the converted result.</returns>
    public abstract Task<TConvert> CallFunctionConvert<T, TConvert>(ILLmCall call, LLmCallRequest callRequest, Func<LLmCallRequest, T, TConvert> converter) 
        where T : class 
        where TConvert : class;

    /// <summary>
    /// Executes an LLM function call and attempts to deserialize the response into one of the specified types.
    /// </summary>
    /// <param name="call">The LLM call interface to use.</param>
    /// <param name="callRequest">The request containing prompt and parameters.</param>
    /// <param name="types">Array of candidate types to deserialize into.</param>
    /// <param name="verifier">Optional predicate to verify the deserialized result.</param>
    /// <returns>A task representing the asynchronous operation, returning the function call result.</returns>
    public abstract Task<object> CallFunction(ILLmCall call, LLmCallRequest callRequest, Type[] types, Predicate<object> verifier = null);

    /// <summary>
    /// Generates an image from the given text prompt using an image generation model.
    /// </summary>
    /// <param name="input">The text prompt describing the image to generate.</param>
    /// <param name="level">The model performance level to use.</param>
    /// <param name="aspectRatio">The desired aspect ratio for the generated image.</param>
    /// <returns>A task representing the asynchronous operation, returning the generated image as a Bitmap.</returns>
    public abstract Task<Bitmap> GenerateImage(string input, AigcModelLevel level = AigcModelLevel.Default, ImageAspectRatio aspectRatio = ImageAspectRatio.Default);

    /// <summary>
    /// Extracts a code block from a markdown-formatted string.
    /// </summary>
    /// <param name="markdown">The markdown string containing code blocks.</param>
    /// <returns>The extracted code block content, or the original string if no code block is found.</returns>
    public abstract string ExtractCodeBlock(string markdown);

    /// <summary>
    /// Resolves the output of an LLM call into an SObject representation.
    /// </summary>
    /// <param name="call">The LLM call interface to resolve output from.</param>
    /// <returns>The resolved SObject representing the LLM output.</returns>
    public abstract SObject ResolveSObjectOutput(ILLmCall call);

    /// <summary>
    /// Creates a stream appender for appending LLM streaming responses to a conversation.
    /// </summary>
    /// <param name="conversation">The conversation handler to append streams to.</param>
    /// <returns>A new LLmStreamAppender instance for the specified conversation.</returns>
    public abstract LLmStreamUpdater CreateLLmStreamAppender(IConversationHandler conversation);

    /// <summary>
    /// Creates a looped symbol indicator for a conversation handler, typically used for loading or thinking animations.
    /// </summary>
    /// <param name="conversation">The conversation handler to display the symbol for.</param>
    /// <returns>A disposable object that controls the lifecycle of the looped symbol.</returns>
    public abstract IDisposable CreateLoopedSymbol(IConversationHandler conversation);

    /// <summary>
    /// Gets the localized speech language code for text-to-speech or speech-to-text operations.
    /// </summary>
    public abstract string LocalizedSpeechLanguage { get;  }

    /// <summary>
    /// Formats a TimeSpan into a string in the format "HH:mm:ss".
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <returns>A string representation of the time span in "HH:mm:ss" format.</returns>
    public static string FormatTimeSpan(TimeSpan timeSpan)
    {
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            (int)timeSpan.TotalHours,
            timeSpan.Minutes,
            timeSpan.Seconds);
    }
}