using ComputerBeacon.Json;
using Suity.Collections;
using Suity.Editor.AIGC.RAG;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Transferring;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Json;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Provides extension methods for AI assistant operations including document processing,
/// knowledge management, LLM calls, conversation handling, and data selection.
/// </summary>
public static class AIAssistantExtensions
{
    #region Complex Type

    /// <summary>
    /// Determines whether the specified field supports AI generation.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <returns>True if the field supports AI generation; otherwise, false.</returns>
    public static bool SupportGeneration(this DStructField field)
    {
        if (field is null)
        {
            return false;
        }

        if (field.IsHiddenOrDisabled)
        {
            return false;
        }

        if (field.IsConnector)
        {
            return false;
        }

        if (field.AutoFieldType.HasValue)
        {
            return false;
        }

        if (field.ContainsAttribute<SkipAIGenerationAttribute>())
        {
            return false;
        }

        var dtype = field.FieldType?.OriginType?.Target;
        if (dtype?.ContainsAttribute<SkipAIGenerationAttribute>() == true)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified field supports simple (non-complex) AI generation.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <returns>True if the field supports simple AI generation; otherwise, false.</returns>
    public static bool SupportSimpleGeneration(this DStructField field)
    {
        if (!field.SupportGeneration())
        {
            return false;
        }

        return !field.FieldType.IsComplexType();
    }

    /// <summary>
    /// Determines whether the specified field supports complex AI generation.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <returns>True if the field supports complex AI generation; otherwise, false.</returns>
    public static bool SupportComplexGeneration(this DStructField field)
    {
        if (!field.SupportGeneration())
        {
            return false;
        }

        return field.FieldType.IsComplexType();
    }

    /// <summary>
    /// Determines whether the specified type is a complex type (data link, struct, abstract, or asset link).
    /// </summary>
    /// <param name="type">The type definition to check.</param>
    /// <returns>True if the type is a complex type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsComplexType(this TypeDefinition type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsDataLink || (type.IsArray && type.ElementType.IsDataLink))
        {
            return true;
        }

        if (type.IsStruct || (type.IsArray && type.ElementType.IsStruct))
        {
            return true;
        }

        if (type.IsAbstract || (type.IsArray && type.ElementType.IsAbstract))
        {
            return true;
        }

        if (type.IsAssetLink || (type.IsArray && type.ElementType.IsAssetLink))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified compound object contains any complex type fields.
    /// </summary>
    /// <param name="dcompond">The compound object to check.</param>
    /// <returns>True if the compound contains complex types; otherwise, false.</returns>
    public static bool ContainsComplexType(this DCompond dcompond)
    {
        return dcompond.PublicStructFields.Any(o => o.FieldType.IsComplexType());
    }

    /// <summary>
    /// Determines whether the specified type definition contains any complex type fields.
    /// </summary>
    /// <param name="type">The type definition to check.</param>
    /// <returns>True if the type contains complex types; otherwise, false.</returns>
    public static bool ContainsComplexType(this TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return false;
        }

        if (type.Target is DCompond dcompond)
        {
            return dcompond.PublicStructFields.Any(o => o.FieldType.IsComplexType());
        }

        return false;
    }

    #endregion

    #region Request

    /// <summary>
    /// Creates a new AI request with the specified user message.
    /// </summary>
    /// <param name="origin">The original AI request to base the new request on.</param>
    /// <param name="message">The user message to set.</param>
    /// <param name="increaseDepth">Whether to increase the request depth.</param>
    /// <returns>A new AI request with the specified message.</returns>
    public static AIRequest CreateWithMessage(this AIRequest origin, string message, bool increaseDepth = false)
    {
        return new AIRequest(origin, increaseDepth)
        {
            UserMessage = message
        };
    }

