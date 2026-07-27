using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public class AigcNoticePage : AigcTaskPage
{
    readonly NoticeInstance _instance;

    public AigcNoticePage()
    {
        _instance = new(this);
    }

    public AigcNoticePage(NoticeTypes noticeType, string message)
    {
        _instance = new(this, noticeType, message);
    }

    public override LLmMessage[] GetChatMessages(bool input, bool output)
    {
        return null;
    }

    public override IPageAsset GetPageAsset()
    {
        return null;
    }

    public override IPageInstance GetPageInstance() => _instance;

    public override Task<bool> RunTask(AIRequest request, TaskEventTypes eventType, string commitName, object parameter)
    {
        return Task.FromResult(true);
    }

    public override string DisplayText => _instance.NoticeType.ToDisplayTextL();
}

public enum NoticeTypes
{
    [DisplayText("Custom")]
    Custom,

    [DisplayText("New User Requirement")]
    NewUserRequirement,

    [DisplayText("Value Duplicated")]
    ValueDuplicated,

    [DisplayText("Value Missing")]
    ValueMissing,

    [DisplayText("Value Invalid")]
    ValueInvalid,

    [DisplayText("Value Out Of Range")]
    ValueOutOfRange,

    [DisplayText("Value Not Supported")]
    ValueNotSupported,

    [DisplayText("Value Not Match")]
    ValueNotMatch,

    [DisplayText("Value Not Expected")]
    ValueNotExpected,

    [DisplayText("Value Not Found")]
    ValueNotFound,
}

public class NoticeInstance : IPageInstance, IViewObject
{
    readonly AigcNoticePage _owner;
    readonly ValueProperty<NoticeTypes> _noticeType = new("NoticeType", "Notice Type", NoticeTypes.Custom, "The type of notice.");
    readonly TextBlockProperty _message = new("Message", "Notice Message", string.Empty, "The message to be displayed in the notice.");

    public NoticeInstance(AigcNoticePage owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public NoticeInstance(AigcNoticePage owner, NoticeTypes noticeType, string message)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _noticeType.Value = noticeType;
        _message.Text = message;
    }

    public NoticeTypes NoticeType
    {
        get => _noticeType.Value;
        set => _noticeType.Value = value;
    }

    public string Message
    {
        get => _message.Text;
        set => _message.Text = value;
    }

    #region IViewObject
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _noticeType.Sync(sync);
        _message.Sync(sync);
    }

    public void SetupView(IViewObjectSetup setup)
    {
        _noticeType.InspectorField(setup);
        _message.InspectorField(setup);
    }

    #endregion

    #region IPageInstance

    public object Owner => _owner;

    public string Name => _noticeType.Value.ToDisplayText();

    public string FullName => _noticeType.Value.ToDisplayText();

    public ImageDef Icon => CoreIconCache.Notice;

    public IConversationHandler Conversation => null;



    public SimpleType ToSimpleType(FlowDirections direction) => null;

    public object GetParameter(string name) => null;

    public bool SetParameter(string name, object value) => false;

    public bool GetError() => false;

    public string GetErrorMessage() => null;

    public bool? GetIsDone() => true;

    public bool? GetIsDoneInputs() => true;

    public bool? GetIsDoneOutputs() => true;

    public HistoryText GetTaskCommit(ResolveChatIntents intent) => $"<Notice type=`{_noticeType.Value}`>\r\n{_message.Text}\r\n</Notice>";

    public string GetResponseString(ResolveChatIntents intent) => GetFullText();

    public TaskCommitParameter GetTaskCommitParameter()
    {
        return new TaskCommitParameter(TaskCommitStatus.TaskFinished, GetFullText());
    }


    #endregion

    public string GetFullText() => $"{_noticeType.Value}: {_message.Text}";
}