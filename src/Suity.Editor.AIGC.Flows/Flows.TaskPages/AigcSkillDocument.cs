using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.AIGC;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Suity.Editor.Flows.TaskPages;

/// <summary>
/// Document representing an AIGC skill, containing page definitions, tools, and configuration.
/// </summary>
[DocumentFormat(FormatName = "AigcSkill", Extension = "sskill", DisplayText = "Skill", Icon = "*CoreIcon|Skill", Categoty = "AIGC", CanShowView = false)]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AigcSkillDocument")]
[NativeAlias("Suity.Editor.AIGC.Flows.AigcSkillDocument")]
public class AigcSkillDocument : SAssetDocument<AigcSkillAssetBuilder>, IAigcSkill
{
    private SubFlowInstance _rootElement;

    private readonly StringProperty _skillName = new("SkillName", "Skill Name");

    private readonly StringProperty _description = new("Description", "Description");

    private readonly AssetProperty<ImageAsset> _iconSelection = new("Icon", "Icon");

    private readonly ValueProperty<Color> _colorSelection = new("Color", "Color");

    private readonly AssetProperty<ISubFlowDefAsset> _baseFlow
        = new("BaseFlow", "Base Execution Flow");

    private readonly AssetListProperty<ISubFlowAsset> _tools
        = new("Tools", "Tools");

    private readonly ValueProperty<bool> _isStartupPage
        = new("IsStartupPage", "Startup Page", false, "When enabled, this skill can be used as the startup page.");

    private readonly ValueProperty<bool> _useParentArticle =
        new("UseParentArticle", "Use Parent Article", false, "Use parent article as the article record for this page content. This setting will override the value of the base execution flow.");

    private readonly TextBlockProperty _overview
        = new("Overview", "Overview", string.Empty, "Used to provide a brief summary of the skill.");

    private readonly TextBlockProperty _promptHint
        = new("PromptHint", "Prompt Hint", string.Empty, "Used to provide guidance on how to use the skill effectively.");

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcSkillDocument"/> class.
    /// </summary>
    public AigcSkillDocument()
    {
        _skillName.ValueChanged += (s, e) =>
        {
            string id = _skillName.Value;
            if (NamingVerifier.VerifyNameSpace(id))
            {
                this.AssetBuilder.SetImportedId(id);
            }
            else
            {
                this.AssetBuilder.SetImportedId(null);
            }
        };

        _description.ValueChanged += (s, e) => this.AssetBuilder.SetDescription(_description.Value);
        _iconSelection.SelectionChanged += (s, e) => this.AssetBuilder.SetIconId(_iconSelection.Id);
        _colorSelection.ValueChanged += (s, e) => this.AssetBuilder.SetColor(_colorSelection.Value);

        _baseFlow.SelectionChanged += _baseFlow_SelectionChanged;
        _baseFlow.TargetUpdated += _baseFlow_TargetUpdated;
        _baseFlow.ListenEnabled = true;

        _tools.Property.WithExpand();
    }

    /// <inheritdoc/>
    protected internal override void OnDestroy()
    {
        _baseFlow.ListenEnabled = false;

        _description.ValueChanged -= (s, e) => this.AssetBuilder.SetDescription(_description.Value);
        _iconSelection.SelectionChanged -= (s, e) => this.AssetBuilder.SetIconId(_iconSelection.Id);
        _colorSelection.ValueChanged -= (s, e) => this.AssetBuilder.SetColor(_colorSelection.Value);

        _baseFlow.SelectionChanged -= _baseFlow_SelectionChanged;
        _baseFlow.TargetUpdated -= _baseFlow_TargetUpdated;

        base.OnDestroy();
    }

    #region Props

