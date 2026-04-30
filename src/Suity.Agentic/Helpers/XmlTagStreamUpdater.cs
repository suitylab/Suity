using Suity.Editor.AIGC;
using Suity.Views;
using System.Text;

namespace Suity.Editor.Helpers;

/// <summary>
/// Stream Updater class, inherits from LLmStreamUpdater, used to handle streaming output display
/// </summary>
public class XmlTagStreamStreamUpdater : LLmStreamUpdater
{
    public bool ShowFullText { get; set; } = true;


    private DisposableDialogItem? _msg;
    private readonly TextSegmenter _paragraphBuffer = new();

    /// <summary>
    /// Gets a value indicating whether to display the full result
    /// </summary>
    /// <value>Always returns true</value>
    public override bool DisplayingFullResult => true;

    public XmlTagStreamStreamUpdater()
    {
        _paragraphBuffer.OnParagraphCompleted += paragraph =>
        {
            UpdateMessageBox(Conversation, paragraph);
        };

        _paragraphBuffer.OnParagraphStarted += paragraph =>
        {
            NewMessageBox();
        };
    }

    /// <summary>
    /// Method to update conversation content
    /// </summary>
    /// <param name="conversation">Conversation handler interface</param>
    /// <param name="text">Currently received text</param>
    /// <param name="fullText">Accumulated full text</param>
    protected override void UpdateConversation(IConversationHandler conversation, string text, StringBuilder fullText)
    {
        _paragraphBuffer.InputText(text);
        if (_paragraphBuffer.CurrentParagraph is { } paragraph)
        {
            UpdateMessageBox(conversation, paragraph);
        }
    }

    private void UpdateMessageBox(IConversationHandler conversation, string msg)
    {
        _msg?.Dispose();
        if (string.IsNullOrWhiteSpace(msg))
        {
            return;
        }

        _msg = conversation.AddSystemMessage(msg);
    }

    private void UpdateMessageBox(IConversationHandler conversation, Paragraph paragraph)
    {
        _msg?.Dispose();
        if (paragraph is null || string.IsNullOrWhiteSpace(paragraph.Text))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(paragraph.TagText))
        {
            _msg = conversation.AddSystemMessage(paragraph.TagText, msg => 
            {
                if (ShowFullText)
                {
                    msg.AddCode(paragraph.Text);
                }
                else
                {
                    msg.AddCode($"[{paragraph.Text.Length}]");
                }
            });
        }
        else
        {
            _msg = conversation.AddSystemMessage(paragraph.Text);
        }
    }

    private void NewMessageBox()
    {
        _msg = null;
    }

    /// <summary>
    /// Method to release resources
    /// </summary>
    public override void Dispose()
    {
        //_msg?.Dispose();
    }
}
