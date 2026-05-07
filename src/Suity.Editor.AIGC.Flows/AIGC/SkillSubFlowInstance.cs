using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using System.Linq;

namespace Suity.Editor.AIGC;

public class SkillSubFlowInstance : SubFlowInstance, IHasSkill
{
    /// <summary>
    /// Property key used to store the skill asset reference.
    /// </summary>
    public const string SKILL_PROP = "__skill__";

    private readonly AssetProperty<ISubFlowAsset> _skill = new(SKILL_PROP, "Skill");
    private string _skillName;


    public SkillSubFlowInstance(SubFlowDefinitionDiagramItem pageDefinition, PageElementOption option, ISubFlowAsset skill = null) 
        : base(pageDefinition, option)
    {
        _skill.Target = skill;
    }

    /// <summary>
    /// Gets the selection for the skill asset associated with this page.
    /// </summary>
    public AssetSelection<ISubFlowAsset> SkillAssetSelection => _skill.Selection;

    /// <inheritdoc/>
    public override string Name => _skillName ?? base.Name;

    /// <summary>
    /// Gets the skill definition associated with this page, if any.
    /// </summary>
    /// <returns>The skill definition, or null if no skill is set.</returns>
    public IAigcSkill GetSkill() => (_skill.Target as IHasSkill)?.GetSkill();

    public override ISubFlowAsset GetToolAsset()
    {
        if (_skill.Target is { } skillAsset)
        {
            return skillAsset;
        }

        return base.GetToolAsset();
    }

    protected override void OnBuild()
    {
        base.OnBuild();

        UseParentArticle = GetSkill()?.UseParentArticle ?? (PageNode?.UseParentArticle == true);

        //Name
        if (GetSkill()?.SkillName is { } skillName && !string.IsNullOrWhiteSpace(skillName))
        {
            _skillName = skillName;
        }

        //Tooltips
        if (GetSkill()?.SkillTooltips is { } skillTooltips && !string.IsNullOrWhiteSpace(skillTooltips))
        {
            Tooltips = skillTooltips;
        }
        else
        {
            Tooltips = PageNode.GetAttribute<ToolTipsAttribute>()?.ToolTips;
        }
    }

    public override void UpdateFromOther(SubFlowInstance otherRoot)
    {
        base.UpdateFromOther(otherRoot);

        _skill.TargetAsset = (otherRoot as SkillSubFlowInstance)?._skill.TargetAsset;
    }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        base.Sync(sync, context);

        _skill.Sync(sync);
    }

    public override ISubFlowAsset[] GetToolList()
    {
        var tools = PageNode.Tools.SkipNull();

        if (GetSkill()?.Tools?.ToArray() is { } exTools && exTools.Length > 0)
        {
            tools = tools.Concat(exTools.SkipNull());
        }

        return [.. tools];
    }

    public override ISubFlowAsset GetDefinitionPage()
    {
        if (_skill.Target is { } skill)
        {
            return skill;
        }

        return BaseAsset;
    }
}
