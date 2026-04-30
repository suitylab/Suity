using static Suity.Helpers.GlobalLocalizer;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Represents multiple sections extracted from an origin document.
/// </summary>
public class MultipleSection
{
    /// <summary>
    /// Gets or sets the sections from the origin document.
    /// </summary>
    [Description("Sections from the origin document.")]
    public List<string> Sections { get; set; }
}

/// <summary>
/// Represents multiple subdivided tasks from an origin document.
/// </summary>
public class MultipleTask
{
    /// <summary>
    /// Gets or sets the subdivided tasks from the origin document.
    /// </summary>
    [Description("Subdivided tasks from the origin document.")]
    public List<string> Tasks { get; set; }
}

/// <summary>
/// Represents a collection of multiple options.
/// </summary>
public class MultipleOption
{
    /// <summary>
    /// Gets or sets the list of options.
    /// </summary>
    public List<MultipleOptionItem> Options { get; set; }
}

/// <summary>
/// Represents a single option item with a name and description.
/// </summary>
public class MultipleOptionItem
{
    /// <summary>
    /// Gets or sets the name of the option.
    /// </summary>
    [Description("Name of the option.")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the option.
    /// </summary>
    [Description("Description of the option.")]
    public string Description { get; set; }
}


/// <summary>
/// Represents a chain of assistant calls.
/// </summary>
public class AssistantCallChain
{
    /// <summary>
    /// Gets or sets the list of assistant call items in the chain.
    /// </summary>
    public List<AssistantCallItem> Items { get; set; } = [];

    /// <summary>
    /// Validates the assistant call chain.
    /// </summary>
    /// <returns>True if all items are valid; otherwise, false.</returns>
    /// <exception cref="AigcException">Thrown when no assistant calls were decomposed.</exception>
    public bool Varify()
    {
        if (Items == null || Items.Count == 0)
        {
            throw new AigcException(L("No assistant calls were decomposed."));
        }

        foreach (var item in Items)
        {
            if (!item.Varify())
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Represents a single assistant call item with an assistant ID and calling message.
/// </summary>
public class AssistantCallItem
{
    /// <summary>
    /// Gets or sets the assistant full ID.
    /// </summary>
    [Description("Assistant full Id.")]
    public string AssistantId { get; set; }

    /// <summary>
    /// Gets or sets the message to call the assistant.
    /// </summary>
    [Description("Message to call the assistant.")]
    public string CallingMessage { get; set; }

    /// <summary>
    /// Validates the assistant call item.
    /// </summary>
    /// <returns>True if the item is valid.</returns>
    /// <exception cref="AigcException">Thrown when the assistant name or call information is empty.</exception>
    public bool Varify()
    {
        if (string.IsNullOrWhiteSpace(AssistantId))
        {
            throw new AigcException(L("Assistant name cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(CallingMessage))
        {
            throw new AigcException(L("Assistant call information cannot be empty."));
        }

        return true;
    }
}
