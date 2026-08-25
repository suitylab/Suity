using Suity.Collections;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Conversation;

internal class CliConversation : ConversationImGui
{
    public CliConversation(string id) : base(id)
    {
    }

    protected override void OnMessageAdded(IDIalogItem item)
    {
    }
}