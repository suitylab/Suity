using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Base class for sub-flow design-time flow nodes.
/// Provides common functionality for nodes that define sub-flow structures.
/// </summary>
[DisplayText("Sub-flow node", "*CoreIcon|Page")]
[ToolTipsText("Sub-flow related nodes")]
public abstract class SubFlowNode : DesignFlowNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowNode"/> class.
    /// </summary>
    protected SubFlowNode()
        : base()
    {
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        setup.LabelWithIcon("#Page", "Page", CoreIconCache.Page);
    }

    /// <summary>
    /// Verifies that a name is a valid identifier and does not start with "__".
    /// </summary>
    /// <param name="name">The name to verify.</param>
    /// <returns>True if the name is valid; otherwise, false.</returns>
    public static bool VerifyName(string name) => NamingVerifier.VerifyIdentifier(name) && !name.StartsWith("__");
}

/// <summary>
/// Base class for sub-flow type definition nodes that support type selection and configuration.
/// Provides properties for task completion, commit, chat history, and type definition settings.
/// </summary>
public abstract class SubFlowTypeNode : SubFlowNode, ISubFlowTypeNode
{
    private ITypeDesignSelection _valueType;
    private bool _isArray;
    private bool _optional;

    private bool _editTypeEnabled = true;

    private readonly ValueProperty<bool> _taskCompletion = new("TaskCompletion", "Task Completion", true, "Used to determine if task is completed.");
    private readonly ValueProperty<bool> _chatHistory = new("History", "Chat History", false, "Retained as historical conversation during dialogue.");
    private readonly ValueProperty<bool> _taskCommit = new("TaskCommit", "Task Commit", false, "Submit this value after completing the task.");
    private readonly ValueProperty<bool> _linkedMode = new("LinkedMode", "Linked Mode", false, "When enabled, will be displayed as a link address in task submissions and chat history, instead of content.\r\nOnly effective when value is a link type.");

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowTypeNode"/> class.
    /// </summary>
    protected SubFlowTypeNode()
    {
        _valueType = DTypeManager.Instance.CreateTypeDesignSelection();
    }

    /// <inheritdoc/>
    public bool LinkedMode { get => _linkedMode.Value; protected set => _linkedMode.Value = value; }

    /// <summary>
    /// Gets or sets a value indicating whether this property is retained as historical conversation during dialogue.
    /// </summary>
    public bool ChatHistory { get => _chatHistory.Value; protected set => _chatHistory.Value = value; }

    /// <summary>
    /// Gets or sets a value indicating whether this property is used to determine task completion.
    /// </summary>
    public bool TaskCompletion { get => _taskCompletion.Value; protected set => _taskCompletion.Value = value; }

    /// <summary>
    /// Gets or sets a value indicating whether this property should be submitted after completing the task.
    /// </summary>
    public bool TaskCommit { get => _taskCommit.Value; protected set => _taskCommit.Value = value; }


    /// <summary>
    /// Gets the type design selection used for configuring the value type.
    /// </summary>
    public ITypeDesignSelection ValueType => _valueType;

    /// <summary>
    /// Gets or sets the type definition for this node. Handles array types automatically.
    /// </summary>
    public TypeDefinition TypeDef
    {
        get
        {
            var type = _valueType?.GetTypeDefinition();
            if (_isArray)
            {
                type = type?.MakeArrayType();
            }

            return type ?? TypeDefinition.Empty;
        }
        set
        {
            if (_valueType is null)
            {
                return;
            }

            if (TypeDefinition.IsNullOrEmpty(value))
            {
                _valueType.SelectedKey = null;
                return;
            }

            if (value.IsArray)
            {
                _valueType.SelectedKey = value.ElementType.TypeCode;
                _isArray = true;
            }
            else
            {
                _valueType.SelectedKey = value.TypeCode;
                _isArray = false;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether type editing is enabled.
    /// </summary>
    public bool EditTypeEnabled
    {
        get => _editTypeEnabled;
        protected set => _editTypeEnabled = value;
    }

    /// <summary>
    /// Gets a value indicating whether this is a preset parameter.
    /// </summary>
    public virtual bool IsPresetParameter => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubFlowTypeNode"/> class with a specified type ID.
    /// </summary>
    /// <param name="typeId">The unique identifier for the type.</param>
    protected SubFlowTypeNode(Guid typeId) : this()
    {
        _valueType.Id = typeId;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the type value is optional.
    /// </summary>
    protected bool Optional
    {
        get => _optional;
        set
        {
            _optional = value;
            _valueType?.Optional = value;
        }
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _taskCompletion.Sync(sync);
        _chatHistory.Sync(sync);
        _taskCommit.Sync(sync);
        _linkedMode.Sync(sync);

        if (_editTypeEnabled)
        {
            _valueType = sync.Sync("ValueType", _valueType, SyncFlag.NotNull | SyncFlag.AffectsOthers);
            _valueType?.Optional = _optional;
            _isArray = sync.Sync("IsArray", _isArray);

            OnSyncValue(sync, context);

            if (sync.IsSetterOf("ValueType") || sync.IsSetterOf("IsArray"))
            {
                UpdateConnectorQueued();
            }
        }
    }

    /// <summary>
    /// Called during synchronization to allow derived classes to sync additional values.
    /// </summary>
    /// <param name="sync">The property sync instance.</param>
    /// <param name="context">The sync context.</param>
    protected virtual void OnSyncValue(IPropertySync sync, ISyncContext context)
    {
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        if (_editTypeEnabled)
        {
            UpdateDefaultValue();

            setup.InspectorField(_valueType, new ViewProperty("ValueType", "Type").WithOptional(Optional));
            setup.InspectorField(_isArray, new ViewProperty("IsArray", "Array"));

            OnSetupViewValue(setup);
        }
    }

    /// <summary>
    /// Called during view setup to allow derived classes to configure additional view properties.
    /// </summary>
    /// <param name="setup">The view setup instance.</param>
    protected virtual void OnSetupViewValue(IViewObjectSetup setup)
    {
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _taskCompletion.InspectorField(setup);
        _chatHistory.InspectorField(setup);
        _taskCommit.InspectorField(setup);
        _linkedMode.InspectorField(setup);
    }

    /// <summary>
    /// Called to update default values when the type configuration changes.
    /// Override this method in derived classes to provide custom default value logic.
    /// </summary>
    protected virtual void UpdateDefaultValue()
    {
    }
}

/// <summary>
/// Abstract base class for sub-flow definition nodes that support parameter completion conditions.
/// </summary>
public abstract class SubflowDefNode : SubFlowNode, ISubFlowPage
{
    readonly ValueProperty<ParameterConditions> _completionCondition = new("CompletionCondition", "Completion Condition", ParameterConditions.All, "Condition for parameter completion. All means all must be met, Any means only one needs to be met.");

    /// <summary>
    /// Gets the condition for parameter completion.
    /// </summary>
    public ParameterConditions CompletionCondition => _completionCondition.Value;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _completionCondition.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupViewContent(IViewObjectSetup setup)
    {
        base.OnSetupViewContent(setup);

        _completionCondition.InspectorField(setup);
    }

    /// <inheritdoc/>
    public abstract ISubFlowPage GetPageDefinition();

    /// <inheritdoc/>
    public abstract ISubFlowPage GetPageResult();

    /// <inheritdoc/>
    public abstract object GetDocumentItem();
}
