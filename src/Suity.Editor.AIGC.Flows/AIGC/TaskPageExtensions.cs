using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows.SubFlows;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.AIGC;

public static class TaskPageExtensions
{
    public static IDisposable AddToolMessage(this ToolCallContext context, string content, Action<IDialogMessage> config = null)
    {
        return new ToolCallDialogMessage(context, content, TextStatus.Normal, config);
    }

    public static IDisposable AddToolMessage(this ToolCallContext context, string content, TextStatus status, Action<IDialogMessage> config = null)
    {
        return new ToolCallDialogMessage(context, content, status, config);
    }

    public static DisposableDialogItem AddRunningMessage(this AigcWorkflowPage page, AIRequest request, string content)
    {
        return request.Conversation.AddSystemMessage(content, msg =>
        {
            msg.AddButtons(string.Empty, [
                new()
                {
                    Key = "Feedback",
                    Text = "Feedback",
                    CallBack = () => AppendFeedBack(page)
                },
                new()
                {
                    Key = "Open",
                    Text = "Open",
                    CallBack = () => page.SelectTaskInView()
                },
            ]);
        });
    }

    private static AigcNoticePage AppendFeedBack(this AigcWorkflowPage page)
    {
        if (page.GetDocument() is not AigcLoopDocument doc)
        {
            return null;
        }

        var item = new AigcNoticePage(NoticeTypes.UserFeedback, string.Empty);
        int index = page.GetIndex();

        page.ParentList?.Insert(index + 1, item);

        doc.MarkDirtyAndSaveDelayed(page);
        doc.View?.RefreshView();
        item.SelectTaskInView();

        return item;
    }
}

class ToolCallDialogMessage : IDialogMessage, IDisposable
{
    readonly DisposableDialogItem _localMessage;
    readonly DisposableDialogItem _globalMessage;
    readonly Action<IDialogMessage> _config;


    public ToolCallDialogMessage(ToolCallContext context, string content, TextStatus status, Action<IDialogMessage> config)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Status = status;
        Message = content;
        _config = config;

        string toolContent = "Run tool: " + content;

        _localMessage = context.ToolInstance?.Conversation?.AddSystemMessage(toolContent, Status, _config);
        _globalMessage = context.Conversation?.AddSystemMessage(toolContent, Status, msg =>
        {
            if (context.ToolInstance?.Owner is AigcTaskPage taskPage)
            {
                msg.AddButton("Open", "Open", () =>
                {
                    taskPage.SelectTaskInView();
                });
            }
            _config?.Invoke(msg);
        });
    }

    public ToolCallContext Context { get; }

    public TextStatus Status { get; }

    public ConversationRole Role { get; }

    public string Message { get; }

    public void AddButton(string key, string text, Action callBack = null)
    {
        (_localMessage?.DialogItem as IDialogMessage)?.AddButton(key, text, callBack);
        (_globalMessage?.DialogItem as IDialogMessage)?.AddButton(key, text, callBack);
    }

    public void AddButtons(string title, IEnumerable<ConversationButton> buttons)
    {
        (_localMessage?.DialogItem as IDialogMessage)?.AddButtons(title, buttons);
        (_globalMessage?.DialogItem as IDialogMessage)?.AddButtons(title, buttons);
    }

    public void AddCode(string code)
    {
        (_localMessage?.DialogItem as IDialogMessage)?.AddCode(code);
        (_globalMessage?.DialogItem as IDialogMessage)?.AddCode(code);
    }

    public void AddLine()
    {
        (_localMessage?.DialogItem as IDialogMessage)?.AddLine();
        (_globalMessage?.DialogItem as IDialogMessage)?.AddLine();
    }

    public void AddProgressBar(float progress, float max)
    {
        (_localMessage?.DialogItem as IDialogMessage)?.AddProgressBar(progress, max);
        (_globalMessage?.DialogItem as IDialogMessage)?.AddProgressBar(progress, max);
    }

    public void AddText(string text)
    {
        (_localMessage?.DialogItem as IDialogMessage)?.AddText(text);
        (_globalMessage?.DialogItem as IDialogMessage)?.AddText(text);
    }

    public void Dispose()
    {
        _localMessage?.Dispose();
        _globalMessage?.Dispose();
    }
}