using static Suity.Helpers.GlobalLocalizer;
using System.Threading.Tasks;
using Suity.Views;
using System;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// A base creator assistant class for full pipeline creation with resume support.
/// </summary>
public abstract class RootCreatorAssistant : AIAssistant, IRootCreatorAssistant
{
    private bool _toolCall;

    /// <summary>
    /// Indicates this assistant supports tool call.
    /// </summary>
    public bool ToolCall
    {
        get => _toolCall;
        protected set => _toolCall = value;
    }

    public override Task<AICallResult> HandleRequest(AIRequest request)
    {
        var canvas = AIAssistantService.Instance.ResolveCanvasContext();

        if (canvas != null)
        {
            // Resume operation if user message is empty
            if (string.IsNullOrWhiteSpace(request.UserMessage))
            {
                return HandleRootResume(request, canvas);
            }
            else
            {
                if (_toolCall)
                {
                    return HandleTollCall(request, canvas, 0, request.UserMessage);
                }
                else
                {
                    return Task.FromResult(AICallResult.Empty);
                }
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.UserMessage))
            {
                return HandleRootCreate(request);
            }
            else
            {
                return Task.FromResult(AICallResult.Empty);
            }
        }
    }

    public virtual Task<AICallResult> HandleRootCreate(AIRequest request)
    {
        return Task.FromResult(AICallResult.Empty);
    }

    public virtual Task<AICallResult> HandleRootResume(AIRequest request, CanvasContext canvas)
    {
        return Task.FromResult(AICallResult.Empty);
    }

    protected async virtual Task<AICallResult> HandleTollCall(AIRequest request, CanvasContext canvas, int index, string subTask)
    {
        var taskReq = request.CreateWithMessage(subTask);
        var callChain = await AIAssistantService.Instance.SelectAssistants<IRootUpdaterAssistant>(taskReq, canvas);
        if (callChain is null || callChain.Calls.Count == 0)
        {
            request.Conversation.AddWarningMessage(L("No suitable AI assistant found for instruction: ") + subTask);
            return null;
        }

        DisposableDialogItem callChainMsg = null;
        DisposableDialogItem callMsg = null;

        try
        {
            callChainMsg = request.Conversation.AddInfoMessage(L("Current process: ") + callChain.ToFullText());

            for (int j = 0; j < callChain.Calls.Count; j++)
            {
                var aCall = callChain.Calls[j];
                if (aCall is null || aCall.Assistant is null || string.IsNullOrEmpty(aCall.CallingMessage))
                {
                    continue;
                }

                callMsg?.Dispose();
                callMsg = request.Conversation.AddInfoMessage(L("Current process: ") + $"{index + 1}.{j + 1} {aCall.ToFullText()}");

                try
                {
                    var reqCall = request.CreateWithMessage(aCall.CallingMessage);
                    return await aCall.Assistant.HandleRootUpdate(reqCall);
                }
                catch (Exception err)
                {
                    request.Conversation.AddException(err, L("AI assistant execution failed: ") + aCall.Assistant.ToDisplayText());
                }
            }
        }
        catch (Exception err)
        {
            throw;
        }
        finally
        {
            callChainMsg?.Dispose();
            callMsg?.Dispose();

            request.Conversation?.AddInfoMessage(L("Process completed."));
        }

        return null;
    }
}