    /// <summary>
    /// Creates a new AI request with the specified user message and knowledge.
    /// </summary>
    /// <param name="origin">The original AI request to base the new request on.</param>
    /// <param name="message">The user message to set.</param>
    /// <param name="knowledge">The knowledge to include.</param>
    /// <param name="increaseDepth">Whether to increase the request depth.</param>
    /// <returns>A new AI request with the specified message and knowledge.</returns>
    public static AIRequest CreateWithMessage(this AIRequest origin, string message, string knowledge, bool increaseDepth = false)
    {
        return new AIRequest(origin, increaseDepth)
        {
            UserMessage = message,
            Knowledge = knowledge
        };
    }

    /// <summary>
    /// Creates a new AI request with the specified knowledge.
    /// </summary>
    /// <param name="origin">The original AI request to base the new request on.</param>
    /// <param name="knowledge">The knowledge to include.</param>
    /// <param name="increaseDepth">Whether to increase the request depth.</param>
    /// <returns>A new AI request with the specified knowledge.</returns>
    public static AIRequest CreateWithKnowledge(this AIRequest origin, string knowledge, bool increaseDepth = false)
    {
        return new AIRequest(origin, increaseDepth)
        {
            Knowledge = knowledge
        };
    }
    #endregion

    #region LLm

    /// <summary>
    /// Creates an LLM call instance using the specified preset model and configuration.
    /// </summary>
    /// <param name="presetType">The LLM model preset type.</param>
    /// <param name="level">The AI model level. Defaults to Default.</param>
    /// <param name="conversation">The conversation handler, or null.</param>
    /// <param name="context">The function context, or null.</param>
    /// <returns>An LLM call instance.</returns>
    public static ILLmCall CreateLLmCall(this LLmModelPreset presetType, AigcModelLevel level = AigcModelLevel.Default, IConversationHandler conversation = null, FunctionContext context = null)
        => AIAssistantService.Instance.CreateLLmCall(presetType, level, conversation, context);

    /// <summary>
    /// Creates an LLM call instance from an AI request using the specified preset model.
    /// </summary>
    /// <param name="request">The AI request containing model and conversation settings.</param>
    /// <param name="presetType">The LLM model preset type to use if no custom model is configured.</param>
    /// <param name="level">The AI model level. Defaults to Default.</param>
    /// <returns>An LLM call instance.</returns>
    /// <exception cref="AigcException">Thrown when the custom LLM model is not configured.</exception>
    public static ILLmCall CreateLLmCall(this AIRequest request, LLmModelPreset presetType, AigcModelLevel level = AigcModelLevel.Default)
    {
        if (request.CustomLLmModel is { } modelSetting && modelSetting.IsOptional)
        {
            var model = modelSetting.GetModel()
                ?? throw new AigcException("LLm Model is not configured.");

            return AIAssistantService.Instance.CreateLLmCall(model, modelSetting.Parameters, request.Conversation, request.FuncContext);
        }
        else
        {
            return AIAssistantService.Instance.CreateLLmCall(presetType, level, request.Conversation, request.FuncContext);
        }
    }

    /// <summary>
    /// Creates an LLM call instance from an AI request using the specified prompt builder.
    /// </summary>
    /// <param name="request">The AI request containing model and conversation settings.</param>
    /// <param name="builder">The prompt builder to use for the LLM call.</param>
    /// <returns>An LLM call instance.</returns>
    /// <exception cref="AigcException">Thrown when the custom LLM model is not configured.</exception>
    public static ILLmCall CreateLLmCall(this AIRequest request, PromptBuilder builder)
    {
        if (request.CustomLLmModel is { } modelSetting && modelSetting.IsOptional)
        {
            var model = modelSetting.GetModel()
                ?? throw new AigcException("LLm Model is not configured.");

            return AIAssistantService.Instance.CreateLLmCall(builder, model, modelSetting.Parameters, request.Conversation, request.FuncContext);
        }
        else
        {
            return AIAssistantService.Instance.CreateLLmCall(builder, request.Conversation, request.FuncContext);
        }
    }

    #endregion

    #region Subdivide

