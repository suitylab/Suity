using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Text;

namespace Suity.Editor;

#region OriginBriefAttribute

/// <summary>
/// A brief attribute that returns the original brief text without modification.
/// </summary>
[NativeType(CodeBase = "*Brief", Name = "OriginBrief", Description = "Original Brief", Icon = "*CoreIcon|Brief")]
public class OriginBriefAttribute : DesignAttribute, IBrief
{
    /// <inheritdoc/>
    public string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief)
    {
        return originBrief();
    }
}

#endregion

#region FieldBriefAttribute

/// <summary>
/// A brief attribute that generates display text by combining a base brief with a selected field value.
/// </summary>
[NativeType(CodeBase = "*Brief", Name = "FieldBrief", Description = "Field Brief", Icon = "*CoreIcon|Brief")]
public class FieldBriefAttribute : DesignAttribute, IBrief
{
    /// <summary>
    /// Tooltip format string explaining pattern placeholders.
    /// </summary>
    private const string ToolTip = "{0} represents base brief, {1} represents field value";

    /// <summary>
    /// The field selection used to extract a value from the object.
    /// </summary>
    private DStructFieldSelection _field = new();

    /// <summary>
    /// The pattern used to format the brief text.
    /// </summary>
    private string _pattern = string.Empty;

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        UpdateObjectType();
        _field = sync.Sync("Field", _field, SyncFlag.NotNull);
        _pattern = sync.Sync("Pattern", _pattern, SyncFlag.NotNull);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.InspectorField(_field, new ViewProperty("Field", "Field"));
        setup.InspectorField(_pattern, new ViewProperty("Pattern", "Pattern ").WithToolTips(ToolTip));
    }

    /// <inheritdoc/>
    public string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief)
    {
        var sobj = obj as SObject;
        if (sobj is null)
        {
            return baseBrief();
        }

        var type = sobj.ObjectType?.Target as DCompond;
        if (type is null)
        {
            return baseBrief();
        }

        var field = _field?.Target;
        if (field != null)
        {
            string s = sobj.GetProperty(field.Id)?.ToString();

            if (!string.IsNullOrWhiteSpace(s) && !string.IsNullOrWhiteSpace(_pattern))
            {
                return string.Format(_pattern, baseBrief(), s);
            }
            else
            {
                return baseBrief();
            }
        }
        else
        {
            return baseBrief();
        }
    }

    /// <inheritdoc/>
    protected override void OnUpdateStatus()
    {
        base.OnUpdateStatus();

        UpdateObjectType();
    }

    /// <inheritdoc/>
    public override void HandleObjectUpdate(Guid id, EditorObject obj, EntryEventArgs args, ref bool handled)
    {
        base.HandleObjectUpdate(id, obj, args, ref handled);

        UpdateObjectType();
    }

    /// <summary>
    /// Updates the object type for the field selection based on the attribute owner.
    /// </summary>
    private void UpdateObjectType()
    {
        _field.ObjectType = base.AttributeOwner as DCompond;
    }
}

#endregion

#region CssStyleBriefAttribute

/// <summary>
/// A brief attribute that generates CSS-style class names from object field values.
/// </summary>
[NativeType(CodeBase = "*Brief", Name = "CssStyleBrief", Description = "Css Style Brief", Icon = "*CoreIcon|Brief")]
public class CssStyleBriefAttribute : DesignAttribute, IBrief
{
    /// <inheritdoc/>
    public string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief)
    {
        var sobj = obj as SObject;
        if (sobj is null)
        {
            return null;
        }

        var type = sobj.ObjectType?.Target as DCompond;
        if (type is null)
        {
            return null;
        }

        StringBuilder builder = CorePlugin.StringBuilderPool.Acquire();
        builder.Clear();

        foreach (DField field in type.PublicFields)
        {
            object value = sobj.GetPropertyFormatted(field.Name);
            string s = GetValueText(value);

            if (field.Attributes.GetIsHiddenOrDisabled())
            {
                s = s.Replace("_", "-").Replace("--", "-");
            }
            else
            {
                s = s.Replace("_", "-").Replace("--", $"-{field.Name}-");
            }

            if (!string.IsNullOrWhiteSpace(s))
            {
                builder.Append(s);
                builder.Append(' ');
            }
        }

        string str = builder.ToString().Trim();
        builder.Clear();
        CorePlugin.StringBuilderPool.Release(builder);

        return str;
    }

    /// <summary>
    /// Converts a field value to its text representation for CSS class name generation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The text representation, or empty string if the value is hidden.</returns>
    private string GetValueText(object value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        switch (value)
        {
            case SEnum sEnum:
                {
                    bool hidden = sEnum.Field?.Attributes.GetIsHiddenOrDisabled() == true;
                    return hidden ? string.Empty : sEnum.Value;
                }

            default:
                return value.ToString();
        }
    }
}

