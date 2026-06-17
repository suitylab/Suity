using Suity.Drawing;
using Suity.Helpers;
using Suity.Views.Im;
using System.Drawing;

namespace Suity.Editor.AIGC;

internal class AgentTaskTheme : ImGuiTheme
{
    public static AgentTaskTheme Instance { get; } = new();

    public EditorColorScheme ColorScheme { get; } = EditorColorScheme.Default;

    protected override void OnBuildTheme()
    {
        this.ClassStyle("taskFrame")
        .SetColor(ColorScheme.EditorBG2.MultiplyAlpha(0.3f))
        .SetBorder(1, ColorScheme.EditorBG);

        var font = new FontDef(ImGuiTheme.DefaultFont, 14);
        var fontBold = new FontDef(ImGuiTheme.DefaultFont, 14, FontStyle.Bold);

        this.ClassStyle("textLight")
        .SetFont(font, Color.White.MultiplyAlpha(0.5f));

        this.ClassStyle("textBold")
        .SetFont(fontBold, Color.White);
    }
}
