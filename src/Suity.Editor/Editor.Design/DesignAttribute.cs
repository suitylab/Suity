using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Selecting;

namespace Suity.Editor.Design;

/// <summary>
/// Base class for design attributes.
/// </summary>
[NativeAbstract("*Suity|Attribute")]
public class DesignAttribute : SObjectController
{
    public object AttributeOwner { get; internal set; }

    public override string ToString()
    {
        var attr = GetType().GetAttributeCached<NativeTypeAttribute>();

        return attr?.Description ?? attr?.Name ?? GetType().Name;
    }
}

/// <summary>
/// Base class for design attributes that provide selection lists.
/// </summary>
public abstract class SelectionDesignAttribute : DesignAttribute
{
    /// <summary>
    /// Gets the selection list for the given function context.
    /// </summary>
    public abstract ISelectionList GetSelectionList(FunctionContext context);
}

/// <summary>
/// Defines how data is used or displayed in the editor.
/// </summary>
public enum DataUsageMode
{
    [DisplayText("None")]
    None,

    [DisplayText("Data Grid")]
    DataGrid,

    [DisplayText("Flow Chart")]
    FlowGraph,

    [DisplayText("Tree View")]
    TreeGraph,

    [DisplayText("Config")]
    Config,

    [DisplayText("Entity Data")]
    EntityData,

    [DisplayText("Entity")]
    Entity,

    [DisplayText("Action")]
    Action,

    [DisplayText("Activity")]
    Activity,

    [DisplayText("Nullable")]
    Nullable,
}

/// <summary>
/// Defines the data-driven generation mode.
/// </summary>
public enum DataDrivenMode
{
    [DisplayText("None")]
    None,

    [DisplayText("Active Generation")]
    Active,

    [DisplayText("Unique Driven")]
    Unique,

    [DisplayText("Shared Driven")]
    Shared,
}