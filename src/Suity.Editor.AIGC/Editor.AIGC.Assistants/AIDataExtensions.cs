using ComputerBeacon.Json;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Services;
using Suity.Editor.Transferring;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Provides extension methods for AI data operations, including data validation, field filling, and object generation.
/// </summary>
public static class AIDataExtensions
{
    /// <summary>
    /// Determines whether the data container has data of the specified compound type.
    /// </summary>
    /// <param name="dataDoc">The data container to check.</param>
    /// <param name="name">The name of the data entry.</param>
    /// <param name="dataType">The compound data type to look for.</param>
    /// <returns>True if the container has data matching the specified type; otherwise, false.</returns>
    public static bool HasDataOfType(this IDataContainer dataDoc, string name, DCompond dataType)
    {
        if (dataDoc is null || dataType is null)
        {
            return false;
        }

        if (dataDoc.GetData(name) is not { } current)
        {
            return false;
        }

        if (dataType is DAbstract dAbstract)
        {
            return current.Components.Any(o => o.InputType == dataType.Definition || o.ObjectType?.BaseAbstractType == dataType.Definition);
        }
        else
        {
            return current.Components.Any(o => o.ObjectType == dataType.Definition);
        }
    }
    
    #region Fill Data
    /// <summary>
    /// Fills data using the specified AI data request.
    /// </summary>
    /// <param name="request">The AI data request containing the fill parameters.</param>
    /// <returns>A task representing the asynchronous operation, returning the AI call result.</returns>
    public static Task<AICallResult> FillData(this AIDataRequest request) => AIDataService.Instance.FillData(request);

    /// <summary>
    /// Fills complex fields in the AI request from the specified document's item collection.
    /// </summary>
    /// <param name="request">The AI request to fill.</param>
    /// <param name="compose">The data compose context.</param>
    /// <param name="doc">The document to extract items from.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task FillComplexFields(this AIRequest request, IDataCompose compose, Document doc)
    {
        if (doc is not SNamedDocument sdoc)
        {
            return Task.CompletedTask;
        }

        var items = sdoc.ItemCollection.AllItems.ToArray();

        return AIDataService.Instance.FillComplexFields(request, compose, doc, items);
    }

    /// <summary>
    /// Fills complex fields in the AI request using objects from the specified AI call result.
    /// </summary>
    /// <param name="request">The AI request to fill.</param>
    /// <param name="compose">The data compose context.</param>
    /// <param name="doc">The document context.</param>
    /// <param name="result">The AI call result containing objects to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task FillComplexFields(this AIRequest request, IDataCompose compose, Document doc, AICallResult result)
    {
        IEnumerable<object> objs;

        if (result is AIGenerativeCallResult genResult)
        {
            objs = genResult.BaseResult;
        }
        else
        {
            objs = result.Results;
        }

        return AIDataService.Instance.FillComplexFields(request, compose, doc, objs);
    }

    /// <summary>
    /// Fills complex fields in the AI request using the specified objects and optional field filter.
    /// </summary>
    /// <param name="request">The AI request to fill.</param>
    /// <param name="compose">The data compose context.</param>
    /// <param name="doc">The document context.</param>
    /// <param name="objs">The collection of objects to process.</param>
    /// <param name="fields">Optional collection of struct fields to include.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task FillComplexFields(this AIRequest request, IDataCompose compose, Document doc, IEnumerable<object> objs, IEnumerable<DStructField> fields = null) 
        => AIDataService.Instance.FillComplexFields(request, compose, doc, objs, fields);

    /// <summary>
    /// Creates a linked data handler for the specified data compose context.
    /// </summary>
    /// <param name="compose">The data compose context.</param>
    /// <returns>A new linked data handler instance.</returns>
    public static ILinkedDataHandler CreateLiinkedDataHandler(this IDataCompose compose) 
        => AIDataService.Instance.CreateLinkedDataHandler(compose);
    #endregion

    #region Object resolving

    /// <summary>
    /// Generates an SObject from the AI request using the specified canvas context and compound function call type.
    /// </summary>
    /// <param name="request">The AI request to generate from.</param>
    /// <param name="canvas">The canvas context.</param>
    /// <param name="type">The compound function call type defining the target type and excluded fields.</param>
    /// <returns>A task representing the asynchronous operation, returning the generated SObject.</returns>
    public static Task<SObject> GenerateSObject(this AIRequest request, CanvasContext canvas, DCompondFunctionCallType type)
       => GenerateSObject(request, canvas, type.Type, type.ExcludedFields);

    /// <summary>
    /// Generates an SObject by first producing a JSON object and then filling complex fields.
    /// </summary>
    /// <param name="request">The AI request to generate from.</param>
    /// <param name="canvas">The canvas context.</param>
    /// <param name="type">The compound type defining the structure of the generated object.</param>
    /// <param name="excludedFields">Optional collection of fields to exclude from generation.</param>
    /// <returns>A task representing the asynchronous operation, returning the generated SObject.</returns>
    public static async Task<SObject> GenerateSObject(this AIRequest request, CanvasContext canvas, DCompond type,
        ICollection<DStructField> excludedFields = null)
    {
        var jobj = await request.GenerateJsonObject(type, excludedFields);

        var sobj = new SObject(type.Definition);

        var option = new SItemResourceOptions
        {
            AutoAddNewEnumValue = AIAssistantService.Config.AutoAddNewEnumValue
        };
        DataRW.InputJson(sobj, jobj, option);

        var complexReq = new AIComplexFieldsRequest(request, canvas)
        {
            Object = sobj,
            ExcludedFields = excludedFields,
            TargetGroupPath = type?.Name,
        };

        // Fill link fields and abstract fields step by step
        await AIDataService.Instance.FillComplexFields(complexReq);

        return sobj;
    }

