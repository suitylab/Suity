using Suity.Helpers;
using System;
using System.Drawing;

namespace Suity.Views.Im;

/// <summary>
/// Defines a set of colors used for theming the user interface of an editor component.
/// </summary>
public class EditorColorScheme
{
    /// <summary>
    /// Gets the default editor color scheme.
    /// </summary>
    public static EditorColorScheme Default { get; } = new();

    /// <summary>
    /// Gets the main background color.
    /// </summary>
    public virtual Color Background => Color.FromArgb(26, 26, 26);

    /// <summary>
    /// Gets the primary editor background color.
    /// </summary>
    public virtual Color EditorBG => Color.FromArgb(36, 36, 36);

    /// <summary>
    /// Gets the secondary editor background color.
    /// </summary>
    public virtual Color EditorBG2 => Color.FromArgb(28, 28, 28);

    /// <summary>
    /// Gets the color when mouse is over an editor element.
    /// </summary>
    public virtual Color EditorMouseIn => Color.FromArgb(47, 47, 47);

    /// <summary>
    /// Gets the selection color.
    /// </summary>
    public virtual Color EditorSelection => Color.FromArgb(69, 69, 69);

    /// <summary>
    /// Gets the selection color when mouse is over it.
    /// </summary>
    public virtual Color EditorSelectionMouseIn => Color.FromArgb(80, 80, 80);

    /// <summary>
    /// Gets the tool button background color.
    /// </summary>
    public virtual Color ToolButton => Color.FromArgb(56, 56, 56);

    /// <summary>
    /// Gets the tool button color when mouse is over it.
    /// </summary>
    public virtual Color ToolButtonMouseIn => Color.FromArgb(64, 64, 64);

    /// <summary>
    /// Gets the component background color.
    /// </summary>
    public virtual Color ComponentBG => Color.FromArgb(26, 26, 26);

    /// <summary>
    /// Gets the string input background color.
    /// </summary>
    public virtual Color StringInput => Color.FromArgb(15, 15, 15);

    /// <summary>
    /// Gets the string input color when mouse is over it.
    /// </summary>
    public virtual Color StringInputMouseIn => Color.DimGray;

    /// <summary>
    /// Gets the icon color.
    /// </summary>
    public virtual Color Icon => Color.FromArgb(157, 157, 157);

    /// <summary>
    /// Gets the scroll bar color.
    /// </summary>
    public virtual Color ScrollBar => Color.FromArgb(128, 69, 69, 69);

    /// <summary>
    /// Gets the header background color.
    /// </summary>
    public virtual Color Header => Color.FromArgb(47, 47, 47);

    /// <summary>
    /// Gets the button background color.
    /// </summary>
    public virtual Color ButtonBG => Color.FromArgb(15, 15, 15);

    /// <summary>
    /// Gets the button border color.
    /// </summary>
    public virtual Color ButtonBorder => Color.FromArgb(69, 69, 69);

    /// <summary>
    /// Gets the button text color.
    /// </summary>
    public virtual Color ButtonText => Color.FromArgb(192, 192, 192);

    /// <summary>
    /// Gets the color for values with multiple selections.
    /// </summary>
    public virtual Color ValueMultiple => Color.FromArgb(80, 15, 40);

    /// <summary>
    /// Gets the color for read-only values.
    /// </summary>
    public virtual Color ValueReadonly => Color.FromArgb(60, 60, 60);

    /// <summary>
    /// Gets the placement text color.
    /// </summary>
    public virtual Color PlacementText => Color.FromArgb(100, 100, 100);

    /// <summary>
    /// Gets the brief text color.
    /// </summary>
    public virtual Color BriefText => Color.FromArgb(200, 200, 200);

    /// <summary>
    /// Gets the brief text color for multiple selections.
    /// </summary>
    public virtual Color BriefTextMultiple => Color.FromArgb(222, 65, 125);