    /// <summary>
    /// Gets the name of the skill. Falls back to the file name if no explicit name is set.
    /// </summary>
    public string Name
    {
        get
        {
            string name = _skillName.Text;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (FileName.FullPath is { } fullPath && !string.IsNullOrWhiteSpace(fullPath))
            {
                return Path.GetFileNameWithoutExtension(fullPath);
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the description text of the skill.
    /// </summary>
    public string Description => _description.Text;

    /// <summary>
    /// Gets the overview text of the skill.
    /// </summary>
    public string Overview => _overview.Text;

    /// <summary>
    /// Gets the prompt hint text for the skill, which can be used to provide guidance on how to use the skill effectively.
    /// </summary>
    public string PromptHint => _promptHint.Text;

    /// <summary>
    /// Gets or sets the root page instance for this skill.
    /// </summary>
    public SubFlowInstance Instance
    {
        get => _rootElement;
        set
        {
            if (ReferenceEquals(_rootElement, value))
            {
                return;
            }

            if (_rootElement != null)
            {
                _rootElement.ResultOutput -= _rootElement_ResultOutput;
                _rootElement.RefreshRequesting -= _rootElement_RefreshRequesting;
            }

            _rootElement = value;

            if (_rootElement != null)
            {
                _rootElement.ResultOutput += _rootElement_ResultOutput;
                _rootElement.RefreshRequesting += _rootElement_RefreshRequesting;
            }
        }
    }

    /// <summary>
    /// Gets the base execution flow page definition asset.
    /// </summary>
    public ISubFlowDefAsset BaseFlow => _baseFlow.Target;

    /// <summary>
    /// Gets the base execution flow as a diagram item.
    /// </summary>
    public SubFlowDefinitionDiagramItem BaseFlowPage => (_baseFlow.Target as SubFlowDefinitionAsset)?.GetDiagramItem();
    #endregion

    #region IAigcSkill

    /// <summary>
    /// Gets the skill name, alias for <see cref="Name"/>.
    /// </summary>
    public string SkillName => this.Name;

    /// <summary>
    /// Gets the tooltip text for the skill, using overview if available, otherwise description.
    /// </summary>
    public string SkillTooltips
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_overview.Text))
            {
                return _overview.Text;
            }

            return _description.Text;
        }
    }

    /// <summary>
    /// Gets the collection of tools associated with this skill.
    /// </summary>
    public IEnumerable<ISubFlowAsset> Tools => _tools.List.Select(o => o.Target).SkipNull();

    /// <summary>
    /// Gets a value indicating whether this skill can be used as a startup page.
    /// </summary>
    public bool IsStartupPage => _isStartupPage.Value;

    /// <summary>
    /// Gets a value indicating whether to use the parent article as the article record for this page content.
    /// </summary>
    public bool UseParentArticle => _useParentArticle.Value;

    /// <summary>
    /// Attempts to retrieve a parameter value by name from the skill instance.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">When this method returns, contains the parameter value if found; otherwise, null.</param>
    /// <returns>True if the parameter was found; otherwise, false.</returns>
    public bool TryGetParameter(string name, out object value)
    {
        var instance = EnsureSkillInstance();
        if (instance != null)
        {
            return instance.TryGetParameter(null, name, out value);
        }

        value = null;
        return false;
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _skillName.Sync(sync);
        _description.Sync(sync);
        _iconSelection.Sync(sync);
        _colorSelection.Sync(sync);

        _baseFlow.Sync(sync);
        _tools.Sync(sync);
        _isStartupPage.Sync(sync);
        _useParentArticle.Sync(sync);

        _overview.Sync(sync);
        _promptHint.Sync(sync);

        var element = EnsureSkillInstance();
        sync.Sync("Page", element, SyncFlag.GetOnly | SyncFlag.AffectsParent);

        if (sync.IsSetterOf(_isStartupPage.Property.Name))
        {
            AssetBuilder.SetIsStartupPage(_isStartupPage.Value);
        }

        if (sync.IsSetter())
        {
            this.MarkDirtyAndSaveDelayed(this);
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (string.IsNullOrWhiteSpace(_skillName.Text))
        {
            if (FileName.FullPath is { } fullPath && !string.IsNullOrWhiteSpace(fullPath))
            {
                string hintName = Path.GetFileNameWithoutExtension(fullPath);
                _skillName.Property.WithHintText(hintName);
            }
        }

        setup.LabelWithIcon("Properties", CoreIconCache.Field);
        _skillName.InspectorField(setup);
        _description.InspectorField(setup);
        _overview.InspectorField(setup);
        _promptHint.InspectorField(setup);
        _iconSelection.InspectorField(setup);
        _colorSelection.InspectorField(setup);

        setup.LabelWithIcon("Skill", CoreIconCache.Skill);
        _baseFlow.InspectorField(setup);
        _tools.InspectorField(setup);
        _isStartupPage.InspectorField(setup);
        _useParentArticle.InspectorField(setup);

        setup.LabelWithIcon("Skill Settings", CoreIconCache.Page);
        setup.InspectorField(Instance, new ViewProperty("Page", "Skill Parameters").WithExpand());
    }

    /// <summary>
    /// Gets the underlying page definition for this skill.
    /// </summary>
    /// <returns>The <see cref="ISubFlowDef"/> definition, or null if not available.</returns>
    public ISubFlowDef GetPageDefinition() => _baseFlow.Target?.GetBaseDefinition();

    /// <summary>
    /// Ensures that a skill instance exists, building one if necessary.
    /// </summary>
    /// <returns>The current <see cref="SubFlowInstance"/> for this skill.</returns>
    public SubFlowInstance EnsureSkillInstance()
    {
        if (Instance is null)
        {
            BuildInstance();
        }

        return Instance;
    }

    private void BuildInstance()
    {
        if (BaseFlowPage is { } page)
        {
            var last = Instance;

            if (last?.DiagramItem == page)
            {
                last.Build();
            }
            else
            {
                var option = new PageElementOption
                {
                    Mode = PageElementMode.Skill,
                    Owner = this,
                };

                var newInstance = new SubFlowInstance(page, option);
                if (last != null)
                {
                    newInstance.UpdateFromOther(last);
                }

                Instance = newInstance;
            }
        }
        else
        {
            Instance = null;
        }
    }

    private void _baseFlow_SelectionChanged(object sender, EventArgs e)
    {
        QueuedAction.Do(BuildInstance);

        AssetBuilder.SetBaseFlow(_baseFlow.Target);
    }

    private void _baseFlow_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        QueuedAction.Do(() =>
        {
            BuildInstance();
        });
    }

    private void _rootElement_RefreshRequesting(object sender, EventArgs e)
    {
    }

    private void _rootElement_ResultOutput(object sender, EventArgs e)
    {
    }

}

