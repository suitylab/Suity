using static Suity.Helpers.GlobalLocalizer;
using ComputerBeacon.Json;
using Suity.Collections;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.AIGC.Tools;
using Suity.Editor.Documents;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Transferring;
using Suity.Helpers;
using Suity.Json;
using Suity.Reflecting;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Suity;
using Suity.Editor.Services;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Backward-compatible implementation of the AI assistant service.
/// </summary>
internal class AIAssistantServiceBK : AIAssistantService
{
    /// <summary>
    /// Gets the singleton instance of the service.
    /// </summary>
    public new static AIAssistantServiceBK Instance { get; } = new();

    private AIAssistantServiceBK()
    {
    }

    /// <summary>
    /// Initializes the service and registers it as the external AI assistant service.
    /// </summary>
    public void Initialize()
    {
        AIAssistantService._external = this;
    }

    #region LLm

    /// <inheritdoc/>
    public override ILLmCall CreateLLmCall(ILLmModel model, LLmModelParameter config = null, IConversationHandler conversation = null, FunctionContext context = null)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        var call = model.CreateCall(config, context);
        if (conversation != null)
        {
            // Wrap the assistant call to display call information when invoked.
            return new WrappedLLmCall(call, LLmModelPreset.Default, conversation);
        }
        else
        {
            return call;
        }
    }

    /// <inheritdoc/>
    public override ILLmCall CreateLLmCall(LLmModelPreset preset, AigcModelLevel level = AigcModelLevel.Default, IConversationHandler conversation = null, FunctionContext context = null)
    {
        var call = AIAssistantPlugin.Instance.CreatePresetCall(preset, level, context)
            ?? throw new AigcException(L("Model not configured: ") + preset.ToDisplayText());

        if (conversation != null)
        {
            // Wrap the assistant call to display call information when invoked.
            return new WrappedLLmCall(call, preset, conversation);
        }
        else
        {
            return call;
        }
    }

    /// <inheritdoc/>
    public override ILLmCall CreateLLmCall(PromptBuilder builder, ILLmModel model, LLmModelParameter config = null, IConversationHandler conversation = null, FunctionContext context = null)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        var call = model.CreateCall(config, context);

        if (conversation != null)
        {
            // Wrap the assistant call to display call information when invoked.
            return new WrappedLLmCall(call, LLmModelPreset.Default, conversation, builder.TemplateName);
        }
        else
        {
            return call;
        }
    }

    /// <inheritdoc/>
    public override ILLmCall CreateLLmCall(PromptBuilder builder, IConversationHandler conversation = null, FunctionContext context = null)
    {
        var record = builder.GetPromptRecord()
            ?? throw new AigcException(L($"Failed to get prompt config from {nameof(PromptBuilder)}."));

        var call = AIAssistantPlugin.Instance.CreatePresetCall(record.ModelPreset, AigcModelLevel.Default, context)
            ?? throw new AigcException(L("Model not configured: ") + record.ModelPreset.ToDisplayText());

        if (conversation != null)
        {
            // Wrap the assistant call to display call information when invoked.
            return new WrappedLLmCall(call, record.ModelPreset, conversation, builder.TemplateName);
        }
        else
        {
            return call;
        }
    }

    /// <inheritdoc/>
    public override ILLmChat CreateAssistantChat(AIAssistant assistant, FunctionContext context)
    {
        if (assistant is null)
        {
            throw new ArgumentNullException(nameof(assistant));
        }

        return new AssistantChat(assistant, context);
    }

    /// <inheritdoc/>
    public override string GetSpeechLanguage()
    {
        return AIAssistantPlugin.Instance.GetSpeechLanguage();
    }

    #endregion

    #region Prompt

    /// <inheritdoc/>
    public override AIPromptRecord GetPromptRecord(string promptId) => AIAssistantPlugin.Instance.GetPrompt(promptId);

    #endregion

    #region Classify

    /// <inheritdoc/>
    /// <inheritdoc/>
    public override async Task<UserMainInputTypes> ClassifyMainInputType(AIRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);
        string sysPrompt = AIAssistantPlugin.Instance.ClassifyConfig.PromptMainClassifier.Text;
        string result = await call.Call(sysPrompt, [request.UserMessage], request.Cancellation);

        if (Enum.TryParse<UserMainInputTypes>(result, out var resultType))
        {
            return resultType;
        }
        else
        {
            request.Conversation?.AddErrorMessage(L("Unrecognized user operation type: ") + result);
            return UserMainInputTypes.Unknown;
        }
    }

    /// <inheritdoc/>
    public override async Task<UserOperationTypes> ClassifyDocumentOperation(AIRequest request, bool hasSelection)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);

        string selPrompt = AIAssistantPlugin.Instance.ClassifyConfig.GetPromptSelection(hasSelection);
        string sysPrompt = selPrompt + AIAssistantPlugin.Instance.ClassifyConfig.PromptDocumentClassifier.Text;

        string result = await call.Call(sysPrompt, [request.UserMessage], request.Cancellation);

        if (Enum.TryParse<UserOperationTypes>(result, out var resultType))
        {
            return resultType;
        }
        else
        {
            request.Conversation?.AddErrorMessage(L("Unrecognized document operation type: ") + result);
            return UserOperationTypes.Unknown;
        }
    }

    /// <inheritdoc/>
    public override async Task<UserOperationTypes> ClassifyRagOperation(AIRequest request, bool hasSelection)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);

        string selPrompt = AIAssistantPlugin.Instance.ClassifyConfig.GetPromptSelection(hasSelection);
        string sysPrompt = selPrompt + AIAssistantPlugin.Instance.ClassifyConfig.PromptKnowledgeClassifier.Text;

        string result = await call.Call(sysPrompt, [request.UserMessage], request.Cancellation);

        if (Enum.TryParse<UserOperationTypes>(result, out var resultType))
        {
            return resultType;
        }
        else
        {
            request.Conversation?.AddErrorMessage(L("Unrecognized RAG operation type: ") + result);
            return UserOperationTypes.Unknown;
        }
    }

    /// <inheritdoc/>
    public override async Task<QueryScopeTypes> ClassifyQueryScope(AIRequest request, bool hasSelection)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);

        string selPrompt = AIAssistantPlugin.Instance.ClassifyConfig.GetPromptSelection(hasSelection);
        string sysPrompt = selPrompt + AIAssistantPlugin.Instance.ClassifyConfig.PromptQueryScopeClassifier.Text;

        string result = await call.Call(sysPrompt, [request.UserMessage], request.Cancellation);

        if (Enum.TryParse<QueryScopeTypes>(result, out var resultType))
        {
            return resultType;
        }
        else
        {
            request.Conversation?.AddErrorMessage(L($"Unrecognized scope type: ") + result);
            return QueryScopeTypes.Unknown;
        }
    }

    /// <inheritdoc/>
    public override async Task<GenerateMultipleTypes> ClassifyGenerateMultipleType(AIRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);

        string sysPrompt = AIAssistantPlugin.Instance.ClassifyConfig.PromptGenerateMultipleClassifier.Text;

        string result = await call.Call(sysPrompt, [request.UserMessage], request.Cancellation);

        if (Enum.TryParse<GenerateMultipleTypes>(result, out var resultType))
        {
            return resultType;
        }
        else
        {
            request.Conversation?.AddErrorMessage(L("Unrecognized multiple type: ") + result);
            return GenerateMultipleTypes.Unknown;
        }
    }

    /// <inheritdoc/>
    public override async Task<GenerateSourceTypes> ClassifyGenerateSourceType(AIRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);

        string sysPrompt = AIAssistantPlugin.Instance.ClassifyConfig.PromptGenerateSourceClassifier.Text;

        string result = await call.Call(sysPrompt, [request.UserMessage], request.Cancellation);

        if (Enum.TryParse<GenerateSourceTypes>(result, out var resultType))
        {
            return resultType;
        }
        else
        {
            request.Conversation?.AddErrorMessage(L("Unrecognized knowledge source type: ") + result);
            return GenerateSourceTypes.Unknown;
        }
    }

    private readonly string[] _targetTypes = ["Structure", "Data Struct", "Data Modelling", "Data", "Fill Data", "Article", "Other"];
    private string _targetTypesStr;

    /// <inheritdoc/>
    public override async Task<OperationTargetTypes> ClassifyTarget(AIRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);

        var sysPrompt = "Please classify the user's operation target objects into one of the following target categories:\r\n\r\n{0}\r\n\r\nPlease return only the category name.";

        if (_targetTypesStr is null)
        {
            var builder = new StringBuilder();
            foreach (var str in _targetTypes)
            {
                builder.AppendLine(str);
            }

            _targetTypesStr = builder.ToString();
        }

        sysPrompt = string.Format(sysPrompt, _targetTypesStr);

        string result = await call.Call(sysPrompt, [request.UserMessage], request.Cancellation);

        return result switch
        {
            "Structure" or "Data Struct" or "Data Modelling" => OperationTargetTypes.Structure,
            "Data" or "Fill Data" => OperationTargetTypes.Data,
            "Article" => OperationTargetTypes.Article,
            _ => OperationTargetTypes.Unknown,
        };
    }

    /// <inheritdoc/>
    public override async Task<float> ClassifyCorrelation(AIRequest request, string source)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Classify);

        string sysPrompt = AIAssistantPlugin.Instance.ClassifyConfig.PromptCorrelationClassifier.Text;

        string result = await call.Call(sysPrompt, [request.UserMessage, source], request.Cancellation);

        result = result?.Trim() ?? string.Empty;

        if (float.TryParse(result, out var c))
        {
            return Mathf.Clamp01(c);
        }
        else
        {
            request.Conversation?.AddErrorMessage(L("Unrecognized correlation value: ") + result);
            return 0;
        }
    }

    #endregion

    #region Subdivide

    /// <inheritdoc/>
    public override async Task<string[]> Segmentation(AIRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Knowledge))
        {
            return [];
        }

        var call = request.CreateLLmCall(LLmModelPreset.DesignWriting);
        string sysPrompt = AIAssistantPlugin.Instance.SubdivideConfig.PromptSegment.Text;

        var callReq = new LLmCallRequest(sysPrompt, request.UserMessage, request.Knowledge)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = L("Text Segmentation"),
        };
        var result = await call.CallFunction<MultipleSection>(callReq);

        return result?.Sections?.ToArray() ?? [];
    }

    /// <inheritdoc/>
    public override async Task<string[]> TaskSubdivision(AIRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.DesignWriting);
        string sysPrompt = AIAssistantPlugin.Instance.SubdivideConfig.PromptTaskSubdivide.Text;

        MultipleTask result;
        var callReq = new LLmCallRequest(sysPrompt, request.UserMessage)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = L("Task Decomposition"),
        };

        if (!string.IsNullOrWhiteSpace(request.Knowledge))
        {
            string knowledgeRef = $"{AIAssistantPlugin.Instance.KnowledgeConfig.PromptRagReference.Text}\r\n\r\n{request.Knowledge}";
            callReq.AppendUserMessage(knowledgeRef);
            result = await call.CallFunction<MultipleTask>(callReq);
        }
        else
        {
            result = await call.CallFunction<MultipleTask>(callReq);
        }

        return result?.Tasks?.ToArray() ?? [];
    }

    /// <inheritdoc/>
    public override async Task<string[]> BrainStorming(AIRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.CreativeToolCalling);
        string sysPrompt = AIAssistantPlugin.Instance.SubdivideConfig.PromptBrainStorming.Text;

        MultipleOption result;
        var callReq = new LLmCallRequest(sysPrompt, request.UserMessage)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = L("Brainstorming"),
        };

        if (!string.IsNullOrWhiteSpace(request.Knowledge))
        {
            string knowledgeRef = $"{AIAssistantPlugin.Instance.KnowledgeConfig.PromptRagReference.Text}\r\n\r\n{request.Knowledge}";
            callReq.AppendUserMessage(knowledgeRef);

            result = await call.CallFunction<MultipleOption>(callReq);
        }
        else
        {
            result = await call.CallFunction<MultipleOption>(callReq);
        }

        return result?.Options?.Select(o => $"{o.Name} - {o.Description}").ToArray() ?? [];
    }

    #endregion

    #region Assistant

    /// <inheritdoc/>
    public override async Task<T> SelectAssistant<T>(AIRequest request, CanvasContext selection)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        List<string> templates = [];
        foreach (var info in AIAssistantInfoManager.Instance.GetAIAssistantInfos<T>())
        {
            string str = $"<Assistant>{info.AssistantType.FullName}</Assistant>\n{info.ToolTips}";
            templates.Add(str);
        }

        var builder = PromptBuilder.FromTemplate("Core.Assistant.SelectAssistant");
        builder.Replace(TAG.FULL_LIST, string.Join("\n\n", string.Join("\n\n", templates)));
        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt, request.UserMessage)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = L("Select AI Assistant"),
        };
        var result = await call.Call(callReq);

        result = result?.Trim() ?? string.Empty;
        if (result.StartsWith("<Assistant>"))
        {
            result = result[11..];
        }
        if (result.EndsWith("</Assistant>"))
        {
            result = result[..^12];
        }

        result = result.Trim('<', '>');

        var selected = AIAssistantInfoManager.Instance.GetAssistantInfo(result);
        if (selected is null)
        {
            request.Conversation.AddErrorMessage(L("No suitable AI assistant found."));
            return null;
        }

        try
        {
            var assistant = (T)selected.AssistantType.CreateInstanceOf();

            if (assistant is AICanvasAssistant canvasAssistant && selection != null)
            {
                canvasAssistant.InitializeCanvas(selection);
            }

            return assistant;
        }
        catch (Exception err)
        {
            request.Conversation.AddException(err, L("Failed to create AI assistant: ") + typeof(T).FullName);
        }

        return null;
    }

    /// <inheritdoc/>
    public override async Task<AssistantCallChain<T>> SelectAssistants<T>(AIRequest request, CanvasContext selection = null)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        string promptFormat = @"
