using Suity.Editor.Types;
using Suity.Views.Im;
using System;
using System.Threading.Tasks;
using System.Threading;
using static Suity.Helpers.GlobalLocalizer;
using System.Drawing;

namespace Suity.Editor.AIGC;


#region ILLmModel

/// <summary>
/// Represents a language model that can be used for various natural language processing tasks.
/// </summary>
[NativeType("ILLmModel", Description = "LLm Model", CodeBase = "*AIGC")]
public interface ILLmModel
{
    /// <summary>
    /// Gets the unique identifier of the model.
    /// </summary>
    string ModelId { get; }

    /// <summary>
    /// Indicates whether the model is manually managed.
    /// </summary>
    bool IsManual { get; }

    /// <summary>
    /// Indicates whether the API key for the model is valid.
    /// </summary>
    bool ApiKeyValid { get; }

    /// <summary>
    /// Indicates whether the model supports tool calling.
    /// </summary>
    bool SupportToolCalling { get; }

    /// <summary>
    /// Indicates whether the model supports reasoning.
    /// </summary>
    bool SupportReasoning { get; }

    /// <summary>
    /// Gets a value indicating whether streaming is supported by the implementation.
    /// </summary>
    /// <remarks>When <see langword="true"/>, operations that utilize streaming may be available. If <see
    /// langword="false"/>, only non-streaming operations are supported. The availability of streaming may affect
    /// performance and resource usage for large data transfers.</remarks>
    bool SupportStreaming { get; }

    /// <summary>
    /// Creates a new call instance for the model.
    /// </summary>
    /// <param name="config">The configuration for the call. Can be null.</param>
    /// <param name="context">The function context. Can be null.</param>
    /// <returns>An instance of ILLmCall.</returns>
    ILLmCall CreateCall(LLmModelParameter config = null, FunctionContext context = null);

    /// <summary>
    /// Creates a new conversation instance for the model.
    /// </summary>
    /// <param name="config">The configuration for the conversation. Can be null.</param>
    /// <param name="context">The function context. Can be null.</param>
    /// <returns>An instance of ILLmChat.</returns>
    ILLmChat CreateConversation(LLmModelParameter config = null, FunctionContext context = null);
}

#endregion

#region BasicLLmModel

/// <summary>
/// Represents a basic language model that implements the ILLmModel interface.
/// </summary>
public class BasicLLmModel : ILLmModel
{
    /// <summary>
    /// Gets the unique identifier of the model.
    /// </summary>
    public string ModelId { get; }

    /// <summary>
    /// Indicates whether the model is manually managed.
    /// </summary>
    public bool IsManual { get; init; }

    /// <summary>
    /// Indicates whether the API key for the model is valid.
    /// </summary>
    public bool ApiKeyValid => true;

    /// <summary>
    /// Indicates whether the model supports tool calling.
    /// </summary>
    public bool SupportToolCalling { get; init; }

    /// <summary>
    /// Indicates whether the model supports reasoning.
    /// </summary>
    public bool SupportReasoning { get; init; }

    /// <summary>
    /// Gets a value indicating whether streaming is supported by this model implementation.
    /// </summary>
    public bool SupportStreaming { get; init; }

    /// <summary>
    /// Creates a new call instance for the model.
    /// </summary>
    /// <param name="config">The configuration for the call. Can be null.</param>
    /// <param name="context">The function context. Can be null.</param>
    /// <returns>An instance of ILLmCall.</returns>
    public ILLmCall CreateCall(LLmModelParameter config = null, FunctionContext context = null) => null;

    /// <summary>
    /// Creates a new conversation instance for the model.
    /// </summary>
    /// <param name="config">The configuration for the conversation. Can be null.</param>
    /// <param name="context">The function context. Can be null.</param>
    /// <returns>An instance of ILLmChat.</returns>
    public ILLmChat CreateConversation(LLmModelParameter config = null, FunctionContext context = null) => null;

    /// <summary>
    /// Initializes a new instance of the BasicLLmModel class with the specified model ID.
    /// </summary>
    /// <param name="modelId">The unique identifier of the model.</param>
    public BasicLLmModel(string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new ArgumentException(L($"'{nameof(modelId)}' cannot be null or white space."), nameof(modelId));
        }

        ModelId = modelId;
    }
}


#endregion

#region ILLmCall

/// <summary>
/// Represents a call to a language model, allowing for interaction with the model.
/// </summary>
public interface ILLmCall : IDisposable
{
    /// <summary>
    /// Gets the language model associated with this call.
    /// </summary>
    ILLmModel Model { get; }

    /// <summary>
    /// Gets the function context associated with this call.
    /// </summary>
    FunctionContext Context { get; }

    /// <summary>
    /// Indicates whether the call has a function associated with it.
    /// </summary>
    bool HasFunction { get; }
    /// <summary>
    /// Gets or sets the function call associated with this call.
    /// </summary>
    string FunctionCall { get; set; }


    /// <summary>
    /// Gets the last text output from the model.
    /// </summary>
    string LastTextOutput { get; }
    /// <summary>
    /// Gets the last function name called by the model.
    /// </summary>
    string LastFunctionName { get; }
    /// <summary>
    /// Gets the last function output from the model.
    /// </summary>
    string LastFunctionOutput { get; }

    /// <summary>
    /// Gets the stream appender used for streaming output.
    /// </summary>
    LLmStreamUpdater Appender { get; }

    /// <summary>
    /// Resets the call to a new message state.
    /// </summary>
    void NewMessage();

    /// <summary>
    /// Appends a message to the current call.
    /// </summary>
    /// <param name="msg">The message content.</param>
    void AppendMessage(LLmMessage msg);

