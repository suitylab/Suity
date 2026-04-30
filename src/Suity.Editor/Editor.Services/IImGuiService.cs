using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Base options for tree views.
/// </summary>
public record BaseTreeOptions
{
    /// <summary>
    /// Gets or sets the menu name for the tree.
    /// </summary>
    public string MenuName { get; init; }

    /// <summary>
    /// Gets or sets whether to show display text.
    /// </summary>
    public bool? ShowDisplayText { get; init; }

    /// <summary>
    /// Gets or sets whether to show status icon at the end.
    /// </summary>
    public bool? StatusIconAtTheEnd { get; init; }
}

/// <summary>
/// Tree options without headers.
/// </summary>
public record HeaderlessTreeOptions : BaseTreeOptions
{
}

/// <summary>
/// Tree options with columns.
/// </summary>
public record ColumnTreeOptions : BaseTreeOptions
{
    /// <summary>
    /// Gets or sets whether the column resizer is full width.
    /// </summary>
    public bool? FullColumnResizer { get; init; }

    /// <summary>
    /// Gets or sets the maximum resizer width.
    /// </summary>
    public int? ResizerMax { get; init; }

    /// <summary>
    /// Gets or sets whether to show the name column.
    /// </summary>
    public bool? NameColumn { get; init; }

    /// <summary>
    /// Gets or sets whether to show the description column.
    /// </summary>
    public bool? DescriptionColumn { get; init; }

    /// <summary>
    /// Gets or sets whether to show the preview column.
    /// </summary>
    public bool? PreviewColumn { get; init; }
}

/// <summary>
/// Options for dialog windows.
/// </summary>
public class DialogOptions
{
    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets or sets the dialog icon.
    /// </summary>
    public Image Icon { get; init; }

    /// <summary>
    /// Gets or sets whether this is a dialog (vs a window).
    /// </summary>
    public bool IsDialog { get; init; }

    /// <summary>
    /// Gets or sets the dialog width.
    /// </summary>
    public int Width { get; init; } = 800;

    /// <summary>
    /// Gets or sets the dialog height.
    /// </summary>
    public int Height { get; init; } = 480;

    /// <summary>
    /// Gets or sets whether the dialog has a fixed size.
    /// </summary>
    public bool FixedSize { get; init; }
}


/// <summary>
/// Interface for conversation ImGui components.
/// </summary>
public interface IConversationImGui : IConversationHandler, IDrawImGuiNode
{
}

/// <summary>
/// Service interface for ImGui rendering and UI creation.
/// </summary>
public interface IImGuiService
{
    /// <summary>
    /// Creates an ImGui instance for the specified graphic context.
    /// </summary>
    /// <param name="context">The graphic context.</param>
    /// <param name="config">The ImGui configuration.</param>
    /// <returns>A new ImGui instance.</returns>
    ImGui CreateImGui(IGraphicContext context, ImGuiConfig config);

    /// <summary>
    /// Creates a dialog with the specified ImGui content.
    /// </summary>
    /// <param name="imGui">The ImGui content to display.</param>
    /// <param name="option">The dialog options.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateImGuiDialog(IDrawImGui imGui, DialogOptions option);

    /// <summary>
    /// Creates an ImGui control wrapper for the specified ImGui content.
    /// </summary>
    /// <param name="imGui">The ImGui content to wrap.</param>
    /// <returns>An object representing the control.</returns>
    object CreateImGuiControl(IDrawImGui imGui);

    /// <summary>
    /// Creates an expanded view for the specified object type.
    /// </summary>
    /// <param name="objectType">The type of object to create the view for.</param>
    /// <returns>An expanded view instance.</returns>
    IDrawExpandedImGui CreateExpandedView(Type objectType);

    /// <summary>
    /// Creates a simple tree ImGui with headerless options.
    /// </summary>
    /// <param name="option">The tree options.</param>
    /// <returns>An undoable tree view.</returns>
    IUndoableViewObjectImGui CreateSimpleTreeImGui(HeaderlessTreeOptions option);

    /// <summary>
    /// Creates a column tree ImGui with column options.
    /// </summary>
    /// <param name="option">The column tree options.</param>
    /// <returns>An undoable column tree view.</returns>
    IUndoableViewObjectImGui CreateColumnTreeImGui(ColumnTreeOptions option);

    /// <summary>
    /// Creates a conversation ImGui instance.
    /// </summary>
    /// <param name="id">The conversation identifier.</param>
    /// <param name="disableOldMessage">Whether to disable old messages.</param>
    /// <returns>A conversation ImGui instance.</returns>
    IConversationImGui CreateConversationImGui(string id, bool disableOldMessage = true);

    /// <summary>
    /// Gets the editor theme.
    /// </summary>
    /// <param name="preview">Whether this is for preview mode.</param>
    /// <returns>The editor theme.</returns>
    ImGuiTheme GetEditorTheme(bool preview);

    /// <summary>
    /// Draws an item using the specified ImGui and pipeline.
    /// </summary>
    /// <param name="gui">The ImGui instance.</param>
    /// <param name="item">The item to draw.</param>
    /// <param name="pipeline">The rendering pipeline.</param>
    /// <param name="context">The drawing context.</param>
    /// <param name="allDrawers">Whether to use all drawers.</param>
    /// <returns>True if the item was drawn successfully.</returns>
    bool DrawItem(ImGui gui, object item, EditorImGuiPipeline pipeline, IDrawContext context, bool allDrawers = true);
}