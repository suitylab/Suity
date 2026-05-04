using static Suity.Helpers.GlobalLocalizer;
using Suity.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;

#region AssistantChat

/// <summary>
/// Represents a chat session backed by an AI assistant, handling message processing and conversation flow.
/// </summary>
public class AssistantChat : BaseLLmChat
{
    private readonly AIAssistant _assistant;

    private bool _handling = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssistantChat"/> class.
    /// </summary>
    /// <param name="assistant">The AI assistant to handle requests.</param>
    /// <param name="context">The function context for the chat session.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="assistant"/> is null.</exception>
    public AssistantChat(AIAssistant assistant, FunctionContext context = null)
        : base(assistant.GetType().Name, assistant.ToDisplayText(), context)
    {
        _assistant = assistant ?? throw new ArgumentNullException(nameof(assistant));
    }

    /// <inheritdoc/>
    protected override Task<object> HandleStart(string msg, object option, CancellationTokenSource cancelSource)
    {
        if (_handling)
        {
            _conversation.AddErrorMessage(L("Processing, please try again later."));
            return null;
        }

        _handling = true;

        var request = new AIRequest
        {
            UserMessage = msg ?? string.Empty,
            Conversation = _conversation,
            FuncContext = this._context,
            Option = option,
            Cancel = cancelSource.Token,
            RequestCancel = () =>
            {
                cancelSource.Cancel();
                // _conversation.AddSystemMessage(L("Request cancelled."));
            }
        };

        return HandleRequest(request);
    }

    /// <inheritdoc/>
    protected override void HandleConversation(IConversationHandler conversation)
    {
        try
        {
            _assistant.HandleConversation(conversation);
        }
        catch (OperationCanceledException)
        {
            _conversation.AddSystemMessage(L("Operation cancelled."));
        }
        catch (AigcException llmErr)
        {
            _conversation.AddException(llmErr, L("Failed to execute session."));
        }
        catch (Exception err)
        {
            _conversation.AddException(err, L("Failed to execute session."));
        }
        finally
        {
            _handling = false;
        }
    }

    private async Task<object> HandleRequest(AIRequest request)
    {
        try
        {
            var result = await _assistant.HandleRequest(request);
            if (result?.Status == AICallStatus.Failed)
            {
                _conversation.AddErrorMessage(result.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _conversation.AddSystemMessage(L("Operation cancelled."));

            return null;
        }
        catch (AigcException llmErr)
        {
            _conversation.AddException(llmErr, L("Failed to execute session."));

            return null;
        }
        catch (Exception err)
        {
            _conversation.AddException(err, L("Failed to execute session."));

            return null;
        }
        finally
        {
            _handling = false;
        }
    }
}

#endregion

#region BasicChatAssistant

/// <summary>
/// Provides a basic chat assistant implementation with auto-creation support.
/// </summary>
[AssetAutoCreate]
public class BasicChatProvider : AssistantChatProvider<BasicChatAssistant>
{
}

/// <summary>
/// A basic chat bot assistant that processes user messages through a standard LLM chat model.
/// </summary>
[DisplayText("Basic Chat Bot", "*CoreIcon|AI")]
public class BasicChatAssistant : AIAssistant
{
    /// <inheritdoc/>
    public override async Task<AICallResult> HandleRequest(AIRequest request)
    {
        // Normal chat
        var call = request.CreateLLmCall(LLmModelPreset.Chat);

        var result = await call.Call(string.Empty, [request.UserMessage], request.Cancel) ?? string.Empty;

        if (call.Appender?.DisplayingFullResult == true)
        {
        }
        else
        {
            request.Conversation.AddSystemMessage(result);
        }
            

        return AICallResult.FromMessage(result);
    }
}

#endregion
