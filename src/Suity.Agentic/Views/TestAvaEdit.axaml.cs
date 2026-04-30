using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Highlighting;

namespace Suity.Editor;

public partial class TestAvaEdit : Window
{
    public TestAvaEdit()
    {
        InitializeComponent();

        // 1. Set initial text
        Editor.Text = "<Root>\n    <Item Name=\"Avalonia 11\" />\n</Root>";

        // 2. Load built-in highlight definition (e.g. XML)
        Editor.SyntaxHighlighting = HighlightingManager.Instance
            .GetDefinition("XML");

        // 3. Configure other editor options
        Editor.Options.ShowTabs = true;
        Editor.Options.IndentationSize = 4;
    }

    private void OnGetTextClick(object sender, RoutedEventArgs e)
    {
        // Get the current text of the editor
        var content = Editor.Text;
        System.Diagnostics.Debug.WriteLine(content);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
    }
}