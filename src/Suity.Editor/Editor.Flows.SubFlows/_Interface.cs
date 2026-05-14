using Suity.Editor.Types;
using Suity.Views.Named;
using System.Collections.Generic;

namespace Suity.Editor.Flows.SubFlows;

#region IPageAsset

/// <summary>
/// Represents a asset that can create page instances and provides access to page and preset definitions.
/// </summary>
[NativeType(CodeBase = "SubFlow", Description = "Page Asset", Color = FlowColors.Tool, Icon = "*CoreIcon|Page")]
[NativeAlias("*AIGC|IPageAsset")]
public interface IPageAsset : INamed, IHasId
{
    /// <summary>
    /// Gets the description of the tool asset.
    /// </summary>
    string Description { get; }

    IPageInstance CreatePageInstance(PageCreateOption option);
}

#endregion

#region ISubFlowAsset

/// <summary>
/// Represents a sub-flow asset that can create page instances and provides access to page and preset definitions.
/// </summary>
[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Asset", Color = FlowColors.Tool, Icon = "*CoreIcon|Workflow")]
[NativeAlias("*AIGC|ISubFlowAsset")]
public interface ISubFlowAsset : IPageAsset
{
    /// <summary>
    /// Gets whether this tool asset is a startup page.
    /// </summary>
    bool IsStartup { get; }

    /// <summary>
    /// Gets the base page definition.
    /// </summary>
    ISubFlow GetBaseDefinition();
}

#endregion

#region ISubFlow

/// <summary>
/// Represents an sub-flow page that can provide its definition, branch, result, and associated document item.
/// </summary>
[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Definition", Color = FlowColors.Task, Icon = "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.TaskPages.IAigcPage")]
[NativeAlias("Suity.Editor.Flows.SubFlows.ISubFlowDef")]
[NativeAlias("*Suity|ISubFlow")]
public interface ISubFlow : INamed
{
    /// <summary>
    /// Gets the page definition.
    /// </summary>
    ISubFlow GetDefinitionPage();

    /// <summary>
    /// Gets the page result.
    /// </summary>
    ISubFlow GetResultPage();

    /// <summary>
    /// Gets the associated document item for this page.
    /// </summary>
    object GetDocumentItem();
}

#endregion

#region ISubFlowDefAsset

/// <summary>
/// Represents an asset that defines an Sub-flow definition page.
/// </summary>
[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Definition Asset", Color = FlowColors.Page, Icon = "*CoreIcon|Page")]
[NativeAlias("*AIGC|ISubFlowDefAsset")]
public interface ISubFlowDefAsset : ISubFlowAsset
{
}

#endregion

#region ISubFlowPreset

/// <summary>
/// Represents an Sub-flow preset that provides tools and parameter access.
/// </summary>
[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Preset", Color = FlowColors.Page, Icon = "*CoreIcon|Preset")]
public interface ISubFlowPreset
{
    /// <summary>
    /// Gets the base execution flow page definition asset.
    /// </summary>
    ISubFlowDefAsset BaseFlow { get; }

    /// <summary>
    /// Gets the name of the preset.
    /// </summary>
    string PresetName { get; }

    /// <summary>
    /// Gets the tooltips/description of the preset.
    /// </summary>
    string PresetTooltips { get; }

    /// <summary>
    /// Used to provide guidance on how to use the preset effectively.
    /// </summary>
    string PromptHint { get; }

    /// <summary>
    /// Gets all tools provided by this preset.
    /// </summary>
    IEnumerable<IPageAsset> Tools { get; }

    /// <summary>
    /// Gets whether this preset is a startup page.
    /// </summary>
    bool IsStartupPage { get; }

    /// <summary>
    /// Gets whether this preset uses the parent article.
    /// </summary>
    bool UseParentArticle { get; }

    /// <summary>
    /// Attempts to retrieve a parameter value by name.
    /// </summary>
    bool TryGetParameter(string name, out object value);
}


#endregion

#region ISubFlowPresetAsset

[NativeType(CodeBase = "SubFlow", Description = "Sub-flow Preset Asset", Color = FlowColors.Page, Icon = "*CoreIcon|Preset")]
public interface ISubFlowPresetAsset : ISubFlowAsset
{
    /// <summary>
    /// Gets the preset definition.
    /// </summary>
    ISubFlowPreset GetPresetDefinition();
}

#endregion

