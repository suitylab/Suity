namespace Suity;

/// <summary>
/// Specifies the status or state of text content.
/// </summary>
public enum TextStatus
{
    Normal = 0,

    Reference,

    FileReference,

    EnumReference,

    Add,

    Remove,

    Modify,

    Disabled,

    Anonymous,

    Import,

    Tag,

    UserCode,

    ResourceUse,

    Comment,

    Preview,

    Unchecked,

    Checked,

    Denied,

    Info,

    Warning,

    Error,
}