#endregion

#region TeminalBriefAttribute

/// <summary>
/// A brief attribute that extracts the terminal (last) segment from a path-based brief.
/// </summary>
[NativeType(CodeBase = "*Brief", Name = "TeminalBrief", Description = "Path Terminal Brief", Icon = "*CoreIcon|Brief")]
public class TeminalBriefAttribute : DesignAttribute, IBrief
{
    /// <inheritdoc/>
    public string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief)
    {
        string brief = baseBrief();

        return brief?.GetPathTerminal() ?? string.Empty;
    }
}

#endregion

#region TypeNameAndBriefAttribute

/// <summary>
/// A brief attribute that combines the type name with the base brief in the format "Type (brief)".
/// </summary>
[NativeType(CodeBase = "*Brief", Name = "TypeNameAndBrief", Description = "Type Name and Brief", Icon = "*CoreIcon|Brief")]
public class TypeNameAndBriefAttribute : DesignAttribute, IBrief
{
    /// <inheritdoc/>
    public string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief)
    {
        var sobj = obj as SObject;
        if (sobj is null)
        {
            return baseBrief();
        }

        var type = sobj.ObjectType?.Target as DCompond;
        if (type is null)
        {
            return baseBrief();
        }

        string brief = baseBrief();

        if (!string.IsNullOrWhiteSpace(brief))
        {
            return $"{type} ({brief})";
        }
        else
        {
            return type.ToString();
        }
    }
}

#endregion

#region BriefOrTypeNameAttribute

/// <summary>
/// A brief attribute that returns the base brief if available, otherwise falls back to the type name.
/// </summary>
[NativeType(CodeBase = "*Brief", Name = "BriefOrTypeName", Description = "Brief or Type Name", Icon = "*CoreIcon|Brief")]
public class BriefOrTypeNameAttribute : DesignAttribute, IBrief
{
    /// <inheritdoc/>
    public string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief)
    {
        string brief = baseBrief();

        if (!string.IsNullOrWhiteSpace(brief))
        {
            return brief;
        }

        var sobj = obj as SObject;
        if (sobj is null)
        {
            return null;
        }

        var type = sobj.ObjectType?.Target as DCompond;
        if (type is null)
        {
            return null;
        }

        return type.ToString();
    }
}

#endregion

#region NoneDefaultFieldsAttribute

/// <summary>
/// A brief attribute that generates display text listing all fields that differ from their default values.
/// </summary>
[NativeType(CodeBase = "*Brief", Name = "NoneDefaultFields", Description = "Non-Default Fields Brief", Icon = "*CoreIcon|Brief")]
public class NoneDefaultFieldsAttribute : DesignAttribute, IBrief
{
    /// <summary>
    /// Thread-local string builder for constructing brief text.
    /// </summary>
    [ThreadStatic]
    private static StringBuilder _builder;

    /// <inheritdoc/>
    public string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief)
    {
        var sobj = obj as SObject;
        if (sobj is null)
        {
            return baseBrief();
        }

        var type = sobj.ObjectType?.Target as DCompond;
        if (type is null)
        {
            return baseBrief();
        }

        _builder ??= new StringBuilder();

        _builder.Length = 0;
        bool append = false;

        foreach (var field in type.PublicStructFields)
        {
            var value = sobj.GetProperty(field.Id);
            if (!field.EqualsDefaultValue(value))
            {
                if (append)
                {
                    _builder.Append(", ");
                }
                else
                {
                    append = true;
                }

                _builder.Append(field.DisplayText);
            }
        }

        var s = _builder.ToString();
        _builder.Length = 0;

        return s;
    }
}

#endregion
