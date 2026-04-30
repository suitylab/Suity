using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suity.Editor.Design;

/// <summary>
/// Defines the structure type of a data type.
/// </summary>
public enum DataStructureType
{
    [Description("represent an enum type")]
    Enum,

    [Description("represent a struct type")]
    Struct,

    [Description("represent abstract type")]
    Abstract,

    [Description("represent an event type")]
    [Obsolete]
    Event,
}

/// <summary>
/// Represents a type design document.
/// </summary>
[NativeType(Name = "TypeDesignDocument", Description = "Type Design Document", CodeBase = "*Core", Icon = "*CoreIcon|Type")]
public interface ITypeDesignDocument : IMemberContainer, IHasAsset, IHasId
{
    /// <summary>
    /// Gets all type items in this document.
    /// </summary>
    IEnumerable<TypeDesignItem> TypeItems { get; }

    /// <summary>
    /// Adds a new type item to this document.
    /// </summary>
    TypeDesignItem AddTypeItem(DataStructureType type, string name, string description = null, string groupPath = null);

    /// <summary>
    /// Removes a type item from this document.
    /// </summary>
    bool RemoveTypeItem(TypeDesignItem item);

    /// <summary>
    /// Adds a field to a type item.
    /// </summary>
    DesignField AddField(TypeDesignItem item, string name, string description = null);

    /// <summary>
    /// Sets the type of a field.
    /// </summary>
    bool SetFieldType(DesignField field, TypeDefinition type);
}