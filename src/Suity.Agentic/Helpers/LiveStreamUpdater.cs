using OpenAI_API.Chat;
using Suity.Editor.AIGC;
using Suity.Views;
using System.Text;

namespace Suity.Editor.Helpers;

/// <summary>
/// Stream Updater class, inherits from LiveStreamUpdater, used to handle streaming output display
/// </summary>
public class LiveStreamUpdater : LLmStreamUpdater
{
    private const int MaxParagraphLength = 5000;

    private DisposableDialogItem? _msg;
    private readonly StringBuilder _paragraphBuffer = new();

    /// <summary>
    /// Gets a value indicating whether to display the full result
    /// </summary>
    /// <value>Always returns true</value>
    public override bool DisplayingFullResult => true;

    /// <summary>
    /// Method to update conversation content
    /// </summary>
    /// <param name="conversation">Conversation handler interface</param>
    /// <param name="text">Currently received text</param>
    /// <param name="fullText">Accumulated full text</param>
    protected override void UpdateConversation(IConversationHandler conversation, string text, StringBuilder fullText)
    {
        while (true)
        {
            if (_paragraphBuffer.Length + text.Length < MaxParagraphLength)
            {
                _paragraphBuffer.Append(text);
                UpdateMessageBox(conversation, _paragraphBuffer.ToString());
                break;
            }

            var newlineIndex = text.IndexOfAny(['\n', '\r']);
            if (newlineIndex < 0)
            {
                _paragraphBuffer.Append(text);
                UpdateMessageBox(conversation, _paragraphBuffer.ToString());
                break;
            }

            _paragraphBuffer.Append(text, 0, newlineIndex);
            UpdateMessageBox(conversation, _paragraphBuffer.ToString());

            _paragraphBuffer.Clear();
            text = text.Substring(newlineIndex + 1);
            NewMessageBox();
        }
    }

    private void UpdateMessageBox(IConversationHandler conversation, string msg)
    {
        _msg?.Dispose();
        _msg = conversation.AddSystemMessage(msg);
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
