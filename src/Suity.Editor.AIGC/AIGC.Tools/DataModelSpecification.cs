using Suity.Collections;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Design;
using Suity.Editor.Types;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Suity.Editor.AIGC.Tools;


/// <summary>
/// Represents a complete data model specification containing multiple structure definitions.
/// </summary>
[ToolReturnType(typeof(IEnumerable<TypeDesignItem>))]
public class DataModelSpecification
{
    /// <summary>
    /// Delegate for building a <see cref="DataModelSpecification"/> from a type design document.
    /// </summary>
    /// <param name="doc">The type design document.</param>
    /// <param name="names">The names of structures to include.</param>
    /// <returns>A new <see cref="DataModelSpecification"/> instance.</returns>
    public delegate DataModelSpecification BuildSpecFunc(ITypeDesignDocument doc, IEnumerable<string> names);

    /// <summary>
    /// Gets or sets the function used to build specifications from type design documents.
    /// </summary>
    public static BuildSpecFunc BuildSpec { get; set; }

    /// <summary>
    /// Gets or sets the list of data structures in the game.
    /// </summary>
    [Description("the data structures of the game")]
    public List<StructureSpecification> Structures { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of structures marked for deletion. This field is for internal use only; leave it empty.
    /// </summary>
    [Description("This field is for internal deletion use only, leave it empty.")]
    public List<string> DeletedStructureNames { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DataModelSpecification"/> class.
    /// </summary>
    public DataModelSpecification()
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
    public Dictionary<string, StructureSpecification> ToDictionary()
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

    /// <summary>
    /// Converts this specification to a <see cref="DataModelSegmentation"/>.
    /// </summary>
    /// <returns>A new <see cref="DataModelSegmentation"/> instance.</returns>
    public DataModelSegmentation ToSegmentation()
    {
        var segs = new DataModelSegmentation();

        StringBuilder builder = new();

        foreach (var spec in Structures)
        {
            builder.Clear();

            foreach (var field in spec.Items)
            {
                builder.Append("- ");
                field.BuildFullText(builder);
                builder.AppendLine();
            }

            var seg = new StructureSegment 
            {
                Name = spec.Name,
                Type = spec.Type,
                Brief = spec.Brief,
                Usage = spec.Usage,
                DrivenMode = spec.DrivenMode,
                DerivedFrom = spec.DerivedFrom,
                ItemDefinition = builder.ToString().Trim(),
                Attributes = [..spec.Attributes],
            };

            segs.Structures.Add(seg);
        }

        return segs;
    }


    /// <summary>
    /// Attempts to parse a text string into a <see cref="DataModelSpecification"/>.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="specs">When this method returns, contains the parsed specification, or <c>null</c> if parsing failed.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string text, out DataModelSpecification specs)
    {
        if (DataModelSegmentation.TryParse(text, out var segs))
        {
            return TryParse(segs, out specs);
        }

        specs = null;
        return false;
    }

    /// <summary>
    /// Attempts to parse a <see cref="DataModelSegmentation"/> into a <see cref="DataModelSpecification"/>.
    /// </summary>
    /// <param name="segs">The segmentation to parse.</param>
    /// <param name="specs">When this method returns, contains the parsed specification, or <c>null</c> if parsing failed.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParse(DataModelSegmentation segs, out DataModelSpecification specs)
    {
        if (segs is null || segs.Structures is null)
        {
            specs = null;
            return false;
        }

        var specsParse = new DataModelSpecification();
        foreach (var seg in segs.Structures)
        {
            if (DataModelService.Instance.TryParseSpecification(seg, out var spec))
            {
                specsParse.Structures.Add(spec);
            }
            else
            {
                specs = null;
                return false;
            }
        }

        specs = specsParse;
        return true;
    }
}

/// <summary>
/// Represents the specification of a single data structure, including its name, type, fields, and metadata.
/// </summary>
public class StructureSpecification
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
    public string DerivedFrom { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a brief introduction of the data structure.
    /// </summary>
    [Description("The brief introduction of the data structure.")]
    public string Brief { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of fields (items) in the data structure.
    /// </summary>
    [Description("the items of the data structure")]
    public List<FieldSpecification> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the attributes associated with the data structure.
    /// </summary>
    [Description("The attributes of the data structure.")]
    public List<string> Attributes { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StructureSpecification"/> class.
    /// </summary>
    public StructureSpecification()
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
        string derived = !string.IsNullOrWhiteSpace(DerivedFrom) ? $" : derived from {DerivedFrom}" : "";

        string brief = !string.IsNullOrWhiteSpace(Brief) ? $" - {Brief}" : "";

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

        string derived = !string.IsNullOrWhiteSpace(DerivedFrom) ? $" base='{DerivedFrom}'" : "";

        string usage = Usage != DataUsageMode.None ? $" usage='{Usage}'" : "";
        string driven = DrivenMode != DataDrivenMode.None ? $" driven='{DrivenMode}'" : "";

        Attributes ??= [];
        string attr = Attributes.Count > 0 ? $" attr='{string.Join(",", Attributes)}'" : "";

        builder.AppendLine($"<type name='{name}' def='{Type}'{derived}{usage}{driven}{attr}>\n{Brief}");

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
            Brief = Brief,
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
        if (!string.IsNullOrWhiteSpace(Brief))
        {
            builder.Append($" # {Brief}");
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
            if (!string.IsNullOrWhiteSpace(DerivedFrom))
            {
                builder.AppendLine("  derivedFrom: " + DerivedFrom);
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
    /// Creates a <see cref="StructureSpecification"/> from a <see cref="DCompond"/>.
    /// </summary>
    /// <param name="dCompond">The compound type to convert.</param>
    /// <param name="fullName">Whether to use full type names.</param>
    /// <returns>A new <see cref="StructureSpecification"/> instance, or <c>null</c> if input is null.</returns>
    public static StructureSpecification FromDCompond(DCompond dCompond, bool fullName = false)
    {
        if (dCompond is null)
        {
            return null;
        }

        DataUsageMode usage = dCompond.GetDataUsageMode();
        DataDrivenMode drivenMode = dCompond.GetDataDrivenMode();

        var spec = new StructureSpecification
        {
            Name = fullName ? dCompond.FullTypeName : dCompond.Name,
            Type = dCompond is DAbstract ? DataStructureType.Abstract : DataStructureType.Struct,
            Usage = usage,
            DrivenMode = drivenMode,
            DerivedFrom = fullName ? dCompond.BaseType?.FullTypeName : dCompond.BaseType?.Name,
            Brief = dCompond.ToolTips
        };

        foreach (var field in dCompond.PublicStructFields)
        {
            var fieldSpec = FieldSpecification.FromDStructField(field, fullName);
            spec.Items.Add(fieldSpec);
        }

        return spec;
    }

    /// <summary>
    /// Creates a <see cref="StructureSpecification"/> from a <see cref="DEnum"/>.
    /// </summary>
    /// <param name="dEnum">The enum type to convert.</param>
    /// <returns>A new <see cref="StructureSpecification"/> instance, or <c>null</c> if input is null.</returns>
    public static StructureSpecification FromDEnum(DEnum dEnum)
    {
        if (dEnum is null)
        {
            return null;
        }

        var spec = new StructureSpecification
        {
            Name = dEnum.FullTypeName,
            Type = DataStructureType.Enum,
            Usage = DataUsageMode.None,
            DrivenMode = DataDrivenMode.None,
            Brief = dEnum.ToolTips,
        };

        foreach (var field in dEnum.EnumFields)
        {
            var fieldSpec = FieldSpecification.FromDEnumField(field);
            spec.Items.Add(fieldSpec);
        }

        return spec;
    }

    /// <summary>
    /// Converts a collection of structure specifications to a concatenated tag string.
    /// </summary>
    /// <param name="specs">The collection of specifications to convert.</param>
    /// <returns>A string containing the tag representation of all specifications.</returns>
    public static string ToTags(IEnumerable<StructureSpecification> specs)
    {
        var builder = new StringBuilder();

        foreach (var spec in specs)
        {
            spec.BuildTag(builder);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    /// <summary>
    /// Attempts to parse a <see cref="StructureSegment"/> into a <see cref="StructureSpecification"/>.
    /// </summary>
    /// <param name="seg">The structure segment to parse.</param>
    /// <param name="spec">When this method returns, contains the parsed specification, or <c>null</c> if parsing failed.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParse(StructureSegment seg, out StructureSpecification spec)
    {
        return DataModelService.Instance.TryParseSpecification(seg, out spec);
    }
}

/// <summary>
/// Represents the specification of a single field within a data structure.
/// </summary>
public class FieldSpecification
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
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the field, without any symbols (e.g., no angle brackets or array notation).
    /// </summary>
    [Description("The type of the field, without any symbols.")]
    public string FieldType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the field is an array.
    /// </summary>
    [Description("Whether the field is an array.")]
    public bool IsArray { get; set; }

    /// <summary>
    /// Gets or sets the attributes associated with the field.
    /// </summary>
    public List<string> Attributes { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldSpecification"/> class.
    /// </summary>
    public FieldSpecification()
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
        if (!string.IsNullOrWhiteSpace(Description))
        {
            s += $" # {Description}";
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
        if (!string.IsNullOrWhiteSpace(Description))
        {
            builder.Append($" # {Description}");
        }
    }


    /// <summary>
    /// Creates a <see cref="FieldSpecification"/> from a <see cref="DStructField"/>.
    /// </summary>
    /// <param name="field">The struct field to convert.</param>
    /// <param name="fullName">Whether to use full type names.</param>
    /// <returns>A new <see cref="FieldSpecification"/> instance.</returns>
    public static FieldSpecification FromDStructField(DStructField field, bool fullName = false)
    {
        var fieldType = field.FieldType;

        var spec = new FieldSpecification
        {
            Name = field.Name,
            Description = field.ToolTips,
            FieldType = fullName ? fieldType.OriginType.GetFullTypeName() : fieldType.OriginType.GetShortTypeName(),
            IsArray = fieldType.IsArray,
        };

        if (field.Optional)
        {
            spec.Attributes.Add("Nullable");
        }

        if (field.GetAttribute<NumericRangeAttribute>() is { } range)
        {
            spec.Attributes.Add($"{range.Min}..{range.Max}");
        }

        return spec;
    }

    /// <summary>
    /// Creates a <see cref="FieldSpecification"/> from a <see cref="DEnumField"/>.
    /// </summary>
    /// <param name="field">The enum field to convert.</param>
    /// <returns>A new <see cref="FieldSpecification"/> instance.</returns>
    public static FieldSpecification FromDEnumField(DEnumField field)
    {
        var spec = new FieldSpecification
        {
            Name = field.Name,
            Description = field.ToolTips,
        };

        return spec;
    }
}