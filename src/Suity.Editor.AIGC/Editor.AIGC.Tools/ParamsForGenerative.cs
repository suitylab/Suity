using Suity.Editor.AIGC.Assistants;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Parameters for creating objects based on a given prompt.
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
[Description("This tool is used to create a objects based on the given prompt. If user only required to create objects, this is the best choice.")]
public class GenerativeCreateParam
{
    /// <summary>
    /// Gets or sets the prompt describing what objects to create.
    /// </summary>
    [Description("The prompt to create the object.")]
    public string Prompt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to query the knowledge base before creating objects.
    /// </summary>
    [Description("User mentioned that query the Knowledge base before creating the objects.")]
    public bool FromKnowledgeBase { get; set; }
}

/// <summary>
/// Parameters for creating multiple objects based on the knowledge base (RAG).
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
[Description("This tool is used to populate multiple object based on knowledge base.")]
public class GenerativeCreateRagParam
{
    /// <summary>
    /// Gets or sets the prompt describing what objects to create.
    /// </summary>
    [Description("The prompt to create the object.")]
    public string Prompt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to query the knowledge base before creating objects.
    /// </summary>
    [Description("User mentioned that query the Knowledge base before creating the objects.")]
    public bool FromKnowledgeBase { get; set; }
}

/// <summary>
/// Parameters for creating multiple objects based on batch creation guidings.
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
[Description("This tool is used to create multiple objects based on batch creation guidings.")]
public class GenerativeBatchParam
{
    /// <summary>
    /// Gets or sets the array of guiding items for batch object creation.
    /// </summary>
    [Description("Multiple guiding to create the objects.")]
    public GenerativeGuidingItem[] Guidings { get; set; }
}

/// <summary>
/// Parameters for creating, editing, updating, modifying, renaming, deleting, or fixing objects.
/// </summary>
[Description("This tool is used to create, edit, update, modify, rename, delete, fix the objects. If other tools are suitable, this is the best choice.")]
public class GenerativeEditParam
{
    /// <summary>
    /// Gets or sets the prompt describing what changes to make to the objects.
    /// </summary>
    [Description("The prompt to update, delete or fix the objects.")]
    public string Prompt { get; set; }
}

