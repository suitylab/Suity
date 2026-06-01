using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Types;

namespace Suity.Editor.Flows.TaskPages;

#region GetCurrentPresetSkill

/// <summary>
/// A flow node that retrieves the skill from a preset associated with the current task page, if available.
/// </summary>
[SimpleFlowNodeStyle(Color = FlowColors.TaskBG, HasHeader = false, Category = "Preset")]
[DisplayText("Get Current Preset Skill", "*CoreIcon|Task")]
public class GetCurrentPresetSkill : TaskPageNode
{
    readonly FlowNodeConnector _skill;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentPresetSkill"/> class.
    /// </summary>
    public GetCurrentPresetSkill()
    {
        var skillType = TypeDefinition.FromAssetLink<PromptAsset>();

        _skill = AddDataOutputConnector("Skill", skillType, "Preset Skill");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        var task = compute.Context.GetArgument<IAigcWorkflowPage>();
        var pageAsset = task.GetPageAsset() as SubFlowPresetAsset;

        var skill = (pageAsset.GetPresetDefinition() as SubFlowPresetDocument)?.Skill;

        compute.SetValue(_skill, skill);
    }
}

#endregion