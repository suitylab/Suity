using Suity.Synchonizing.Core;
using Suity.Views;
using System;

namespace Suity.Selecting;

/// <summary>
/// Represents a selection that is serialized as a string key, typically used for enum-like selections.
/// </summary>
[Serializable]
public struct EnumSelection : ISerializeAsString, INavigable
{
    /// <summary>
    /// Gets the fully qualified type name associated with this selection.
    /// </summary>
    public readonly string TypeName;
    /// <summary>
    /// Gets the selection list associated with this selection.
    /// </summary>
    public readonly ISelectionList List;
    /// <summary>
    /// Gets or sets the selected enum key.
    /// </summary>
    public string EnumKey;

    /// <summary>
    /// Initializes a new instance of <see cref="EnumSelection"/> with a selection list.
    /// </summary>
    /// <param name="list">The selection list to use.</param>
    public EnumSelection(ISelectionList list)
    {
        TypeName = null;
        List = list;
        EnumKey = null;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EnumSelection"/> with a selection list and a key.
    /// </summary>
    /// <param name="list">The selection list to use.</param>
    /// <param name="key">The initial selected key.</param>
    public EnumSelection(ISelectionList list, string key)
    {
        TypeName = null;
        List = list;
        EnumKey = key;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EnumSelection"/> with a type name and a selection list.
    /// </summary>
    /// <param name="typeName">The fully qualified type name.</param>
    /// <param name="list">The selection list to use.</param>
    public EnumSelection(string typeName, ISelectionList list)
    {
        TypeName = typeName;
        List = list;
        EnumKey = null;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EnumSelection"/> with a type name, selection list, and key.
    /// </summary>
    /// <param name="typeName">The fully qualified type name.</param>
    /// <param name="list">The selection list to use.</param>
    /// <param name="key">The initial selected key.</param>
    public EnumSelection(string typeName, ISelectionList list, string key)
    {
        TypeName = typeName;
        List = list;
        EnumKey = key;
    }

    /// <summary>
    /// Gets or sets the navigation key for this selection.
    /// </summary>
    public string NaviKey { get => EnumKey; set => EnumKey = value; }
    /// <summary>
    /// Gets or sets the key for this selection.
    /// </summary>
    public string Key { get => EnumKey; set => EnumKey = value; }

    /// <summary>
    /// Returns the enum key as a string representation.
    /// </summary>
    public override string ToString() => EnumKey;

    /// <summary>
    /// Gets the navigation target for this selection.
    /// </summary>
    object INavigable.GetNavigationTarget() => EnumKey;
}