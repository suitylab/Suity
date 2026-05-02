using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Views;

public interface IValueProperty
{
    ViewProperty Property { get; }

    void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null);
}

#region ValueProperty

public class ValueProperty<T> : IValueProperty
{
    public ViewProperty Property { get; }

    public string Name => Property.Name;

    public SyncFlag Flag { get; set; }

    public T Value { get; set; }

    public T DefaultValue { get; set; }

    public event EventHandler ValueChanged;

    public ValueProperty(string name, string description = null, T value = default, string toolTips = null, SyncFlag flag = SyncFlag.None)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);

        Value = value;

        // Only value types have default values, otherwise reference issues may occur
        if (typeof(T).IsValueType)
        {
            DefaultValue = value;
        }

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }

        Flag = flag;
    }

    public ValueProperty(ViewProperty property, T value = default)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));

        Value = DefaultValue = value;
    }

    public virtual T Sync(IPropertySync sync)
    {
        var flag = Flag;

        if (flag.HasFlag(SyncFlag.GetOnly) || sync.IsGetter())
        {
            sync.Sync(Property.Name, Value, flag, DefaultValue);
        }
        else
        {
            Value = sync.Sync(Property.Name, Value, flag, DefaultValue);

            if (sync.IsSetter(Property.Name))
            {
                OnValueChanged();
            }
        }

        return Value;
    }

    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(Value, prop);
    }

    protected void OnValueChanged()
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public static implicit operator T(ValueProperty<T> property) => property.Value;
}

#endregion

#region ValueProperty

public class ReadonlyValueProperty<T> : IValueProperty
    where T : class
{
    public ViewProperty Property { get; }

    public SyncFlag Flag { get; set; }

    public T Value { get; }

    public event EventHandler ValueChanged;

    public ReadonlyValueProperty(T value, string name, string description = null, string toolTips = null)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }
    }

    public ReadonlyValueProperty(T value, ViewProperty property)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));

        Property = property ?? throw new ArgumentNullException(nameof(property));
    }

    public virtual void Sync(IPropertySync sync)
    {
        sync.Sync(Property.Name, Value, Flag | SyncFlag.GetOnly);
    }

    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(Value, prop);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public static implicit operator T(ReadonlyValueProperty<T> property) => property.Value;
}

#endregion

#region ListProperty

public class ListProperty<T> : IValueProperty
{
    public ViewProperty Property { get; }

    public SyncFlag Flag { get; set; } = SyncFlag.GetOnly;

    public List<T> List { get; } = [];

    public event EventHandler ValueChanged;


    public ListProperty(string name, string description = null, string toolTips = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);
        Property.WithWriteBack();

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }
    }

    public ListProperty(ViewProperty property)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Property.WithWriteBack();
    }

    public virtual void Sync(IPropertySync sync)
    {
        sync.Sync(Property.Name, List, Flag | SyncFlag.GetOnly);

        if (sync.IsSetter(Property.Name))
        {
            OnValueChanged();
        }
    }

    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(List, prop);
    }

    protected void OnValueChanged()
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() => $"[{List?.Count ?? 0}]";
}

#endregion

#region SyncListProperty

public class SyncListProperty<T> : IValueProperty
{
    public ViewProperty Property { get; }

    public SyncFlag Flag { get; set; } = SyncFlag.GetOnly;

    public SyncList<T> SyncList { get; }

    public List<T> List => SyncList.List;

    public event EventHandler ValueChanged;


    public SyncListProperty(string name, Func<T> factoryFunc, Predicate<T> checkFunc = null, string description = null, string toolTips = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);
        Property.WithWriteBack();

        SyncList = new FactorySyncList<T>(factoryFunc, checkFunc);

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }
    }

    public SyncListProperty(ViewProperty property, Func<T> factoryFunc, Predicate<T> checkFunc = null)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Property.WithWriteBack();

        SyncList = new FactorySyncList<T>(factoryFunc, checkFunc);
    }

    public virtual void Sync(IPropertySync sync)
    {
        sync.Sync(Property.Name, SyncList, Flag | SyncFlag.GetOnly);

        if (sync.IsSetter(Property.Name))
        {
            OnValueChanged();
        }
    }

    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(SyncList, prop);
    }

    protected void OnValueChanged()
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() => $"[{List?.Count ?? 0}]";
}

#endregion

#region StringProperty

public class StringProperty : ValueProperty<string>
{
    public string AutoFillText { get; set; }

    public StringProperty(string name, string description = null, string defaultValue = null, string toolTips = null, bool autoFillDefault = false)
        : base(name, description, defaultValue ?? string.Empty, toolTips)
    {
        if (autoFillDefault)
        {
            AutoFillText = defaultValue;
        }
    }

    public StringProperty(ViewProperty property)
        : base(property)
    {
        Value = DefaultValue = string.Empty;
    }