You are an intelligent agent that converts a single user request into a sequential chain of AI assistant calls. 
Your task is to break down the user’s request into a series of steps. For each step, provide:

1. The full name of the assistant to call.
2. The user message to pass to the assistant.

The following is a list of available assistant types:
{0}

<Assistant id='Not-Found'>
User request is not recognized.
</Assistant>

Please select one of the assistant full id based on the user's needs. 
If the user's request is ambiguous, please select the template that is most likely to meet the user's needs.
If the user's request is not in the list, please select 'Not-Found'.
If the user's request is not recognized, please select 'Not-Found'.
";

        List<string> templates = [];
        foreach (var info in AIAssistantInfoManager.Instance.GetAIAssistantInfos<T>())
        {
            string str = $"<Assistant id='{info.AssistantType.FullName}'>\n{info.ToolTips}\n</Assistant>\n";
            templates.Add(str);
        }

        string sysPrompt = string.Format(promptFormat, string.Join("\n\n", templates));

        var call = AIAssistantPlugin.Instance.CreatePresetCall(LLmModelPreset.Classify, ctx: request.FuncContext);
        var callReq = new LLmCallRequest(sysPrompt, request.UserMessage)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = L("Select Assistant"),
        };
        var result = await call.CallFunction<AssistantCallChain>(callReq, o => o.Varify());

        if (result is null || result.Items is null || result.Items.Count == 0 || result.Items.Any(o => o is null))
        {
            request.Conversation.AddErrorMessage(L("No suitable AI assistant found."));
            return null;
        }

        List<AssistantCall<T>> aCalls = [];

        foreach (var item in result.Items)
        {
            string name = item?.AssistantId?.Trim().Trim('\'') ?? string.Empty;
            string msg = item?.CallingMessage;

            var selected = AIAssistantInfoManager.Instance.GetAssistantInfo(name);
            if (selected is null)
            {
                request.Conversation.AddErrorMessage(L("No suitable AI assistant found."));
                return null;
            }

            try
            {
                var assistant = (T)selected.AssistantType.CreateInstanceOf();

                if (assistant is AICanvasAssistant canvasAssistant && selection != null)
                {
                    canvasAssistant.InitializeCanvas(selection);
                }

                var assistantCall = new AssistantCall<T> 
                {
                    Assistant = assistant,
                    CallingMessage = msg,
                };

                aCalls.Add(assistantCall);
            }
            catch (Exception err)
            {
                request.Conversation.AddException(err, L("Failed to create AI assistant: ") + typeof(T).FullName);
                return null;
            }
        }

        var callChain = new AssistantCallChain<T>() 
        {
            Instruction = request.UserMessage,
            Calls = aCalls,
        };

        return callChain;
    }
    
    /// <inheritdoc/>
    public override T CreateCanvasAssistant<T>(CanvasContext selection)
    {
        if (selection is null)
        {
            throw new ArgumentNullException(nameof(selection));
        }

        var assistant = new T();
        assistant.InitializeCanvas(selection);

        return assistant;
    }

    /// <inheritdoc/>
    public override AIDocumentAssistant CreateDocumentAssistant(CanvasContext selection)
    {
        var assistant = AIDocumentAssistantResolver.Instance.CreateDocumentAssistant(selection);

        return assistant;
    }

    /// <inheritdoc/>
    public override AIDocumentAssistant[] CreateDocumentAssistants(CanvasContext selection)
    {
        var assistants = AIDocumentAssistantResolver.Instance.CreateDocumentAssistants(selection);

        return assistants;
    }

    /// <inheritdoc/>
    public override AIDocumentAssistant CreateRAGAssistant(CanvasContext selection)
    {
        var assistant = AIDocumentAssistantResolver.Instance.CreateRAGAssistant(selection);

        return assistant;
    }

    /// <inheritdoc/>
    public override Task<AICallResult> HandleResume(AIRequest request, CanvasContext canvas)
    {
        if (canvas?.Canvas is null)
        {
            throw new AigcException(L("Please open a canvas first before resuming execution."));
        }

        if (canvas.Canvas is not IHasAttributeDesign attr)
        {
            throw new AigcException(L("The current canvas does not support AI assistant."));
        }

        var assistantProvider = attr.GetAttribute<IAssistantProvider>();
        if (assistantProvider is null)
        {
            throw new AigcException(L("The current canvas has no associated AI assistant."));
        }

        var assistant = assistantProvider.CreateAssistant();
        if (assistant is null)
        {
            throw new AigcException(L($"Failed to create AI assistant from {assistantProvider.GetType().Name}."));
        }

        //if (assistant is not AICanvasAssistant canvasAssistant)
        //{
        //    throw new AigcException(L($"AI assistant type {assistant.GetType().Name} is not a subclass of {typeof(AICanvasAssistant)}."));
        //}

        if (assistant is not IRootCreatorAssistant rootAssistant)
        {
            throw new AigcException(L($"AI assistant type {assistant.GetType().Name} is not a subclass of {typeof(IRootCreatorAssistant)}."));
        }

        return rootAssistant.HandleRootResume(request, canvas);
    }

    #endregion

    #region Tool

    /// <inheritdoc/>
    public override async Task<object> SelectToolParameter(IEnumerable<Type> toolParameterTypes, AIRequest request)
    {
        if (toolParameterTypes is null)
        {
            throw new ArgumentNullException(nameof(toolParameterTypes));
        }

        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (!toolParameterTypes.Any())
        {
            return null;
        }

        Type[] toolTypeAry = [.. toolParameterTypes.SkipNull(), typeof(ToolNotFound)];
        if (toolTypeAry.Length == 1)
        {
            return null;
        }

        var builder = PromptBuilder.FromTemplate("Core.Assistant.SelectTool");
        var sysPrompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(sysPrompt, request.UserMessage)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = L("Select Tool"),
        };
        var result = await call.CallFunction(callReq, toolTypeAry, o => o != null);

        if (result is not ToolNotFound)
        {
            return result;
        }

        return result;
    }

    /// <inheritdoc/>
    public override AITool CreateTool(Type parameterType) => AIToolInfoManager.Instance.CreateTool(parameterType);

    /// <inheritdoc/>
    public override AITool<T> CreateTool<T>() => AIToolInfoManager.Instance.CreateTool<T>();

    #endregion

    #region Selection

    /// <inheritdoc/>
    public override ICanvasDocument CreateCanvas(string rFilePath, bool showView = true)
    {
        var docFormat = DocumentManager.Instance.GetDocumentFormat("Canvas");
        var docEntry = docFormat.AutoNewDocument(rFilePath);

        var canvasDoc = docEntry?.Content as ICanvasDocument
            ?? throw new AigcException(L("Failed to create canvas document."));

        if (showView)
        {
            docEntry.ShowView();
        }

        return canvasDoc;
    }

    /// <inheritdoc/>
    public override CanvasContext ResolveCanvasContext()
    {
        var doc = EditorServices.DocumentViewManager.ActiveDocument?.Content;
        if (doc is not ICanvasDocument canvas)
        {
            return null;
        }

        var canvasView = (canvas as Document)?.View;
        var selectedItems = (canvasView as IViewSelectionInfo)?.SelectedObjects?.ToArray() ?? [];
        if (selectedItems.Length == 0)
        {
            return CanvasContext.Create(canvas);
        }

        if (selectedItems.Length != 1)
        {
            throw new AigcException(L("Please select exactly one document in the canvas."));
        }

        var canvasNode = (selectedItems[0] as IFlowDiagramItem)?.Node as CanvasAssetNode;
        var targetDoc = canvasNode?.TargetAsset?.GetDocument(true);
        if (targetDoc is null)
        {
            throw new AigcException(L("Please select exactly one document in the canvas."));
        }

        object[] selection = null;

        var viewNode = canvasNode.GetViewNode(canvasView as IFlowView);
        if (viewNode?.ExpandedView is IViewSelectionInfo selInfo)
        {
            selection = selInfo?.SelectedObjects?.ToArray() ?? [];
        }

        return CanvasContext.Create(canvas, canvasNode, selection);
    }

    /// <inheritdoc/>
    public override void ValidateSelectionCount(object[] selection, int maxSelection)
    {
        if (selection is null || selection.Length == 0)
        {
            throw new AigcException(L("Please select at least 1 object."));
        }

        if (selection.Length > 3)
        {
            throw new AigcException(L($"Please select at most {maxSelection} objects."));
        }
    }

    #endregion

    #region Text

    /// <inheritdoc/>
    public override async Task<string> CreateIdentifier(AIRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Identifier);

        string sysPrompt = @"
