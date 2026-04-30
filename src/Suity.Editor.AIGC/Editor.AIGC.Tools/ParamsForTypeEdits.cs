using Suity.Editor.AIGC.Assistants;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Extracts types from an existing document into <see cref="DataModelSpecification"/>.
/// </summary>
/// <summary>
/// Parameters for extracting types from an existing document into a <see cref="DataModelSpecification"/>.
/// </summary>
[ToolReturnType(typeof(DataModelSpecification))]
public class DataModelSpecFromDocumentParam
{
    /// <summary>
    /// Gets or sets the names of items to extract from the document.
    /// </summary>
    public List<string> ItemNames { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataModelSpecFromDocumentParam"/> class.
    /// </summary>
    public DataModelSpecFromDocumentParam()
    {
    }
}

/// <summary>
/// Parameters for adding a single enum item to an existing enum type.
/// </summary>
public class AddEnumItemParam
{
    /// <summary>
    /// Gets or sets the name of the enum type to add the item to.
    /// </summary>
    public string EnumTypeName { get; set; }

    /// <summary>
    /// Gets or sets the name of the new enum item.
    /// </summary>
    public string ItemName { get; set; }
}

/// <summary>
/// Parameters for generating a data model document based on detailed text prompts.
/// </summary>
[ToolReturnType(typeof(string))]
[Obsolete]
public class DataModelTextDocumentParam
{
    /// <summary>
    /// Gets or sets the system prompt that guides the AI generation.
    /// </summary>
    public string SystemPrompt { get; set; }

    /// <summary>
    /// Gets or sets the content to process for data model generation.
    /// </summary>
    public string Content { get; set; }
}

/// <summary>
/// Parameters for extracting types from text content into a <see cref="DataModelSpecification"/>.
/// </summary>
[ToolReturnType(typeof(DataModelSpecification))]
[Obsolete]
public class DataModelSpecCreateParam
{
    /// <summary>
    /// Gets or sets the text content to extract types from.
    /// </summary>
    public string Content { get; set; }
}

/// <summary>
/// Parameters for extracting types from an array of guiding items into a <see cref="DataModelSpecification"/>.
/// </summary>
[ToolReturnType(typeof(DataModelSpecification))]
[Obsolete]
public class DataModelSpecCreateSegParam
{
    /// <summary>
    /// Gets or sets the array of guiding items for type extraction.
    /// </summary>
    public GenerativeGuidingItem[] Guidings { get; set; }
}

/// <summary>
/// Parameters for extracting a specific type from a <see cref="StructureSegment"/> into a <see cref="StructureSpecification"/>.
/// </summary>
[ToolReturnType(typeof(StructureSpecification))]
[Obsolete]
public class DataModelSpecCreateOneParam
{
    /// <summary>
    /// Gets or sets the reference materials for type extraction.
    /// </summary>
    public string Referances { get; set; }

    /// <summary>
    /// Gets or sets the structure segment that needs to be specified.
    /// </summary>
    public StructureSegment SegmentItem { get; set; }
}

/// <summary>
/// Parameters for creating multiple objects based on a data model segmentation.
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
[Description("This tool is used to create multiple objects based on data model segmentation.")]
public class GenerativeDataModelSegParam
{
    /// <summary>
    /// Gets or sets the segmentation document used for data modeling.
    /// </summary>
    [Description("Segmentation document of the data modeling")]
    public DataModelSegmentation Segmentation { get; set; }

    /// <summary>
    /// Gets or sets the category name for this segmentation. Leave empty if not specified.
    /// </summary>
    [Description("The category name of this segmentation. Leave empty if not specified.")]
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to override the existing data model.
    /// </summary>
    [Description("Override the existing data model.")]
    public bool Overrides { get; set; }
}

/// <summary>
/// Parameters for creating multiple objects based on a data model specification.
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
[Description("This tool is used to create multiple objects based on data model specification.")]
public class GenerativeDataModelSpecParam
{
    /// <summary>
    /// Gets or sets the specification document used for data modeling.
    /// </summary>
    [Description("Specification document of the data modeling")]
    public DataModelSpecification Specification { get; set; }

    /// <summary>
    /// Gets or sets the category name for this specification. Leave empty if not specified.
    /// </summary>
    [Description("The category name of this segmentation. Leave empty if not specified.")]
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to override the existing data model.
    /// </summary>
    [Description("Override the existing data model.")]
    public bool Overrides { get; set; }
}