    /// <summary>
    /// Subdivides the user requirement into smaller, manageable tasks.
    /// </summary>
    /// <param name="request">The AI request containing the requirement to subdivide.</param>
    /// <returns>A task that represents the asynchronous operation, containing an array of subdivided task strings.</returns>
    public static Task<string[]> SubdivideUserRequirement(this AIRequest request)
        => AIAssistantService.Instance.TaskSubdivision(request);

    /// <summary>
    /// Performs brainstorming on the user requirement to generate creative ideas.
    /// </summary>
    /// <param name="request">The AI request containing the requirement to brainstorm.</param>
    /// <returns>A task that represents the asynchronous operation, containing an array of brainstormed idea strings.</returns>
    public static Task<string[]> BrainStormingUserRequirement(this AIRequest request)
        => AIAssistantService.Instance.BrainStorming(request);

    #endregion

    #region Text

    /// <summary>
    /// Creates a unique identifier using AI.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated identifier string.</returns>
    public static Task<string> CreateIdentifier(this AIRequest request) => AIAssistantService.Instance.CreateIdentifier(request);

    /// <summary>
    /// Creates a summary of the specified result using AI.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="result">The result to summarize.</param>
    /// <returns>A task that represents the asynchronous operation, containing the summary string.</returns>
    public static Task<string> CreateSummary(this AIRequest request, string result)
    {
        var request2 = new AISummaryRequest(request)
        {
            Result = result,
        };

        return AIAssistantService.Instance.CreateSummary(request2);
    }

    /// <summary>
    /// Creates a comparative summary between the current and previous results using AI.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="msg">The message context for the summary.</param>
    /// <param name="result">The current result to summarize.</param>
    /// <param name="before">The previous result to compare against.</param>
    /// <returns>A task that represents the asynchronous operation, containing the comparative summary string.</returns>
    public static Task<string> CreateSummaryCompare(this AIRequest request, string msg, string result, string before)
    {
        var request2 = new AISummaryRequest(request)
        {
            Result = result,
            Before = before,
        };

        return AIAssistantService.Instance.CreateSummaryCompare(request2);
    }

    #endregion

    #region Json

    /// <summary>
    /// Gets the JSON string representation of the specified items in the document.
    /// </summary>
    /// <param name="doc">The document containing the items.</param>
    /// <param name="items">The items to serialize to JSON.</param>
    /// <returns>The JSON string representation of the items.</returns>
    /// <exception cref="AigcException">Thrown when the document does not support data transfer operation.</exception>
    public static string GetDocumentJsonString(this Document doc, object[] items)
    {
        var transfer = DataRW.GetTransfer(doc.GetType())
            ?? throw new AigcException(L("Document does not support data transfer operation."));

        var writer = new JsonDataWriter();
        transfer.Output(doc, new DataRW { Writer = writer }, items);
        var json = writer.ToString();

        return json;
    }

    /// <summary>
    /// Gets the JSON string representation of the selected items in the canvas context.
    /// </summary>
    /// <param name="selection">The canvas context containing the target document and selection.</param>
    /// <returns>The JSON string representation of the selected items.</returns>
    public static string GetDocumentJsonString(this CanvasContext selection)
        => GetDocumentJsonString(selection.TargetDocument, selection.Selection);

    /// <summary>
    /// Get JSON string of document
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="AigcException"></exception>
    public static string ResolveItemJsonString(object item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var transfer = DataRW.GetTransfer(item.GetType())
            ?? throw new AigcException(L("Document does not support data transfer operation."));

        var writer = new JsonDataWriter();
        transfer.Output(item, new DataRW { Writer = writer });
        var json = writer.ToString();

        return json;
    }

