using Suity.Editor.Types;
using Suity.Views.Named;
using System.Collections.Generic;

namespace Suity.Editor.Flows.SubFlows;

#region ISubFlowPage

/// <summary>
/// Represents an sub-flow page that can provide its definition, branch, result, and associated document item.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Sub-flow Definition", Color = FlowColors.Task, Icon = "*CoreIcon|Page")]
[NativeAlias("Suity.Editor.AIGC.TaskPages.IAigcPage")]
[NativeAlias("Suity.Editor.Flows.SubFlows.ISubFlowDef")]
public interface ISubFlowPage : INamed
{
    /// <summary>
    /// Gets the page definition.
    /// </summary>
    ISubFlowPage GetPageDefinition();

    /// <summary>
    /// Gets the page result.
    /// </summary>
    ISubFlowPage GetPageResult();

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
[NativeType(CodeBase = "AIGC", Description = "Sub-flow Definition Asset", Color = FlowColors.Page, Icon = "*CoreIcon|Page")]
public interface ISubFlowDefAsset : INamed, IHasId
{
    /// <summary>
    /// Gets the base definition of the page.
    /// </summary>
    ISubFlowPage GetBaseDefinition();
}

#endregion

#region SubFlowEventTypes

/// <summary>
/// Represents the types of events that can occur during an Sub-flow task lifecycle.
/// </summary>
public enum SubFlowEventTypes
{
    /// <summary>
    /// No event.
    /// </summary>
    [DisplayText("None")]
    None,

    /// <summary>
    /// Indicates that a task has begun.
    /// </summary>
    [DisplayText("Task Start")]
    TaskBegin,

    /// <summary>
    /// Indicates that a subtask has completed successfully.
    /// </summary>
    [DisplayText("Subtask Completed")]
    SubTaskFinished,

    /// <summary>
    /// Indicates that a subtask has failed.
    /// </summary>
    [DisplayText("Subtask Failed")]
    SubTaskFailed,
}

#endregion

#region IToolDefAsset

/// <summary>
/// Represents a tool asset that can create page instances and provides access to page and preset definitions.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Tool Definition Asset", Color = FlowColors.Tool, Icon = "*CoreIcon|Tool")]
public interface IToolDefAsset
{
    /// <summary>
    /// Gets the description of the tool asset.
    /// </summary>
    string Description { get; }
}

#endregion

#region ISubFlowAsset

/// <summary>
/// Represents a sub-flow asset that can create page instances and provides access to page and preset definitions.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Sub-flow Asset", Color = FlowColors.Tool, Icon = "*CoreIcon|Workflow")]
public interface ISubFlowAsset : INamed, IHasId
{
    /// <summary>
    /// Gets the description of the tool asset.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets whether this tool asset is a startup page.
    /// </summary>
    bool IsStartup { get; }

    /// <summary>
    /// Gets the base page definition.
    /// </summary>
    ISubFlowPage GetBaseDefinition();

    /// <summary>
    /// Creates a new page instance with the specified options.
    /// </summary>
    ISubFlowInstance CreateInstance(PageElementOption option);
}

#endregion

#region ISubFlowPreset

/// <summary>
/// Represents an Sub-flow preset that provides tools and parameter access.
/// </summary>
public interface ISubFlowPreset
{
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
    IEnumerable<IToolDefAsset> Tools { get; }

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

#region IHasPreset

public interface IHasPreset
{
    /// <summary>
    /// Gets the preset definition.
    /// </summary>
    ISubFlowPreset GetPreset();
}

#endregion