    /// <summary>
    /// Adds a function to the call.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="type">The type of the function.</param>
    /// <param name="description">The description of the function. Can be null.</param>
    void AddFunction(string name, object type, string description = null);


    /// <summary>
    /// Calls the model with the given configuration and cancellation token.
    /// </summary>
    /// <param name="cancel">The cancellation token to cancel the operation.</param>
    /// <param name="parameter">The configuration for the call. Can be null.</param>
    /// /// <param name="option">The additional options for the call. Can be null.</param>
    /// <param name="title">The title of the call. Can be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the output of the call.</returns>
    Task<string> Call(CancellationToken cancel, LLmModelParameter parameter = null, LLmCallOption option = null, string title = null);

    /// <summary>
    /// Gets a function by its name.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <returns>The function object.</returns>
    object GetFunction(string name);

    /// <summary>
    /// Clears the call.
    /// </summary>
    void Clear();
}

#endregion

#region ILLmChat

/// <summary>
/// Represents a chat session with a language model.
/// </summary>
public enum LLmChatStates
{
    /// <summary>
    /// The chat is stopped.
    /// </summary>
    Stopped,
    /// <summary>
    /// The chat is starting.
    /// </summary>
    Starting,
    /// <summary>
    /// The chat is started.
    /// </summary>
    Started,
}

/// <summary>
/// Represents a chat session with a language model.
/// </summary>
public interface ILLmChat : IDrawImGuiNode, IDisposable
{
    /// <summary>
    /// Gets the current state of the chat.
    /// </summary>
    LLmChatStates State { get; }

    /// <summary>
    /// Starts the chat session.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <param name="attachments">Any attachments to the message. Can be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<object> Start(string msg, object attachments = null, object option = null);

    /// <summary>
    /// Starts the chat session without sending a message.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the output of the chat session.</returns>
    Task<object> StartWithoutMessage(object option = null);

    /// <summary>
    /// Stops the chat session.
    /// </summary>
    void Stop();

    /// <summary>
    /// Clears the chat session.
    /// </summary>
    void Clear();

    /// <summary>
    /// Sends a message to the chat session.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <param name="attachments">Any attachments to the message. Can be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the output of the chat session.</returns>
    Task<object> Send(string msg, object attachments = null, object option = null);

    /// <summary>
    /// Renders the chat settings GUI.
    /// </summary>
    /// <param name="gui">The ImGui instance to use for rendering.</param>
    void OnSettingGui(ImGui gui);
}

#endregion

#region ILLmChatProvider

/// <summary>
/// Represents a provider for creating chat sessions with language models.
/// </summary>
public interface ILLmChatProvider
{
    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    /// <param name="context">The function context. Can be null.</param>
    /// <returns>An instance of ILLmChat.</returns>
    ILLmChat CreateChat(FunctionContext context);
}

#endregion

#region IAssistantChatProvider

/// <summary>
/// Represents a chat provider for an assistant.
/// </summary>
public interface IAssistantChatProvider : ILLmChatProvider
{
    /// <summary>
    /// Gets the type of the assistant.
    /// </summary>
    public Type AssistantType { get; }
}

#endregion

#region IVectorEmbedding

/// <summary>
/// Represents an embedding model that can generate vector representations of documents.
/// </summary>
public interface IEmbeddingModel
{
    /// <summary>
    /// Generates a vector representation of the given document.
    /// </summary>
    /// <param name="document">The document to be vectorized.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the vector representation of the document.</returns>
    public Task<double[]> GetVector(string document);
}

#endregion

#region IImageGenModel

/// <summary>
/// Represents an image generation model that can create images from prompts.
/// </summary>
public interface IImageGenModel
{
    /// <summary>
    /// Gets the unique identifier of the image generation model.
    /// </summary>
    string ModelId { get; }

    /// <summary>
    /// Creates a new call instance for the image generation model.
    /// </summary>
    /// <param name="context">The function context. Can be null.</param>
    /// <returns>An instance of IImageGenCall.</returns>
    IImageGenCall CreateCall(FunctionContext context = null);
}

#endregion

#region IImageGenCall

/// <summary>
/// Specifies the aspect ratio for generated images.
/// </summary>
public enum ImageAspectRatio
{
    /// <summary>
    /// Uses the default aspect ratio defined by the model.
    /// </summary>
    Default = 0,
    /// <summary>
    /// Generates a square image (1:1 ratio).
    /// </summary>
    Square = 1,
    /// <summary>
    /// Generates a landscape-oriented image (wider than tall).
    /// </summary>
    Landscape = 2,
    /// <summary>
    /// Generates a portrait-oriented image (taller than wide).
    /// </summary>
    Portrait = 3,
}

/// <summary>
/// Represents a call to an image generation model, allowing for image creation from text prompts.
/// </summary>
public interface IImageGenCall
{
    /// <summary>
    /// Generates an image based on the provided prompt and aspect ratio.
    /// </summary>
    /// <param name="prompt">The text description of the image to generate.</param>
    /// <param name="aspectRatio">The desired aspect ratio for the generated image.</param>
    /// <param name="cancel">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated image as a Bitmap.</returns>
    Task<Bitmap> GenerateImage(string prompt, ImageAspectRatio aspectRatio, CancellationToken cancel = default);
}

#endregion

#region ImageGenOptions

/// <summary>
/// Represents the configuration options for image generation.
/// </summary>
public class ImageGenOptions
{
    /// <summary>
    /// Gets or sets the quality level of the AI-generated content model.
    /// </summary>
    public AigcModelLevel ModelLevel { get; set; } = AigcModelLevel.Default;

    /// <summary>
    /// Gets or sets the aspect ratio for the generated image.
    /// </summary>
    public ImageAspectRatio AspectRatio { get; set; } = ImageAspectRatio.Default;
}

#endregion