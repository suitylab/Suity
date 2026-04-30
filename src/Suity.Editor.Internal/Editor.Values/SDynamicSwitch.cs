using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Values;

/// <summary>
/// Represents a dynamic switch value that changes based on conditions.
/// </summary>
/// <remarks>This class is obsolete. Use <see cref="SConditional"/> instead.</remarks>
[DisplayText("Condition")]
[Obsolete]
internal class SDynamicSwitch : SDynamic
{
    private readonly SConditionList _list;

    /// <summary>
    /// Initializes a new instance of <see cref="SDynamicSwitch"/>.
    /// </summary>
    public SDynamicSwitch()
    {
        _list = new SConditionList(this);
        _list.UpdateInputType();
    }

    /// <summary>
    /// Initializes a new instance with the specified value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public SDynamicSwitch(object value)
        : base(value)
    {
        _list = new SConditionList(this);
        _list.UpdateInputType();
    }

    /// <inheritdoc/>
    public override Image Icon => CoreIconCache.Switch;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        sync.Sync("Conditions", _list, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_list, new ViewProperty("Conditions", "Condition"));
    }

    /// <inheritdoc/>
    protected override void OnInputTypeChanged()
    {
        base.OnInputTypeChanged();
        _list?.UpdateInputType();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string s;

        string str = base.ToString();
        if (_list.Count > 0)
        {
            s = $"{str} {string.Join(" ", _list.Items)}";
        }
        else
        {
            s = str;
        }

        return L("Condition") + " " + s;
    }
}
