using System;

namespace Suity.Synchonizing;

[Flags]
public enum SyncFlag
{
    None = 0x0,

    /// <summary>
    /// The value can only be obtained and no new value will be set.
    /// </summary>
    GetOnly = 0x1,

    /// <summary>
    /// The value should not be empty. If the result is empty, the passed value will be returned.
    /// </summary>
    NotNull = 0x2,

    /// <summary>
    /// The value is passed by reference, and this object will not be cloned when cloned.
    /// </summary>
    ByRef = 0x4,

    /// <summary>
    /// Affects other values, and notifies the parent editor to update when this value changes.
    /// </summary>
    AffectsOthers = 0x8,

    /// <summary>
    /// Affects parent values.
    /// </summary>
    AffectsParent = 0x10,

    /// <summary>
    /// Attribute pattern (used in serialization)
    /// </summary>
    AttributeMode = 0x20,

    /// <summary>
    /// Indicate that the target is an element branch
    /// </summary>
    Element = 0x40,

    /// <summary>
    /// Hide this path node when obtaining the path
    /// </summary>
    PathHidden = 0x80,

    /// <summary>
    /// No serialization
    /// </summary>
    NoSerialize = 0x100,
}