using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.AIGC.Tools;
using Suity.Editor.Documents;
using Suity.Views;
using System.Threading.Tasks;
using System;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// AI intelligent document assistant base class
/// </summary>
public abstract class AIDocumentAssistant : ToolingAssistant, IDocumentAssistant
{
    protected AIDocumentAssistant()
    {
        // Adding tools to the assistant is a formality, actual tools will be called in global UpdateAssistant.

        //AddParameterType<DocumentElementCreateParam>();
        //AddParameterType<DocumentElementCreateSegmentedParam>();
        
        // Updates can include create, update, delete, etc.
        //AddParameterType<DocumentElementUpdateParam>();
    }


    /// <summary>
    /// Handle create flow
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual Task<AICallResult> HandleElementCreate(AIRequest request)
    {
        return Task.FromResult(AICallResult.Empty);
    }

    /// <summary>
    /// Handles batch creation of document elements using multiple generative prompts.
    /// </summary>
    /// <param name="request">The AI request containing context and parameters.</param>
    /// <param name="prompts">The array of generative guiding items to process.</param>
    /// <param name="componentId">Optional component ID to associate with the created elements.</param>
    /// <param name="groupPath">Optional group path for organizing the created elements.</param>
    /// <param name="recordKnowledge">Whether to record knowledge for the created elements.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public virtual Task<AICallResult> HandleBatchCreate(AIRequest request, GenerativeGuidingItem[] prompts, Guid? componentId = null, string groupPath = null, bool recordKnowledge = false)
    {
        return Task.FromResult(AICallResult.Empty);
    }

    /// <summary>
    /// Handle update flow
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="msg"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public virtual Task<AICallResult> HandleElementEdit(AIRequest request)
    {
        return Task.FromResult(AICallResult.Empty);
    }



    /// <summary>
    /// Handles a user operation by classifying it and delegating to the appropriate handler.
    /// </summary>
    /// <param name="request">The AI request containing context and user input.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public virtual async Task<AICallResult> HandleOperation(AIRequest request)
    {
        bool hasSelection = Context?.Selection?.Length > 0;

        request.FuncContext.SetArgument<Document>(Context.TargetDocument);
        request.FuncContext.SetArgument<IDocumentView>(Context.GetCanvasView());

        var op = await AIAssistantService.Instance.ClassifyDocumentOperation(request, hasSelection);

        request.Conversation?.AddSystemMessage(L("Operation type: ") + op.ToDisplayText());

        return op switch
        {
            UserOperationTypes.Create => await HandleElementCreate(request),
            UserOperationTypes.Update => await HandleElementEdit(request),
            UserOperationTypes.Knowledge => await HandleKnowledge(request),
            UserOperationTypes.Get => await HandleGet(request),
            UserOperationTypes.Ask => await HandleAsk(request),
            UserOperationTypes.Unknown => throw new AigcException(L("Unable to recognize user operation.")),
            _ => throw new AigcException(L("Feature not implemented.")),
        };
    }

    /// <summary>
    /// Handles a knowledge request by delegating to the RAG assistant.
    /// </summary>
    /// <param name="request">The AI request containing context and user input.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public virtual Task<AICallResult> HandleKnowledge(AIRequest request)
    {
        // Navigate to RAG assistant
        var ragAssist = AIAssistantService.Instance.CreateRAGAssistant(Context)
            ?? throw new AigcException(L(AIAssistant.ERROR_MSG_ASSISTANT_NOT_FOUND));

        return ragAssist.HandleRequest(request);
    }

    /// <summary>
    /// Handles a get/retrieve request by delegating to the RAG assistant.
    /// </summary>
    /// <param name="request">The AI request containing context and user input.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public virtual async Task<AICallResult> HandleGet(AIRequest request)
    {
        // Navigate to RAG assistant
        var assistant = AIAssistantService.Instance.CreateRAGAssistant(Context)
            ?? throw new AigcException(L(AIAssistant.ERROR_MSG_ASSISTANT_NOT_FOUND));

        return await assistant.HandleGet(request);
    }

    /// <summary>
    /// Handles an ask/question request by delegating to the RAG assistant.
    /// </summary>
    /// <param name="request">The AI request containing context and user input.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public virtual async Task<AICallResult> HandleAsk(AIRequest request)
    {
        // Navigate to RAG assistant
        var assistant = AIAssistantService.Instance.CreateRAGAssistant(Context)
            ?? throw new AigcException(L(AIAssistant.ERROR_MSG_ASSISTANT_NOT_FOUND));

        return await assistant.HandleAsk(request);
    }
}

/// <summary>
/// A placeholder document assistant that throws exceptions when any operation is attempted,
/// indicating that no concrete document assistant has been implemented.
/// </summary>
public class EmptyDocumentAssistant : AIDocumentAssistant
{
    /// <summary>
    /// Throws an exception indicating that the document assistant is not implemented for element creation.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <returns>This method always throws an exception.</returns>
    public override Task<AICallResult> HandleElementCreate(AIRequest request)
    {
        throw new AigcException(L("Document assistant not implemented: ") + Context?.TargetDocument?.Format?.DisplayText);
    }

    /// <summary>
    /// Throws an exception indicating that the document assistant is not implemented for batch creation.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="prompts">The generative guiding items.</param>
    /// <param name="componentId">Optional component ID.</param>
    /// <param name="groupPath">Optional group path.</param>
    /// <param name="recordKnowledge">Whether to record knowledge.</param>
    /// <returns>This method always throws an exception.</returns>
    public override Task<AICallResult> HandleBatchCreate(AIRequest request, GenerativeGuidingItem[] prompts, Guid? componentId = null, string groupPath = null, bool recordKnowledge = false)
    {
        throw new AigcException(L("Document assistant not implemented: ") + Context?.TargetDocument?.Format?.DisplayText);
    }

    /// <summary>
    /// Throws an exception indicating that the document assistant is not implemented for element editing.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <returns>This method always throws an exception.</returns>
    public override Task<AICallResult> HandleElementEdit(AIRequest request)
    {
        throw new AigcException(L("Document assistant not implemented: ") + Context?.TargetDocument?.Format?.DisplayText);
    }
}
