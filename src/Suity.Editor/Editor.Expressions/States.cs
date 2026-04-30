namespace Suity.Editor.Expressions;

/// <summary>
/// Represents the kind of type definition.
/// </summary>
public enum TypeKind
{
    /// <summary>A class type.</summary>
    Class,
    /// <summary>An enumeration type.</summary>
    Enum,
    /// <summary>A structure type.</summary>
    Struct,
    /// <summary>An interface type.</summary>
    Interface,
}

/// <summary>
/// Represents the access level of a type or member.
/// </summary>
public enum AccessState
{
    /// <summary>No explicit access modifier.</summary>
    None,
    /// <summary>Public access.</summary>
    Public,
    /// <summary>Protected access.</summary>
    Protected,
    /// <summary>Internal access.</summary>
    Internal,
    /// <summary>Private access.</summary>
    Private,
}

/// <summary>
/// Represents the static/readonly modifier mode.
/// </summary>
public enum StaticReadonlyMode
{
    /// <summary>No modifier.</summary>
    None,
    /// <summary>Readonly modifier.</summary>
    Readonly,
    /// <summary>Static modifier.</summary>
    Static,
    /// <summary>Static readonly modifier.</summary>
    StaticReadonly,
    /// <summary>Const modifier.</summary>
    Const,
}

/// <summary>
/// Represents the virtual state of a member.
/// </summary>
public enum VirtualState
{
    /// <summary>Normal non-virtual member.</summary>
    Normal,
    /// <summary>Virtual member.</summary>
    Virtual,
    /// <summary>Abstract member.</summary>
    Abstract,
    /// <summary>Override member.</summary>
    Override,
}