    public override string Sync(IPropertySync sync)
    {
        var flag = Flag;

        if (flag.HasFlag(SyncFlag.GetOnly) || sync.IsGetter())
        {
            sync.Sync(Property.Name, Value, flag | SyncFlag.NotNull);
        }
        else
        {
            Value = sync.Sync(Property.Name, Value, flag | SyncFlag.NotNull) ?? DefaultValue ?? string.Empty;

            if (sync.IsSetter() && string.IsNullOrWhiteSpace(Value) && !string.IsNullOrWhiteSpace(AutoFillText))
            {
                Value = AutoFillText;
            }

            OnValueChanged();
        }

        return Value;
    }

    public string Text
    {
        get => Value ?? string.Empty;
        set => Value = value ?? string.Empty;
    }

    public override string ToString() => Text?.ToString() ?? string.Empty;

    public static implicit operator string(StringProperty property) => property.Text;
}

#endregion

#region ColorProperty

public class ColorProperty : ValueProperty<Color>
{
    public ColorProperty(string name, string description = null, Color defaultValue = default, string toolTips = null)
        : base(name, description, defaultValue, toolTips)
    {
    }

    public ColorProperty(ViewProperty property)
        : base(property)
    {
        Value = DefaultValue = Color.Empty;
    }

    public override void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        prop.WithColor(Value);
        setup.InspectorField(Value, prop);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Color(ColorProperty property) => property.Value;
}

#endregion

#region TextBlockProperty

public class TextBlockProperty : ValueProperty<TextBlock>
{
    public string AutoFillText { get; set; }

    public string Text
    {
        get => Value?.Text ?? string.Empty;
        set => (Value ??= new()).Text = value;
    }

    public TextBlockProperty(string name, string description = null, string defaultValue = null, string toolTips = null, bool autoFillDefault = false)
        : base(name, description, new TextBlock(defaultValue), toolTips)
    {
        if (autoFillDefault)
        {
            AutoFillText = defaultValue;
        }
    }

    public TextBlockProperty(ViewProperty property)
        : base(property)
    {
        Value = new TextBlock();
    }

    public override TextBlock Sync(IPropertySync sync)
    {
        var flag = Flag;

        if (flag.HasFlag(SyncFlag.GetOnly) || sync.IsGetter())
        {
            sync.Sync(Property.Name, Value, flag | SyncFlag.NotNull);
        }
        else
        {
            Value = sync.Sync(Property.Name, Value, flag | SyncFlag.NotNull) ?? new TextBlock();

            if (sync.IsSetter() && string.IsNullOrWhiteSpace(Value.Text) && !string.IsNullOrWhiteSpace(AutoFillText))
            {
                Value.Text = AutoFillText;
            }

            OnValueChanged();
        }

        return Value;
    }

    public override string ToString() => Text?.ToString() ?? string.Empty;

    public static implicit operator string(TextBlockProperty property) => property.Text;
}

#endregion

#region LabelProperty

public class LabelProperty : IValueProperty
{
    public ViewProperty Property { get; }

    public LabelProperty(string name, string description = null, string toolTips = null, object icon = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }

        if (icon != null)
        {
            Property.WithIcon(icon);
        }
    }

    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorFieldOf<LabelValue>(prop);
    }

    public override string ToString() => Property.DisplayName;
}

#endregion

#region ButtonProperty

public class ButtonProperty : IValueProperty
{
    public event EventHandler Clicked;

    public ViewProperty Property { get; }

    public ButtonProperty(string name, string description = null, string toolTips = null, object icon = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name, description);

        if (toolTips != null)
        {
            Property.WithToolTips(toolTips);
        }

        if (icon != null)
        {
            Property.WithIcon(icon);
        }
    }

    public virtual ButtonValue Sync(IPropertySync sync)
    {
        if (sync.Intent == SyncIntent.View)
        {
            if (sync.Sync(Property.Name, ButtonValue.Empty) == ButtonValue.Clicked)
            {
                Clicked?.Invoke(this, EventArgs.Empty);

                return ButtonValue.Clicked;
            }
            else
            {
                return ButtonValue.Empty;
            }
        }
        else
        {
            return ButtonValue.Empty;
        }
    }

    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorFieldOf<ButtonValue>(prop);
    }

    public override string ToString() => Property.DisplayName;
}

#endregion

#region MultipleButtonProperty

public class MultipleButtonProperty : IValueProperty
{
    public MultipleButtonValue Buttons { get; } = new();

    public event EventHandler<string> Clicked;

    public ViewProperty Property { get; }

    public MultipleButtonProperty(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Property = new ViewProperty(name);
    }

    public virtual MultipleButtonValue Sync(IPropertySync sync)
    {
        if (sync.Intent == SyncIntent.View)
        {
            var clickedBtn = sync.Sync(Property.Name, Buttons, SyncFlag.NotNull);

            if (clickedBtn?.ClickedButton is { } key && !string.IsNullOrWhiteSpace(key))
            {
                Clicked?.Invoke(this, key);
            }

            return clickedBtn;
        }
        else
        {
            return Buttons;
        }
    }

    public virtual void InspectorField(IViewObjectSetup setup, Action<ViewProperty> config = null)
    {
        var prop = Property;
        config?.Invoke(prop);
        setup.InspectorField(Buttons, prop);
    }

    public override string ToString() => Property.DisplayName;
}

#endregion