    /// <summary>
    /// Gets a string representation of the item's information, either as JSON or plain text.
    /// </summary>
    /// <param name="item">The item to get information for. Can be null.</param>
    /// <returns>A string representation of the item, or null if the item is null.</returns>
    public static string GetItemInfoString(object item)
    {
        if (item is null)
        {
            return null;
        }

        if (item is string str)
        {
            return str;
        }

        if (item is Asset asset)
        {
            item = asset.GetStorageObject(true);
        }

        var transfer = DataRW.GetTransfer(item.GetType());
        if (transfer is null)
        {
            return item.ToString();
        }

        var writer = new JsonDataWriter();
        transfer.Output(item, new DataRW { Writer = writer });
        var json = writer.ToString();

        return json;
    }

    /// <summary>
    /// Resolves and repairs invalid JSON using AI.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="json">The JSON string to resolve and repair.</param>
    /// <returns>A task that represents the asynchronous operation, containing the resolved JSON object.</returns>
    public static Task<JsonObject> ResolveAndRepairJson(this AIRequest request, string json)
        => AIAssistantService.Instance.ResolveAndRepairJson(request, json);

    #endregion

    #region Knowledge

    /// <summary>
    /// Creates query keywords from the AI request for knowledge base searching.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated query keywords string.</returns>
    public static Task<string> CreateQueryKeywords(this AIRequest request) => RAGService.Instance.CreateQueryKeywords(request);

    /// <summary>
    /// Create knowledge base index
    /// </summary>
    /// <param name="row"></param>
    /// <param name="requirement"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static Task IndexRAG(this AIRequest request, Document document, object item, string content, string sourceTag, string sourceHash = null)
    {
        return RAGService.Instance.IndexItem(request, document, item, content, sourceTag, sourceHash);
    }

    #endregion

    #region Knowledge Input

    /// <summary>
    /// Get knowledge input from Canvas connections
    /// </summary>
    /// <returns></returns>
    public static IKnowledgeBase[] GetKnowledgeInputs(this AIDocumentAssistant assistant)
    {
        return (assistant.Context?.CanvasNode as ICanvasSwitchableNode)?.GetKnowledgeInputs() ?? [];
    }

    /// <summary>
    /// Gets the knowledge inputs from the canvas context's switchable node.
    /// </summary>
    /// <param name="selection">The canvas context to get knowledge inputs from.</param>
    /// <returns>An array of knowledge bases, or an empty array if none are available.</returns>
    public static IKnowledgeBase[] GetKnowledgeInputs(this CanvasContext selection)
    {
        return (selection?.CanvasNode as ICanvasSwitchableNode)?.GetKnowledgeInputs() ?? [];
    }

    /// <summary>
    /// Get input knowledge
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static async Task<string> QueryInputVecKnowledge(this CanvasContext selection, AIRequest request)
    {
        if (selection.GetKnowledgeInputs().OfType<IVectorKnowledge>().FirstOrDefault() is not { } vec)
        {
            return null;
        }

        request.Conversation.AddSystemMessage(L("Obtained input knowledge base: ") + vec);

        string knowledge = await request.QueryKnowledge(vec, false);
        if (string.IsNullOrWhiteSpace(knowledge))
        {
            knowledge = null;
        }

        return knowledge;
    }

    /// <summary>
    /// Enumerate features from input feature library according to the prompt
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static async Task<FeatureQueryResult[]> EnumerateInputFeatures(this AIRequest request, CanvasContext selection)
    {
        if (selection.GetKnowledgeInputs().OfType<IFeatureKnowledge>().FirstOrDefault() is not { } ft)
        {
            return [];
        }

        request.Conversation.AddSystemMessage(L("Obtained input knowledge base: ") + ft);

        var query = await request.CreateQueryKeywords();
        return await ft.EnumerateFeature(query, request.TopK, request.Cancel);
    }

    /// <summary>
    /// Get feature from input feature library
    /// </summary>
    /// <param name="tag">Feature Tag</param>
    /// <returns></returns>
    public static Task<FeatureQueryResult> GetInputFeature(this CanvasContext selection, string tag)
    {
        if (selection.GetKnowledgeInputs().OfType<IFeatureKnowledge>().FirstOrDefault() is not { } ft)
        {
            return Task.FromResult<FeatureQueryResult>(null);
        }

        return ft.GetFeature(tag);
    }

