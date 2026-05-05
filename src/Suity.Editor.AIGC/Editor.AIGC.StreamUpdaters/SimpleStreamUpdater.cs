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
        _msg = conversation.AddSystemMessage(fullText.ToString());
    }

    public override void Dispose()
    {
    }
}