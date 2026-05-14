using Suity.Collections;
using Suity.Drawing;
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
/// Document representing an Sub-flow preset, containing page definitions, tools, and configuration.
/// </summary>
[DocumentFormat(FormatName = "SubFlowPreset", Extension = "spreset", DisplayText = "Preset", Icon = "*CoreIcon|Preset", Categoty = "AIGC", CanShowView = false)]
[NativeAlias("Suity.Editor.AIGC.Flows.Pages.AigcSkillDocument")]
[NativeAlias("Suity.Editor.AIGC.Flows.AigcSkillDocument")]
[NativeAlias("Suity.Editor.Flows.TaskPages.AigcSkillDocument")]
public class SubFlowPresetDocument : SAssetDocument<SubFlowPresetAssetBuilder>, ISubFlowPreset
{
    private SubFlowInstance _rootElement;

    private readonly StringProperty _presetName = new("PresetName", "Preset Name");

    private readonly StringProperty _description = new("Description", "Description");

    private readonly AssetProperty<ImageAsset> _iconSelection = new("Icon", "Icon");

    private readonly ValueProperty<Color> _colorSelection = new("Color", "Color");

    private readonly AssetProperty<ISubFlowDefAsset> _baseFlow
        = new("BaseFlow", "Base Execution Flow");

    private readonly AssetListProperty<IPageAsset> _tools
        = new("Tools", "Tools");

    private readonly ValueProperty<bool> _isStartup
        = new("IsStartup", "Is Startup", false, "When enabled, this preset can be used as the startup page.");

    private readonly ValueProperty<bool> _useParentArticle =
        new("UseParentArticle", "Use Parent Article", false, "Use parent article as the article record for this page content. This setting will override the value of the base execution flow.");

    private readonly TextBlockProperty _overview
        = new("Overview", "Overview", string.Empty, "Used to provide a brief summary of the preset.");