/// <summary>
/// Asset builder for <see cref="AigcSkillAsset"/>, handling updates to base flow and startup page settings.
/// </summary>
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AigcSkillAssetBuilder")]
public class AigcSkillAssetBuilder : AssetBuilder<AigcSkillAsset>
{
    ISubFlowDefAsset _baseFlow;
    bool _startupPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcSkillAssetBuilder"/> class.
    /// </summary>
    public AigcSkillAssetBuilder()
    {
        AddAutoUpdate(nameof(AigcSkillAsset.BaseFlow), v => v.BaseFlow = _baseFlow);
        AddAutoUpdate(nameof(AigcSkillAsset.IsStartupPage), v => v.IsStartupPage = _startupPage);
    }

    /// <summary>
    /// Sets the base execution flow for the skill asset.
    /// </summary>
    /// <param name="baseflow">The base flow page definition asset.</param>
    public void SetBaseFlow(ISubFlowDefAsset baseflow)
    {
        _baseFlow = baseflow;
        this.UpdateAuto(nameof(AigcSkillAsset.BaseFlow));
    }

    /// <summary>
    /// Sets whether this skill is configured as a startup page.
    /// </summary>
    /// <param name="isStartupPage">True if this is a startup page; otherwise, false.</param>
    public void SetIsStartupPage(bool isStartupPage)
    {
        _startupPage = isStartupPage;
        this.UpdateAuto(nameof(AigcSkillAsset.IsStartupPage));
    }
}

/// <summary>
/// Represents a skill asset that can be used as a tool and contains a page definition.
/// </summary>
[NativeType(Name = "AigcSkillAsset", Description = "Skill Asset", CodeBase = "*AIGC", Icon = "*CoreIcon|Skil", Color = FlowColors.Agent)]
public class AigcSkillAsset : Asset, IViewObject, IInspectorEditNotify, ISubFlowAsset, IHasSkill
{
    readonly EditorAssetRef<ISubFlowDefAsset> _baseFlow = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcSkillAsset"/> class.
    /// </summary>
    public AigcSkillAsset()
    {
        UpdateAssetTypes(typeof(ISubFlowAsset));

        base.AddUpdateRelationship(_baseFlow);
    }