    /// <summary>
    /// Get feature corresponding to document in input feature library
    /// </summary>
    /// <param name="id">Document Object Id</param>
    /// <returns></returns>
    public static Task<FeatureQueryResult> GetInputFeatureById(this CanvasContext selection, Guid id)
    {
        return selection.GetInputFeatureById(id.ToString());
    }

    /// <summary>
    /// Get feature corresponding to document in input feature library
    /// </summary>
    /// <param name="id">Document Object Id</param>
    /// <returns></returns>
    public static Task<FeatureQueryResult> GetInputFeatureById(this CanvasContext selection, string id)
    {
        if (RAGService.Instance.TryGetSourceTag(selection.TargetDocument, id, out string sourceTag, out string sourceHash))
        {
            return selection.GetInputFeature(sourceTag);
        }

        return Task.FromResult<FeatureQueryResult>(null);
    }

    /// <summary>
    /// Get feature knowledge in input feature library
    /// </summary>
    /// <param name="id">Document Object Id</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static Task<FeatureQueryResult> QueryInputFeatureKnowledgeById(this CanvasContext selection, AIRequest request, Guid id)
    {
        return selection.QueryInputFeatureKnowledgeById(request, id.ToString());
    }

    /// <summary>
    /// Get feature knowledge in input feature library
    /// </summary>
    /// <param name="id">Document Object Id</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static async Task<FeatureQueryResult> QueryInputFeatureKnowledgeById(this CanvasContext selection, AIRequest request, string id)
    {
        var feature = await selection.GetInputFeatureById(id);
        if (feature is null)
        {
            return null;
        }

        feature.Knowledge = await selection.QueryInputFeatureKnowledge(request, feature.Name);
        if (string.IsNullOrWhiteSpace(feature.Knowledge))
        {
            feature.Knowledge = null;
        }

        return feature;
    }

    /// <summary>
    /// Get feature knowledge in input feature library
    /// </summary>
    /// <param name="featureName">Feature Name</param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static Task<string> QueryInputFeatureKnowledge(this CanvasContext selection, AIRequest request, string featureName)
    {
        if (selection.GetKnowledgeInputs().OfType<IFeatureKnowledge>().FirstOrDefault() is not { } ft)
        {
            return null;
        }

        return ft.QueryFeatureKnowledge(request, featureName);
    }

    #endregion

    #region Retry action

    /// <summary>
    /// Executes a task with retry logic managed by the AI assistant service.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="request">The AI request.</param>
    /// <param name="title">The title for the retry action.</param>
    /// <param name="task">The task function to execute.</param>
    /// <param name="acceptNull">Whether to accept null results. Defaults to false.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of type T.</returns>
    public static Task<T> DoRetryAction<T>(this AIRequest request, string title, Func<Task<T>> task, bool acceptNull = false) where T : class
    {
        return AIAssistantService.Instance.DoRetryAction<T>(request, title, task);
    }

    #endregion

    #region Document Action

    /// <summary>
    /// Executes a canvas action, either through the undo/redo manager or directly, and marks the document as dirty for delayed save.
    /// </summary>
    /// <param name="canvas">The canvas context to perform the action on.</param>
    /// <param name="action">The undo/redo action to execute.</param>
    public static void DoCanvasAction(this CanvasContext canvas, UndoRedoAction action)
    {
        if (canvas.GetCanvasView()?.GetService<UndoRedoManager>() is { } undo)
        {
            undo.Do(action);
        }
        else
        {
            action.Do();
        }

        var doc = canvas.TargetDocument;

        doc?.MarkDirtyAndSaveDelayed(canvas);
    }

    /// <summary>
    /// Applies the AI-generated JSON data to the target document in the canvas context.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="selection">The canvas context containing the target document.</param>
    /// <param name="json">The JSON data to apply to the document.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ApplyToDocument(this AIRequest request, CanvasContext selection, object json)
    {
        await AIAssistantService.Instance.ApplyTarget(request, selection.GetCanvasView(), selection.TargetDocument, json);
    }

