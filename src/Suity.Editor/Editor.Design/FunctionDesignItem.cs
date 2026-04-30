using Suity.Editor.Documents.Linked;
using Suity.Editor.Expressions;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Design;

/// <summary>
/// Represents a list of function parameters.
/// </summary>
[NativeAlias]
public class FunctionParameterList : SNamedFieldList<FunctionParameter>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionParameterList"/> class.
    /// </summary>
    public FunctionParameterList(string fieldName, SNamedItem parentElement)
        : base(fieldName, parentElement)
    {
    }
}

/// <summary>
/// Base class for function design items.
/// </summary>
public abstract class FunctionDesignItem : DesignItem<DFunctionBuilder>,
    IPreviewDisplay,
    INavigable,
    IFunction
{
    private readonly FunctionParameterList _fieldList;

    private bool _isUser;
    private readonly SArray _actions;
    private VariableOwnerTextBlock _brief = new();
    private readonly ITypeDesign _returnType;
    private bool _actionMode;

    public FunctionDesignItem()
    {
        _fieldList = new("Parameters", this)
        {
            FieldDescription = "Parameters",
            FieldIcon = CoreIconCache.Field
        };

        AddPrimaryFieldList(_fieldList);

        _actions = new SArray(NativeTypes.ActionArrayType)
        {
            Context = this
        };

        _returnType = DTypeManager.Instance.CreateTypeDesign(this);
        _returnType.BaseType.SelectedKey = NativeTypes.VoidTypeName;
        _returnType.FieldTypeChanged += (s, e) =>
        {
            AssetBuilder.SetReturnType(_returnType.FieldType ?? TypeDefinition.Empty);
        };

        AccessMode = AssetAccessMode.Public;
        AssetBuilder.SetReturnType(_returnType.FieldType ?? TypeDefinition.Empty);

        base.ShowRenderTargets = false;
        base.ShowUsings = true;
    }

    /// <summary>
    /// Gets or sets whether this function supports parameters.
    /// </summary>
    public bool SupportParameter { get; protected set; }

    /// <summary>
    /// Gets the return type design for this function.
    /// </summary>
    public ITypeDesign ReturnType => _returnType;

    /// <summary>
    /// Gets or sets whether this function operates in action mode.
    /// </summary>
    public bool ActionMode
    {
        get => _actionMode;
        set
        {
            if (_actionMode != value)
            {
                _actionMode = value;
                AssetBuilder.SetActionMode(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this function is user-defined.
    /// </summary>
    public bool IsUser
    {
        get => _isUser;
        set
        {
            if (_isUser != value)
            {
                _isUser = value;
            }
            ShowRenderTargets = value;
        }
    }

    /// <summary>
    /// Gets the number of parameters in this function.
    /// </summary>
    public int ParameterCount => _fieldList.OfType<FunctionParameter>().Count(o => o.IsParameter);

    /// <summary>
    /// Gets the actions associated with this function.
    /// </summary>
    internal SArray Actions => _actions;

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        IsUser = sync.Sync("IsUser", IsUser);
        sync.Sync("ReturnType", ReturnType, SyncFlag.GetOnly);
        _brief = sync.Sync("Brief", _brief, SyncFlag.NotNull);
        sync.Sync(_fieldList.FieldName, _fieldList, SyncFlag.GetOnly);
        sync.Sync("Actions", _actions, SyncFlag.GetOnly);
        ActionMode = sync.Sync("ActionMode", ActionMode);

        _brief.VariableOwner = this;
        if (sync.IsSetterOf("Brief"))
        {
            AssetBuilder.SetBrief(_brief.Text);
        }
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        if (setup.SupportDetailTreeView())
        {
            setup.DetailTreeViewField(_fieldList, new ViewProperty(_fieldList.FieldName, "Parameters"));
            if (!_isUser)
            {
                setup.DetailTreeViewField(_actions, new ViewProperty("Actions", "Actions", CoreIconCache.Action));
            }
        }

        base.OnSetupView(setup);

        if (setup.SupportInspector())
        {
            setup.InspectorField(IsUser, new ViewProperty("IsUser", "External Implementation"));
            setup.InspectorField(ReturnType, new ViewProperty("ReturnType", "Return Type") { Expand = true });
            setup.InspectorField(_brief, new ViewProperty("Brief", "Summary"));
        }
    }

    protected override Image OnGetIcon()
    {
        var icon = SelectedIcon;
        if (icon != null)
        {
            return icon;
        }
        else
        {
            return _isUser ? CoreIconCache.FunctionUser : CoreIconCache.Function;
        }
    }

    protected internal override string OnGetSuggestedPrefix() => "Function";

    protected override TextStatus OnGetTextStatus() => _isUser ? TextStatus.Import : TextStatus.Normal;

    public override Image TypeIcon => CoreIconCache.Function;
    public override Color? TypeColor => DFunction.FunctionTypeColor;

    protected internal override void OnFieldListItemAdded(NamedFieldList list, NamedField item, bool isNew)
    {
        base.OnFieldListItemAdded(list, item, isNew);

        if (item is FunctionParameter parameter)
        {
            if (!SupportParameter)
            {
                parameter.IsParameter = false;
            }

            UpdateParameter(parameter, false, isNew);
        }
    }

    protected internal override void OnFieldListItemRemoved(NamedFieldList list, NamedField item)
    {
        if (item is FunctionParameter parameter)
        {
            AssetBuilder.RemoveField(parameter.Name);
        }
    }

    protected internal override void OnFieldListItemUpdated(NamedFieldList list, NamedField item, bool forceUpdate)
    {
        base.OnFieldListItemUpdated(list, item, forceUpdate);

        if (item is FunctionParameter parameter)
        {
            UpdateParameter(parameter, forceUpdate, false);
        }
    }

    protected internal override void OnFieldListItemRenamed(NamedFieldList list, NamedField item, string oldName)
    {
        if (item is FunctionParameter parameter)
        {
            AssetBuilder.RenameField(oldName, parameter.Name);
            if (_brief.Rename(oldName, parameter.Name))
            {
                AssetBuilder.SetBrief(_brief.Text);
            }
        }
    }

    protected internal override void OnFieldListArrageItem(NamedFieldList list)
    {
        base.OnFieldListArrageItem(list);

        int index = 0;
        FunctionParameter[] items = _fieldList.OfType<FunctionParameter>().ToArray();

        foreach (var p in items)
        {
            AssetBuilder.UpdateFieldDisplay(p.Name, index: index, description: p.Description);

            index++;
        }

        AssetBuilder.Sort();
    }

    internal DFunction Product => base.AssetBuilder?.Asset;

    private void UpdateParameter(FunctionParameter p, bool forceUpdate, bool isNew)
    {
        // Convert to function type
        var varFuncType = p.VariableType.FieldType.MakeAbstractFunctionType();

        var connAttrs = p.IsConnector ? SingleAttributes<ConnectorAttribute>.Instance : null;

        AssetBuilder.AddOrUpdateField(
            p.Name,
            varFuncType,
            p.AccessMode,
            null,
            p.Optional,
            null,
            connAttrs,
            forceUpdate: forceUpdate,
            isNew: isNew,
            recorededId: p.RecorededFieldId
            );

        AssetBuilder.UpdateFieldDisplay(p.Name, description: p.Description);

        AssetBuilder.NotifyUpdated(true);
    }

    #region IPreviewDisplay

    public override string PreviewText => ReturnType.DisplayText;

    public override object PreviewIcon => ReturnType.Icon;

    #endregion

    #region INavigable

    object INavigable.GetNavigationTarget() => ReturnType.FieldType;

    #endregion

    #region IVariableContainer Members

    /// <summary>
    /// Gets the asset key for the variable container.
    /// </summary>
    string IVariableContainer.AssetKey
    {
        get
        {
            if (AssetBuilder.Parent is AssetBuilder parentBuilder)
            {
                return KeyCode.Combine(parentBuilder.LocalName ?? string.Empty, this.Name);
            }
            else
            {
                return KeyCode.Combine(string.Empty, this.Name);
            }
        }
    }

    IEnumerable<IVariable> IVariableContainer.Variables => _fieldList.OfType<IVariable>();

    IVariable IVariableContainer.GetVariable(string name) => _fieldList.GetItem(name) as IVariable;

    IEnumerable<IMember> IMemberContainer.Members => _fieldList.OfType<IMember>();

    IMember IMemberContainer.GetMember(string name) => _fieldList.GetItem(name) as IMember;

    int IMemberContainer.MemberCount => _fieldList.Count;

    #endregion

    #region IFunction Members

    string INamed.Name => this.Name;

    bool IFunction.IsPublic => AccessMode == AssetAccessMode.Public;

    bool IFunction.IsUser => _isUser;

    TypeDefinition IFunction.ReturnType => this.ReturnType.FieldType;

    SArray IFunction.Actions => this.Actions;

    #endregion
}