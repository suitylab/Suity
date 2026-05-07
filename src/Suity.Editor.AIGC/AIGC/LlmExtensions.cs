using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public static class LlmExtensions
{
    /// <summary>
    /// Sets the model identifier for the LLM model asset builder.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="modelId">The model identifier to set.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithModelId<T>(this LLmModelAssetBuilder<T> builder, string modelId)
        where T : LLmModelAsset, new()
    {
        builder.SetModelId(modelId);
        return builder;
    }

    /// <summary>
    /// Sets the description for the LLM model asset builder.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="description">The description to set.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithDescription<T>(this LLmModelAssetBuilder<T> builder, string description)
        where T : LLmModelAsset, new()
    {
        builder.SetDescription(description);
        return builder;
    }

    /// <summary>
    /// Configures whether the LLM model supports reasoning capabilities.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="reasoning">True to enable reasoning support; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithReasoning<T>(this LLmModelAssetBuilder<T> builder, bool reasoning)
        where T : LLmModelAsset, new()
    {
        builder.SetSupportReasoning(reasoning);
        return builder;
    }

    /// <summary>
    /// Configures whether the LLM model supports multimodal inputs.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="multimodel">True to enable multimodal support; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithMultimodel<T>(this LLmModelAssetBuilder<T> builder, bool multimodel)
        where T : LLmModelAsset, new()
    {
        builder.SetSupportMultimodel(multimodel);
        return builder;
    }

    /// <summary>
    /// Configures whether the LLM model supports tool calling.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="toolCalling">True to enable tool calling support; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithToolCalling<T>(this LLmModelAssetBuilder<T> builder, bool toolCalling)
        where T : LLmModelAsset, new()
    {
        builder.SetSupportToolCalling(toolCalling);
        return builder;
    }

    /// <summary>
    /// Configures whether the LLM model supports streaming responses.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="streaming">True to enable streaming support; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithStreaming<T>(this LLmModelAssetBuilder<T> builder, bool streaming)
        where T : LLmModelAsset, new()
    {
        builder.SetSupportStreaming(streaming);
        return builder;
    }

    /// <summary>
    /// Configures whether the LLM model supports web search.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="webSearch">True to enable web search support; otherwise, false.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithWebSearch<T>(this LLmModelAssetBuilder<T> builder, bool webSearch)
        where T : LLmModelAsset, new()
    {
        builder.SetWebSearch(webSearch);
        return builder;
    }

    /// <summary>
    /// Sets the context size in thousands of tokens for the LLM model.
    /// </summary>
    /// <typeparam name="T">The type of LLM model asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="contextSizeK">The context size in K tokens.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmModelAssetBuilder<T> WithContextSizeK<T>(this LLmModelAssetBuilder<T> builder, int contextSizeK)
        where T : LLmModelAsset, new()
    {
        builder.SetContextSizeK(contextSizeK);
        return builder;
    }

    /// <summary>
    /// Sets the model identifier for the embedding asset builder.
    /// </summary>
    /// <typeparam name="T">The type of embedding asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="modelId">The model identifier to set.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static LLmEmbeddingAssetBuilder<T> WithModelId<T>(this LLmEmbeddingAssetBuilder<T> builder, string modelId)
        where T : LLmEmbeddingAsset, new()
    {
        builder.SetModelId(modelId);
        return builder;
    }


    /// <summary>
    /// Sets the model identifier for the image generation asset builder.
    /// </summary>
    /// <typeparam name="T">The type of image generation asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="modelId">The model identifier to set.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ImageGenAssetBuilder<T> WithModelId<T>(this ImageGenAssetBuilder<T> builder, string modelId)
        where T : ImageGenAsset, new()
    {
        builder.SetModelId(modelId);
        return builder;
    }

    /// <summary>
    /// Sets the description for the image generation asset builder.
    /// </summary>
    /// <typeparam name="T">The type of image generation asset.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="description">The description to set.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static ImageGenAssetBuilder<T> WithDescription<T>(this ImageGenAssetBuilder<T> builder, string description)
        where T : ImageGenAsset, new()
    {
        builder.SetDescription(description);
        return builder;
    }


    /// <summary>
    /// Appends a system message to the LLM call.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="systemPrompt">The system prompt content.</param>
    public static void AppendSystemMessage(this ILLmCall call, string systemPrompt)
    {
        var msg = new LLmMessage
        {
            Role = LLmMessageRole.System,
            Message = systemPrompt,
        };

        call.AppendMessage(msg);
    }

    /// <summary>
    /// Appends a user message to the LLM call.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="userPrompt">The user prompt content.</param>
    public static void AppendUserMessage(this ILLmCall call, string userPrompt)
    {
        var msg = new LLmMessage
        {
            Role = LLmMessageRole.User,
            Message = userPrompt,
        };

        call.AppendMessage(msg);
    }

    /// <summary>
    /// Appends an assistant message to the LLM call.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="assistantPrompt">The assistant prompt content.</param>
    public static void AppendAssistantMessage(this ILLmCall call, string assistantPrompt)
    {
        var msg = new LLmMessage
        {
            Role = LLmMessageRole.Assistant,
            Message = assistantPrompt,
        };

        call.AppendMessage(msg);
    }


    /// <summary>
    /// Executes an LLM call with a user prompt.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="userPrompt">The user prompt content.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <param name="config">Optional model parameter configuration.</param>
    /// <param name="title">Optional title for the call.</param>
    /// <param name="retry">Optional retry count.</param>
    /// <returns>A task representing the asynchronous operation, returning the LLM response.</returns>
    public static Task<string> Call(this ILLmCall call, string userPrompt,
        CancellationToken cancel = default, LLmModelParameter config = null, string title = null, int? retry = null)
    {
        var callRequest = new LLmCallRequest(null, userPrompt)
        {
            Cancel = cancel,
            Parameter = config,
            Title = title,
            RetryCount = retry,
        };

        return LLmService.Instance.Call(call, callRequest);
    }

    /// <summary>
    /// Executes an LLM call with a system prompt and multiple user prompts.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="systemPrompt">The system prompt content.</param>
    /// <param name="userPrompts">Collection of user prompt contents.</param>
    /// <param name="cancel">Cancellation token.</param>
    /// <param name="config">Optional model parameter configuration.</param>
    /// <param name="title">Optional title for the call.</param>
    /// <param name="retry">Optional retry count.</param>
    /// <returns>A task representing the asynchronous operation, returning the LLM response.</returns>
    public static Task<string> Call(this ILLmCall call, string systemPrompt, IEnumerable<string> userPrompts,
        CancellationToken cancel = default, LLmModelParameter config = null, string title = null, int? retry = null)
    {
        var callRequest = new LLmCallRequest([systemPrompt], userPrompts)
        {
            Cancel = cancel,
            Parameter = config,
            Title = title,
            RetryCount = retry,
        };

        return LLmService.Instance.Call(call, callRequest);
    }

    /// <summary>
    /// Executes an LLM call with the specified request.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <returns>A task representing the asynchronous operation, returning the LLM response.</returns>
    public static Task<string> Call(this ILLmCall call, LLmCallRequest callRequest)
        => LLmService.Instance.Call(call, callRequest);

    /// <summary>
    /// Executes an LLM call and converts the response using the specified converter.
    /// </summary>
    /// <typeparam name="TConvert">The type to convert the response to.</typeparam>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <param name="converter">Function to convert the response.</param>
    /// <returns>A task representing the asynchronous operation, returning the converted result.</returns>
    public static Task<TConvert> CallConvert<TConvert>(this ILLmCall call, LLmCallRequest callRequest, Func<LLmCallRequest, string, TConvert> converter) 
        where TConvert : class
        => LLmService.Instance.CallConvert<TConvert>(call, callRequest, converter);


    /// <summary>
    /// Executes an LLM call with a compound type for structured output.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <param name="type">The compound type for structured output.</param>
    /// <returns>A task representing the asynchronous operation, returning the LLM response.</returns>
    public static Task<string> Call(this ILLmCall call, LLmCallRequest callRequest, DCompond type) 
        => LLmService.Instance.Call(call, callRequest, type);

    /// <summary>
    /// Executes an LLM call with a function call type for structured output.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <param name="type">The function call type for structured output.</param>
    /// <returns>A task representing the asynchronous operation, returning the LLM response.</returns>
    public static Task<string> Call(this ILLmCall call, LLmCallRequest callRequest, IFunctionCallType type)
        => LLmService.Instance.Call(call, callRequest, type);

    /// <summary>
    /// Executes an LLM call with a specified type for structured output.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <param name="type">The type for structured output.</param>
    /// <returns>A task representing the asynchronous operation, returning the LLM response.</returns>
    public static Task<string> Call(this ILLmCall call, LLmCallRequest callRequest, Type type) 
        => LLmService.Instance.Call(call, callRequest, type);


    /// <summary>
    /// Executes an LLM call and returns a deserialized object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <param name="verifier">Optional predicate to verify the result.</param>
    /// <returns>A task representing the asynchronous operation, returning the deserialized object.</returns>
    public static Task<T> CallFunction<T>(this ILLmCall call, LLmCallRequest callRequest, Predicate<T> verifier = null) 
        where T : class
        => LLmService.Instance.CallFunction(call, callRequest, verifier);

    /// <summary>
    /// Executes an LLM call and converts the function result using the specified converter.
    /// </summary>
    /// <typeparam name="T">The type of the function result.</typeparam>
    /// <typeparam name="TConvert">The type to convert the result to.</typeparam>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <param name="converter">Function to convert the result.</param>
    /// <returns>A task representing the asynchronous operation, returning the converted result.</returns>
    public static Task<TConvert> CallFunctionConvert<T, TConvert>(this ILLmCall call, LLmCallRequest callRequest, Func<LLmCallRequest, T, TConvert> converter) 
        where T : class
        where TConvert : class
        => LLmService.Instance.CallFunctionConvert(call, callRequest, converter);


    /// <summary>
    /// Executes an LLM call and returns a deserialized object from one of the specified types.
    /// </summary>
    /// <param name="call">The LLM call instance.</param>
    /// <param name="callRequest">The call request containing all parameters.</param>
    /// <param name="types">Array of possible types for the response.</param>
    /// <param name="verifier">Optional predicate to verify the result.</param>
    /// <returns>A task representing the asynchronous operation, returning the deserialized object.</returns>
    public static Task<object> CallFunction(this ILLmCall call, LLmCallRequest callRequest, Type[] types, Predicate<object> verifier = null)
        => LLmService.Instance.CallFunction(call, callRequest, types, verifier);
}