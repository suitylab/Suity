using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Contains parsed information from a user instruction. (Obsolete)
/// </summary>
[Obsolete]
public class InstructInfo
{
    /// <summary>
    /// Gets or sets the intent of the user instruction.
    /// </summary>
    [Description("The intent of user instruction.")]
    public string Intent { get; set; }

    /// <summary>
    /// Gets or sets the key entities extracted from the user instruction.
    /// </summary>
    [Description("The key entities of user instruction.")]
    public List<InstructEntityInfo> Entities { get; set; }
}

/// <summary>
/// Represents an entity extracted from a user instruction. (Obsolete)
/// </summary>
[Obsolete]
public class InstructEntityInfo
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    [Description("The name of the entity.")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity.
    /// </summary>
    [Description("The type of the entity.")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the attributes associated with the entity.
    /// </summary>
    [Description("The attributes of the entity.")]
    public List<InstructAttributeInfo> Attributes { get; set; }
}

/// <summary>
/// Represents an attribute of an entity from a user instruction. (Obsolete)
/// </summary>
[Obsolete]
public class InstructAttributeInfo
{
    /// <summary>
    /// Gets or sets the name of the attribute.
    /// </summary>
    [Description("The name of the attribute.")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the attribute.
    /// </summary>
    [Description("The value of the attribute.")]
    public string Value { get; set; }
}
