using Suity.Editor.AIGC;
using Suity.Views;
using System.Text;

namespace Suity.Editor.Helpers;

/// <summary>
/// A stream updater that processes LLM output by segmenting it into paragraphs based on XML tags,
/// and updates the conversation UI accordingly.
/// </summary>
public class XmlTagStreamStreamUpdater : LLmStreamUpdater
{
    /// <summary>
    /// Gets or sets a value indicating whether to display the full text content in code blocks.
    /// When false, only the text length is shown.
    /// </summary>
    public bool ShowFullText { get; set; } = true;


    private DisposableDialogItem? _msg;
    /// <summary>
    /// Buffer used to segment incoming text stream into structured paragraphs.
    /// </summary>
    private readonly TextSegmenter _paragraphBuffer = new();

    /// <summary>
    /// Gets a value indicating whether to display the full result
    /// </summary>
    /// <value>Always returns true</value>
    public override bool DisplayingFullResult => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTagStreamStreamUpdater"/> class
    /// and subscribes to paragraph segmentation events.
    /// </summary>
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
    /// Updates the conversation content by feeding incoming text into the paragraph segmenter.
    /// </summary>
    /// <param name="conversation">The conversation handler interface.</param>
    /// <param name="text">The newly received text chunk.</param>
    /// <param name="fullText">The accumulated full text (unused in this implementation).</param>
    protected override void UpdateConversation(IConversationHandler conversation, string text, StringBuilder fullText)
    {
        _paragraphBuffer.InputText(text);
        if (_paragraphBuffer.CurrentParagraph is { } paragraph)
        {
            UpdateMessageBox(conversation, paragraph);
        }
    }

    /// <summary>
    /// Updates the message box with plain text content.
    /// </summary>
    /// <param name="conversation">The conversation handler interface.</param>
    /// <param name="msg">The text content to display.</param>
    private void UpdateMessageBox(IConversationHandler conversation, string msg)
    {
        _msg?.Dispose();
        if (string.IsNullOrWhiteSpace(msg))
        {
            return;
        }

        _msg = conversation.AddSystemMessage(msg);
    }

    /// <summary>
    /// Updates the message box with a structured paragraph, optionally displaying the full text or just its length.
    /// </summary>
    /// <param name="conversation">The conversation handler interface.</param>
    /// <param name="paragraph">The paragraph object containing tag and text information.</param>
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

    /// <summary>
    /// Resets the current message box reference to prepare for a new message.
    /// </summary>
    private void NewMessageBox()
    {
        _msg = null;
    }

    /// <summary>
    /// Releases resources used by the stream updater.
    /// </summary>
    public override void Dispose()
    {
        //_msg?.Dispose();
    }
}
