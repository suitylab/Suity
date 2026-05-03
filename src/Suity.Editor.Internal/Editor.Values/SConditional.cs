using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Values;

#region SDynamicConditional

/// <summary>
/// Represents a dynamic value that changes based on active conditions.
/// </summary>
[DisplayText("Condition")]
[NativeAlias]
public class SConditional : SDynamic
{
    private readonly SConditionList _list;

    /// <summary>
    /// Initializes a new instance of <see cref="SConditional"/>.
    /// </summary>
    public SConditional()
    {
        _list = new SConditionList(this);
        _list.UpdateInputType();
    }

    /// <summary>
    /// Initializes a new instance with the specified value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public SConditional(object value)
        : base(value)
    {
        _list = new SConditionList(this);
        _list.UpdateInputType();
    }

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Condition;

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
    public override object GetValue(ICondition condition = null)
    {
        condition ??= RootContext as ICondition;

        if (_list.TryGetValue(condition, out var value))
        {
            return value;
        }

        return base.Value;
    }

    /// <summary>
    /// Checks whether a value exists for the specified condition.
    /// </summary>
    /// <param name="condition">The condition name.</param>
    /// <returns>True if a distinct value exists for the condition.</returns>
    public bool HasValue(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return true;
        }

        if (!_list.HasValue(condition))
        {
            return false;
        }

        var value = _list.GetValue(condition);
        if (Equality.ObjectEquals(value, base.Value))
        {
            // Equal to the original value
            _list.RemoveValue(condition);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the value for the specified condition.
    /// </summary>
    /// <param name="condition">The condition name.</param>
    /// <returns>The value for the condition, or the base value if not found.</returns>
    public object GetValue(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return base.Value;
        }

        return _list.GetValue(condition) ?? base.Value;
    }

    /// <summary>
    /// Sets the value for the specified condition.
    /// </summary>
    /// <param name="condition">The condition name.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(string condition, object value)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            base.Value = value;
        }

        _list.SetValue(condition, value);
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

        return L("Condition") + s;
    }
}

#endregion

#region SConditionList

/// <summary>
/// Manages a list of conditional items for <see cref="SConditional"/>.
/// </summary>
internal class SConditionList : ISyncList
{
    private readonly SDynamic _parent;
    private readonly WatchableList<SConditionalItem> _list = [];

    /// <summary>
    /// Initializes a new instance with the specified parent.
    /// </summary>
    /// <param name="parent">The parent SDynamic.</param>
    public SConditionList(SDynamic parent)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _list.Updated += _list_Updated;
    }

    /// <summary>
    /// Handles updates to the condition list, ensuring default values are synchronized.
    /// </summary>
    /// <param name="mode">The update mode.</param>
    /// <param name="index">The index of the changed item.</param>
    /// <param name="value">The changed item.</param>
    private void _list_Updated(EventListUpdateMode mode, int index, SConditionalItem value)
    {
        switch (mode)
        {
            case EventListUpdateMode.Added:
                UpdateDefaultValue(_list[index]);
                break;

            case EventListUpdateMode.Removed:
                break;

            case EventListUpdateMode.Changed:
                UpdateDefaultValue(_list[index]);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Gets the number of conditional items.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets all conditional items.
    /// </summary>
    public IEnumerable<SConditionalItem> Items => _list;

    /// <inheritdoc/>
    public void Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIList(_list, typeof(SConditionalItem));
    }

    /// <summary>
    /// Updates default values for all items based on the parent's current value.
    /// </summary>
    public void UpdateInputType()
    {
        foreach (var item in _list)
        {
            UpdateDefaultValue(item);
        }
    }

    /// <summary>
    /// Tries to get a value matching the current context conditions.
    /// </summary>
    /// <param name="context">The condition context.</param>
    /// <param name="value">The matched value, if found.</param>
    /// <returns>True if a matching value was found.</returns>
    public bool TryGetValue(ICondition context, out object value)
    {
        if (context != null)
        {
            foreach (string c in context.Conditions)
            {
                var item = _list.Find(o => o.Condition == c);
                if (item != null)
                {
                    value = item.Value;

                    return true;
                }
            }
        }

        value = null;

        return false;
    }

    /// <summary>
    /// Checks whether a value exists for the specified condition.
    /// </summary>
    /// <param name="condition">The condition name.</param>
    /// <returns>True if a value exists for the condition.</returns>
    public bool HasValue(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return false;
        }

        return _list.Any(o => o.Condition == condition);
    }

    /// <summary>
    /// Gets the value for the specified condition.
    /// </summary>
    /// <param name="condition">The condition name.</param>
    /// <returns>The value, or null if not found.</returns>
    public object GetValue(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return null;
        }

        return _list.Find(o => o.Condition == condition)?.Value;
    }

    /// <summary>
    /// Removes the value for the specified condition.
    /// </summary>
    /// <param name="condition">The condition name.</param>
    /// <returns>True if a value was removed.</returns>
    public bool RemoveValue(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return false;
        }

        return _list.RemoveAll(o => o.Condition == condition) > 0;
    }

    /// <summary>
    /// Sets the value for the specified condition.
    /// </summary>
    /// <param name="condition">The condition name.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(string condition, object value)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return;
        }

        var item = _list.Find(o => o.Condition == condition);
        if (item is null)
        {
            item = new SConditionalItem { Condition = condition };
            _list.Add(item);
        }

        item.Value = value;

        UpdateDefaultValue(item);
    }

    /// <summary>
    /// Updates the default value of an item to match the parent's value type.
    /// </summary>
    /// <param name="item">The item to update.</param>
    private void UpdateDefaultValue(SConditionalItem item)
    {
        if (item.Value?.GetType() != _parent.Value?.GetType())
        {
            item.Value = SItem.ResolveValue(_parent.Value);
        }
    }
}

