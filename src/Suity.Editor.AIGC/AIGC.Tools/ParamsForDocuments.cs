using Suity.Editor.AIGC.Assistants;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Parameters for selecting a single element in a document.
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
[Description("This tool is used for selecting elements in document.")]
public class SingleDocumentElementSelectParam
{
    /// <summary>
    /// Gets or sets the selection requirement specified by the user.
    /// </summary>
    [Description("The selection requirement by user.")]
    public string Requirement { get; set; }
}

/// <summary>
/// Parameters for creating a new element in a document.
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
public class DocumentElementCreateParam
{
    /// <summary>
    /// Gets or sets the prompt describing the element to create.
    /// </summary>
    public string Prompt { get; set; }
}

/// <summary>
/// Parameters for batch creating elements in a document.
/// </summary>
[ToolReturnType(typeof(IEnumerable<object>))]
public class DocumentBatchCreateParam
{
    /// <summary>
    /// Gets or sets the array of guiding items for batch element creation.
    /// </summary>
    public GenerativeGuidingItem[] Guidings { get; set; }
}

/// <summary>
/// Parameters for creating, adding, updating, renaming, fixing, or deleting elements in a document.
/// </summary>
[ToolReturnType(typeof(IEnumerable<AICallResult>))]
//[Description("This tool is used for create, update, delete elements in document.")]
[Description("This tool is used for create, add, update, rename, fix, delete elements in document. This tool update selected docuemnts only.")]
public class DocumentElementEditParam
{
    /// <summary>
    /// Gets or sets the requirement describing what changes to make to the document elements.
    /// </summary>
    [Description("The requirement of the update.")]
    public string Prompt { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the system should discover issues in the document.
    /// </summary>
    [Description("If user require to fix the document, set true to let system discover issues.")]
    public bool RequireDiscoverIssues { get; set; }
}
