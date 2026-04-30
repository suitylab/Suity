using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Generative intent
/// </summary>
public enum GenerativeIntent
{
    /// <summary>
    /// Create new object
    /// </summary>
    Create,

    /// <summary>
    /// Edit existing object
    /// </summary>
    Edit,
}

/// <summary>
/// Pre-generation pipeline
/// </summary>
public enum GenerativePipeline
{
    /// <summary>
    /// None
    /// </summary>
    None,

    /// <summary>
    /// Get source knowledge base so AI understands the background knowledge of the current document
    /// Should set <see cref="AIGenerativeRequest.SourceKnowledgeBase"/> after entering the pipeline, but be careful not to set it repeatedly.
    /// </summary>
    BuildSourceKnowlegeBase,

    /// <summary>
    /// Build editable list so AI knows all editable objects in the current document to select objects to edit
    /// Should set <see cref="AIGenerativeRequest.EditableList"/> after entering the pipeline, but be careful not to set it repeatedly.
    /// </summary>
    BuildEditableList,

    /// <summary>
    /// Build guiding knowledge so AI understands the current generation approach to generate content that better meets requirements
    /// Should set <see cref="AIGenerativeRequest.GuidingKnowledge"/> after entering the pipeline, but be careful not to set it repeatedly.
    /// </summary>
    BuildGuidingKnowledge,

    /// <summary>
    /// Build edit list knowledge so AI knows all editable objects in the current document to select objects to edit
    /// Should set <see cref="AIGenerativeRequest.EditListKnowledge"/> after entering the pipeline, but be careful not to set it repeatedly.
    /// </summary>
    BuildEditListKnowledge,

    /// <summary>
    /// Enter generation flow, build generation context so AI understands the overall generation situation when generating individual objects
    /// Should set <see cref="AIGenerativeRequest.GenerativeContext"/> after entering the pipeline, but be careful not to set it repeatedly.
    /// Can optionally set <see cref="AIGenerativeRequest.Preparation"/> to execute actions during preparation.
    /// </summary>
    PrepareGenerative,

    /// <summary>
    /// Complete generation flow and generate summary
    /// Should set <see cref="AIGenerativeRequest.Conclusion"/> after entering the pipeline, but be careful not to set it repeatedly.
    /// </summary>
    FinishGenerative,
}

/// <summary>
/// Generative edit type
/// </summary>
public enum GenerativeEditType
{
    [Description("Create a new item.")]
    [DisplayText("Create")]
    Create,

    [Description("Modify, fix the selected item.")]
    [DisplayText("Modify")]
    Modify,

    [Description("Rename the selected item.")]
    [DisplayText("Rename")]
    Rename,

    [Description("Delete the selected item.")]
    [DisplayText("Delete")]
    Delete,
}

/// <summary>
/// Represents a list of editable items in a document that AI can modify.
/// </summary>
public class GenerativeEditableList
{
    /// <summary>
    /// Gets or sets the list of editable items.
    /// </summary>
    public List<GenerativeEditableItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets additional information about the editable list.
    /// </summary>
    public string AdditionalInformation { get; set; }

    /// <summary>
    /// Converts the entire editable list to a full text representation for AI prompts.
    /// </summary>
    /// <returns>A formatted string containing all editable items and additional information.</returns>
    public string ToFullText()
    {
        string editableStr = string.Join("\n", Items.Select(o => o.ToFullText()));
        if (!string.IsNullOrWhiteSpace(AdditionalInformation))
        {
            editableStr += $"\n<additional_information>\n{AdditionalInformation}\n</additional_information>";
        }

        return editableStr;
    }
}

/// <summary>
/// Used to list objects in the current document
/// </summary>
public class GenerativeEditableItem
{
    /// <summary>
    /// Gets or sets the name of the editable item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type name of the editable item.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the position of the item in the graph editor.
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// Gets or sets a brief description of the item.
    /// </summary>
    public string Brief { get; set; }

    /// <summary>
    /// Converts this item to a full text representation for AI prompts.
    /// </summary>
    /// <returns>A formatted XML-like string describing the item.</returns>
    public string ToFullText()
    {
        string nameStr = !string.IsNullOrWhiteSpace(Name) ? $" name='{Name}'" : "";
        string typeStr = !string.IsNullOrWhiteSpace(Type) ? $" type='{Type}'" : "";
        string posStr = Position != Point.Empty ? $" position='{Position.X},{Position.Y}'" : "";

        return $"<Item{nameStr}{typeStr}{posStr}>\n{Brief}\n</Item>";
    }

    public override string ToString() => ToFullText();
}

/// <summary>
/// Used to generate edit guidance
/// </summary>
public class GenerativeEditInfo
{
    /// <summary>
    /// Gets or sets the thinking process behind this edit operation.
    /// </summary>
    [Description("Provide thinking of this edit.")]
    public string Thought { get; set; }

    /// <summary>
    /// Gets or sets the items to be edited.
    /// </summary>
    [Description("Items to be edited.")]
    public GenerativeGuidingItem[] Items { get; set; } = [];

    /// <summary>
    /// Gets or sets additional information for this edit, such as the relationship between items, shared knowledge, etc.
    /// </summary>
    [Description("Additional information for this edit, such as the relationship between items, shared knowledge, etc.")]
    public string AdditionalInformation { get; set; }

    /// <summary>
    /// Converts the edit info to a full text representation for AI prompts.
    /// </summary>
    /// <returns>A formatted string containing all items and additional information.</returns>
    public string ToFullText()
    {
        var text = string.Join("\n", Items.Select(x => x.ToFullText()));
        if (!string.IsNullOrWhiteSpace(AdditionalInformation))
        {
            text += "\n" + AdditionalInformation;
        }

        return text;
    }

