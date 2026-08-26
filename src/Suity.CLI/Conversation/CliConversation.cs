using Suity.Views;

namespace Suity.Editor.Conversation;

internal class CliConversation : ConversationHost
{
    public CliConversation(string id) : base(id)
    {
    }

    protected override void OnMessageAdded(DialogItem item)
    {
        if (base.Level != ConversationLevels.Main)
        {
            return;
        }

        item.WriteConsole();
    }
}