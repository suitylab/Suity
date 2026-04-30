using Suity.Editor.Flows;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Parameters for selecting tables in the canvas.
/// </summary>
[ToolReturnType(typeof(IEnumerable<CanvasFlowNode>))]
[Description("This tool is used for selecting tables in the canvas.")]
public class DataTableSelectParam
{
    /// <summary>
    /// Gets or sets the selection requirement specified by the user.
    /// </summary>
    [Description("The selection requirement by user.")]
    public string Requirement { get; set; }
}