    /// <summary>
    /// Converts only the Create and Modify items to a full text representation.
    /// </summary>
    /// <param name="tagName">The XML tag name to use for each item.</param>
    /// <param name="additional">The tag name for additional information.</param>
    /// <returns>A formatted string containing only Create and Modify items.</returns>
    public string ToCreateModifyFullText(string tagName = "design", string additional = "additional_information")
    {
        var items = Items
            .Where(o => o.EditType == GenerativeEditType.Create || o.EditType == GenerativeEditType.Modify);

        var text = string.Join("\n", items.Select(x => x.ToFullText(tagName)));
        if (!string.IsNullOrWhiteSpace(AdditionalInformation))
        {
            text += $"\n<{additional}>\n{AdditionalInformation}\n</{additional}>";
        }

        return text;
    }
}

/// <summary>
/// Represents a single item to be edited in a generative operation.
/// </summary>
public class GenerativeEditItem
{
    /// <summary>
    /// Gets or sets the name of the item to be edited.
    /// </summary>
    [Description("The item name to be edited.")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of edit operation to perform.
    /// </summary>
    [Description("The edit type.")]
    public GenerativeEditType EditType { get; set; }

    /// <summary>
    /// Gets or sets the detailed requirement for this edit.
    /// </summary>
    [Description("The detail requirement of this edit, such as the type of the item, content, etc.")]
    public string Prompt { get; set; }

    /// <summary>
    /// Converts this edit item to a <see cref="GenerativeGuidingItem"/>.
    /// </summary>
    /// <returns>A new <see cref="GenerativeGuidingItem"/> with the same properties.</returns>
    public GenerativeGuidingItem ToGuidingItem()
    {
        return new GenerativeGuidingItem
        {
            Name = Name,
            EditType = EditType,
            Prompt = Prompt,
        };
    }

    /// <summary>
    /// Converts this edit item to a full text representation for AI prompts.
    /// </summary>
    /// <returns>A formatted XML-like string describing the edit item.</returns>
    public string ToFullText()
    {
        return $"<item name='{Name}' editType='{EditType}'>\n{Prompt}\n</item>";
    }

    public override string ToString()
    {
        return $"[{EditType}] {Name}";
    }
}


/// <summary>
/// Design guiding information
/// </summary>
public class GenerativeGuidingItem
{
    /// <summary>
    /// Gets or sets the short name of the design in PascalCase.
    /// </summary>
    [Description("Short name of the design in PascalCase.")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the localized name of the design.
    /// </summary>
    [Description("The localized name of the design.")]
    public string LocalizedName { get; set; }

    /// <summary>
    /// Gets or sets the edit type for this guiding item.
    /// </summary>
    [Description("The edit type.")]
    public GenerativeEditType EditType { get; set; }

    /// <summary>
    /// Gets or sets a brief description of the design in one sentence.
    /// </summary>
    [Description("Brief of the design in one sentence.")]
    public string Brief { get; set; }

    /// <summary>
    /// Gets or sets the HTML color code according to the meaning of the design, in hex format (e.g., #000000).
    /// </summary>
    [Description("html color code according to the meaning of the design, output hex format : #000000")]
    public string HtmlColor { get; set; }

    /// <summary>
    /// Gets or sets the full document of the design in detail.
    /// </summary>
    [Description("Full document of the design, in detailed.")]
    public string Prompt { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeGuidingItem"/> class.
    /// </summary>
    public GenerativeGuidingItem()
    {
    }

    /// <summary>
    /// Converts this guiding item to a full text representation for AI prompts.
    /// </summary>
    /// <param name="tagName">The XML tag name to use for this item.</param>
    /// <returns>A formatted XML-like string describing the guiding item.</returns>
    public string ToFullText(string tagName = "design")
    {
        string editModeStr = $" editMode='{EditType}'";
        string colorStr = !string.IsNullOrWhiteSpace(HtmlColor) ? $" color='{HtmlColor}'" : string.Empty;
        string briefStr = !string.IsNullOrWhiteSpace(Brief) ? Brief : "---";

        return $"<{tagName} name='{Name}'{editModeStr}{colorStr}>\n## Brief\n{briefStr}\n\n## Knowledge\n{Prompt}\n</{tagName}>";
    }

    /// <summary>
    /// Converts a collection of guiding items to a full text representation.
    /// </summary>
    /// <param name="items">The collection of guiding items to convert.</param>
    /// <returns>A formatted string containing all guiding items.</returns>
    public static string ToFullText(IEnumerable<GenerativeGuidingItem> items)
    {
        return string.Join("\n", items.Select(x => x.ToFullText()));
    }

    /// <summary>
    /// Converts a collection of guiding items filtered by edit type to a full text representation.
    /// </summary>
    /// <param name="items">The collection of guiding items to convert.</param>
    /// <param name="editType">The edit type to filter by.</param>
    /// <returns>A formatted string containing only guiding items matching the specified edit type.</returns>
    public static string ToFullText(IEnumerable<GenerativeGuidingItem> items, GenerativeEditType editType)
    {
        return string.Join("\n", items.Where(x => x.EditType == editType).Select(x => x.ToFullText()));
    }


    public override string ToString()
    {
        string str = $"[{EditType}] {Name}";
        if (!string.IsNullOrWhiteSpace(Brief))
        {
            str += $" - {Brief}";
        }

        return str;
    }
}


/// <summary>
/// Represents a collection of generative guiding items.
/// </summary>
public class MultipleGuidingItem
{
    /// <summary>
    /// Gets or sets the list of guiding items.
    /// </summary>
    public List<GenerativeGuidingItem> Items { get; set; } = [];
}