    private readonly TextBlockProperty _promptHint
        = new("PromptHint", "Prompt Hint", string.Empty, "Used to provide guidance on how to use the preset effectively.");

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowPresetDocument"/> class.
    /// </summary>
    public SubFlowPresetDocument()
    {
        _presetName.ValueChanged += (s, e) =>
        {
            string id = _presetName.Value;
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
    /// Gets the name of the preset. Falls back to the file name if no explicit name is set.
    /// </summary>
    public string Name
    {
        get
        {
            string name = _presetName.Text;
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
    /// Gets the description text of the preset.
    /// </summary>
    public string Description => _description.Text;

    /// <summary>
    /// Gets the overview text of the preset.
    /// </summary>
    public string Overview => _overview.Text;

    /// <summary>
    /// Gets the prompt hint text for the preset, which can be used to provide guidance on how to use the preset effectively.
    /// </summary>
    public string PromptHint => _promptHint.Text;

    /// <summary>
    /// Gets or sets the root page instance for this preset.
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

    /// <inheritdoc/>
    public ISubFlowDefAsset BaseFlow => _baseFlow.Target;

    /// <summary>
    /// Gets the base execution flow as a diagram item.
    /// </summary>
    public SubFlowDefinitionDiagramItem BaseFlowPage => (_baseFlow.Target as SubFlowDefinitionAsset)?.GetDiagramItem();
    #endregion

    #region ISubFlowPreset

    /// <summary>
    /// Gets the preset name, alias for <see cref="Name"/>.
    /// </summary>
    public string PresetName => this.Name;

    /// <summary>
    /// Gets the tooltip text for the preset, using overview if available, otherwise description.
    /// </summary>
    public string PresetTooltips
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
    /// Gets the collection of tools associated with this preset.
    /// </summary>
    public IEnumerable<IPageAsset> Tools => _tools.List.Select(o => o.Target).SkipNull();

    /// <summary>
    /// Gets a value indicating whether this preset can be used as a startup page.
    /// </summary>
    public bool IsStartupPage => _isStartup.Value;

    /// <summary>
    /// Gets a value indicating whether to use the parent article as the article record for this page content.
    /// </summary>
    public bool UseParentArticle => _useParentArticle.Value;

    /// <summary>
    /// Attempts to retrieve a parameter value by name from the preset instance.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">When this method returns, contains the parameter value if found; otherwise, null.</param>
    /// <returns>True if the parameter was found; otherwise, false.</returns>
    public bool TryGetParameter(string name, out object value)
    {
        var instance = EnsureSubFlowInstance();
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

        _presetName.Sync(sync);
        _description.Sync(sync);
        _iconSelection.Sync(sync);
        _colorSelection.Sync(sync);

        _baseFlow.Sync(sync);
        _tools.Sync(sync);
        _isStartup.Sync(sync);
        _useParentArticle.Sync(sync);

        _overview.Sync(sync);
        _promptHint.Sync(sync);

        var element = EnsureSubFlowInstance();
        sync.Sync("Page", element, SyncFlag.GetOnly | SyncFlag.AffectsParent);

        if (sync.IsSetterOf(_isStartup.Property.Name))
        {
            AssetBuilder.SetIsStartupPage(_isStartup.Value);
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

        if (string.IsNullOrWhiteSpace(_presetName.Text))
        {
            if (FileName.FullPath is { } fullPath && !string.IsNullOrWhiteSpace(fullPath))
            {
                string hintName = Path.GetFileNameWithoutExtension(fullPath);
                _presetName.Property.WithHintText(hintName);
            }
        }

        setup.LabelWithIcon("Properties", CoreIconCache.Field);
        _presetName.InspectorField(setup);
        _description.InspectorField(setup);
        _overview.InspectorField(setup);
        _promptHint.InspectorField(setup);
        _iconSelection.InspectorField(setup);
        _colorSelection.InspectorField(setup);

        setup.LabelWithIcon("Preset", CoreIconCache.Preset);
        _baseFlow.InspectorField(setup);
        _tools.InspectorField(setup);
        _isStartup.InspectorField(setup);
        _useParentArticle.InspectorField(setup);

        setup.LabelWithIcon("Preset Settings", CoreIconCache.Page);
        setup.InspectorField(Instance, new ViewProperty("Page", "Preset Parameters").WithExpand());
    }

    /// <summary>
    /// Gets the underlying page definition for this preset.
    /// </summary>
    /// <returns>The <see cref="ISubFlow"/> definition, or null if not available.</returns>
    public ISubFlow GetPageDefinition() => _baseFlow.Target?.GetBaseDefinition();

    /// <summary>
    /// Ensures that a preset instance exists, building one if necessary.
    /// </summary>
    /// <returns>The current <see cref="SubFlowInstance"/> for this preset.</returns>
    public SubFlowInstance EnsureSubFlowInstance()
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
                var option = new PageCreateOption
                {
                    Mode = PageElementMode.Preset,
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
/// Asset builder for <see cref="SubFlowPresetAsset"/>, handling updates to base flow and startup page settings.
/// </summary>
public class SubFlowPresetAssetBuilder : AssetBuilder<SubFlowPresetAsset>
{
    ISubFlowDefAsset _baseFlow;
    bool _startupPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowPresetAssetBuilder"/> class.
    /// </summary>
    public SubFlowPresetAssetBuilder()
    {
        AddAutoUpdate(nameof(SubFlowPresetAsset.BaseFlow), v => v.BaseFlow = _baseFlow);
        AddAutoUpdate(nameof(SubFlowPresetAsset.IsStartup), v => v.IsStartup = _startupPage);
    }

    /// <summary>
    /// Sets the base execution flow for the preset asset.
    /// </summary>
    /// <param name="baseflow">The base flow page definition asset.</param>
    public void SetBaseFlow(ISubFlowDefAsset baseflow)
    {
        _baseFlow = baseflow;
        this.UpdateAuto(nameof(SubFlowPresetAsset.BaseFlow));
    }

    /// <summary>
    /// Sets whether this preset is configured as a startup page.
    /// </summary>
    /// <param name="isStartupPage">True if this is a startup page; otherwise, false.</param>
    public void SetIsStartupPage(bool isStartupPage)
    {
        _startupPage = isStartupPage;
        this.UpdateAuto(nameof(SubFlowPresetAsset.IsStartup));
    }
}

/// <summary>
/// Represents a preset asset that can be used as a tool and contains a page definition.
/// </summary>
[NativeType(Name = "SubFlowPresetAsset", Description = "Preset Asset", CodeBase = "SubFlow", Icon = "*CoreIcon|Skil", Color = FlowColors.Agent)]
public class SubFlowPresetAsset : Asset,
    IPageAsset,
    ISubFlowAsset,
    ISubFlowPresetAsset,
    IViewObject,
    IInspectorEditNotify
{
    readonly EditorAssetRef<ISubFlowDefAsset> _baseFlow = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowPresetAsset"/> class.
    /// </summary>
    public SubFlowPresetAsset()
    {
        UpdateAssetTypes(typeof(IPageAsset), typeof(ISubFlowAsset), typeof(ISubFlowPresetAsset));

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
    /// Gets a value indicating whether this preset is configured as a startup page.
    /// </summary>
    public bool IsStartup { get; internal set; }

    /// <inheritdoc/>
    public override ImageDef DefaultIcon => CoreIconCache.Preset;

    /// <inheritdoc/>
    public override bool CanExportToLibrary => true;

    /// <summary>
    /// Synchronizes the asset properties with the given sync context.
    /// </summary>
    /// <param name="sync">The property sync interface.</param>
    /// <param name="context">The sync context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        if (this.GetDocument<SubFlowPresetDocument>() is { } doc)
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

        if (this.GetDocument<SubFlowPresetDocument>() is { } doc)
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
        var doc = this.GetDocument<SubFlowPresetDocument>();
        if (doc != null)
        {
            doc.MarkDirty(this);
            doc.Entry.SaveDelayed();

            this.NotifyUpdated(true);
        }
    }

    #endregion

    #region IPageAsset

    /// <inheritdoc/>
    public IPageInstance CreatePageInstance(PageCreateOption option) => CreateSubFlowInstance(option);

    #endregion

    #region ISubFlowAsset

    /// <summary>
    /// Gets the base page definition from the associated preset document.
    /// </summary>
    /// <returns>The <see cref="ISubFlow"/> definition, or null.</returns>
    public ISubFlow GetBaseDefinition() => this.GetDocument<SubFlowPresetDocument>()?.GetPageDefinition();

    /// <summary>
    /// Gets the preset definition from the associated preset document.
    /// </summary>
    /// <returns>The <see cref="ISubFlowPreset"/> definition, or null.</returns>
    public ISubFlowPreset GetPresetDefinition() => this.GetDocument<SubFlowPresetDocument>();

    /// <summary>
    /// Creates a new page instance for this tool asset with the specified options.
    /// </summary>
    /// <param name="option">The page element options.</param>
    /// <returns>A new <see cref="ISubFlowInstance"/>, or null if creation fails.</returns>
    public ISubFlowInstance CreateSubFlowInstance(PageCreateOption option)
    {
        if (this.GetDocument<SubFlowPresetDocument>() is not { } doc)
        {
            return null;
        }

        if (doc.BaseFlowPage is not { } toolPageItem)
        {
            return null;
        }

        if (doc.EnsureSubFlowInstance() is not { } root)
        {
            return null;
        }

        var instance = new SubFlowInstance(toolPageItem, option, this);

        string presetName = doc.Name;
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