    /// <summary>
    /// Gets the base execution flow page definition asset.
    /// </summary>
    public ISubFlowDefAsset BaseFlow
    {
        get => _baseFlow.Target;
        internal set => _baseFlow.Target = value;
    }

    /// <summary>
    /// Gets a value indicating whether this skill is configured as a startup page.
    /// </summary>
    public bool IsStartupPage { get; internal set; }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Skill;

    /// <inheritdoc/>
    public override bool CanExportToLibrary => true;

    /// <summary>
    /// Synchronizes the asset properties with the given sync context.
    /// </summary>
    /// <param name="sync">The property sync interface.</param>
    /// <param name="context">The sync context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        if (this.GetDocument<AigcSkillDocument>() is { } doc)
        {
            (doc as ISyncObject)?.Sync(sync, context);

            if (sync.IsSetter())
            {
                doc.MarkDirtyAndSaveDelayed(this);
                this.NotifyUpdated(true);
            }
        }

        sync.Sync(nameof(Id), Id, SyncFlag.GetOnly);
        sync.Sync(nameof(AssetKey), AssetKey, SyncFlag.GetOnly);
        sync.Sync(nameof(NativeTypeName), NativeTypeName, SyncFlag.GetOnly);
    }

    /// <summary>
    /// Sets up the view for this asset in the inspector.
    /// </summary>
    /// <param name="setup">The view object setup interface.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        setup.LabelWithIcon("Asset", CoreIcon.Asset);
        setup.InspectorField(Id, new ViewProperty(nameof(Id)) { ReadOnly = true });
        setup.InspectorField(AssetKey, new ViewProperty(nameof(AssetKey), "Asset Key") { ReadOnly = true });
        setup.InspectorField(NativeTypeName, new ViewProperty(nameof(NativeTypeName), "Asset Type") { ReadOnly = true });

        if (this.GetDocument<AigcSkillDocument>() is { } doc)
        {
            (doc as IViewObject)?.SetupView(setup);
        }
    }

    #region IInspectorEditNotify

    /// <summary>
    /// Notifies that the inspector has been edited, triggering a save.
    /// </summary>
    void IInspectorEditNotify.NotifyInspectorEdited()
    {
        var doc = this.GetDocument<AigcSkillDocument>();
        if (doc != null)
        {
            doc.MarkDirty(this);
            doc.Entry.SaveDelayed();

            this.NotifyUpdated(true);
        }
    }

    #endregion

    #region IAigcToolAsset

    /// <summary>
    /// Gets the base page definition from the associated skill document.
    /// </summary>
    /// <returns>The <see cref="ISubFlowDef"/> definition, or null.</returns>
    public ISubFlowDef GetBaseDefinition() => this.GetDocument<AigcSkillDocument>()?.GetPageDefinition();

    /// <summary>
    /// Gets the skill definition from the associated skill document.
    /// </summary>
    /// <returns>The <see cref="IAigcSkill"/> definition, or null.</returns>
    public IAigcSkill GetSkill() => this.GetDocument<AigcSkillDocument>();

    /// <summary>
    /// Creates a new page instance for this tool asset with the specified options.
    /// </summary>
    /// <param name="option">The page element options.</param>
    /// <returns>A new <see cref="ISubFlowInstance"/>, or null if creation fails.</returns>
    public ISubFlowInstance CreateInstance(PageElementOption option)
    {
        if (this.GetDocument<AigcSkillDocument>() is not { } doc)
        {
            return null;
        }

        if (doc.BaseFlowPage is not { } toolPageItem)
        {
            return null;
        }

        if (doc.EnsureSkillInstance() is not { } root)
        {
            return null;
        }

        var instance = new SkillSubFlowInstance(toolPageItem, option, this);

        string skillName = doc.Name;
        string description = doc.Overview;
        if (string.IsNullOrWhiteSpace(description))
        {
            description = doc.Description;
        }

        instance.UpdateFromOther(root);

        return instance;
    }

    #endregion
}
