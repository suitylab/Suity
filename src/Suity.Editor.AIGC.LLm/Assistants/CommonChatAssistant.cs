using Suity.Views;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;

[DisplayText("Common chat")]
[ToolTipsText(@"
Assistant for common chat, Use this assistant when:
 - user only want to chat with the AI assistant,
 - user's prompt is not an instruction.
 - user's prompt will not make any task to be done.
")]
/// <summary>
/// Assistant for common chat interactions. Use this assistant when the user only wants to chat,
/// when the prompt is not an instruction, or when the prompt will not make any task to be done.
/// </summary>
public class CommonChatAssistant : AIAssistant
{
    /// <inheritdoc/>
    public override Task<AICallResult> HandleRequest(AIRequest request)
    {
        return HandleCommonChat(request);
    }

    private async Task<AICallResult> HandleCommonChat(AIRequest request)
    {
        var call = AIAssistantPlugin.Instance.CreatePresetCall(LLmModelPreset.Chat, ctx: request.FuncContext);

        var result = await call.Call(string.Empty, [request.UserMessage], request.Cancellation) ?? string.Empty;

        request.Conversation.AddSystemMessage(result);

        return AICallResult.FromMessage(result);
    }
}
