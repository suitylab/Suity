namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Defines theme constants and helper methods for the property grid UI.
/// </summary>
public static class PropertyGridThemes
{
    /// <summary>
    /// CSS class name for a standard property input field.
    /// </summary>
    public const string ClassPropertyInput = "propInput";

    /// <summary>
    /// CSS class name for a property input field displaying multiple values.
    /// </summary>
    public const string ClassPropertyInputMultiple = "propInputMultiple";

    /// <summary>
    /// CSS class name for a read-only property input field.
    /// </summary>
    public const string ClassPropertyInputReadonly = "propInputReadonly";

    /// <summary>
    /// CSS class name for a standard property line.
    /// </summary>
    public const string ClassPropertyLine = "propLine";

    /// <summary>
    /// CSS class name for a selected property line.
    /// </summary>
    public const string ClassPropertyLineSel = "propLineSel";

    /// <summary>
    /// CSS class name for a property cell.
    /// </summary>
    public const string ClassPropertyCell = "propCell";

    /// <summary>
    /// CSS class name for the property resizer handle.
    /// </summary>
    public const string ClassPropertyResizer = "propResizer";

    /// <summary>
    /// CSS class name for embossed property elements.
    /// </summary>
    public const string ClassPropertyEmboss = "propEmboss";

    /// <summary>
    /// CSS class name for the property background.
    /// </summary>
    public const string ClassBG = "propBG";

    /// <summary>
    /// CSS class name for property labels.
    /// </summary>
    public const string ClassLabel = "propLabel";

    /// <summary>
    /// CSS class name for the label cell in a property row.
    /// </summary>
    public const string ClassLabelCell = "propLabelCell";

    /// <summary>
    /// Gets the appropriate CSS class for a property input field based on its state.
    /// </summary>
    /// <param name="valueMultiple"><c>true</c> if the field displays multiple values.</param>
    /// <param name="isReadonly"><c>true</c> if the field is read-only.</param>
    /// <returns>The CSS class name for the property input.</returns>
    public static string GetPropertyInputClass(bool valueMultiple, bool isReadonly)
    {
        if (valueMultiple) return ClassPropertyInputMultiple;

        return isReadonly ? ClassPropertyInputReadonly : ClassPropertyInput;
    }

    /// <summary>
    /// Gets the appropriate CSS class for a brief display based on its state.
    /// </summary>
    /// <param name="valueMultiple"><c>true</c> if the brief displays multiple values.</param>
    /// <returns>The CSS class name for the brief display.</returns>
    public static string GetBriefClass(bool valueMultiple)
    {
        return valueMultiple ? "briefMultiple" : "brief";
    }
}
