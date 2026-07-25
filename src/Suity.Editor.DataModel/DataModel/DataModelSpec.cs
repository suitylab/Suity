using Suity.Collections;
using Suity.Editor.AIGC;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
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
    /// Converts the entire specification to a full text representation.
    /// </summary>
    /// <returns>A string containing the full text of all structures.</returns>
    public string ToFullText()
    {
        var builder = new StringBuilder();
        BuildFullText(builder);

        return builder.ToString();
    }

    /// <summary>
    /// Appends the full text representation of all structures to the specified builder.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    public void BuildFullText(StringBuilder builder)
    {
        foreach (var structure in Structures)
        {
            structure.BuildFullText(builder);
            builder.AppendLine();
        }
    }

    /// <summary>
    /// Converts the specification to a tag-based string representation.
    /// </summary>
    /// <returns>A string containing the tag representation of all structures.</returns>
    public string ToTag()
    {
        var builder = new StringBuilder();
        BuildTag(builder);

        return builder.ToString();
    }

    /// <summary>
    /// Appends the tag representation of all structures to the specified builder.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    public void BuildTag(StringBuilder builder)
    {
        foreach (var structure in Structures)
        {
            structure.BuildTag(builder);
            builder.AppendLine();
        }
    }

    /// <summary>
    /// Gets a brief text summary of all structures in the specification.
    /// </summary>
    /// <returns>A string containing brief info for each structure.</returns>
    public string ToBriefInfo()
    {
        return string.Join("\n", Structures.Select(x => x.ToBriefInfo()));
    }

    /// <summary>
    /// Converts the structures to a dictionary keyed by structure name.
    /// </summary>
    /// <returns>A dictionary mapping structure names to their specifications.</returns>
    public Dictionary<string, TypeSpec> ToDictionary()
    {
        return Structures.ToDictionarySafe(x => x.Name, x => x);
    }

    /// <summary>
    /// Converts all structures to an array of guiding items for AI generation.
    /// </summary>
    /// <returns>An array of <see cref="GenerativeGuidingItem"/> instances.</returns>
    public GenerativeGuidingItem[] ToGuidingItems()
    {
        return Structures.Select(x => x.ToGuidingItem()).ToArray();
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
    public override string ToString() => ToBriefInfo();

    /// <summary>
    /// Gets a brief text representation of this structure.
    /// </summary>
    /// <param name="withType">Whether to include the type name in the output.</param>
    /// <returns>A string containing the brief info.</returns>
    public string ToBriefInfo(bool withType = true)
    {
        string usage = Usage != DataUsageMode.None ? $"[{Usage}] " : "";
        string derived = !string.IsNullOrWhiteSpace(BaseType) ? $" : derived from {BaseType}" : "";

        string brief = !string.IsNullOrWhiteSpace(Tooltip) ? $" - {Tooltip}" : "";

        if (withType)
        {
            return $"{usage}{GetTypeName()} {Name}{derived}{brief}";
        }
        else
        {
            return $"{usage}{Name}{derived}{brief}";
        }
    }

    /// <summary>
    /// Converts this structure to a tag-based string representation.
    /// </summary>
    /// <param name="nameSpace">Optional namespace to prefix the structure name.</param>
    /// <returns>A string containing the tag representation.</returns>
    public string ToTag(string nameSpace = null)
    {
        var builder = new StringBuilder();
        BuildTag(builder, nameSpace);

        return builder.ToString();
    }

    /// <summary>
    /// Appends the tag representation of this structure to the specified builder.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    /// <param name="nameSpace">Optional namespace to prefix the structure name.</param>
    public void BuildTag(StringBuilder builder, string nameSpace = null)
    {
        string name = Name;
        if (!string.IsNullOrWhiteSpace(nameSpace))
        {
            name = $"{nameSpace}.{name}";
        }

        string derived = !string.IsNullOrWhiteSpace(BaseType) ? $" base='{BaseType}'" : "";

        string usage = Usage != DataUsageMode.None ? $" usage='{Usage}'" : "";
        string driven = DrivenMode != DataDrivenMode.None ? $" driven='{DrivenMode}'" : "";

        Attributes ??= [];
        string attr = Attributes.Count > 0 ? $" attr='{string.Join(",", Attributes)}'" : "";

        builder.AppendLine($"<type name='{name}' def='{Type}'{derived}{usage}{driven}{attr}>\n{Tooltip}");

        if (Type == DataStructureType.Enum)
        {
            builder.Append("Value: ");
            builder.AppendLine(string.Join(", ", Items.Select(o => o.Name)));
        }
        else
        {
            builder.AppendLine("Fields:");
            foreach (var item in Items)
            {
                builder.Append("- ");
                item.BuildFullText(builder);
                builder.AppendLine();
            }
        }

        builder.AppendLine("</type>");
    }

    /// <summary>
    /// Converts this structure to a full text representation.
    /// </summary>
    /// <returns>A string containing the full text of the structure.</returns>
    public string ToFullText()
    {
        var builder = new StringBuilder();
        BuildFullText(builder);

        return builder.ToString();
    }

    /// <summary>
    /// Converts this structure to a <see cref="GenerativeGuidingItem"/> for AI generation.
    /// </summary>
    /// <returns>A new <see cref="GenerativeGuidingItem"/> instance.</returns>
    public GenerativeGuidingItem ToGuidingItem()
    {
        return new GenerativeGuidingItem
        {
            Name = Name,
            Brief = Tooltip,
            HtmlColor = string.Empty,
            Prompt = ToFullText()
        };
    }

    /// <summary>
    /// Appends the full text representation of this structure to the specified builder.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    public void BuildFullText(StringBuilder builder)
    {
        string typeName = Type switch
        {
            DataStructureType.Struct or DataStructureType.Abstract => "struct",
            DataStructureType.Enum => "enum",
            _ => "unknown"
        };

        builder.Append($"{typeName} : {Name}");
        if (!string.IsNullOrWhiteSpace(Tooltip))
        {
            builder.Append($" # {Tooltip}");
        }
        builder.AppendLine();

        builder.AppendLine("{");

        if (Type == DataStructureType.Enum)
        {
            foreach (var item in Items)
            {
                builder.Append(' ', 2);
                item.BuildFullText(builder);
                builder.AppendLine();
            }

            builder.AppendLine("}");
        }
        else
        {
            builder.AppendLine("  isAbstract: " + (Type == DataStructureType.Abstract).ToString().ToLower());
            if (!string.IsNullOrWhiteSpace(BaseType))
            {
                builder.AppendLine("  derivedFrom: " + BaseType);
            }
            if (Usage != DataUsageMode.None)
            {
                builder.AppendLine("  usage: " + Usage.ToString());
            }
            if (DrivenMode != DataDrivenMode.None)
            {
                builder.AppendLine("  driven: " + DrivenMode.ToString());
            }
            builder.AppendLine("  fields: {");
            foreach (var item in Items)
            {
                builder.Append(' ', 4);
                item.BuildFullText(builder);
                builder.AppendLine();
            }
            builder.AppendLine("  }");

            builder.AppendLine("}");
        }
    }

    /// <summary>
    /// Gets the display name of the structure type.
    /// </summary>
    /// <returns>A string representing the type name.</returns>
    /// <exception cref="AigcException">Thrown when the structure type is unknown.</exception>
    public string GetTypeName() => Type switch
    {
        DataStructureType.Struct => "Struct",
        DataStructureType.Enum => "Enum",
        DataStructureType.Abstract => "Abstract Struct",
        DataStructureType.Event => "Event",
        _ => throw new AigcException("Unknown data structure type : " + Type)
    };

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

    /// <summary>
    /// Converts a collection of structure specifications to a concatenated tag string.
    /// </summary>
    /// <param name="specs">The collection of specifications to convert.</param>
    /// <returns>A string containing the tag representation of all specifications.</returns>
    public static string ToTags(IEnumerable<TypeSpec> specs)
    {
        var builder = new StringBuilder();

        foreach (var spec in specs)
        {
            spec.BuildTag(builder);
            builder.AppendLine();
        }

        return builder.ToString();
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
    public override string ToString() => ToBriefInfo();

    /// <summary>
    /// Gets a brief text representation of this field.
    /// </summary>
    /// <returns>A string containing the field name and type.</returns>
    public string ToBriefInfo()
    {
        if (string.IsNullOrWhiteSpace(FieldType))
        {
            return Name;
        }

        string type = FieldType;
        if (IsArray)
        {
            type += "[]";
        }

        return $"{Name}: {type}";
    }

    /// <summary>
    /// Gets a full text representation of this field, including the description if available.
    /// </summary>
    /// <returns>A string containing the full text of the field.</returns>
    public string ToFullText()
    {
        string s = ToBriefInfo();
        if (!string.IsNullOrWhiteSpace(Tooltip))
        {
            s += $" # {Tooltip}";
        }

        return s;
    }

    /// <summary>
    /// Appends the full text representation of this field to the specified builder.
    /// </summary>
    /// <param name="builder">The string builder to append to.</param>
    public void BuildFullText(StringBuilder builder)
    {
        builder.Append(ToBriefInfo());
        if (!string.IsNullOrWhiteSpace(Tooltip))
        {
            builder.Append($" # {Tooltip}");
        }
    }


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