Please generate a name according to the user's prompt.
The name should be an in PascalCase English identifier, and can contain multiple English words, without spaces between words.
";

        return await DoRetryAction<string>(request, L("Create Naming Identifier"), async () =>
        {
            var callReq = new LLmCallRequest(sysPrompt, request.UserMessage)
            {
                Conversation = request.Conversation,
                Cancel = request.Cancellation,
                Title = L("Create Naming Identifier"),
            };
            string id = await call.Call(callReq);
            // Some models may have leading or trailing spaces or line breaks
            id = id.Trim();
            if (NamingVerifier.VerifyIdentifier(id))
            {
                request.Conversation?.AddSystemMessage(L("Naming Identifier: ") + id).RemoveOn(3);
                return id;
            }
            else
            {
                return null;
            }
        });
    }

    /// <inheritdoc/>
    public override async Task<string> CreateSummary(AISummaryRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Summary);

        string sysPrompt = AIAssistantPlugin.Instance.SupportConfig.PromptSummary.Text;
        var callReq = new LLmCallRequest(sysPrompt, request.UserMessage, request.Result)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = L("Summary"),
        };
        string resp = await call.Call(callReq);

        request.Conversation?.AddSystemMessage(resp);

        return resp;
    }

    /// <inheritdoc/>
    public override async Task<string> CreateSummaryCompare(AISummaryRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Summary);

        string sysPrompt = @"
