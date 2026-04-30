using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Request context for generating or modifying complex fields on an SObject.
/// </summary>
public class AIComplexFieldsRequest : AIRequest
{
    /// <summary>
    /// Gets the canvas context for the operation.
    /// </summary>
    public CanvasContext Context { get; init; }

    /// <summary>
    /// Gets the SObject to fill complex fields on.
    /// </summary>
    public SObject Object { get; init; }

    /// <summary>
    /// Gets the original SObject being edited, if this is a modification operation.
    /// </summary>
    public SObject OriginToEdit { get; init; }

    /// <summary>
    /// Gets the specific fields to include in generation. If null, all eligible fields are considered.
    /// </summary>
    public ICollection<DStructField> IncludedFields { get; init; }

    /// <summary>
    /// Gets the fields to exclude from generation.
    /// </summary>
    public ICollection<DStructField> ExcludedFields { get; init; }

    /// <summary>
    /// Gets the target group path for organizing generated data.
    /// </summary>
    public string TargetGroupPath { get; init; }

    /// <summary>
    /// Gets a value indicating whether to only process fields that support AI generation.
    /// </summary>
    public bool OnlyGenerationFields { get; init; }

    /// <summary>
    /// Gets a function to retrieve consistency values for fields.
    /// </summary>
    public Func<DStructField, SObject, SItem> ConsistencyGetter { get; init; }

    /// <summary>
    /// Gets the callback invoked after a field is successfully applied.
    /// </summary>
    public Action<DStructField, SObject> ApplyCallBack { get; init; }

    public AIComplexFieldsRequest()
    {
    }

    // Creating complex fields generates recursion, so depth limit is needed
    public AIComplexFieldsRequest(AIRequest origin, CanvasContext context, bool increaseDepth = true)
        : base(origin, increaseDepth)
    {
        Context = context;
    }
}

/// <summary>
/// Request context for generating or modifying a single complex field.
/// </summary>
public class AIComplexFieldRequest : AIRequest
{
    /// <summary>
    /// Gets the canvas context for the operation.
    /// </summary>
    public CanvasContext Canvas { get; init; }

    /// <summary>
    /// Gets the SObject containing the field to process.
    /// </summary>
    public SObject Object { get; init; }

    /// <summary>
    /// Gets the JSON string representation of the object.
    /// </summary>
    public string ObjectJsonString { get; init; }

    /// <summary>
    /// Gets the specific field to generate or modify.
    /// </summary>
    public DStructField Field { get; init; }

    /// <summary>
    /// Gets the existing field item to modify, if this is an edit operation.
    /// </summary>
    public SItem FieldItemToModify { get; init; }

    /// <summary>
    /// Gets the target group path for organizing generated data.
    /// </summary>
    public string TargetGroupPath { get; init; }

    /// <summary>
    /// Gets the callback invoked after the field is successfully applied.
    /// </summary>
    public Action ApplyCallBack { get; init; }

    public AIComplexFieldRequest()
    {
    }

    public AIComplexFieldRequest(AIRequest origin, CanvasContext canvas, bool increaseDepth = false)
        : base(origin, increaseDepth)
    {
        Canvas = canvas;
    }
}

/// <summary>
/// Handles linked data operations such as selecting and creating data items.
/// </summary>
public interface ILinkedDataHandler
{
    /// <summary>
    /// Selects a single data item based on the guiding instructions.
    /// </summary>
    /// <param name="request">The AI request context.</param>
    /// <param name="dataType">The type of data to select.</param>
    /// <param name="guiding">The guiding instructions for selection.</param>
    /// <returns>The selected data item.</returns>
    Task<SItem> SelectData(AIRequest request, DCompond dataType, string guiding);

    /// <summary>
    /// Selects multiple data items based on the guiding instructions.
    /// </summary>
    /// <param name="request">The AI request context.</param>
    /// <param name="dataType">The type of data to select.</param>
    /// <param name="guiding">The guiding instructions for selection.</param>
    /// <param name="count">The number of items to select.</param>
    /// <returns>An array of selected data items.</returns>
    Task<SItem[]> SelectDatas(AIRequest request, DCompond dataType, string guiding, int count);

    /// <summary>
    /// Determines whether this handler supports creating new data of the specified type.
    /// </summary>
    /// <param name="request">The AI request context.</param>
    /// <param name="dataType">The type of data to create.</param>
    /// <returns>True if data creation is supported; otherwise, false.</returns>
    bool SupportDataCreation(AIRequest request, DCompond dataType);

    /// <summary>
    /// Creates new data items based on the guiding instructions.
    /// </summary>
    /// <param name="request">The AI request context.</param>
    /// <param name="dataType">The type of data to create.</param>
    /// <param name="guidings">The guiding instructions for each item to create.</param>
    /// <param name="groupPath">Optional group path for organizing created data.</param>
    /// <returns>An array of created data items.</returns>
    Task<SItem[]> CreateDatas(AIRequest request, DCompond dataType, GenerativeGuidingItem[] guidings, string groupPath = null);
}