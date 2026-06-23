using Suity.Drawing;
using Suity.Helpers;
using Suity.Views.Im;
using System.Drawing;

namespace Suity.Editor.AIGC.Agentic;

internal class AgentTaskTheme : ImGuiTheme
{
    public static AgentTaskTheme Instance { get; } = new();

    public EditorColorScheme ColorScheme { get; } = EditorColorScheme.Default;

    protected override void OnBuildTheme()
    {
        this.ClassStyle("loopFrame")
        .SetColor(ColorScheme.EditorBG2.MultiplyAlpha(0.3f))
        .SetBorder(1, ColorScheme.EditorBG);
        //this.PseudoMouseIn()
        //.SetBorder(1, Color.White.MultiplyAlpha(0.5f));

        this.ClassStyle("loopFrame-running")
        .SetColor(Color.Cyan.MultiplyAlpha(0.35f))
        .SetBorder(1, ColorScheme.EditorBG);

        this.ClassStyle("loopFrame-delegating")
        .SetColor(Color.Orange.MultiplyAlpha(0.35f))
        .SetBorder(1, ColorScheme.EditorBG);

        var font = new FontDef(ImGuiTheme.DefaultFont, 14);
        var fontBold = new FontDef(ImGuiTheme.DefaultFont, 14, FontStyle.Bold);
        var fontSub = new FontDef(ImGuiTheme.DefaultFont, 12);

        this.ClassStyle("textLight")
        .SetFont(font, Color.White.MultiplyAlpha(0.5f));

        this.ClassStyle("textBold")
        .SetFont(fontBold, Color.White);

        this.ClassStyle("textSub")
        .SetFont(fontSub, Color.LightGray.MultiplyAlpha(0.5f));

        this.ClassStyle("textBoldRed")
        .SetFont(fontBold, Color.Red);
    }
}
