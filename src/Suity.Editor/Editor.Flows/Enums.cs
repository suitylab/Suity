using Suity.Editor.Types;

namespace Suity.Editor.Flows;

/// <summary>
/// Flow diagram connection type
/// </summary>
public enum FlowConnectorTypes
{
    /// <summary>
    /// Action
    /// </summary>
    Action,

    /// <summary>
    /// Data
    /// </summary>
    Data,

    /// <summary>
    /// Associate
    /// </summary>
    Associate,

    /// <summary>
    /// Control
    /// </summary>
    Control,
}

/// <summary>
/// Flow diagram connection direction
/// </summary>
public enum FlowDirections
{
    /// <summary>
    /// Input
    /// </summary>
    [DisplayText("Input")]
    Input,

    /// <summary>
    /// Output
    /// </summary>
    [DisplayText("Output")]
    Output,
}

/// <summary>
/// Position auto-fill mode
/// </summary>
public enum PositionAutomationMode
{
    /// <summary>
    /// None
    /// </summary>
    None,

    /// <summary>
    /// Automate
    /// </summary>
    Automate,
}

/// <summary>
/// Node graph context scope
/// </summary>
[NativeType("FlowContextScopes", CodeBase = "*AIGC")]
public enum FlowContextScopes
{
    [DisplayText("Current Flow")]
    Local,

    [DisplayText("Current Diagram")]
    Diagram,

    [DisplayText("Global Diagram")]
    Global,
}