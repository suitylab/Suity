using Suity.Editor.Types;

namespace Suity.Editor.AIGC;

/// <summary>
/// Language Model Type
/// </summary>
[NativeType(CodeBase = "AIGC")]
public enum LLmModelType
{
    [DisplayText("Default")]
    Default,

    [DisplayText("Planning")]
    Planning,

    [DisplayText("Tool Calls")]
    ToolCalls,

    [DisplayText("Web Search")]
    WebSearch,

    [DisplayText("Code-specific")]
    Coding,

    [DisplayText("Lightweight")]
    Lightweight,
}

/// <summary>
/// Represents the capability level of an AIGC model.
/// </summary>
[NativeType(CodeBase = "AIGC")]
public enum AigcModelLevel
{
    /// <summary>
    /// Default model level.
    /// </summary>
    [DisplayText("Default")]
    Default,

    /// <summary>
    /// Low-level model with basic capabilities.
    /// </summary>
    [DisplayText("Low-level")]
    Low,

    /// <summary>
    /// Mid-level model with moderate capabilities.
    /// </summary>
    [DisplayText("Mid-level")]
    Medium,

    /// <summary>
    /// High-level model with advanced capabilities.
    /// </summary>
    [DisplayText("High-level")]
    High,
}

