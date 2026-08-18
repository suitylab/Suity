using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("ManualReply", CodeBase = "*Suity", Icon = "*CoreIcon|System", Category = "WorkSpace Tools")]
[DisplayText("Manual reply")]
[ToolTipsText("Do not execute any commands. Pause and wait for manual reply of the provided question, then output the reply result.")]
public class ManualReply : ToolCommand<ManualReply.Output>
{
    public class Output : SObjectController
    {
        readonly TextBlockProperty _result = new("Result");

        public string Result { get => _result.Text; set => _result.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _result.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _result.InspectorField(setup);
        }
    }

    readonly TextBlockProperty _question = new("Question");

    public string Question { get => _question.Text; set => _question.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _question.Sync(sync);
    }
    public override void SetupView(IViewObjectSetup setup)
    {
        _question.InspectorField(setup);
    }

    public override async Task<Output> Run(ToolCallContext context)
    {
        string question = this.Question;
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new NullReferenceException("Question is not set");
        }

        context?.AddToolMessage("Manual reply", msg =>
        {
            msg.AddCode(question);
        });

        IConversationHandler conversation = context?.Conversation; 
        if (conversation is null)
        {
            conversation = context?.ToolInstance?.Conversation;
        }
        if (conversation is null)
        {
            throw new NullReferenceException("Conversation is not found");
        }

        conversation.AddInfoMessage("Please enter your reply message in the input field at the bottom.");
        string result = await conversation.WaitForTextInput(context.Cancellation);

        return new Output
        {
            Result = result,
        };
    }
}