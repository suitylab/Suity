using Suity.Editor.Flows;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using Suity.Views.Im.PropertyEditing;
using System.Drawing;

namespace Suity.Editor;

/// <summary>
/// Editor plugin that manages view-related settings including language, debugging options,
/// UI behavior preferences, and property editor configuration.
/// </summary>
public class ViewPlugin : EditorPlugin, IViewObject
{
    /// <summary>
    /// Gets the singleton instance of <see cref="ViewPlugin"/>.
    /// </summary>
    public static ViewPlugin Instance { get; private set; }

    private readonly ValueProperty<LocalizedLanguage> _language
        = new(nameof(Language), "Language", LocalizedLanguage.en);

    private readonly ValueProperty<bool> _debugShowFullTypeName
        = new(nameof(DebugShowFullTypeName), "Show Node Type Full Name", false, "Show the full type name of nodes in the property editor");

    private readonly ValueProperty<bool> _autoLocateInPorject
        = new(nameof(AutoLocateInProject), "Auto Locate In Project", false, "Automatically locate in project when switching document views");

    private readonly ValueProperty<bool> _separateBaseFields
        = new(nameof(SeparateBaseFields), "Separate Base Class Fields", false, "Explicitly separate base class field display in the property editor");

    private readonly ValueProperty<bool> _cacheExpandableNodeContent
        = new(nameof(CacheExpandableNodeContent), "Cache Expandable Node Content", false, "Cache expanded node content as images to speed up node graph browsing.");

    private readonly ValueProperty<bool> _customInspectorGuiEnabled
        = new(nameof(CustomInspectorGuiEnabled), "Custom Inspector GUI", true, "Show third-party custom inspector GUI");

    private readonly ValueProperty<bool> _detailedArrayElementToolButton
        = new(nameof(DetailedArrayElementToolButton), "Detailed Array Element Buttons", false, "Show more detailed array element tool buttons in the property editor.");

    private readonly ValueProperty<bool> _debugImGui
        = new(nameof(DebugImGui), "Debug ImGui", false);

    /// <summary>
    /// Gets the current localization language setting.
    /// </summary>
    public LocalizedLanguage Language => _language.Value;
    /// <summary>
    /// Gets a value indicating whether to automatically locate files in the project when switching document views.
    /// </summary>
    public bool AutoLocateInProject => _autoLocateInPorject.Value;
    /// <summary>
    /// Gets a value indicating whether to separate base class fields in the property editor.
    /// </summary>
    public bool SeparateBaseFields => _separateBaseFields.Value;
    /// <summary>
    /// Gets a value indicating whether to cache expanded node content as images for performance.
    /// </summary>
    public bool CacheExpandableNodeContent => _cacheExpandableNodeContent.Value;
    /// <summary>
    /// Gets a value indicating whether custom inspector GUI is enabled.
    /// </summary>
    public bool CustomInspectorGuiEnabled => _customInspectorGuiEnabled.Value;
    /// <summary>
    /// Gets a value indicating whether to show detailed array element tool buttons.
    /// </summary>
    public bool DetailedArrayElementToolButton => _detailedArrayElementToolButton.Value;

    /// <summary>
    /// Gets a value indicating whether to show full type names in debug mode.
    /// </summary>
    public bool DebugShowFullTypeName => _debugShowFullTypeName.Value;
    /// <summary>
    /// Gets a value indicating whether ImGui debug drawing is enabled.
    /// </summary>
    public bool DebugImGui => _debugImGui.Value;


    /// <inheritdoc/>
    public override string Description => "View";
    /// <inheritdoc/>
    public override Image Icon => CoreIconCache.View;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewPlugin"/> class.
    /// Sets up language synchronization and initializes the singleton instance.
    /// </summary>
    public ViewPlugin()
    {
        Instance ??= this;

        _language.ValueChanged += (s, e) => 
        {
            // Sync language configuration
            string language = _language.Value.ToString().Replace('_', '-');
            //EditorServices.SystemLog.AddLog("Initailize localization...");
            LocalizeManager.Instance.UpdateLanguage(language);
        };

        // Sync language configuration
        _language.Value = LocalizeManager.ParseLanguage(LocalizeManager.Instance.LanguageCode);
    }

    #region Sync

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        // Synchronize all view plugin settings
        _language.Sync(sync);
        _autoLocateInPorject.Sync(sync);
        _separateBaseFields.Sync(sync);
        _cacheExpandableNodeContent.Sync(sync);
        _customInspectorGuiEnabled.Sync(sync);
        _detailedArrayElementToolButton.Sync(sync);

        _debugShowFullTypeName.Sync(sync);
        _debugImGui.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateProperty();
        }
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        // Sync language configuration
        _language.Value = LocalizeManager.ParseLanguage(LocalizeManager.Instance.LanguageCode);
        //TODO: Add localization
        // _language.InspectorField(setup);
        
        _autoLocateInPorject.InspectorField(setup);
        _separateBaseFields.InspectorField(setup);
        _cacheExpandableNodeContent.InspectorField(setup);

        //if (PropertyGridExternalBK.Instance.HasCustomDrawer())
        //{
        //    _customInspectorGuiEnabled.InspectorField(setup);
        //}
        //_detailedArrayElementToolButton.InspectorField(setup);

        setup.LabelWithIcon("Debug", CoreIconCache.Debug);
        _debugShowFullTypeName.InspectorField(setup);
        _debugImGui.InspectorField(setup);
    }

    #endregion

    /// <summary>
    /// Applies the current property values to the corresponding editor and UI settings.
    /// </summary>
    private void UpdateProperty()
    {
        FlowNode.ShowFullTypeName = _debugShowFullTypeName.Value;
        ImExpandableNode.RenderSnapshot = _cacheExpandableNodeContent.Value;
        ImGuiBK.GlobalDebugDraw = _debugImGui.Value;

        if (PropertyGridExternalBK.Instance._customDrawerEnabled != _customInspectorGuiEnabled.Value)
        {
            PropertyGridExternalBK.Instance._customDrawerEnabled = _customInspectorGuiEnabled.Value;
            EditorUtility.Inspector?.UpdateInspector();
        }

        if (PropertyFieldExternalBK.Instance.DetailedArrayElementToolButton != _detailedArrayElementToolButton.Value)
        {
            PropertyFieldExternalBK.Instance.DetailedArrayElementToolButton = _detailedArrayElementToolButton.Value;
            //EditorUtility.Inspector?.UpdateInspector();
        }
    }
}
