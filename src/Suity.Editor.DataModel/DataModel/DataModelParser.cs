using System;
using System.Xml.Linq;
using Suity.Editor.Design;

namespace Suity.Editor.DataModel;

/// <summary>
/// Provides XML serialization and deserialization for <see cref="DataModelSpec"/>.
/// <para>
/// Implicit attribute elements (Deprecated, LengthRange, Pattern, Optional, ValueRange, etc.)
/// are mapped to/from <see cref="AttributeSpec"/> in <see cref="FieldSpec.Attributes"/>.
/// </para>
/// </summary>
public static class DataModelParser
{
    public static DataModelSpec Deserialize(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new ArgumentException("XML string is null or empty.", nameof(xml));

        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidOperationException("Invalid DataModel XML: root element is missing.");

        var spec = new DataModelSpec();

        foreach (var element in root.Elements())
        {
            var typeSpec = element.Name.LocalName switch
            {
                "Enum" => ParseTypeSpec(element, DataStructureType.Enum),
                "Struct" => ParseTypeSpec(element, DataStructureType.Struct),
                "Abstract" => ParseTypeSpec(element, DataStructureType.Abstract),
                _ => throw new InvalidOperationException($"Unknown type element: '{element.Name.LocalName}'.")
            };
            spec.Structures.Add(typeSpec);
        }

        return spec;
    }

    public static string Serialize(DataModelSpec spec)
    {
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));

        var root = new XElement("DataModel",
            new XAttribute("version", "1.0"));

        foreach (var structure in spec.Structures)
        {
            root.Add(WriteTypeSpec(structure));
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", null), root)
            .ToString(SaveOptions.OmitDuplicateNamespaces);
    }

    #region Deserialization

    private static TypeSpec ParseTypeSpec(XElement element, DataStructureType type)
    {
        var spec = new TypeSpec
        {
            Name = GetRequiredAttribute(element, "name"),
            Type = type
        };

        if (element.Attribute("base") is XAttribute baseAttr)
            spec.BaseType = baseAttr.Value;

        if (element.Attribute("usage") is XAttribute usageAttr &&
            Enum.TryParse<DataUsageMode>(usageAttr.Value, true, out var usage))
            spec.Usage = usage;

        if (element.Attribute("driven") is XAttribute drivenAttr &&
            Enum.TryParse<DataDrivenMode>(drivenAttr.Value, true, out var driven))
            spec.DrivenMode = driven;

        foreach (var child in element.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "Tooltip":
                    spec.Tooltip = child.Value;
                    break;
                case "Item" when type == DataStructureType.Enum:
                    spec.Items.Add(ParseItem(child));
                    break;
                case "Field" when type != DataStructureType.Enum:
                    spec.Items.Add(ParseField(child));
                    break;
            }
        }

        return spec;
    }

    private static FieldSpec ParseItem(XElement element)
    {
        var spec = new FieldSpec
        {
            Name = GetRequiredAttribute(element, "name")
        };

        ParseFieldChildren(element, spec);
        return spec;
    }

    private static FieldSpec ParseField(XElement element)
    {
        var typeStr = GetRequiredAttribute(element, "type");
        var isArray = typeStr.EndsWith("[]", StringComparison.Ordinal);

        var spec = new FieldSpec
        {
            Name = GetRequiredAttribute(element, "name"),
            FieldType = isArray ? typeStr.Substring(0, typeStr.Length - 2) : typeStr,
            IsArray = isArray
        };

        if (element.Attribute("default") is XAttribute defaultAttr)
            spec.DefaultValue = defaultAttr.Value;

        ParseFieldChildren(element, spec);
        return spec;
    }

    private static void ParseFieldChildren(XElement element, FieldSpec spec)
    {
        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName == "Tooltip")
            {
                spec.Tooltip = child.Value;
            }
            else
            {
                var attrSpec = new AttributeSpec(child.Name.LocalName);
                foreach (var xAttr in child.Attributes())
                {
                    attrSpec.Parameters[xAttr.Name.LocalName] = xAttr.Value;
                }
                spec.Attributes.Add(attrSpec);
            }
        }
    }

    #endregion

    #region Serialization

    private static XElement WriteTypeSpec(TypeSpec spec)
    {
        var elementName = spec.Type switch
        {
            DataStructureType.Enum => "Enum",
            DataStructureType.Struct => "Struct",
            DataStructureType.Abstract => "Abstract",
            _ => throw new InvalidOperationException($"Unsupported data structure type for XML: {spec.Type}")
        };

        var element = new XElement(elementName, new XAttribute("name", spec.Name));

        if (!string.IsNullOrWhiteSpace(spec.BaseType))
            element.Add(new XAttribute("base", spec.BaseType));

        if (spec.Usage != DataUsageMode.None)
            element.Add(new XAttribute("usage", spec.Usage));

        if (spec.DrivenMode != DataDrivenMode.None)
            element.Add(new XAttribute("driven", spec.DrivenMode));

        if (!string.IsNullOrWhiteSpace(spec.Tooltip))
            element.Add(new XElement("Tooltip", spec.Tooltip));

        foreach (var item in spec.Items)
        {
            element.Add(WriteFieldSpec(item, spec.Type == DataStructureType.Enum));
        }

        return element;
    }

    private static XElement WriteFieldSpec(FieldSpec spec, bool isEnum)
    {
        var elementName = isEnum ? "Item" : "Field";
        var element = new XElement(elementName, new XAttribute("name", spec.Name));

        if (!isEnum)
        {
            var typeStr = spec.IsArray ? spec.FieldType + "[]" : spec.FieldType;
            element.Add(new XAttribute("type", typeStr));

            if (!string.IsNullOrWhiteSpace(spec.DefaultValue))
                element.Add(new XAttribute("default", spec.DefaultValue));
        }

        if (!string.IsNullOrWhiteSpace(spec.Tooltip))
            element.Add(new XElement("Tooltip", spec.Tooltip));

        foreach (var attr in spec.Attributes)
        {
            var attrElement = new XElement(attr.Name);
            foreach (var param in attr.Parameters)
            {
                attrElement.Add(new XAttribute(param.Key, param.Value));
            }
            element.Add(attrElement);
        }

        return element;
    }

    #endregion

    private static string GetRequiredAttribute(XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? throw new InvalidOperationException(
                $"Missing required attribute '{name}' on <{element.Name.LocalName}> element.");
    }
}
