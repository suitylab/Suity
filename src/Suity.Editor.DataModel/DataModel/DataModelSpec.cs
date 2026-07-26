using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Types;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Suity.Editor.DataModel;

/// <summary>
/// Represents a complete data model specification containing multiple structure definitions.
/// </summary>
public class DataModelSpec
{
    /// <summary>
    /// Delegate for building a <see cref="DataModelSpec"/> from a type design document.
    /// </summary>
    /// <param name="doc">The type design document.</param>
    /// <param name="names">The names of structures to include.</param>
    /// <returns>A new <see cref="DataModelSpec"/> instance.</returns>
    public delegate DataModelSpec BuildSpecFunc(ITypeDesignDocument doc, IEnumerable<string> names);

    /// <summary>
    /// Gets or sets the function used to build specifications from type design documents.
    /// </summary>
    public static BuildSpecFunc BuildSpec { get; set; }

    /// <summary>
    /// Gets or sets the list of data structures in the game.
    /// </summary>
    [Description("the data structures of the game")]
    public List<TypeSpec> Structures { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of structures marked for deletion. This field is for internal use only; leave it empty.
    /// </summary>
    [Description("This field is for internal deletion use only, leave it empty.")]
    public List<string> DeletedStructureNames { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DataModelSpec"/> class.
    /// </summary>
    public DataModelSpec()
    {
    }

    /// <summary>
    /// Converts the structures to a dictionary keyed by structure name.
    /// </summary>
    /// <returns>A dictionary mapping structure names to their specifications.</returns>
    public Dictionary<string, TypeSpec> ToDictionary()
    {
        return Structures.ToDictionarySafe(x => x.Name, x => x);
    }

}

/// <summary>
/// Represents the specification of a single data structure, including its name, type, fields, and metadata.
/// </summary>
public class TypeSpec
{
    /// <summary>
    /// Gets or sets the name of the data structure. Must be a valid identifier in PascalCase.
    /// </summary>
    [Description("The name of the data structure, must be a valid identifier, and in PascalCase.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the data structure (e.g., struct, enum, abstract).
    /// </summary>
    [Description("the type of the data structure")]
    public DataStructureType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the data structure is used to create a data table.
    /// </summary>
    [Description("Indicate if the data structure is used to create a data table.")]
    public DataUsageMode Usage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the data structure is driven by other data structures. Default is 'Active'.
    /// </summary>
    [Description("Indicate if the data structure is driven by other data structures. Default is 'Active'")]
    public DataDrivenMode DrivenMode { get; set; }

    /// <summary>
    /// Gets or sets the name of the abstract struct this structure derives from. Leave empty if not derived.
    /// </summary>
    [Description("If the data structure is an derived struct, fill in the name of the abstract struct it derived from, otherwise leave it empty")]
    public string BaseType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a brief introduction of the data structure.
    /// </summary>
    [Description("The brief introduction of the data structure.")]
    public string Tooltip { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of fields (items) in the data structure.
    /// </summary>
    [Description("the items of the data structure")]
    public List<FieldSpec> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the attributes associated with the data structure.
    /// </summary>
    [Description("The attributes of the data structure.")]
    public List<string> Attributes { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeSpec"/> class.
    /// </summary>
    public TypeSpec()
    {
    }


    /// <summary>
    /// Returns a brief text representation of this structure.
    /// </summary>
    /// <returns>A string containing the brief info.</returns>
    public override string ToString() => Name;


    /// <summary>
    /// Creates a <see cref="TypeSpec"/> from a <see cref="DCompond"/>.
    /// </summary>
    /// <param name="dCompond">The compound type to convert.</param>
    /// <param name="fullName">Whether to use full type names.</param>
    /// <returns>A new <see cref="TypeSpec"/> instance, or <c>null</c> if input is null.</returns>
    public static TypeSpec FromDCompond(DCompond dCompond, bool fullName = false)
    {
        if (dCompond is null)
        {
            return null;
        }

        DataUsageMode usage = dCompond.GetDataUsageMode();
        DataDrivenMode drivenMode = dCompond.GetDataDrivenMode();

        var spec = new TypeSpec
        {
            Name = fullName ? dCompond.FullTypeName : dCompond.Name,
            Type = dCompond is DAbstract ? DataStructureType.Abstract : DataStructureType.Struct,
            Usage = usage,
            DrivenMode = drivenMode,
            BaseType = fullName ? dCompond.BaseType?.FullTypeName : dCompond.BaseType?.Name,
            Tooltip = dCompond.ToolTips
        };

        foreach (var field in dCompond.PublicStructFields)
        {
            var fieldSpec = FieldSpec.FromDStructField(field, fullName);
            spec.Items.Add(fieldSpec);
        }

        return spec;
    }

    /// <summary>
    /// Creates a <see cref="TypeSpec"/> from a <see cref="DEnum"/>.
    /// </summary>
    /// <param name="dEnum">The enum type to convert.</param>
    /// <returns>A new <see cref="TypeSpec"/> instance, or <c>null</c> if input is null.</returns>
    public static TypeSpec FromDEnum(DEnum dEnum)
    {
        if (dEnum is null)
        {
            return null;
        }

        var spec = new TypeSpec
        {
            Name = dEnum.FullTypeName,
            Type = DataStructureType.Enum,
            Usage = DataUsageMode.None,
            DrivenMode = DataDrivenMode.None,
            Tooltip = dEnum.ToolTips,
        };

        foreach (var field in dEnum.EnumFields)
        {
            var fieldSpec = FieldSpec.FromDEnumField(field);
            spec.Items.Add(fieldSpec);
        }

        return spec;
    }

}

/// <summary>
/// Represents the specification of a single field within a data structure.
/// </summary>
public class FieldSpec
{
    /// <summary>
    /// Gets or sets the name of the field. Must be a valid identifier in PascalCase.
    /// </summary>
    [Description("the name of the field, must be a valid identifier, and in PascalCase.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the field.
    /// </summary>
    [Description("the description of the field")]
    public string Tooltip { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the field, without any symbols (e.g., no angle brackets or array notation).
    /// </summary>
    [Description("The type of the field, without any symbols.")]
    public string FieldType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default value of the field.
    /// </summary>
    [Description("The default value of the field.")]
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the field is an array.
    /// </summary>
    [Description("Whether the field is an array.")]
    public bool IsArray { get; set; }

    /// <summary>
    /// Gets or sets the attributes associated with the field.
    /// </summary>
    public List<AttributeSpec> Attributes { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldSpec"/> class.
    /// </summary>
    public FieldSpec()
    {
    }

    /// <summary>
    /// Returns a brief text representation of this field.
    /// </summary>
    /// <returns>A string containing the brief info.</returns>
    public override string ToString() => Name;


    /// <summary>
    /// Creates a <see cref="FieldSpec"/> from a <see cref="DStructField"/>.
    /// </summary>
    /// <param name="field">The struct field to convert.</param>
    /// <param name="fullName">Whether to use full type names.</param>
    /// <returns>A new <see cref="FieldSpec"/> instance.</returns>
    public static FieldSpec FromDStructField(DStructField field, bool fullName = false)
    {
        var fieldType = field.FieldType;

        var spec = new FieldSpec
        {
            Name = field.Name,
            Tooltip = field.ToolTips,
            FieldType = fullName ? fieldType.OriginType.GetFullTypeName() : fieldType.OriginType.GetShortTypeName(),
            IsArray = fieldType.IsArray,
        };

        if (field.Optional)
        {
            spec.Attributes.Add(new AttributeSpec("Optional"));
        }

        if (field.GetAttribute<NumericRangeAttribute>() is { } range)
        {
            spec.Attributes.Add(new AttributeSpec("ValueRange", new("min", range.Min.ToString()), new("max", range.Max.ToString())));
        }

        return spec;
    }

    /// <summary>
    /// Creates a <see cref="FieldSpec"/> from a <see cref="DEnumField"/>.
    /// </summary>
    /// <param name="field">The enum field to convert.</param>
    /// <returns>A new <see cref="FieldSpec"/> instance.</returns>
    public static FieldSpec FromDEnumField(DEnumField field)
    {
        var spec = new FieldSpec
        {
            Name = field.Name,
            Tooltip = field.ToolTips,
        };

        return spec;
    }
}

public class AttributeSpec
{
    public string Name { get; set; } = string.Empty;

    public Dictionary<string, string> Parameters { get; set; }

    public AttributeSpec()
    {
        Parameters = [];
    }

    public AttributeSpec(string name)
    {
        Name = name;
        Parameters = new Dictionary<string, string>();
    }

    public AttributeSpec(string name, IEnumerable<KeyValuePair<string, string>> parameters)
    {
        Name = name;
        Parameters = parameters.ToDictionary(x => x.Key, x => x.Value);
    }

    public AttributeSpec(string name, params KeyValuePair<string, string>[] parameters)
    {
        Name = name;
        Parameters = parameters.ToDictionary(x => x.Key, x => x.Value);
    }

    public override string ToString() => ToBriefInfo();

    public string ToBriefInfo()
    {
        string s = $"[{Name}]";
        if (Parameters.Count > 0)
        {
            s += $"({string.Join(", ", Parameters.Select(p => $"{p.Key}={p.Value}"))})";
        }

        return s;
    }
}