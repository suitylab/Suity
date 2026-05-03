using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Selecting;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Values;

#region SDynamicValueRef

/// <summary>
/// Represents a dynamic reference to another value, allowing indirect value resolution through selection.
/// </summary>
[DisplayText("Value Link")]
public class SDynamicValueRef : SDynamic, INavigable
{
    private readonly SValueSelection _selection;

    /// <summary>
    /// Initializes a new instance of <see cref="SDynamicValueRef"/>.
    /// </summary>
    public SDynamicValueRef()
    {
        _selection = new SValueSelection(this);
    }

    /// <summary>
    /// Initializes a new instance with the specified value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public SDynamicValueRef(object value)
        : base(value)
    {
        _selection = new SValueSelection(InputType, this);
    }

    /// <inheritdoc/>
    public override ImageDef Icon => _selection.Target?.Icon ?? CoreIconCache.Value;

    /// <summary>
    /// Gets the internal value selection.
    /// </summary>
    internal SValueSelection Selection => _selection;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        // Although read-only is set, the synchronizer still tries to write properties one by one
        sync.Sync("ValueRef", _selection, SyncFlag.GetOnly);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_selection, new ViewProperty("ValueRef", "Value"));
    }

    /// <inheritdoc/>
    public override object GetValue(ICondition condition = null)
    {
        var valueAsset = _selection?.Target;
        object value = valueAsset?.GetValue(this, condition);

        if (value != null && valueAsset.ValueType == InputType)
        {
            return value;
        }
        else
        {
            return base.GetValue(condition);
        }
    }

    /// <inheritdoc/>
    protected override void OnInputTypeChanged()
    {
        base.OnInputTypeChanged();

        _selection?.UpdateValueType(InputType);
    }

    /// <inheritdoc/>
    public override string ToString() => ((ITextDisplay)_selection).DisplayText;

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return _selection.Id;
    }
}

#endregion

#region SDynamicValueRefEditor

/// <summary>
/// Property editor for <see cref="SDynamicValueRef"/> that displays value references with appropriate status indicators.
/// </summary>
public class SDynamicValueRefEditor : ImGuiPropertyEditor<SDynamicValueRef>
{
    /// <inheritdoc/>
    public override ImGuiNode EditorFunction(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        var valueRefs = target.GetValues().As<SDynamicValueRef>();
        var values = valueRefs.Select(o => o?.GetValue());

        if (target.Status < TextStatus.Warning)
        {
            target.Status = valueRefs.Any(o => o?.Selection.Target != null) ? TextStatus.Reference : TextStatus.Warning;
        }

        var convertedTarget = ((PropertyTarget)target).CreateConvertedTarget(Convert, RevertConvert);

        var node = gui.PropertyEditor(convertedTarget, handler);
        if (node != null)
        {
            if (target.Status != TextStatus.Normal)
            {
                node?.OverrideBorder(1f, target.Status.ToColor());
            }

            return node;
        }

        return null;
    }

    /// <inheritdoc/>
    public override ImGuiNode RowFunction(ImGui gui, PropertyTarget target, PropertyRowAction rowAction)
    {
        if (target.Status < TextStatus.Warning)
        {
            target.Status = TextStatus.Reference;
        }

        return base.RowFunction(gui, target, rowAction);
    }

    /// <summary>
    /// Converts <see cref="SDynamicValueRef"/> values to their underlying <see cref="SValueSelection"/> for editing.
    /// </summary>
    /// <param name="target">The property target.</param>
    /// <returns>The converted selections.</returns>
    private IEnumerable<object> Convert(PropertyTarget target)
    {
        var conditionals = target.GetValues().As<SDynamicValueRef>();
        return conditionals.Select(o => o?.Selection);
    }

    /// <summary>
    /// Reverts the edited selections back to <see cref="SDynamicValueRef"/> values.
    /// </summary>
    /// <param name="target">The property target.</param>
    /// <param name="values">The edited selection values.</param>
    /// <returns>The reverted SDynamicValueRef values.</returns>
    private IEnumerable<object> RevertConvert(PropertyTarget target, IEnumerable<object> values)
    {
        var context = target.ServiceProvider?.GetService<IConditionSelection>();
        string c = context?.SelectedCondition;

        var vAry = values.As<SValueSelection>().ToArray();
        var cAry = target.GetValues().As<SDynamicValueRef>().ToArray();

        for (int i = 0; i < cAry.Length; i++)
        {
            var c2 = Cloner.Clone(cAry[i]);
            if (c2 is null)
            {
                continue;
            }

            c2.Selection.Id = vAry.GetArrayItemSafe(i)?.Id ?? Guid.Empty;

            cAry[i] = c2;
        }

        return cAry;
    }
}

#endregion
