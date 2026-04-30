using System.Drawing;

namespace Suity.Views.Im.TreeEditing;

/// <summary>
/// Defines the visual theme for tree view controls in ImGui.
/// </summary>
public class TreeViewTheme : ImGuiTheme
{
    /// <summary>
    /// CSS-like class name for the tree view background.
    /// </summary>
    public const string ClassTreeViewBg = "treeViewBg";

    /// <summary>
    /// CSS-like class name for the tree view header.
    /// </summary>
    public const string ClassHeader = "header";

    /// <summary>
    /// CSS-like class name for tree nodes.
    /// </summary>
    public const string ClassTreeNode = "treeNode";

    /// <summary>
    /// CSS-like class name for column resizers.
    /// </summary>
    public const string ClassResizer = "resizer";

    /// <summary>
    /// CSS-like class name for node icons.
    /// </summary>
    public const string ClassIcon = "icon";

    /// <summary>
    /// Gets the color scheme used by this theme.
    /// </summary>
    public EditorColorScheme ColorScheme { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewTheme"/> class with the default color scheme.
    /// </summary>
    public TreeViewTheme()
        : this(EditorColorScheme.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewTheme"/> class with the specified color scheme.
    /// </summary>
    /// <param name="colorScheme">The color scheme to use for theming.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="colorScheme"/> is null.</exception>
    public TreeViewTheme(EditorColorScheme colorScheme)
    {
        ColorScheme = colorScheme ?? throw new System.ArgumentNullException(nameof(colorScheme));
    }

    /// <inheritdoc/>
    protected override void OnBuildTheme()
    {
        this.ClassStyle(ClassTreeViewBg)
            .SetRectangleFrame(ColorScheme.EditorBG)
            .SetPadding(0)
            .SetSizeRest();

        this.ClassStyle(GuiTreeViewExtensions.ClassTreeView)
            .SetRectangleFrame(ColorScheme.EditorBG)
            .SetBorder(0, ColorScheme.ScrollBar)
            .SetPadding(0);

        this.ClassStyle(ClassHeader)
            .SetColor(ColorScheme.EditorBG)
            .SetCornerRound(0)
            .SetBorder(0)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);

        this.ClassStyle(GuiTreeViewExtensions.ClassTreeViewRowSelected)
            .SetColor(ColorScheme.EditorSelection)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetColor(ColorScheme.EditorSelectionMouseIn);

        this.ClassStyle(GuiTreeViewExtensions.ClassExpandButton)
            .SetColor(Color.Transparent)
            .SetCornerRound(0)
            .SetBorder(0)
            .SetSize(24, 24)
            .SetVerticalAlignment(GuiAlignment.Center);

        this.ClassStyle(ClassTreeNode)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);

        this.ClassStyle(ClassResizer)
            .SetBorder(1, ColorScheme.ComponentBG)
            .SetFullHeight()
            .SetWidth(5)
            .SetHeight(10)
            .SetCenter(true)
            .SetSiblingSpacing(0);
        this.PseudoMouseIn()
            .SetBorder(1, ColorScheme.ComponentBG);

        this.ClassStyle(ClassIcon)
            .SetSize(16, 16)
            .SetVerticalAlignment(GuiAlignment.Center);

        SetOneColorRow();
    }

    /// <summary>
    /// Configures the tree view rows to use a single alternating background color.
    /// </summary>
    public void SetOneColorRow()
    {
        this.ClassStyle(GuiTreeViewExtensions.ClassTreeViewRow1)
            .SetColor(ColorScheme.EditorBG)
            .SetCornerRound(0)
            .SetBorder(0)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetColor(ColorScheme.EditorMouseIn);

        this.ClassStyle(GuiTreeViewExtensions.ClassTreeViewRow2)
            .SetColor(ColorScheme.EditorBG)
            .SetCornerRound(0)
            .SetBorder(0)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetColor(ColorScheme.EditorMouseIn);
    }

    /// <summary>
    /// Configures the tree view rows to use two alternating background colors for a striped appearance.
    /// </summary>
    public void SetTwoColorRow()
    {
        this.ClassStyle(GuiTreeViewExtensions.ClassTreeViewRow1)
            .SetColor(ColorScheme.EditorBG2)
            .SetCornerRound(0)
            .SetBorder(0)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetColor(ColorScheme.EditorMouseIn);

        this.ClassStyle(GuiTreeViewExtensions.ClassTreeViewRow2)
            .SetColor(ColorScheme.EditorBG)
            .SetCornerRound(0)
            .SetBorder(0)
            .SetInputFunctionChain(ImGuiInputSystem.MouseInRender);
        this.PseudoMouseIn()
            .SetColor(ColorScheme.EditorMouseIn);
    }
}