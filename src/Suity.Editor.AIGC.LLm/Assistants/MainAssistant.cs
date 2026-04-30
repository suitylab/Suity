using MarkedNet;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;

#region MainAssistant
/// <summary>
/// Main entry point AI assistant that routes requests to appropriate sub-assistants
/// based on the canvas context and request type.
/// </summary>
[DisplayText("AI Assistant", "*CoreIcon|Assistant")]
public class MainAssistant : AIAssistant
{
    /// <summary>
    /// Gets or sets whether multiple task mode is enabled.
    /// </summary>
    public static bool MultipleTask = false;

    /// <summary>
    /// Gets the inner assistant currently being used to handle requests.
    /// </summary>
    public AIAssistant InnerAssistant { get; private set; }

    /// <inheritdoc/>
    public override async Task<AICallResult> HandleRequest(AIRequest request)
    {
        try
        {
            if (request.Option is AIAssistantOption option)
            {
                if (option.Assistant is { } assistant)
                {
                    InnerAssistant = assistant;

                    request.Conversation?.AddRunningMessage(L("Assistant Start"), msg => 
                    {
                        msg.AddCode(assistant.ToDisplayTextL());
                    });

                    return await assistant.HandleRequest(request);
                }
                else
                {
                    return AICallResult.Empty;
                }
            }
            else
            {
                return await HandleNormal(request);
            }
        }
        finally
        {
            var assistant = InnerAssistant;
            InnerAssistant = null;

            request.Conversation?.AddRunningMessage(L("Assistant End"), msg =>
            {
                if (assistant != null)
                {
                    msg.AddCode(assistant.ToDisplayTextL());
                }
            });
        }
    }

    /// <inheritdoc/>
    public override void HandleConversation(IConversationHandler conversasion)
    {
        if (InnerAssistant is { } assistant)
        {
            assistant.HandleConversation(conversasion);
        }
    }

    private Task<AICallResult> HandleNormal(AIRequest request)
    {
        var canvas = AIAssistantServiceBK.Instance.ResolveCanvasContext();

        // Resume operation if user message is empty
        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            return HandleResume(request, canvas);
        }

        // Debug commands
        if (request.UserMessage.Trim().StartsWith("#"))
        {
            return HandleDebug(request, canvas);
        }

