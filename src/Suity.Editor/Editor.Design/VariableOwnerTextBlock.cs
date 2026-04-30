using Suity.Views;

namespace Suity.Editor.Design
{
/// <summary>
/// Represents a text block that owns variables.
/// </summary>
public class VariableOwnerTextBlock : TextBlock
{
    /// <summary>
    /// Gets or sets the variable container owner.
    /// </summary>
    public IVariableContainer VariableOwner { get; set; }

    /// <summary>
    /// Renames a variable reference in the text.
    /// </summary>
    public bool Rename(string oldName, string newName)
    {
        Text ??= string.Empty;

        string text = Text;
        Text = text.Replace($"{{{oldName}}}", $"{{{newName}}}");
        return Text != text;
    }
}
}