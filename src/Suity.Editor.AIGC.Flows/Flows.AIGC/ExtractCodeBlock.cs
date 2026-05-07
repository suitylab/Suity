using Suity.Editor.Helpers;
using Suity.Editor.Types;

namespace Suity.Editor.Flows.AIGC;

/// <summary>
/// Node that extracts text from the first code block in Markdown text.
/// </summary>
[DisplayText("Extract Code Block Text", "*CoreIcon|Code")]
[ToolTipsText("Extract text from the first code block in Markdown text, if not found, output the entire text.")]
[NativeAlias("Suity.Editor.AIGC.Flows.ExtractCodeBlock")]
public class ExtractCodeBlock : AigcFlowNode
{
    private readonly FlowNodeConnector _textIn;
    private readonly FlowNodeConnector _textOut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractCodeBlock"/> class.
    /// </summary>
    public ExtractCodeBlock()
    {
        _textIn = AddDataInputConnector("TextIn", "string", "Text Input");
        _textOut = AddDataOutputConnector("TextOut", "string", "Text Output");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        string text = compute.GetValue<string>(_textIn) ?? string.Empty;

        string result = text;
        if (ResourceHelper.TryExtactCodeBlock(text, out string code))
        {
            result = code;
        }

        compute.SetValue(_textOut, result); // Output text
    }
}