    /// <summary>
    /// Gets the highlight color.
    /// </summary>
    public virtual Color Highlight => Color.FromArgb(0, 142, 255);
}

/// <summary>
/// A preview color scheme with slightly lighter colors than the default editor scheme.
/// </summary>
public class PreviewColorScheme : EditorColorScheme
{
    /// <summary>
    /// Gets the default preview color scheme.
    /// </summary>
    public static new PreviewColorScheme Default { get; } = new();

    /// <inheritdoc/>
    public override Color EditorBG => base.EditorBG.Add(10, 20, 10);
    /// <inheritdoc/>
    public override Color EditorMouseIn => base.EditorMouseIn.Add(10, 20, 10);
    /// <inheritdoc/>
    public override Color EditorSelection => base.EditorSelection.Add(10, 20, 10);
    /// <inheritdoc/>
    public override Color EditorSelectionMouseIn => base.EditorSelectionMouseIn.Add(10, 20, 10);
}

/// <summary>
/// Specifies the stages of the editor ImGui rendering pipeline.
/// </summary>
public enum EditorImGuiPipeline
{
    /// <summary>
    /// Normal rendering stage.
    /// </summary>
    Normal,

    /// <summary>
    /// Content begins.
    /// </summary>
    Begin,

    /// <summary>
    /// End of content.
    /// </summary>
    End,

    /// <summary>
    /// Property prefix.
    /// </summary>
    Prefix,

    /// <summary>
    /// Property name.
    /// </summary>
    Name,

    /// <summary>
    /// Property description.
    /// </summary>
    Description,

    /// <summary>
    /// Property preview.
    /// </summary>
    Preview,

    /// <summary>
    /// Content.
    /// </summary>
    Content,

    /// <summary>
    /// Input.
    /// </summary>
    Input,

    /// <summary>
    /// Output.
    /// </summary>
    Output,

    /// <summary>
    /// Option.
    /// </summary>
    Option,
}

/// <summary>
/// Interface for draw context data passed during editor rendering.
/// </summary>
public interface IDrawContext
{
}

/// <summary>
/// Defines a contract for rendering custom editor user interfaces using ImGui within an editor pipeline.
/// </summary>
public interface IDrawEditorImGui
{
    /// <summary>
    /// Renders custom editor GUI elements using the specified ImGui context and editor pipeline.
    /// </summary>
    /// <param name="gui">The ImGui context used to render GUI elements.</param>
    /// <param name="pipeline">The editor pipeline stage.</param>
    /// <param name="context">Additional draw context data.</param>
    /// <returns>true if the GUI was rendered and handled successfully; otherwise, false.</returns>
    bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context);
}

/// <summary>
/// Defines a contract for rendering editor GUI elements for specific item types using ImGui.
/// </summary>
public interface IDrawItemImGui
{
    /// <summary>
    /// Gets the types of items that can be contained or processed by the implementing object.
    /// </summary>
    /// <returns>An array of <see cref="Type"/> representing the supported item types.</returns>
    Type[] GetItemTypes();

    /// <summary>
    /// Renders the editor user interface for the specified item.
    /// </summary>
    /// <param name="gui">The ImGui context.</param>
    /// <param name="item">The item to be displayed or edited.</param>
    /// <param name="pipeline">The editor pipeline stage.</param>
    /// <param name="context">Additional draw context data.</param>
    /// <returns>true if the item's state was modified; otherwise, false.</returns>
    bool OnEditorGui(ImGui gui, object item, EditorImGuiPipeline pipeline, IDrawContext context);
}

/// <summary>
/// Represents a method that draws custom editor content using the specified ImGui context and editor pipeline.
/// </summary>
/// <param name="gui">The ImGui context used to render UI elements.</param>
/// <param name="pipeline">The editor pipeline stage.</param>
/// <param name="context">Additional draw context data.</param>
/// <returns>true if the editor content was drawn successfully; otherwise, false.</returns>
public delegate bool DrawEditorImGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context);