        if (canvas?.Canvas != null)
        {
            // Canvas update-oriented assistant
            return HandleUpdate(request, canvas);


            //if (sel.TargetDocument != null && sel.TargetDocument is not ICanvasDocument)
            //{
            //    request.Conversation.AddSystemInfoMessage($"Current selected document: {sel.TargetDocument}");

            //    // Document-oriented assistant
            //    var assistant = AIDocumentAssistantResolver.Instance.CreateDocumentAsisstant(sel)
            //        ?? throw new LLmException(AIAssistant.ERROR_MSG_ASSISTANT_NOT_FOUND);

            //    return await assistant.HandleInput(request);
            //}
            //else
            //{
            //    // Canvas update-oriented assistant
            //    // Canvas update-oriented assistant
            //    return await HandleUpdate(request, sel);
            //}
            //else
            //{
            //    // Canvas update-oriented assistant
            //    return await HandleUpdate(request, sel);
            //}
        }
        else
        {
            // Chat for newly created canvas
            return HandleCreate(request, canvas);
        }
    }

    private async Task<AICallResult> HandleCreate(AIRequest request, CanvasContext canvas)
    {
        var assistant = await AIAssistantServiceBK.Instance.SelectAssistant<IRootCreatorAssistant>(request, canvas);
        if (assistant is null)
        {
            return null;
        }

        try
        {
            return await assistant.HandleRootCreate(request);
        }
        catch (Exception err)
        {
            request.Conversation.AddException(err, L("Failed to execute AI assistant."));
        }

        return null;
    }

    private Task<AICallResult> HandleUpdate(AIRequest request, CanvasContext canvas)
    {
        if (MultipleTask)
        {
            return HandleMultipleTask(request, canvas);
        }
        else
        {
            return HandleTask(request, canvas, 0, request.UserMessage);
        }
    }

    private async Task<AICallResult> HandleDebug(AIRequest request, CanvasContext canvas)
    {
        /*
                switch (request.UserMessage.Trim())
                {
                    case "#1":
                        {
                            var gameMaking = new GameMakingAssistant();
                            return await gameMaking.HandleRootCreate(request);
                        }

                    case "#2":
                        {
                            var gameMaking = new GameMakingAssistant();
                            gameMaking.InitializeCanvas(canvas);
                            return await gameMaking.HandleRootResume(request);
                        }

                    default:
                        break;
                }*/

        return AICallResult.Empty;
    }

    private Task<AICallResult> HandleResume(AIRequest request, CanvasContext canvas)
    {
        return AIAssistantServiceBK.Instance.HandleResume(request, canvas);
    }

    [Obsolete]
    private async Task<AICallResult> HandleMultipleTask(AIRequest request, CanvasContext canvas)
    {
        InstructionInfo instructions = await GetMultipleTask(request);
        if (instructions is null || instructions.Instructions is null || instructions.Instructions.Count == 0)
        {
            request.Conversation.AddWarningMessage(L("No user instruction recognized."));
            return null;
        }

        request.Conversation.AddInfoMessage(instructions.GetInfoString());

        for (int i = 0; i < instructions.Instructions.Count; i++)
        {
            string subTask = instructions.Instructions[i];
            if (string.IsNullOrWhiteSpace(subTask))
            {
                continue;
            }

            await HandleTask(request, canvas, i, subTask);
        }

        return null;
    }

    private async Task<AICallResult> HandleTask(AIRequest request, CanvasContext canvas, int index, string subTask)
    {
        var taskReq = request.CreateWithMessage(subTask);
        var callChain = await AIAssistantServiceBK.Instance.SelectAssistants<IRootUpdaterAssistant>(taskReq, canvas);
        if (callChain is null || callChain.Calls.Count == 0)
        {
            request.Conversation.AddWarningMessage(L("No suitable AI assistant found to handle instruction: ") + subTask);
            return null;
        }

        DisposableDialogItem callChainMsg = null;
        DisposableDialogItem callMsg = null;

        try
        {
            callChainMsg = request.Conversation.AddInfoMessage(L("Current flow: ") + callChain.ToFullText());

            for (int j = 0; j < callChain.Calls.Count; j++)
            {
                var aCall = callChain.Calls[j];
                if (aCall is null || aCall.Assistant is null || string.IsNullOrEmpty(aCall.CallingMessage))
                {
                    continue;
                }

                callMsg?.Dispose();
                callMsg = request.Conversation.AddInfoMessage(L("Current flow: ") + $"{index + 1}.{j + 1} {aCall.ToFullText()}");

                try
                {
                    var reqCall = request.CreateWithMessage(aCall.CallingMessage);
                    return await aCall.Assistant.HandleRootUpdate(reqCall);
                }
                catch (Exception err)
                {
                    request.Conversation.AddException(err, L("Failed to execute AI assistant: ") + aCall.Assistant.ToDisplayText());
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

            request.Conversation?.AddInfoMessage(L("Flow completed."));
        }

        return null;
    }

    private static async Task<InstructionInfo> GetMultipleTask(AIRequest request)
    {
        string prompt = @"
You are an AI specialized in decomposing complex instructions into clear and distinct action steps.
Your task is to analyze the user's prompt and break down any compound components into separate, well-defined instructions while preserving the original intent.

<rule>
Maintain Original Meaning: Do not introduce new steps or alter the user's intent.
Identify Compound Elements: Detect phrases that contain multiple actions, conditions, or objectives.
Separate Actions Clearly: Rewrite these into distinct, standalone instructions while ensuring logical order.
Keep it Concise: Use minimal words while ensuring clarity.
</rule>

<example>
User Prompt:
  'Generate a futuristic city concept with neon lights, flying cars, and a cyberpunk aesthetic, then create a 3D model based on it.'

Output:
  1. 'Generate a futuristic city concept.'
  2. 'Include neon lights, flying cars, and a cyberpunk aesthetic.'
  3. 'Create a 3D model based on the concept.'
</example>

IMPORTANT Notice:
 - Ensure the output remains clear, precise, and faithful to the original request.
 - Saperate user's compond task only, do NOT generate additional task.
 - Keep the spoken language same to the use input.
 - Export json format according to the tool calling schema.
";
        var call = request.CreateLLmCall(LLmModelPreset.DesignWriting);
        var callReq = new LLmCallRequest(prompt, request.UserMessage)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancel,
            Title = L("Get Multiple Tasks"),
        };
        var instructions = await call.CallFunction<InstructionInfo>(callReq);

        return instructions;
    }
}
#endregion

#region InstructionInfo
/// <summary>
/// Represents a collection of segmented user instructions decomposed from a complex request.
/// </summary>
public class InstructionInfo
{
    /// <summary>
    /// Gets or sets the list of segmented user instructions.
    /// </summary>
    [Description("Segmented user instructions.")]
    public List<string> Instructions { get; set; } = [];

    /// <summary>
    /// Returns a formatted string representation of all decomposed instructions.
    /// </summary>
    /// <returns>A string listing all instructions, or a message if no tasks were decomposed.</returns>
    public string GetInfoString()
    {
        if (Instructions is null || Instructions.Count == 0)
        {
            return L("No tasks were decomposed.");
        }

        StringBuilder builder = new();
        builder.AppendLine("Decomposed into the following tasks: ");

        for (int i = 0; i < Instructions.Count; i++)
        {
            builder.Append($"{i + 1}. {Instructions[i]}");
        }

        return builder.ToString();
    }
}
#endregion

#region MainAssistantChatProvider
/// <summary>
/// Auto-created chat provider for the <see cref="MainAssistant"/>.
/// </summary>
[AssetAutoCreate]
public class MainAssistantChatProvider : AssistantChatProvider<MainAssistant>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="MainAssistantChatProvider"/>.
    /// </summary>
    public static MainAssistantChatProvider Instance { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainAssistantChatProvider"/> class.
    /// Sets the singleton instance if not already set.
    /// </summary>
    public MainAssistantChatProvider()
    {
        Instance ??= this;
    }
} 
#endregion