You are a summary assistant to generate a brief comparison summary of the two texts.
Please export the changes between the two texts in a concise and clear manner.
";
        if (!string.IsNullOrWhiteSpace(request.SpeechLanguage))
        {
            sysPrompt += $"The spoken language is {request.SpeechLanguage}.";
        }

        //AIAssistantPlugin.Instance.SupportConfig.PromptSummary.Text;

        call.NewMessage();
        call.AppendSystemMessage(sysPrompt);
        call.AppendUserMessage(request.UserMessage);
        call.AppendUserMessage("Text before: \r\n\r\n" + request.Before);
        call.AppendUserMessage("Text after: \r\n\r\n" + request.Result);

        string resp = await call.Call(request.Cancellation);

        request.Conversation?.AddSystemMessage(resp);

        return resp;
    }

    /// <inheritdoc/>
    public override async Task<string> CreateSummaryPartialUpdate(AISummaryRequest request)
    {
        var call = request.CreateLLmCall(LLmModelPreset.Summary);

        string sysPrompt = @"
You are a summary assistant to generate a update brief summary.
You are given the original text and the partial updated text.
Please export the updated brief summary in a concise and clear manner.
";

        if (!string.IsNullOrWhiteSpace(request.SpeechLanguage))
        {
            sysPrompt += $"The spoken language is {request.SpeechLanguage}.";
        }

        call.NewMessage();
        call.AppendSystemMessage(sysPrompt);
        call.AppendUserMessage(request.UserMessage);
        call.AppendUserMessage("Text origin: \r\n\r\n" + request.Before);
        call.AppendUserMessage("Text partial updated: \r\n\r\n" + request.Result);

        string resp = await call.Call(request.Cancellation);

        request.Conversation?.AddSystemMessage(resp);

        return resp;
    }

    #endregion

    #region Try

    /// <inheritdoc/>
    public override Task<T> DoRetryAction<T>(string title, Func<Task<T>> task, bool acceptNull = false, int? retry = null,
        IConversationHandler conversation = null, CancellationToken cancel = default) where T : class
    {
        return RetryHelper.DoRetryAction(title, task, acceptNull, retry, conversation, cancel);
    }

    #endregion

    /// <inheritdoc/>
    public override async Task<string> RepairJson(AIRequest request, string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            request.Conversation?.AddErrorMessage(L("Json has no content."));
            return string.Empty;
        }

        using var recoverMsg = request.Conversation?.AddWarningMessage(L("Json parsing failed, attempting to fix..."));

        string sysPrompt = @"
