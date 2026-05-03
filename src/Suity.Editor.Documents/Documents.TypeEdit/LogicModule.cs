using Suity.Drawing;
using Suity.Editor.Design;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// A field list for logic module components.
/// </summary>
[NativeAlias]
public class LogicModuleParameterList : SNamedFieldList<LogicModuleComponent>, IHasObjectCreationGUI
{
    static readonly ObjectCreationOption[] _options = [new(typeof(LogicModule), "Logic module")];

    readonly LogicModule _logicModule;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogicModuleParameterList"/> class.
    /// </summary>
    /// <param name="fieldName">The field name for this list.</param>
    /// <param name="logicModule">The owning logic module.</param>
    public LogicModuleParameterList(string fieldName, LogicModule logicModule) 
        : base(fieldName, logicModule)
    {
        _logicModule = logicModule ?? throw new ArgumentNullException(nameof(logicModule));
    }

    /// <inheritdoc/>
    protected override NamedField OnCreateNewItem() => new LogicModuleComponent();

    /// <inheritdoc/>
    protected override async Task<NamedField> OnGuiCreateItemAsync(Type typeHint)
    {
        var result = await DTypeManager.Instance.GetTypes<DStruct>().WithFilter(AssetFilters.Default).ShowSelectionGUIAsync("Select Component");
        if (!result.IsSuccess)
        {
            return null;
        }

        if (result.Item is not DStruct s)
        {
            return null;
        }

        if (this.OfType<LogicModuleComponent>().Any(o => o.ComponentId == s.Id))
        {
            await DialogUtility.ShowMessageBoxAsync("Component already exists");
            return null;
        }

        var c = new LogicModuleComponent
        {
            ComponentId = s.Id
        };

        return c;
    }

    #region IHasObjectCreationGUI

    /// <inheritdoc/>
    public IEnumerable<ObjectCreationOption> CreationOptions => _options;

    /// <inheritdoc/>
    public async Task<object> GuiCreateObjectAsync(Type typeHint = null)
    {
        return await OnGuiCreateItemAsync(typeHint);
    }

    #endregion
}

/// <summary>
/// Represents a logic module type that contains component references.
/// </summary>
[NativeAlias]
[DisplayText("Logic Module", "*CoreIcon|LogicModule")]
[DisplayOrder(950)]
public class LogicModule : TypeDesignItem<DLogicModuleBuilder>,
    IMemberContainer
{
    private readonly LogicModuleParameterList _fieldList;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogicModule"/> class.
    /// </summary>
    public LogicModule()
    {
        _fieldList = new LogicModuleParameterList("Parameters", this);
        AddPrimaryFieldList(_fieldList);

        ShowRenderTargets = true;
        ShowUsings = true;

        AssetBuilder.ComponentGetter = () => _fieldList.OfType<LogicModuleComponent>().Select(o => o.ComponentId).ToArray();
    }

    /// <inheritdoc/>
    public override Color? TypeColor => DLogicModule.LogicModuleColor;

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix()
    {
        return "LogicModule";
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        if (sync.IsSetter())
        {
            AssetBuilder.BeginUpdate();
        }

        sync.Sync(_fieldList.FieldName, _fieldList, SyncFlag.GetOnly);

        if (sync.IsSetter())
        {
            AssetBuilder.EndUpdate();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.DetailTreeViewField(_fieldList, new ViewProperty(_fieldList.FieldName, "Components"));

        base.OnSetupView(setup);
    }

    /// <summary>
    /// Creates a new logic module component by showing a selection dialog.
    /// </summary>
    /// <returns>The created component, or null if cancelled.</returns>
    private async Task<LogicModuleComponent> CreateComponent()
    {
        var result = await DTypeManager.Instance.GetTypes<DStruct>().WithFilter(AssetFilters.Default).ShowSelectionGUIAsync("Select Component");
        if (!result.IsSuccess)
        {
            return null;
        }

        if (result.Item is not DStruct s)
        {
            return null;
        }

        if (_fieldList.OfType<LogicModuleComponent>().Any(o => o.ComponentId == s.Id))
        {
            await DialogUtility.ShowMessageBoxAsync("Component already exists");
            return null;
        }

        var c = new LogicModuleComponent
        {
            ComponentId = s.Id
        };

        return c;
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemAdded(NamedFieldList list, NamedField item, bool isNew)
    {
        base.OnFieldListItemAdded(list, item, isNew);
        if (item is LogicModuleComponent)
        {
            AssetBuilder.UpdateComponents();
        }
    }

    /// <inheritdoc/>
    protected override void OnFieldListItemRemoved(NamedFieldList list, NamedField item)
    {
        base.OnFieldListItemRemoved(list, item);
        if (item is LogicModuleComponent)
        {
            AssetBuilder.UpdateComponents();
        }
    }

    /// <inheritdoc/>
    protected override void OnFieldListArrageItem(NamedFieldList list)
    {
        base.OnFieldListArrageItem(list);

        AssetBuilder.UpdateComponents();
    }

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon()
    {
        return base.OnGetIcon() ?? CoreIconCache.LogicModule;
    }

    /// <inheritdoc/>
    public override string PreviewText => "Logic Module";

    /// <inheritdoc/>
    public override ImageDef TypeIcon => CoreIconCache.LogicModule;

    #region IMemberContainer

    /// <inheritdoc/>
    IEnumerable<IMember> IMemberContainer.Members => _fieldList.OfType<IMember>();

    /// <inheritdoc/>
    int IMemberContainer.MemberCount => _fieldList.Count;

    /// <inheritdoc/>
    IMember IMemberContainer.GetMember(string name) => _fieldList.GetItem(name) as IMember;

    #endregion
}

/// <summary>
/// Represents a component reference within a logic module.
/// </summary>
public class LogicModuleComponent : SNamedField, INavigable, IViewDoubleClickAction, IMember
{
    internal AssetSelection<DStruct> _compSelection = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LogicModuleComponent"/> class.
    /// </summary>
    public LogicModuleComponent()
    {
    }

    /// <summary>
    /// Gets or sets the ID of the referenced component.
    /// </summary>
    public Guid ComponentId
    {
        get => _compSelection?.Id ?? Guid.Empty;
        internal set
        {
            _compSelection ??= new AssetSelection<DStruct>();
            _compSelection.Id = value;
        }
    }

    /// <inheritdoc/>
    protected override string OnGetDisplayText()
    {
        return _compSelection?.ToString() ?? Name;
    }

    /// <inheritdoc/>
    protected override ImageDef OnGetIcon()
    {
        return _compSelection?.Icon ?? CoreIconCache.Object;
    }

    /// <inheritdoc/>
    protected override bool OnCanEditText() => false;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);
        _compSelection = sync.Sync("ParameterType", _compSelection, SyncFlag.NotNull);
    }

    /// <summary>
    /// Gets the parent logic module.
    /// </summary>
    /// <returns>The parent logic module.</returns>
    public LogicModule GetLogicModule() => GetParent<LogicModule>();

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return _compSelection.Target;
    }

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick()
    {
        EditorUtility.NavigateTo(ComponentId);
    }

    #region IMember

    /// <inheritdoc/>
    IMemberContainer IMember.Container => GetLogicModule();

    /// <inheritdoc/>
    public string SelectionKey => this.Name;

    /// <inheritdoc/>
    public string DisplayText => this.Name;

    #endregion
}
