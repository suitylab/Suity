using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Drawing;

namespace Suity.Editor.Design;

/// <summary>
/// Represents a parameter in a function design item.
/// </summary>
public class FunctionParameter : ParameterField, IVariable
{
    private bool _isParameter = true;
    private bool _isConnector = true;

    /// <summary>
    /// Gets the parent function that contains this parameter.
    /// </summary>
    public FunctionDesignItem ParentFunction => ParentSItem as FunctionDesignItem;

    /// <summary>
    /// Gets the asset field corresponding to this parameter.
    /// </summary>
    public override EditorObject AssetField => ParentFunction?.Product?.GetField(Name);

    /// <summary>
    /// Gets or sets whether this parameter is exposed as a public parameter.
    /// </summary>
    public bool IsParameter
    {
        get => _isParameter;
        set
        {
            if (_isParameter == value)
            {
                return;
            }

            _isParameter = value;
            NotifyFieldUpdated();
        }
    }

    /// <summary>
    /// Gets or sets whether this parameter is displayed as a connection port.
    /// </summary>
    public bool IsConnector
    {
        get => _isParameter && _isConnector;
        set
        {
            if (_isConnector == value)
            {
                return;
            }

            _isConnector = value;
            NotifyFieldUpdated();
        }
    }

    public AssetAccessMode AccessMode => IsParameter ? AssetAccessMode.Public : AssetAccessMode.Private;

    protected internal override string OnGetSuggestedPrefix() => "Parameter";

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        IsParameter = sync.Sync("IsParameter", IsParameter, SyncFlag.None, true);
        IsConnector = sync.Sync("IsConnector", IsConnector, SyncFlag.None, true);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (setup.SupportInspector())
        {
            setup.InspectorField(IsParameter, new ViewProperty("IsParameter", "External Parameter"));
            if (IsParameter)
            {
                setup.InspectorField(IsConnector, new ViewProperty("IsConnector", "Connection Port"));
            }
        }
    }

    protected override Image OnGetIcon()
    {
        ImageAsset icon = IconSelection.Target;
        if (icon != null)
        {
            return icon.GetIconSmall();
        }
        else
        {
            return IsParameter ? CoreIconCache.Field : CoreIconCache.Member;
        }
    }

    public override void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        base.CollectProblem(problems, intent);

        bool supportParameter = ParentFunction?.SupportParameter ?? false;

        if (!supportParameter && IsParameter)
        {
            problems.Add(new AnalysisProblem(TextStatus.Error, "Anonymous function cannot have public parameters"));
        }
    }

    #region IVariable

    TypeDefinition IVariable.VariableType => VariableType.FieldType;

    IMemberContainer IMember.Container => ParentFunction;
    string INamed.Name => this.Name;

    Guid IHasId.Id => ParentFunction?.Product?.GetField(Name)?.Id ?? Guid.Empty;

    string IVariable.DisplayName => this.DisplayText;

    #endregion
}