using Suity.Editor.Flows.Gui;
using System.Drawing;

namespace Suity.Editor.AIGC.Flows;

/// <summary>
/// Custom graph data type for conversation threads, with magenta-colored connectors.
/// </summary>
public class ConversationThreadDataType : CustomGraphDataType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationThreadDataType"/> class.
    /// </summary>
    public ConversationThreadDataType()
    {
        _typeName = "<ConversationThread>";
        this._allowMultipleToConnection = true;
        this._linkPen = new Pen(Color.FromArgb(255, 0, 255), 3);
        this._linkArrowBrush = new SolidBrush(Color.FromArgb(255, 0, 255));
        this._connectorOutlinePen = new Pen(Color.FromArgb(255, 0, 255), 3);
        this._connectorFillBrush = new SolidBrush(Color.FromArgb(255, 0, 255));
    }

    /// <inheritdoc/>
    public override string ToString() => "Conversation";
}