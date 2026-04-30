using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using System;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Request context for AI data generation operations.
/// </summary>
public class AIDataRequest : AIRequest
{
    public AIDataRequest()
    {
    }

    public AIDataRequest(AIRequest request)
        : base(request)
    {
    }

    /// <summary>
    /// Gets the data compose that defines the overall data structure and knowledge.
    /// </summary>
    public IDataCompose Compose { get; init; }

    /// <summary>
    /// Gets the data system providing system-level context.
    /// </summary>
    public IDataSystem System { get; init; }

    /// <summary>
    /// Gets the data schema defining the structure of generated data.
    /// </summary>
    public DCompondSchema DataSchema { get; init; }

    /// <summary>
    /// Gets the usage mode for the generated data (e.g., DataGrid, FlowGraph, TreeGraph).
    /// </summary>
    public DataUsageMode DataUsage { get; init; }

    /// <summary>
    /// Gets the target document for data generation.
    /// </summary>
    public Document Document { get; init; }

    /// <summary>
    /// Gets or sets the document assistant responsible for data operations.
    /// </summary>
    public IDocumentAssistant DataAssistant { get; set; }

    /// <summary>
    /// Gets the localized name for the data being generated.
    /// </summary>
    public string DataLocalName { get; init; }

    /// <summary>
    /// Gets the description of the data being generated.
    /// </summary>
    public string DataDescription { get; init; }

    /// <summary>
    /// Gets the category for organizing generated data.
    /// </summary>
    public string Category { get; init; }

    /// <summary>
    /// Gets the group path where generated data should be placed.
    /// </summary>
    public string GroupPath { get; init; }

    /// <summary>
    /// Gets the overall guiding instructions for data generation.
    /// </summary>
    public string DataOverallGuiding { get; init; }

    /// <summary>
    /// Gets a value indicating whether to generate knowledge content.
    /// </summary>
    public bool GenerateKnowledge { get; init; }

    /// <summary>
    /// Gets the callback invoked when knowledge is generated.
    /// </summary>
    public Action<string> KnowledgeCallBack { get; init; }

    /// <summary>
    /// Gets a value indicating whether to record tooltips on generated items.
    /// </summary>
    public bool RecordTooltips { get; init; }
}