#endregion

#region SConditionalItem

/// <summary>
/// Represents a single condition-value pair in a conditional list.
/// </summary>
public class SConditionalItem : IViewObject
{
    internal string[] _codes = [];

    /// <summary>
    /// Gets or sets the condition string.
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value associated with the condition.
    /// </summary>
    public object Value { get; set; }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        Condition = sync.Sync("Condition", Condition, SyncFlag.NotNull, string.Empty);
        Value = sync.Sync("Value", Value, SyncFlag.NotNull);

        if (sync.IsSetterOf("Condition"))
        {
            ParseCondition();
        }
    }

    /// <summary>
    /// Parses the condition string into individual codes.
    /// </summary>
    private void ParseCondition()
    {
        if (string.IsNullOrEmpty(Condition))
        {
            _codes = [];
            return;
        }

        _codes = Condition.Split(' ', ',', ';')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(Condition, new ViewProperty("Condition", "Condition"));
        setup.InspectorField(Value, new ViewProperty("Value", "Value"));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{string.Join("; ", _codes)}={Value}]";
    }
}

#endregion

#region SDynamicConditionalEditor

/// <summary>
/// Property editor for <see cref="SConditional"/> that resolves values based on the active condition.
/// </summary>
public class SDynamicConditionalEditor : ImGuiPropertyEditor<SConditional>
{
    /// <inheritdoc/>
    public override ImGuiNode EditorFunction(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        IConditionSelection context = (target as PropertyTarget)?.ServiceProvider?.GetService<IConditionSelection>();
        string c = context?.SelectedCondition;

        var conditionals = target.GetValues().As<SConditional>();
        var values = conditionals.Select(o => o?.GetValue(c));

        if (target.Status < TextStatus.Warning)
        {
            target.Status = conditionals.Any(o => o?.HasValue(c) == true) ? TextStatus.Reference : TextStatus.Warning;
        }

        // The following approach will cause an infinite loop
        //var convertedTarget = ((PropertyTarget)target).CreateConvertedTarget(Convert, RevertConvert);

        //var node = gui.PropertyEditor(convertedTarget, handler);
        //if (node != null)
        //{
        //    if (target.Status != TextStatus.Normal)
        //    {
        //        node?.OverrideBorder(1f, target.Status.ToColor());
        //    }

        //    return node;
        //}

        //return null;
        // 

        Type commonType = values.GetCommonType();
        var innerFunc = gui.GetPropertyEditorProvider()?.GetEditorFunction(commonType, null);

        if (innerFunc != null)
        {
            var converted = ((PropertyTarget)target).CreateConvertedTarget(Convert, ConvertRevert);

            var node = innerFunc(gui, converted, handler);
            if (target.Status != TextStatus.Normal)
            {
                node?.OverrideBorder(1f, target.Status.ToColor());
            }

            return node;
        }
        else
        {
            //return gui.Text("---");
            return null;
        }
    }

    /// <inheritdoc/>
    public override ImGuiNode RowFunction(ImGui gui, PropertyTarget target, PropertyRowAction rowAction)
    {
        if (target.Status < TextStatus.Warning)
        {
            target.Status = TextStatus.Reference;
        }

        return base.RowFunction(gui, target, rowAction);

        IConditionSelection context = (target as PropertyTarget)?.ServiceProvider?.GetService<IConditionSelection>();
        string c = context?.SelectedCondition;

        var conditionals = target.GetValues().As<SConditional>();
        var values = conditionals.Select(o => o?.GetValue(c));

        Type commonType = values.GetCommonType();
        var innerRow = gui.GetPropertyEditorProvider()?.GetRowFunction(commonType, null);

        if (innerRow != null)
        {
            var converted = ((PropertyTarget)target).CreateConvertedTarget(Convert, ConvertRevert);

            var node = innerRow(gui, converted, rowAction);

            return node;
        }
        else
        {
            return base.RowFunction(gui, target, rowAction);
        }
    }

    /// <summary>
    /// Converts <see cref="SConditional"/> values to their resolved values for the active condition.
    /// </summary>
    /// <param name="target">The property target.</param>
    /// <returns>The resolved values.</returns>
    private IEnumerable<object> Convert(PropertyTarget target)
    {
        IConditionSelection context = target.ServiceProvider?.GetService<IConditionSelection>();
        string c = context?.SelectedCondition;

        var conditionals = target.GetValues().As<SConditional>();

        return conditionals.Select(o => o?.GetValue(c));
    }

    /// <summary>
    /// Reverts edited values back to <see cref="SConditional"/> values for the active condition.
    /// </summary>
    /// <param name="target">The property target.</param>
    /// <param name="values">The edited values.</param>
    /// <returns>The reverted SConditional values.</returns>
    private IEnumerable<object> ConvertRevert(PropertyTarget target, IEnumerable<object> values)
    {
        IConditionSelection context = target.ServiceProvider?.GetService<IConditionSelection>();
        string c = context?.SelectedCondition;

        var vAry = values.ToArray();
        var cAry = target.GetValues().As<SConditional>().ToArray();

        for (int i = 0; i < cAry.Length; i++)
        {
            var c2 = Cloner.Clone(cAry[i]);
            if (c2 is null)
            {
                continue;
            }

            c2.SetValue(c, vAry.GetArrayItemSafe(i));

            cAry[i] = c2;
        }

        return cAry;
    }
}

#endregion