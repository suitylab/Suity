using Suity.Collections;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Design;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Represents a collection of structure segments that define a data model segmentation.
/// </summary>
public class DataModelSegmentation
{
    /// <summary>
    /// Gets or sets the list of data structures in the segmentation.
    /// </summary>
    [Description("the data structures")]
    public List<StructureSegment> Structures { get; set; } = [];

    /// <summary>
    /// Converts the structures to a dictionary keyed by structure name.
    /// </summary>
    /// <returns>A dictionary mapping structure names to their segments.</returns>
    public Dictionary<string, StructureSegment> ToDictionary()
    {
        try
        {
            return Structures.ToDictionarySafe(x => x.Name, x => x);
        }
        catch (Exception)
        {
            throw;
        }
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
    /// Gets a concatenated string of structure definitions.
    /// </summary>
    /// <returns>A string containing the definition of each structure.</returns>
    public string ToDefinition() 
        => string.Join("\n", Structures.Select(x => x.ToDefinition()));

    /// <summary>
    /// Gets a concatenated string of brief info for all structures.
    /// </summary>
    /// <returns>A string containing brief info for each structure.</returns>
    public string ToBriefInfo() 
        => string.Join("\n", Structures.Select(x => x.ToBriefInfo()));

    /// <summary>
    /// Gets a concatenated string of full text for all structures.
    /// </summary>
    /// <returns>A string containing the full text of each structure.</returns>
    public string ToFullContent()
    {
        return string.Join("\n\n", Structures.Select(x => x.ToFullText()));
    }

    /// <summary>
    /// Gets a concatenated string of tag representations for all structures.
    /// </summary>
    /// <returns>A string containing the tag representation of each structure.</returns>
    public string ToTag()
    {
        return string.Join("\n\n", Structures.Select(x => x.ToTag()));
    }

    /// <summary>
    /// Merges the structures from this segmentation into another segmentation.
    /// </summary>
    /// <param name="other">The target segmentation to merge into.</param>
    /// <param name="overrideMode">If <c>true</c>, existing structures with the same name will be overwritten.</param>
    public void MergeTo(DataModelSegmentation other, bool overrideMode)
    {
        if (Structures.Count == 0)
        {
            return;
        }

        // other structures cache
        var othersDic = other.ToDictionary();

        // my structures cache
        var myItems = Structures.ToArray();

        Structures.Clear();

        foreach (var item in myItems.SkipNull())
        {
            if (overrideMode)
            {
                if (othersDic.ContainsKey(item.Name))
                {
                    // Override and try to set to the original position
                    int index = other.Structures.IndexOf(o => o.Name == item.Name);
                    if (index >= 0)
                    {
                        other.Structures[index] = item;
                    }
                    else
                    {
                        other.Structures.Add(item);
                    }

                    othersDic[item.Name] = item;
                }
                else
                {
                    other.Structures.Add(item);
                    othersDic.Add(item.Name, item);
                }
            }
            else
            {
                if (othersDic.ContainsKey(item.Name))
                {
                    // Contains
                    continue;
                }

                other.Structures.Add(item);
                othersDic.Add(item.Name, item);
            }
        }
    }


    /// <summary>
    /// Attempts to parse a tag-based text string into a <see cref="DataModelSegmentation"/>.
    /// </summary>
    /// <param name="tagText">The text to parse.</param>
    /// <param name="segmentation">When this method returns, contains the parsed segmentation, or <c>null</c> if parsing failed.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string tagText, out DataModelSegmentation segmentation)
    {
        if (string.IsNullOrWhiteSpace(tagText))
        {
            segmentation = null;
            return false;
        }

        try
        {
            segmentation = Parse(tagText);
            return true;
        }
        catch (Exception)
        {
            segmentation = null;
            return false;
        }
    }

    /// <summary>
    /// Parses a tag-based text string into a <see cref="DataModelSegmentation"/>.
    /// </summary>
    /// <param name="tagText">The text to parse.</param>
    /// <returns>A new <see cref="DataModelSegmentation"/> instance, or <c>null</c> if no valid tags are found.</returns>
    public static DataModelSegmentation Parse(string tagText)
    {
        var tags = LooseXml.ExtractNodes(tagText, "type");
        if (tags is null || tags.Length == 0)
        {
            return null;
        }

        HashSet<string> names = [];
        var segs = new DataModelSegmentation();
        foreach (var tag in tags)
        {
            string content = tag.InnerText?.Trim() ?? string.Empty;

            string name = tag.GetAttribute("name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (!names.Add(name))
            {
                // Duplicate
                continue;
            }

            string defStr = tag.GetAttribute("def");
            if (string.IsNullOrWhiteSpace(defStr))
            {
                continue;
            }

            string baseType = tag.GetAttribute("base");

            if (!Enum.TryParse<DataStructureType>(defStr, true, out var def))
            {
                continue;
            }

            if (!Enum.TryParse<DataUsageMode>(tag.GetAttribute("usage"), out var usage))
            {
                usage = DataUsageMode.None;
            }

            if (!Enum.TryParse<DataDrivenMode>(tag.GetAttribute("driven"), out var driven))
            {
                driven = DataDrivenMode.None;
            }

            string attr = tag.GetAttribute("attr") ?? string.Empty;
            var attrs = attr.Split([','], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Distinct()
                .ToList();

            string itemList = string.Empty;
            if (content.StartsWith("-"))
            {
                itemList = content;
                content = string.Empty;
            }
            else
            {
                int lineIndex = content.IndexOf('\n');
                if (lineIndex > 0)
                {
                    string fullContent = content;

                    itemList = fullContent[(lineIndex + 1)..].Trim();
                    content = fullContent[..lineIndex].Trim();
                }
            }

            if (tag.GetAttribute("doc") is { } doc && !string.IsNullOrWhiteSpace(doc))
            {
                content = doc;
            }

            var seg = new StructureSegment
            {
                Name = name,
                Type = def,
                Brief = content,
                DerivedFrom = baseType,
                Usage = usage,
                DrivenMode = driven,
                ItemDefinition = itemList,
                Attributes = attrs,
            };

            segs.Structures.Add(seg);
        }

        return segs;
    }
}

/// <summary>
/// Represents a single segment of a data structure, including its name, type, and item definitions.
/// </summary>
public class StructureSegment
{
    /// <summary>
    /// Gets or sets the name of the data structure. Must be a valid identifier in PascalCase.
    /// </summary>
    [Description("The name of the data structure, must be a valid identifier, and in PascalCase.")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the data structure.
    /// </summary>
    [Description("The type of the data structure.")]
    public DataStructureType Type { get; set; }

    /// <summary>
    /// Gets or sets a brief introduction of the data structure.
    /// </summary>
    [Description("The brief introduction of the data structure.")]
    public string Brief { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the data structure is used to create a data table. Default is 'None'.
    /// </summary>
    [Description("Indicate if the data structure is used to create a data table. Default is 'None'")]
    public DataUsageMode Usage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the data structure is driven by other data structures. Default is 'Active'.
    /// </summary>
    [Description("Indicate if the data structure is driven by other data structures. Default is 'Active'")]
    public DataDrivenMode DrivenMode { get; set; }

    /// <summary>
    /// Gets or sets the base class of the data structure.
    /// </summary>
    [Description("The base class of the data structure.")]
    public string DerivedFrom { get; set; }

    /// <summary>
    /// Gets or sets the definition of all fields or values in the data structure.
    /// </summary>
    [Description("All fields or values of the data structure.")]
    public string ItemDefinition { get; set; }

    /// <summary>
    /// Gets or sets the attributes associated with the data structure.
    /// </summary>
    [Description("The attributes of the data structure.")]
    public List<string> Attributes { get; set; } = [];

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
    /// Returns a brief text representation of this segment.
    /// </summary>
    /// <returns>A string containing the brief info.</returns>
    public override string ToString() => ToBriefInfo(true);

    /// <summary>
    /// Gets a definition string for this structure segment.
    /// </summary>
    /// <param name="withType">Whether to include the type name in the output.</param>
    /// <returns>A string containing the definition.</returns>
    public string ToDefinition(bool withType = true)
    {
        string derived = !string.IsNullOrWhiteSpace(DerivedFrom) ? $" extends {DerivedFrom}" : "";
        string usage = Usage != DataUsageMode.None ? $"[{Usage}] " : "";

        string str;

        if (withType)
        {
            str = $"{usage}{GetTypeName()} {Name}{derived}";
        }
        else
        {
            str = $"{usage}{Name}{derived}";
        }

        return str;
    }

    /// <summary>
    /// Gets a brief text representation of this segment.
    /// </summary>
    /// <param name="withType">Whether to include the type name in the output.</param>
    /// <returns>A string containing the brief info.</returns>
    public string ToBriefInfo(bool withType = true)
    {
        string str = ToDefinition(withType);
        if (!string.IsNullOrWhiteSpace(Brief))
        {
            str = $"{str} - {Brief}";
        }

        return str;
    }

    /// <summary>
    /// Converts this segment to a tag-based string representation.
    /// </summary>
    /// <returns>A string containing the tag representation.</returns>
    public string ToTag()
    {
        string derived = !string.IsNullOrWhiteSpace(DerivedFrom) ? $" base='{DerivedFrom}'" : "";

        string usage = Usage != DataUsageMode.None ? $" usage='{Usage}'" : "";
        string driven = DrivenMode != DataDrivenMode.None ? $" driven='{DrivenMode}'" : "";

        Attributes ??= [];
        string attr = Attributes.Count > 0 ? $" attr='{string.Join(",", Attributes)}'" : "";

        string doc = Brief?.Trim() ?? string.Empty;
        doc = doc.Replace("\n", " ");
        doc = doc.Replace("\r", "");
        if (!string.IsNullOrWhiteSpace(doc))
        {
            doc = $" doc='{doc}'";
        }

        string content = !string.IsNullOrWhiteSpace(ItemDefinition) ? $"\n{ItemDefinition}" : string.Empty;

        return $"<type name='{Name}' def='{Type}'{derived}{usage}{driven}{attr}{doc}>{content}\n</type>";
    }

    /// <summary>
    /// Converts this segment to a full text representation.
    /// </summary>
    /// <returns>A string containing the full text of the segment.</returns>
    public string ToFullText()
    {
        string fieldName = Type == DataStructureType.Enum ? "Values" : "Fields";
        ItemDefinition ??= string.Empty;
        string itemStr = ItemDefinition; //string.Join("\n", ItemDescription);
        return $"{ToBriefInfo(true)}\n- Type: {GetTypeName()}\n- {fieldName}: \n{itemStr}";
    }

    /// <summary>
    /// Converts this segment to a <see cref="GenerativeGuidingItem"/> for AI generation.
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
    /// Builds a concatenated string of brief info for a collection of structure segments.
    /// </summary>
    /// <param name="seg">The collection of segments to convert.</param>
    /// <returns>A string containing brief info for each segment.</returns>
    public static string BuildTypeList(IEnumerable<StructureSegment> seg)
    {
        return string.Join("\n", seg.Select(x => x.ToBriefInfo()));
    }

    /// <summary>
    /// Creates a <see cref="StructureSegment"/> from a <see cref="DCompond"/>.
    /// </summary>
    /// <param name="dCompond">The compound type to convert.</param>
    /// <param name="fullName">Whether to use full type names.</param>
    /// <returns>A new <see cref="StructureSegment"/> instance.</returns>
    public static StructureSegment FromDCompond(DCompond dCompond, bool fullName = false)
    {
        var usage = dCompond.GetDataUsageMode();
        var driven = dCompond.GetDataDrivenMode();

        var seg = new StructureSegment
        {
            Name = fullName ? dCompond.FullTypeName : dCompond.Name,
            Type = dCompond is DAbstract ? DataStructureType.Abstract : DataStructureType.Struct,
            DerivedFrom = fullName ? dCompond.BaseType?.FullTypeName : dCompond.BaseType?.Name,
            Usage = usage,
            DrivenMode = driven,
            Brief = dCompond.ToolTips
        };

        List<FieldSpecification> fields = [];

        foreach (var field in dCompond.PublicStructFields)
        {
            var fieldSpec = FieldSpecification.FromDStructField(field, fullName);
            fields.Add(fieldSpec);
        }

        seg.ItemDefinition = string.Join("\n", fields.Select(o => o.ToFullText()));

        return seg;
    }

    /// <summary>
    /// Creates a <see cref="StructureSegment"/> from a <see cref="DEnum"/>.
    /// </summary>
    /// <param name="dEnum">The enum type to convert.</param>
    /// <returns>A new <see cref="StructureSegment"/> instance.</returns>
    public static StructureSegment FromDEnum(DEnum dEnum)
    {
        var seg = new StructureSegment
        {
            Name = dEnum.FullTypeName,
            Type = DataStructureType.Enum,
            Usage = DataUsageMode.None,
            DrivenMode = DataDrivenMode.None,
            Brief = dEnum.ToolTips,
        };

        List<FieldSpecification> fields = [];

        foreach (var field in dEnum.EnumFields)
        {
            var fieldSpec = FieldSpecification.FromDEnumField(field);
            fields.Add(fieldSpec);
        }

        seg.ItemDefinition = string.Join("\n", fields.Select(o => o.ToFullText()));

        return seg;
    }
}
