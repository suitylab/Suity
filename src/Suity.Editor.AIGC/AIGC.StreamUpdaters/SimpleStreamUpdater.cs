using Suity.Views;
using System.Text;

namespace Suity.Editor.AIGC.StreamUpdaters;

[NotAvailable]
public class SimpleStreamUpdater : LLmStreamUpdater
{

    DisposableDialogItem _msg;

    public override bool DisplayingFullResult => true;

    protected override void UpdateConversation(IConversationHandler conversation, string text, StringBuilder fullText)
    {
        _msg?.Dispose();
        string msg = string.Empty;
        lock (fullText)
        {
            msg = fullText.ToString();
        }
        _msg = conversation.AddSystemMessage(msg);
    }

    public override void Dispose()
    {
    }
}