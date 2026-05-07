using Suity.Drawing;
using System.Drawing;

namespace Suity.Editor.Flows;

/// <summary>
/// Color definitions used across the flow node editor components.
/// </summary>
public class FlowColors
{
    /// <summary>
    /// Hex color value for error states.
    /// </summary>
    public const string Error = "#FF0000";

    /// <summary>
    /// Hex color value for plan background.
    /// </summary>
    public const string PlanBg = "#476661";

    /// <summary>
    /// Hex color value for plan and task elements.
    /// </summary>
    public const string PlanAndTask = "#00FFFF";

    /// <summary>
    /// Hex color value for step indicators.
    /// </summary>
    public const string Step = "#00FF66";

    /// <summary>
    /// Hex color value for code design background.
    /// </summary>
    public const string CodeDesignBg = "#8166C0";

    /// <summary>
    /// Hex color value for code design elements.
    /// </summary>
    public const string CodeDesign = "#5B39C5";

    /// <summary>
    /// Hex color value for agent background.
    /// </summary>
    public const string AgentBg = "#704E7D";

    /// <summary>
    /// Hex color value for agent elements.
    /// </summary>
    public const string Agent = "#965CAC";

    /// <summary>
    /// Hex color value for tool background.
    /// </summary>
    public const string ToolBG = "#95714B";

    /// <summary>
    /// Hex color value for tool elements.
    /// </summary>
    public const string Tool = "#BE8C59";

    /// <summary>
    /// Hex color value for LLM (Large Language Model) elements.
    /// </summary>
    public const string LLm = "#135375"; // "#3584C9";

    /// <summary>
    /// Hex color value for workflow elements.
    /// </summary>
    public const string Workflow = "#13A839";


    /// <summary>
    /// Hex color value for task background.
    /// </summary>
    public const string TaskBG = "#7C68E6";

    /// <summary>
    /// Hex color value for task elements.
    /// </summary>
    public const string Task = "#604BCF";
    
    
    /// <summary>
    /// Hex color value for page elements.
    /// </summary>
    public const string Page = "#505050";

    /// <summary>
    /// Hex color value for page parameter elements.
    /// </summary>
    public const string PageParameter = "#42B0FA";

    /// <summary>
    /// Hex color value for message elements.
    /// </summary>
    public const string Message = "#7BA45A";


    /// <summary>
    /// Color object for error states.
    /// </summary>
    public static Color ErrorColor { get; } = ColorTranslators.FromHtml(Error);

    /// <summary>
    /// Color object for plan background.
    /// </summary>
    public static Color PlanBgColor { get; } = ColorTranslators.FromHtml(PlanBg);

    /// <summary>
    /// Color object for agent background.
    /// </summary>
    public static Color AgentBgColor { get; } = ColorTranslators.FromHtml(AgentBg);


    /// <summary>
    /// Color object for code design background.
    /// </summary>
    public static Color CodeDesignBgColor { get; } = ColorTranslators.FromHtml(CodeDesignBg);

    /// <summary>
    /// Color object for workflow elements.
    /// </summary>
    public static Color WorkflowColor { get; } = ColorTranslators.FromHtml(Workflow);


    /// <summary>
    /// Color object for node header with semi-transparent white overlay.
    /// </summary>
    public static Color NodeHaderColor { get; } = Color.FromArgb(30, 255, 255, 255);

    //public static Color GroupHeaderColor { get; } = Color.FromArgb(30, 0, 0, 0);

}