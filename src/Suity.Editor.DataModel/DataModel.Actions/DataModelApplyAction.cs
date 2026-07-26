using Suity;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.DataModel;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Documents.TypeEdit;
using Suity.Editor.Types;
using Suity.Synchonizing.Core;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.DataModel.Actions;

internal class DataModelApplyAction : AIGenerativeApplyAction
{
    static readonly Dictionary<string, Action<StructField>> _fieldSetters = [];

    static DataModelApplyAction()
    {
        _fieldSetters["nullable"] = field => field.Optional = true;
        _fieldSetters["unique"] = field => field.Attributes.SetAttribute<DrivenAttribute>(o => o.Mode = DataDrivenMode.Unique);
        _fieldSetters["shared"] = field => field.Attributes.SetAttribute<DrivenAttribute>(o => o.Mode = DataDrivenMode.Shared);
        _fieldSetters["consistency"] = field => field.Attributes.SetAttribute<ConsistencyAttribute>();
        _fieldSetters["classify"] = field => field.Attributes.SetAttribute<ClassifyAttribute>();
        _fieldSetters["skip"] = field => field.Attributes.SetAttribute<SkipAIGenerationAttribute>();
    }

    private class GenItem
    {
        public string Name;
        public string Description;

        public TypeDesignItem Item;
        public INamedItemList List;
        public int Index;

        public TypeSpec Spec;
    }

    private readonly ITypeDesignDocument _doc;
    private readonly IDocumentView _view;

    private readonly List<GenItem> _newItems = [];
    private readonly List<GenItem> _oldItems = [];

    private readonly string _groupPath;

    private readonly List<TypeDesignItem> _appliedItems = [];

