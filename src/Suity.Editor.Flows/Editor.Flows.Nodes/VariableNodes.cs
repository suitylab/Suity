using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Linq;

namespace Suity.Editor.Flows.Nodes;

#region GetVariable

/// <summary>
/// A flow node that retrieves the value of a temporary variable during chart computation.
/// </summary>
[SimpleFlowNodeStyle(Color = "#6FA72F")]
[DisplayText("Get Variable")]
[ToolTipsText("Get temporary variables in chart computation")]
public class GetVariable : VariableFlowNode
{
    private readonly ConnectorStringProperty _variableName
        = new("VariableName", "Variable Name");

    private readonly ConnectorValueProperty<FlowContextScopes> _scope
        = new("Scope", "Scope", default, "The scope for variable lookup. If not found in the specified scope, it will automatically search the parent scope.");

    private FlowNodeConnector _variable;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVariable"/> class.
    /// </summary>
    public GetVariable()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.Id = NativeTypes.StringType.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _variableName.Sync(sync);
        _scope.Sync(sync);
        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _variableName.InspectorField(setup, this);
        _scope.InspectorField(setup, this);
        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Output Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var type = GetTargetType();

        _variableName.AddConnector(this);
        _scope.AddConnector(this);
        _variable = AddDataOutputConnector("Variable", type.ToTypeName(), "Variable");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        string varName = _variableName.GetValue(compute, this);
        if (string.IsNullOrWhiteSpace(varName))
        {
            compute.SetValue(_variable, null);
            return;
        }

        // context already implements three-level (local, chart, global) automatic upward lookup.
        var scope = _scope.GetValue(compute, this);
        var value = compute.GetVariable(scope, this.Diagram, varName);

        compute.SetValue(_variable, value);
    }


    private TypeDefinition GetTargetType()
    {
        var type = _valueType.GetTypeDefinition();
        if (_isArray)
        {
            type = type.MakeArrayType();
        }

        return type;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (Diagram?.GetIsLinked(_variableName.Connector) == true)
        {
            return DisplayText;
        }

        string varName = _variableName.BaseValue;
        if (!string.IsNullOrWhiteSpace(varName))
        {
            return $"Get {varName} Variable";
        }
        
        return DisplayText;
    }
}
#endregion

#region SetVariable

/// <summary>
/// A flow node that assigns a value to a temporary variable during chart computation.
/// </summary>
[SimpleFlowNodeStyle(Color = "#6FA72F")]
[DisplayText("Set Variable")]
[ToolTipsText("Set temporary variables in chart computation")]
public class SetVariable : VariableFlowNode
{
    private readonly ConnectorStringProperty _variableName
        = new("VariableName", "Variable Name");

    private readonly ConnectorValueProperty<FlowContextScopes> _scope
        = new("Scope", "Scope");

    private FlowNodeConnector _in;
    private FlowNodeConnector _varIn;

    private FlowNodeConnector _out;
    private FlowNodeConnector _varOut;

    private ITypeDesignSelection _valueType;
    private bool _isArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetVariable"/> class.
    /// </summary>
    public SetVariable()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
        _valueType.Id = NativeTypes.ObjectType.TargetId;

        UpdateConnector();
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _variableName.Sync(sync);
        _scope.Sync(sync);
        _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        _isArray = sync.Sync("IsArray", _isArray);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _variableName.InspectorField(setup, this);
        _scope.InspectorField(setup, this);
        setup.InspectorField(_valueType, new ViewProperty("ValueType", "Output Type"));
        setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        var type = GetTargetType();

        _in = AddActionInputConnector("In", "Input");
        _variableName.AddConnector(this);
        _scope.AddConnector(this);
        _varIn = AddDataInputConnector("VarIn", type.ToTypeName(), "Variable");

        _out = AddActionOutputConnector("Out", "Output");
        _varOut = AddDataOutputConnector("VarOut", type.ToTypeName(), "Variable");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        string varName = _variableName.GetValue(compute, this);
        if (string.IsNullOrWhiteSpace(varName))
        {
            compute.SetValue(_varOut, null);
            return;
        }

        var scope = _scope.GetValue(compute, this);
        object value = compute.GetValue(_varIn);
        compute.SetVariable(scope, this.Diagram, varName, value);

        compute.SetValue(_varOut, value);

        compute.SetResult(this, _out);
    }

    private TypeDefinition GetTargetType()
    {
        var type = _valueType.GetTypeDefinition();
        if (_isArray)
        {
            type = type.MakeArrayType();
        }

        return type;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (Diagram?.GetIsLinked(_variableName.Connector) == true)
        {
            return DisplayText;
        }

        string varName = _variableName.BaseValue;
        if (!string.IsNullOrWhiteSpace(varName))
        {
            return $"Set {varName} Variable";
        }

        return DisplayText;
    }
}
#endregion

#region SetVariables

/// <summary>
/// A flow node that assigns values to multiple temporary variables in a single step during chart computation.
/// </summary>
[SimpleFlowNodeStyle(Color = "#6FA72F")]
[DisplayText("Set Multiple Variables")]
[ToolTipsText("Get multiple temporary variables in chart computation")]
public class SetVariables : VariableFlowNode
{
    private FlowNodeConnector _in;
    private FlowNodeConnector _out;

    private readonly ConnectorValueProperty<FlowContextScopes> _scope
        = new("Scope", "Scope");

    private readonly ListProperty<string> _variableNames 
        = new("Variables", "Variable Names");

    /// <summary>
    /// Initializes a new instance of the <see cref="SetVariables"/> class.
    /// </summary>
    public SetVariables()
    {
        UpdateConnector();
    }


    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _variableNames.Sync(sync);
        _scope.Sync(sync);

        if (sync.IsSetter())
        {
            UpdateConnectorQueued();
        }
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _variableNames.InspectorField(setup);
        _scope.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    protected override void OnUpdateConnector()
    {
        base.OnUpdateConnector();

        _in = AddActionInputConnector("In", "Input");

        _out = AddActionOutputConnector("Out", "Output");

        var varNames = _variableNames.List.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
        foreach (var varName in varNames)
        {
            string text = varName;
            if (text.Length > 20)
            {
                text = text.Substring(0, 17) + "...";
            }

            AddDataInputConnector("var-" + varName, "object", text);
        }

        _scope.AddConnector(this);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        base.Compute(compute);

        var context = GetContext(compute, _scope.GetValue(compute, this));
        if (context is null)
        {
            return;
        }

        var varNames = _variableNames.List.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
        foreach (var varName in varNames)
        {
            var conn = GetConnector("var-" + varName);
            if (conn is null)
            {
                continue;
            }

            var value = compute.GetValue(conn);
            context.SetFlowVariable(varName, value);
        }

        compute.SetResult(this, _out);
    }
}

#endregion
