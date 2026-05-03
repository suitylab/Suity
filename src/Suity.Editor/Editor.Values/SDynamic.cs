using Suity.Drawing;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;

namespace Suity.Editor.Values;

[NativeAlias]
/// <summary>
/// Represents a dynamic computed value that can be computed at runtime.
/// </summary>
public class SDynamic : SValue, IViewObject
{
    /// <summary>
    /// Creates an empty SDynamic.
    /// </summary>
    public SDynamic()
    { }

    /// <summary>
    /// Creates an SDynamic with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public SDynamic(object value)
        : base(value) { }

    /// <summary>
    /// Creates an SDynamic with the specified type.
    /// </summary>
    /// <param name="type">The type definition.</param>
    public SDynamic(TypeDefinition type)
        : base(type) { }

    /// <summary>
    /// Creates an SDynamic with the specified type and value.
    /// </summary>
    /// <param name="type">The type definition.</param>
    /// <param name="value">The value.</param>
    public SDynamic(TypeDefinition type, object value)
        : base(type, value) { }

    /// <summary>
    /// Gets the display icon.
    /// </summary>
    public virtual ImageDef Icon => null;

    /// <summary>
    /// Validates the value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    protected override void ValidateValue(object value)
    {
        if (value is SDynamic)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Override this method to implement custom synchronization logic.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The synchronization context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        if (sync.Intent == SyncIntent.View || sync.Intent == SyncIntent.Visit)
        {
            sync.Sync("ComputedValue", GetValue(), SyncFlag.GetOnly);
        }
    }

    /// <summary>
    /// Sets up the view for this dynamic value.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        OnSetupView(setup);

        setup.InspectorField(Value, new ViewProperty("ComputedValue", "Computed value").WithReadOnly().WithObsolete());
    }

    /// <summary>
    /// Override this method to implement custom view setup.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    { }

    /// <summary>
    /// Returns a string representation of this dynamic value.
    /// </summary>
    public override string ToString()
    {
        string s = EditorUtility.GetBriefStringL(GetValue());

        return $"{GetType().ToDisplayTextL()}: {s}";
    }
}