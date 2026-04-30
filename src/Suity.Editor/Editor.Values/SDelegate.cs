using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Collections.Generic;

namespace Suity.Editor.Values;

[NativeAlias]
/// <summary>
/// Represents a delegate containing a collection of actions.
/// </summary>
public class SDelegate : SContainer, ISyncObject, IViewObject
{
    //readonly EditorDelegateVariableList _variables;
    private readonly SArray _actions;

    /// <summary>
    /// Creates an empty SDelegate.
    /// </summary>
    public SDelegate()
        : this(null)
    {
    }

    /// <summary>
    /// Creates an SDelegate with the specified input type.
    /// </summary>
    /// <param name="inputType">The input type definition.</param>
    public SDelegate(TypeDefinition inputType)
        : base(inputType)
    {
        _actions = new SArray(NativeTypes.ActionArrayType)
        {
            Context = this,
            Parent = this
        };
    }

    /// <summary>
    /// Gets the actions array.
    /// </summary>
    public SArray Actions => _actions;

    #region ISyncObject Members

    /// <summary>
    /// Synchronizes the delegate properties.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    /// <param name="context">The sync context.</param>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        //sync.Sync("Variables", _variables, SyncFlag.ReadOnly);
        string inputType = InputType.ToString();
        inputType = sync.Sync(Attribute_InputType, inputType, SyncFlag.AttributeMode);
        if (sync.IsSetter())
        {
            if (string.IsNullOrEmpty(inputType))
            {
                inputType = NativeTypes.DelegateType.ToString();
            }
            if (InputType.ToString() != inputType)
            {
                InputType = TypeDefinition.Resolve(inputType);
            }
        }
        sync.Sync("Actions", _actions, SyncFlag.GetOnly);
    }

    #endregion

    #region IViewObject Members

    /// <summary>
    /// Sets up the view for this delegate.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_actions, new ViewProperty("Actions", "Actions"));
    }

    #endregion

    public override string ToString() => L("Delegate");

    public override IEnumerable<object> GetValues(ICondition context = null)
    {
        yield return _actions;
    }

    public override IEnumerable<SItem> Items
    {
        get { yield return _actions; }
    }
}