    public DataModelApplyAction(ITypeDesignDocument doc, IDocumentView view, string groupPath = null)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _groupPath = groupPath;
    }

    public override string Name => L("AI Generate Data Model");

    public IEnumerable<TypeDesignItem> NewItems => _newItems.Select(o => o.Item);

    public void AddSpec(DataModelSpec spec)
    {
        if (spec is null)
        {
            return;
        }

        // Handle deleted structures
        if (spec.DeletedStructureNames is { Count: > 0 })
        {
            foreach (var name in spec.DeletedStructureNames)
            {
                if (_doc.TypeItems.FirstOrDefault(i => i.Name == name) is TypeDesignItem existingItem)
                {
                    var oldGenItem = new GenItem
                    {
                        Name = name,
                        Description = existingItem.Description,
                        Item = existingItem,
                        List = existingItem.ParentList,
                        Index = existingItem.GetIndex(),
                    };
                    _oldItems.Add(oldGenItem);

                    // Mark for deletion by adding with null item
                    _newItems.Add(new GenItem { Name = name, Item = null });
                }
            }
        }

        // Handle structures to add/update
        foreach (var structure in spec.Structures)
        {
            AddTypeSpec(structure);
        }
    }

    private void AddTypeSpec(TypeSpec typeSpec)
    {
        var oldItem = _doc.TypeItems.FirstOrDefault(i => i.Name == typeSpec.Name) as TypeDesignItem;
        if (oldItem != null)
        {
            var oldGenItem = new GenItem
            {
                Name = typeSpec.Name,
                Description = oldItem.Description,
                Item = oldItem,
                List = oldItem.ParentList,
                Index = oldItem.GetIndex(),
            };
            _oldItems.Add(oldGenItem);
        }

        var designItem = CreateTypeDesignItem(typeSpec.Type);
        if (designItem is null)
        {
            return;
        }

        designItem.Name = typeSpec.Name;

        if (oldItem != null)
        {
            Cloner.CloneProperty(oldItem, designItem);
        }

        var genItem = new GenItem
        {
            Name = typeSpec.Name,
            Description = typeSpec.Tooltip,
            Item = designItem,
            Spec = typeSpec,
        };

        _newItems.Add(genItem);
    }

    public override object[] GetAppliedObjects() => [.. _appliedItems];

    public override void Do()
    {
        _appliedItems.Clear();

        var items = _newItems;

        ApplyItems(items, false);

        _view.RefreshView();
        EditorUtility.Inspector.UpdateInspector();
    }

    public override void Undo()
    {
        _appliedItems.Clear();

        var items = _oldItems;

        ApplyItems(items, true);

        _view.RefreshView();
        EditorUtility.Inspector.UpdateInspector();
    }

    private void ApplyItems(List<GenItem> gens, bool undo)
    {
        _appliedItems.Clear();

        for (int i = 0; i < gens.Count; i++)
        {
            var gen = gens[i];

            var current = _doc.TypeItems.FirstOrDefault(x => x.Name == gen.Name) as TypeDesignItem;
            if (current is not null)
            {
                // Insert new type at the original position
                var list = current.ParentList;
                int index = current.GetIndex();

                list.Remove(current);

                if (gen.Item != null)
                {
                    list.Insert(index, gen.Item);
                }
            }
            else
            {
                // Insert new type at the end
                if (gen.Item != null)
                {
                    if (!undo && !string.IsNullOrWhiteSpace(_groupPath))
                    {
                        var node = (_doc as TypeDesignDocument)?.EnsureGroupByPath(_groupPath);
                        node?.AddItem(gen.Item);
                    }
                    else
                    {
                        if (gen.List != null && gen.Index >= 0)
                        {
                            gen.List.Add(gen.Item);
                        }
                        else
                        {
                            (_doc as TypeDesignDocument)?.ItemCollection.AddItem(gen.Item);
                        }
                    }
                }
            }
        }

        // Property settings need to be processed separately, because the type is assigned an ID after creation
        foreach (var gen in gens)
        {
            if (gen.Item is null)
            {
                continue;
            }

            if (gen.Spec != null)
            {
                ApplySpec(gen.Item, gen.Spec);
            }

            _appliedItems.Add(gen.Item);
        }
    }

    private TypeDesignItem CreateTypeDesignItem(DataStructureType type) => type switch
    {
        DataStructureType.Struct => new StructType(),
        DataStructureType.Abstract => new AbstractType(),
        DataStructureType.Enum => new EnumType(),
        _ => null,
    };

    private void ApplySpec(TypeDesignItem item, TypeSpec spec)
    {
        if (spec.Usage != DataUsageMode.None)
        {
            item.SetAttribute<DataUsageAttribute>(o =>
            {
                o.Usage = spec.Usage;
            });
        }

        if (spec.Usage.GetIsAIGeneration())
        {
            var drivenMode = spec.DrivenMode;
            if (drivenMode == DataDrivenMode.None)
            {
                drivenMode = DataDrivenMode.Active;
            }

            item.Attributes.SetAttribute<DrivenAttribute>(a => a.Mode = drivenMode);
        }

        if (spec.Attributes?.Contains("hori") == true)
        {
            item.Attributes.SetAttribute<HorizontalLayoutAttribute>();
        }

        if (!string.IsNullOrWhiteSpace(spec.Tooltip))
        {
            item.SetAttribute<ToolTipsAttribute>(o => o.ToolTips = spec.Tooltip);
        }

        switch (item)
        {
            case StructType structType:
                if (!string.IsNullOrWhiteSpace(spec.BaseType))
                {
                    string baseType = ResolveTypeString(spec.BaseType, false, false, GetNamespace());
                    structType.BaseTypeTarget = TypeDefinition.Resolve(baseType)?.Target as DAbstract;
                }

                if (spec.Usage != DataUsageMode.DataGrid && structType.FieldList.Count <= 5 && structType.Fields.All(o => !o.FieldType.IsArray))
                {
                    structType.Attributes.SetAttribute<HorizontalLayoutAttribute>();
                }

                ApplySpecStructFields(structType, spec);
                break;

            case AbstractType abstractType:
                ApplySpecStructFields(abstractType, spec);
                break;

            case EnumType enumType:
                ApplySpecEnum(enumType, spec);
                break;
        }
    }

    private void ApplySpecStructFields(StructTypeBase type, TypeSpec spec)
    {
        foreach (var fieldSpec in spec.Items)
        {
            var field = type.GetField(fieldSpec.Name);
            if (field is null)
            {
                field = new StructField { Name = fieldSpec.Name };
                type.FieldList.Add(field);
            }

            var typeStr = ResolveTypeString(fieldSpec.FieldType, false, false, GetNamespace());

            field.FieldType.TypeString = typeStr;
            field.FieldType.IsArray = fieldSpec.IsArray;

            if (!string.IsNullOrWhiteSpace(fieldSpec.Tooltip))
            {
                field.SetAttribute<ToolTipsAttribute>(o => o.ToolTips = fieldSpec.Tooltip);
            }

            if (string.Equals(fieldSpec.Name, "id", StringComparison.OrdinalIgnoreCase))
            {
                field.SetAttribute<AutoFieldAttribute>(o => o.FieldType = AutoFieldType.DataId);
            }

            if (fieldSpec.Attributes?.Count > 0)
            {
                foreach (var attr in fieldSpec.Attributes)
                {
                    if (attr is null)
                    {
                        continue;
                    }

                    string attrName = attr.Name;
                    if (string.IsNullOrWhiteSpace(attrName))
                    {
                        continue;
                    }

                    // Handle implicit attributes from XML: Deprecated, LengthRange, Pattern, Optional, ValueRange
                    switch (attrName.ToLowerInvariant())
                    {
                        case "optional":
                        case "nullable":
                            field.Optional = true;
                            break;

                        case "valuerange":
                        case "lengthrange":
                            if (attr.Parameters.TryGetValue("min", out var minStr) && attr.Parameters.TryGetValue("max", out var maxStr))
                            {
                                if (decimal.TryParse(minStr, out var min) && decimal.TryParse(maxStr, out var max))
                                {
                                    field.SetAttribute<NumericRangeAttribute>(o => { o.Min = min; o.Max = max; });
                                }
                            }
                            else if (attr.Parameters.TryGetValue("max", out var maxOnly))
                            {
                                if (decimal.TryParse(maxOnly, out var max))
                                {
                                    field.SetAttribute<NumericRangeAttribute>(o => { o.Max = max; });
                                }
                            }
                            break;

                        case "deprecated":
                            field.Attributes.SetAttribute<SkipAIGenerationAttribute>();
                            break;

                        case "pattern":
                            // Pattern validation - stored as attribute for future use
                            break;

                        default:
                            // Check legacy attribute format (string-based)
                            if (_fieldSetters.TryGetValue(attrName, out var setter))
                            {
                                setter(field);
                            }
                            break;
                    }
                }
            }
        }

        // Remove extra fields
        HashSet<string> names = [.. spec.Items.Select(o => o.Name)];
        foreach (var field in type.FieldList.ToArray())
        {
            if (!names.Contains(field.Name))
            {
                type.FieldList.Remove(field);
            }
        }
    }

    private void ApplySpecEnum(EnumType enumType, TypeSpec spec)
    {
        foreach (var fieldSpec in spec.Items)
        {
            var field = enumType.GetField(fieldSpec.Name);
            if (field is null)
            {
                field = new EnumItem { Name = fieldSpec.Name };
                enumType.FieldList.Add(field);
            }

            if (!string.IsNullOrWhiteSpace(fieldSpec.Tooltip))
            {
                field.Attributes.SetAttribute<ToolTipsAttribute>().ToolTips = fieldSpec.Tooltip;
            }
        }

        // Remove extra fields
        HashSet<string> names = [.. spec.Items.Select(o => o.Name)];
        foreach (var field in enumType.FieldList.ToArray())
        {
            if (!names.Contains(field.Name))
            {
                enumType.FieldList.Remove(field);
            }
        }
    }

    private string GetNamespace()
    {
        return (_doc as TypeDesignDocument)?.NameSpace;
    }

    private static string ResolveTypeString(string name, bool isLinkedData, bool isArray, string nameSpace)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (name.StartsWith("@"))
        {
            isLinkedData = true;
            name = name.Substring(1);
        }

        string typeString = name;

        do
        {
            var typeDef = TypeDefinition.ResolveWithTypeName(name);
            if (typeDef?.IsPrimitive == true)
            {
                break;
            }

            if (!typeString.Contains(".") && !string.IsNullOrWhiteSpace(nameSpace))
            {
                typeString = nameSpace + "." + typeString;
            }

            if (isLinkedData)
            {
                typeString = "@" + typeString;
            }
        } while (false);

        if (isArray)
        {
            typeString = typeString + "[]";
        }

        return typeString;
    }
}