You are a JSON syntax checker who focuses on identifying syntax defects in JSON documents.
Attempt to fix these defects, and output the repaired JSON document.
 ";

        var call = request.CreateLLmCall(LLmModelPreset.CodeRepair);
        string fix = await call.Call(sysPrompt, [json], request.Cancellation);

        return fix;
    }

    /// <inheritdoc/>
    public override async Task<JsonObject> ResolveAndRepairJson(AIRequest request, string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            request.Conversation?.AddErrorMessage(L("Json has no content."));
            return null;
        }

        if (EditorServices.JsonResource.TryExtractJson(json, out var obj))
        {
            return obj;
        }

        using var recoverMsg = request.Conversation?.AddWarningMessage(L("Json parsing failed, attempting to fix..."));

        string sysPrompt = @"
You are a JSON syntax checker who focuses on identifying syntax defects in JSON documents.
Attempt to fix these defects, and output the repaired JSON document.
";

        var call = request.CreateLLmCall(LLmModelPreset.CodeRepair);
        string fix = await call.Call(sysPrompt, [json], request.Cancellation);

        if (EditorServices.JsonResource.TryExtractJson(fix, out obj))
        {
            return obj;
        }

        request.Conversation?.AddErrorMessage(L("Unable to fix this json document."), finishDialog =>
        {
            finishDialog.AddButton(L("Copy Json"), () =>
            {   
                EditorUtility.SetSystemClipboardText(json);
            });
        });

        throw new AigcException(L("Json fix failed."));
    }

    /// <inheritdoc/>
    public override async Task ApplyTarget(AIRequest request, IDocumentView docView, Document doc, object json)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        var finishMsg = request.Conversation?.AddSystemMessage(L("Generation completed and will be applied to document..."), finishDialog =>
        {
            finishDialog.AddButton(L("Copy Json"), () =>
            {
                EditorUtility.SetSystemClipboardText(json?.ToString() ?? string.Empty);
            });
        });

        JsonObject obj;
        if (json is JsonObject jobj)
        {
            obj = jobj;
        }
        else if (json is string str)
        {
            obj = await ResolveAndRepairJson(request, str);
        }
        else
        {
            throw new AigcException(L("Unable to get Json information."));
        }

        var reader = new JsonDataReader(obj);

        if (docView?.GetService<UndoRedoManager>() is { } undo)
        {
            var action = new AssistantAction(docView, doc, reader);
            undo.Do(action);
        }
        else
        {
            var transfer = DataRW.GetTransfer(doc.GetType())
                ?? throw new AigcException(L("Document does not support data transfer operation."));

            transfer.Input(doc, new DataRW { Reader = reader }, true);
        }

        doc.MarkDirty(this);

        // If document has no view, auto save
        // Note: docView may not be the view of the current document, it may be a canvas view
        if (doc.View is null)
        {
            doc.Save();
        }
    }
}