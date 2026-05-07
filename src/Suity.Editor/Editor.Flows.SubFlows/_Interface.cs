using Suity.Editor.Types;
using Suity.Views.Named;

namespace Suity.Editor.Flows.SubFlows;

#region ISubFlowDef

/// <summary>
/// Represents an sub-flow definition that can provide its definition, result, and associated document item.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Sub-flow Definition", Color = FlowColors.Task, Icon = "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.TaskPages.IAigcPage")]
public interface ISubFlowDef : INamed
{
    /// <summary>
    /// Gets the page definition.
    /// </summary>
    ISubFlowDef GetPageDefinition();

    /// <summary>
    /// Gets the page result.
    /// </summary>
    ISubFlowDef GetPageResult();

    /// <summary>
    /// Gets the associated document item for this page.
    /// </summary>
    object GetDocumentItem();
}

#endregion

#region ISubFlowDefAsset

/// <summary>
/// Represents an asset that defines an AIGC page.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Sub-flow Definition Asset", Color = FlowColors.Page, Icon = "*CoreIcon|Page")]
public interface ISubFlowDefAsset : INamed, IHasId
{
    /// <summary>
    /// Gets the base definition of the page.
    /// </summary>
    ISubFlowDef GetBaseDefinition();
}

#endregion