    #endregion

    #region Attribute

    /// <summary>
    /// Determines whether the specified data usage mode supports AI generation.
    /// </summary>
    /// <param name="usage">The data usage mode to check.</param>
    /// <returns>True if the usage mode supports AI generation; otherwise, false.</returns>
    public static bool GetIsAIGeneration(this DataUsageMode usage)
    {
        switch (usage)
        {
            case DataUsageMode.DataGrid:
            case DataUsageMode.FlowGraph:
            case DataUsageMode.TreeGraph:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether the specified attribute owner supports AI generation based on its data usage attribute.
    /// </summary>
    /// <param name="owner">The attribute getter to check.</param>
    /// <returns>True if the owner supports AI generation; otherwise, false.</returns>
    public static bool GetIsAIGeneration(this IAttributeGetter owner)
    {
        if (owner.GetAttribute<DataUsageAttribute>() is not { } dataUsage)
        {
            return false;
        }

        return dataUsage.Usage.GetIsAIGeneration();
    }

    /// <summary>
    /// Determines whether the specified attribute owner supports active AI generation.
    /// </summary>
    /// <param name="owner">The attribute getter to check.</param>
    /// <returns>True if the owner supports active AI generation; otherwise, false.</returns>
    public static bool GetIsAIActiveGeneration(this IAttributeGetter owner)
    {
        if (!owner.GetIsAIGeneration())
        {
            return false;
        }

        return owner.GetDataDrivenMode() == DataDrivenMode.Active;
    }

    /// <summary>
    /// Determines whether the specified attribute owner supports passive AI generation.
    /// </summary>
    /// <param name="owner">The attribute getter to check.</param>
    /// <returns>True if the owner supports passive AI generation; otherwise, false.</returns>
    public static bool GetIsAIPassiveGeneration(this IAttributeGetter owner)
    {
        var mode = owner.GetDataDrivenMode();

        return mode == DataDrivenMode.Unique || mode == DataDrivenMode.Shared;
    }

    #endregion

    #region DataSelection

    /// <summary>
    /// Converts the data item to an XML tag string with name, localized description, and tooltips.
    /// </summary>
    /// <param name="row">The data item to convert.</param>
    /// <param name="useFullId">Whether to use the full data ID or just the name. Defaults to true.</param>
    /// <returns>An XML tag string representing the data item.</returns>
    public static string ToTag(this IDataItem row, bool useFullId = true)
    {
        string id = useFullId ? row.ToDataId() : row.Name;

        string localizedName = !string.IsNullOrWhiteSpace(row.Description) ? $" local='{row.Description}'" : "";
        string toolTips = (row as IAttributeGetter)?.GetAttribute<ToolTipsAttribute>()?.ToolTips ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(toolTips))
        {
            toolTips = "\n" + toolTips + "\n";
        }

        string tag = $"<data name='{id}'{localizedName}>{toolTips}</data>";

        return tag;
    }

    /// <summary>
    /// Adds the specified GUIDs to the data selection memory for the given type.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="type">The data type to associate the selections with.</param>
    /// <param name="ids">The GUIDs to add to the selection memory.</param>
    public static void AddToDataSelectionMemory(this AIRequest request, DType type, IEnumerable<Guid> ids)
    {
        if (ids is null || !ids.Any())
        {
            return;
        }

        var record = request.GetOrAddMemory<LinkedDataSelectionMemory>().GetOrAddList(type);

        foreach (var id in ids)
        {
            record.Add(id);
        }
    }

    /// <summary>
    /// Adds the specified data IDs to the data selection memory for the given type.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="type">The data type to associate the selections with.</param>
    /// <param name="dataIds">The data IDs to add to the selection memory.</param>
    public static void AddToDataSelectionMemory(this AIRequest request, DType type, IEnumerable<string> dataIds)
    {
        if (dataIds is null || !dataIds.Any())
        {
            return;
        }

        var record = request.GetOrAddMemory<LinkedDataSelectionMemory>().GetOrAddList(type);

        foreach (var dataId in dataIds)
        {
            record.Add(dataId);
        }
    }

    /// <summary>
    /// Adds the specified data items to the data selection memory for the given type.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="type">The data type to associate the selections with.</param>
    /// <param name="rows">The data items to add to the selection memory.</param>
    public static void AddToDataSelectionMemory(this AIRequest request, DType type, IEnumerable<IDataItem> rows)
    {
        if (rows is null || !rows.Any())
        {
            return;
        }

        var record = request.GetOrAddMemory<LinkedDataSelectionMemory>().GetOrAddList(type);

        foreach (var row in rows.SkipNull())
        {
            record.Add(row);
        }
    }

    /// <summary>
    /// Adds the specified keys to the data selection memory based on their input types.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="keys">The keys to add to the selection memory.</param>
    public static void AddToDataSelectionMemory(this AIRequest request, IEnumerable<SKey> keys)
    {
        if (keys is null || !keys.Any())
        {
            return;
        }

        foreach (var key in keys.SkipNull())
        {
            var type = key.InputType?.OriginType?.Target;
            if (type is null)
            {
                continue;
            }

            var record = request.GetOrAddMemory<LinkedDataSelectionMemory>().GetOrAddList(type);
            record.Add(key.TargetId);
        }
    }

    /// <summary>
    /// Gets the data items currently stored in the selection memory for the specified type.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="type">The data type to retrieve selections for.</param>
    /// <returns>An enumerable of data items, or an empty collection if none are stored.</returns>
    public static IEnumerable<IDataItem> GetDataSelectionInMemory(this AIRequest request, DType type)
    {
        var record = request.GetMemory<LinkedDataSelectionMemory>()?.GetList(type);
        return record?.GetDataRows() ?? [];
    }

    /// <summary>
    /// Gets the selection records currently stored in the selection memory for the specified type.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="type">The data type to retrieve selection records for.</param>
    /// <returns>An enumerable of selection records, or an empty collection if none are stored.</returns>
    public static IEnumerable<LinkedDataSelectionRecord> GetDataSelectionRecordInMemory(this AIRequest request, DType type)
    {
        var record = request.GetMemory<LinkedDataSelectionMemory>()?.GetList(type);
        return record?.Selections ?? [];
    }

    #endregion

    #region Consistency

    /// <summary>
    /// Sets a consistency value for a field based on the sync object context.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="field">The field to set the consistency value for.</param>
    /// <param name="obj">The sync object providing context.</param>
    /// <param name="value">The value to set.</param>
    public static void SetConsistencyValue(this AIRequest request, DField field, SObject obj, SItem value)
    {
        if (obj.Context is IDataItem row)
        {
            SetConsistencyValue(request, field, row, value);
        }
        else
        {
            var key = obj.GetSyncPath().ToString();
            SetConsistencyValue(request, field, key, value);
        }
    }

    /// <summary>
    /// Sets a consistency value for a field based on the data item.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="field">The field to set the consistency value for.</param>
    /// <param name="row">The data item to use as context.</param>
    /// <param name="value">The value to set.</param>
    public static void SetConsistencyValue(this AIRequest request, DField field, IDataItem row, SItem value)
    {
        var key = row.GetConsistencyKey();
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var record = request.GetOrAddMemory<ConsistencyValueMemory>().GetOrAddFieldRecord(field);

        record.SetValue(key, value);
    }

    /// <summary>
    /// Sets a consistency value for a field based on a string key.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="field">The field to set the consistency value for.</param>
    /// <param name="key">The key to associate the value with.</param>
    /// <param name="value">The value to set.</param>
    public static void SetConsistencyValue(this AIRequest request, DField field, string key, SItem value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var record = request.GetOrAddMemory<ConsistencyValueMemory>().GetOrAddFieldRecord(field);

        record.SetValue(key, value);
    }

    /// <summary>
    /// Gets the consistency value for a field based on the sync object context.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="field">The field to get the consistency value for.</param>
    /// <param name="obj">The sync object providing context.</param>
    /// <returns>The consistency value, or null if not found.</returns>
    public static SItem GetConsistencyValue(this AIRequest request, DField field, SObject obj)
    {
        if (obj.Context is IDataItem row)
        {
            return GetConsistencyValue(request, field, row);
        }
        else
        {
            var key = obj.GetSyncPath().ToString();
            return GetConsistencyValue(request, field, key);
        }
    }

    /// <summary>
    /// Gets the consistency value for a field based on the data item.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="field">The field to get the consistency value for.</param>
    /// <param name="row">The data item to use as context.</param>
    /// <returns>The consistency value, or null if not found.</returns>
    public static SItem GetConsistencyValue(this AIRequest request, DField field, IDataItem row)
    {
        var key = row.GetConsistencyKey();
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return request.GetConsistencyValue(field, key);
    }

    /// <summary>
    /// Gets the consistency value for a field based on a string key.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="field">The field to get the consistency value for.</param>
    /// <param name="key">The key to look up the value.</param>
    /// <returns>The consistency value, or null if not found.</returns>
    public static SItem GetConsistencyValue(this AIRequest request, DField field, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var record = request.GetMemory<ConsistencyValueMemory>()?.GetFieldRecord(field);

        return record?.GetValue(key);
    }

    /// <summary>
    /// Gets the consistency key for a data item, using group path, document asset key, or data ID.
    /// </summary>
    /// <param name="row">The data item to get the consistency key for.</param>
    /// <returns>The consistency key string.</returns>
    public static string GetConsistencyKey(this IDataItem row)
    {
        if (row is SNamedItem namedItem)
        {
            if (namedItem.GetGroupPath() is string groupPath && !string.IsNullOrWhiteSpace(groupPath))
            {
                return groupPath;
            }
            else if (namedItem.GetDocument() is { } doc)
            {
                return doc.AssetKey;
            }
        }

        return row.ToDataId();
    }

    #endregion

    #region Conversation

    /// <summary>
    /// Adds a system message to the conversation and waits for text input from the user.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="message">The system message to display.</param>
    /// <returns>A task that represents the asynchronous operation, containing the user's text input.</returns>
    public static async Task<string> ConversationInput(this AIRequest request, string message)
    {
        request.Conversation.AddSystemMessage(message);
        string prompt = await request.Conversation.WaitForTextInput(request.Cancel);

        return prompt;
    }

    /// <summary>
    /// Adds a system message with a button to the conversation and waits for text or button input.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="message">The system message to display.</param>
    /// <param name="button">The button text to display.</param>
    /// <returns>A task that represents the asynchronous operation, containing the user's input (text or button key).</returns>
    public static async Task<string> ConversationOrButtonInput(this AIRequest request, string message, string button)
    {
        request.Conversation.AddSystemMessage(message, m => 
        {
            m.AddButton(button, L(button));
        });

        string prompt = await request.Conversation.WaitForTextOrButtonInput(request.Cancel);

        return prompt;
    }

    /// <summary>
    /// Displays a yes/no button dialog in the conversation and returns the user's choice.
    /// </summary>
    /// <param name="request">The AI request.</param>
    /// <param name="message">The system message to display with the buttons.</param>
    /// <returns>True if the user selected "Yes"; otherwise, false.</returns>
    public static async Task<bool> ConversationYesNoButtons(this AIRequest request, string message)
    {
        var msgItem = request.Conversation.AddSystemMessage(message, m =>
        {
            m.AddButtons(string.Empty, new() { Key = "Yes", Text = L("Yes") }, new() { Key = "No", Text = L("No") });
        });

        string button = await request.Conversation.WaitForButtonInput(request.Cancel);

        msgItem.Dispose();

        return button == "Yes";
    }

    #endregion
}
