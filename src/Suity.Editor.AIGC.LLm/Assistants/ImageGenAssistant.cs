using Suity.Views;
using System;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// An AI assistant that handles image generation requests by invoking the configured image generation model.
/// </summary>
[DisplayText("Image Generation Assistant")]
public class ImageGenAssistant : AIAssistant
{
    /// <inheritdoc/>
    public override async Task<AICallResult> HandleRequest(AIRequest request)
    {
        var option = (request.Option as AIAssistantOption)?.Option as ImageGenOptions;

        var imgModel = LLmService.Instance.GetImageGenModel(option?.ModelLevel ?? AigcModelLevel.Default);
        if (imgModel is null)
        {
            return AICallResult.FromFailed(L("Image generation model is not configured."));
        }

        var call = imgModel.CreateCall(request.FuncContext);
        if (call is null)
        {
            return AICallResult.FromFailed(L("Image generation call is not configured."));
        }

        IDisposable symbol = null;
        var conversation = request.Conversation;
        if (conversation != null)
        {
            symbol = LLmService.Instance.CreateLoopedSymbol(conversation);
        }


        string msg = L("Calling model: ") + imgModel.ModelId;
        var msgItem = request.Conversation?.AddRunningMessage(msg);

        try
        {
            var startTime = DateTime.Now;
            var img = await call.GenerateImage(request.UserMessage, option?.AspectRatio ?? ImageAspectRatio.Default);

            var timeSpan = DateTime.Now - startTime;
            var timeStr = LLmService.FormatTimeSpan(timeSpan);

            conversation?.AddSystemMessage(L($"Model {L(imgModel.ModelId)} called, took: {timeStr}.")).RemoveOn(3);

            return AICallResult.FromResult(img);
        }
        catch (Exception err)
        {
            conversation?.AddException(err, "Image generation failed.");
            return AICallResult.FromFailed(err.Message);
        }
        finally
        {
            msgItem?.Dispose();
            symbol?.Dispose();
        }
    }
}