    /// <summary>
    /// Generates an SObject from the AI request using the specified user message, canvas context, and compound function call type.
    /// </summary>
    /// <param name="request">The AI request to generate from.</param>
    /// <param name="canvas">The canvas context.</param>
    /// <param name="msg">The user message to guide generation.</param>
    /// <param name="type">The compound function call type defining the target type and excluded fields.</param>
    /// <returns>A task representing the asynchronous operation, returning the generated SObject.</returns>
    public static Task<SObject> GenerateSObject(this AIRequest request, CanvasContext canvas, string msg, DCompondFunctionCallType type)
        => GenerateSObject(request, canvas, msg, type.Type, type.ExcludedFields);

    /// <summary>
    /// Generates an SObject using the specified user message, producing a JSON object and then filling complex fields.
    /// </summary>
    /// <param name="request">The AI request to generate from.</param>
    /// <param name="canvas">The canvas context.</param>
    /// <param name="msg">The user message to guide generation.</param>
    /// <param name="type">The compound type defining the structure of the generated object.</param>
    /// <param name="excludedFields">Optional collection of fields to exclude from generation.</param>
    /// <returns>A task representing the asynchronous operation, returning the generated SObject.</returns>
    public static async Task<SObject> GenerateSObject(this AIRequest request, CanvasContext canvas, string msg, DCompond type,
        ICollection<DStructField> excludedFields = null)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = null;
        }

        var jsonReq = new AIJsonRequest(request)
        {
            UserMessage = msg ?? request.UserMessage,
            Type = type,
            ExcludedFields = excludedFields,
        };

        var jobj = await AIDataService.Instance.GenerateJsonObject(jsonReq);

        var sobj = new SObject(type.Definition);

        var option = new SItemResourceOptions
        {
            AutoAddNewEnumValue = AIAssistantService.Config.AutoAddNewEnumValue
        };
        DataRW.InputJson(sobj, jobj, option);

        var complexReq = new AIComplexFieldsRequest(request, canvas)
        {
            UserMessage = msg ?? request.UserMessage,
            Object = sobj,
            ExcludedFields = excludedFields,
            TargetGroupPath = type?.Name,
        };

        // Fill link fields and abstract fields step by step
        await AIDataService.Instance.FillComplexFields(complexReq);

        return sobj;
    }

    /// <summary>
    /// Edits an existing SObject by generating modified JSON and filling complex fields.
    /// </summary>
    /// <param name="request">The AI request to generate from.</param>
    /// <param name="canvas">The canvas context.</param>
    /// <param name="origin">The original SObject to edit.</param>
    /// <param name="excludedFields">Optional collection of fields to exclude from editing.</param>
    /// <returns>A task representing the asynchronous operation, returning the edited SObject.</returns>
    public static async Task<SObject> EditSObject(this AIRequest request, CanvasContext canvas, SObject origin,
        ICollection<DStructField> excludedFields = null)
    {
        if (origin.ObjectType.Target is not DCompond dcompond)
        {
            throw new AigcException(L("Source object missing type."));
        }

        // Current component exists, get component json text
        var jstr = AIGenerativeService.Instance.GetSimpleFieldJson(origin, excludedFields);

        var jobj = await request.EditJsonObject(dcompond, jstr, excludedFields);

        var sobj = new SObject(dcompond.Definition);

        var option = new SItemResourceOptions
        {
            AutoAddNewEnumValue = AIAssistantService.Config.AutoAddNewEnumValue
        };
        DataRW.InputJson(sobj, jobj, option);

        var request2 = new AIComplexFieldsRequest(request, canvas)
        {
            Object = sobj,
            OriginToEdit = origin,
            ExcludedFields = excludedFields,
            TargetGroupPath = origin.ObjectType?.Target?.Name,
        };

        // Fill link fields and abstract fields step by step
        await AIDataService.Instance.ModifyComplexFields(request2);

        return sobj;
    }

    /// <summary>
    /// Generates a JSON object for the specified compound type using the AI request.
    /// </summary>
    /// <param name="request">The AI request to generate from.</param>
    /// <param name="type">The compound type defining the structure of the JSON object.</param>
    /// <param name="excludedFields">Optional collection of fields to exclude from generation.</param>
    /// <returns>A task representing the asynchronous operation, returning the generated JSON object.</returns>
    public static Task<JsonObject> GenerateJsonObject(this AIRequest request, DCompond type, ICollection<DStructField> excludedFields = null)
    {
        var request2 = new AIJsonRequest(request)
        {
            Type = type,
            ExcludedFields = excludedFields,
        };

        return AIDataService.Instance.GenerateJsonObject(request2);
    }

    /// <summary>
    /// Edits an existing JSON object for the specified compound type using the AI request.
    /// </summary>
    /// <param name="request">The AI request to generate from.</param>
    /// <param name="type">The compound type defining the structure of the JSON object.</param>
    /// <param name="jsonToEdit">The JSON string to edit.</param>
    /// <param name="excludedFields">Optional collection of fields to exclude from editing.</param>
    /// <returns>A task representing the asynchronous operation, returning the edited JSON object.</returns>
    public static Task<JsonObject> EditJsonObject(this AIRequest request, DCompond type, string jsonToEdit,
        ICollection<DStructField> excludedFields = null)
    {
        var request2 = new AIJsonRequest(request)
        {
            Type = type,
            JsonToEdit = jsonToEdit,
            ExcludedFields = excludedFields,
        };

        return AIDataService.Instance.EditJsonObject(request2);
    }

    #endregion
}
