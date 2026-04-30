using Suity.Editor.Types;
using Suity.Editor.Values;
using System.Collections.Generic;

namespace Suity.Editor.Transferring;

/// <summary>
/// Target context for SObject transfers containing object type and objects dictionary.
/// </summary>
public class SObjectTransferTarget
{
    /// <summary>
    /// Gets or sets the object type definition.
    /// </summary>
    public TypeDefinition ObjectType { get; set; }

    /// <summary>
    /// Gets the dictionary of objects by name.
    /// </summary>
    public Dictionary<string, SObject> Objects { get; } = [];
}

/// <summary>
/// Content transfer for SObject types.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
public class SObjectTransfer<TSource> : ContentTransfer<TSource, SObjectTransferTarget>
{
}
