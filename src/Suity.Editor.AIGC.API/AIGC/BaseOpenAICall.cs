using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.ChatFunctions;
using OpenAI_API.Models;
using Suity.Editor.AIGC.API;
using Suity.Views;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

/// <summary>
/// Abstract base class for making OpenAI-compatible API calls.
/// </summary>
public abstract class BaseOpenAICall : BaseLLmCall
{
    private readonly BaseOpenAIPlugin _plugin;
    private readonly Model? _model;
    private OpenAIAPI _api;

    private readonly ValueStore<Conversation> _request = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseOpenAICall"/> class.
    /// </summary>
    /// <param name="plugin">The OpenAI plugin instance.</param>
    /// <param name="model">The LLM model to use.</param>
    /// <param name="config">Optional model configuration parameters.</param>
    /// <param name="context">Optional function context.</param>
    protected BaseOpenAICall(BaseOpenAIPlugin plugin, ILLmModel model, LLmModelParameter? config = null, FunctionContext? context = null)
        : base(model, config, context)
    {
        _plugin = plugin ?? throw new System.ArgumentNullException(nameof(plugin));
        _model = null;

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new AigcException(L("Base URL not set."));
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new AigcException(L("API Key not set."));
        }
    }

    /// <summary>
    /// Gets the current conversation instance for the API request.
    /// </summary>
    public Conversation Request => _request.Get();

    /// <summary>
    /// Gets the API key used for authentication.
    /// </summary>
    public virtual string ApiKey => _plugin.ApiKey;

    /// <summary>
    /// Gets the base URL for API requests.
    /// </summary>
    public virtual string BaseUrl => _plugin.BaseUrl;

    /// <summary>
    /// Gets the log path identifier for this call, based on the manufacturer ID.
    /// </summary>
    public override string LogPath => _plugin.ManufacturerId;

    /// <summary>
    /// Initializes a new message conversation and sets up the API client.
    /// </summary>
    public override void NewMessage()
    {
        base.NewMessage();

        _api ??= new(ApiKey)
        {
            ApiUrlFormat = OkGoDoItHelper.ResolveApiUrlFormat(BaseUrl),
        };

        _request.Set(_api.Chat.CreateConversation());
    }

    /// <summary>
    /// Appends a message to the conversation based on its role.
    /// </summary>
    /// <param name="msg">The message to append.</param>
    public override void AppendMessage(LLmMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg?.Message))
        {
            return;
        }

        var request = _request.Get();
        if (request is null)
        {
            return;
        }

        switch (msg.Role)
        {
            case LLmMessageRole.System:
                request.AppendSystemMessage(msg.Message);
                break;

            case LLmMessageRole.User:
                request.AppendUserInput(msg.Message);
                break;

            case LLmMessageRole.Assistant:
                request.AppendExampleChatbotOutput(msg.Message);
                break;
        }
    }

    /// <summary>
    /// Accepts multiple function options
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="description"></param>
    public override void AddFunction(string name, object type, string description)
    {
        var request = Request;
        if (request is null)
        {
            return;
        }

        if (Model.SupportToolCalling)
        {
            if (request.Tools?.Any(o => o.Function.Name == name) == true)
            {
                return;
            }

            var prop = ResolveSchema(type, ref name, ref description, base.Context);
            var func = new Function { Name = name, Description = description, Parameters = prop };
            request.AppendTool(func);
        }

        base.AddFunction(name, type, description);
    }

    /// <summary>
    /// Accepts single function option only
    /// </summary>
    /// <param name="name"></param>
    protected override void SetFunctionCall(string name)
    {
        var request = Request;
        if (request is null)
        {
            return;
        }

        if (Model.SupportToolCalling)
        {
            request.FunctionCall = new FunctionCall { Name = name };
        }

        base.SetFunctionCall(name);
    }

    /// <summary>
    /// Executes the API call and returns the response text.
    /// </summary>
    /// <param name="cancel">Cancellation token.</param>
    /// <param name="config">Model configuration parameters.</param>
    /// <param name="option">Call options.</param>
    /// <param name="title">Optional title for the call.</param>
    /// <returns>The response text from the API.</returns>
    public override async Task<string> Call(CancellationToken cancel, LLmModelParameter config, LLmCallOption option = null, string title = null)
    {
        string modelId = base.Model?.ModelId ?? _model ?? throw new AigcException(L("Model Id not set"));

        cancel.ThrowIfCancellationRequested();

        var request = _request.Get();
        if (request is null)
        {
            return null;
        }

        config ??= Config;

        var param = request.RequestParameters;
        param.Model = modelId;
        param.Temperature = GetValidParamValue(config?.Temperature ?? base.Temperature);
        param.TopP = GetValidParamValue(config?.TopP ?? base.TopP);
        param.MaxTokens = GetValidParamValue(config?.MaxTokens ?? base.MaxTokens);
        param.PresencePenalty = GetValidParamValue(config?.PresencePenalty ?? base.PresencePenalty);
        param.FrequencyPenalty = GetValidParamValue(config?.FrequencyPenalty ?? base.FrequencyPenalty);

        // Handle cases where tool calling is not supported
        if (HasFunction && !Model.SupportToolCalling)
        {
            AddManualFunctionPrompt();
        }

        try
        {
            if (Model.SupportStreaming && !HasFunction)
            {
                var conversation = GetConversation();

                var appender = Appender = LLmService.Instance.CreateLLmStreamAppender(conversation)
                    ?? throw new AigcException("Create LLm stream appender failed");

                DisposableDialogItem? msgReasoning = null;
                DisposableDialogItem? msg = null;

                // Need to set incremental output mode
                // request.RequestParameters.IncrementalOutput = true;

                await Task.Run(async () =>
                {
                    var responseStream = request.StreamResponseEnumerableFromChatbotAsync(cancel);
                    await foreach (var t in responseStream)
                    {
                        appender.Append(t);
                    }
                }, cancel);

                msgReasoning?.Dispose();
                msg?.Dispose();

                LastTextOutput = appender.FullText.ToString();
                appender.Dispose();

                if (HasFunction)
                {
                    ProcessManualFunctionCall();
                }
            }
            else
            {
                LastTextOutput = await request.GetResponseFromChatbotAsync(cancel) ?? string.Empty;
                cancel.ThrowIfCancellationRequested();
            }

            ProcessManualFunctionCall(request.MostRecentApiResult);
        }
        finally
        {
            AddToFileLog(request.LastRequestString, request.LastResponseString);
        }

        return LastTextOutput;
    }

    /// <summary>
    /// Clears the current conversation state.
    /// </summary>
    public override void Clear()
    {
        _request.PickUp();
    }

    /// <summary>
    /// Process chat result, attempt to parse function call
    /// </summary>
    /// <param name="chatResult"></param>
    protected virtual void ProcessManualFunctionCall(ChatResult chatResult)
    {
        if (!HasFunction)
        {
            return;
        }

        if (Model.SupportToolCalling)
        {
            var tool = chatResult.Choices?.FirstOrDefault()?.Message?.ToolCalls?.FirstOrDefault()?.Function;
            if (tool is not null)
            {
                LastFunctionName = tool.Name;
                LastFunctionOutput = tool.Arguments;
            }
        }
        else
        {
            ProcessManualFunctionCall();
        }
    }
}