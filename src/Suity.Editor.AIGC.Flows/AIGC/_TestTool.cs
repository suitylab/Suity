using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("TestTool", CodeBase = "*Suity")]
public class TestTool : ToolAsset<TestTool.Input, TestTool.Output>
{
    public class Input : IViewObject
    {
        readonly TextBlockProperty _text = new("Text");

        public string Text { get => _text.Text; set => _text.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _text.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _text.InspectorField(setup);
        }
    }

    public class Output : IViewObject
    {
        readonly TextBlockProperty _text = new("Text");

        public string Text { get => _text.Text; set => _text.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _text.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _text.InspectorField(setup);
        }
    }

    protected override async Task<Output> RunTask(Input input, ToolCallContext context)
    {
        context.Conversation.AddMessage("Handle");

        return new Output
        {
            Text = "Handle: " + input?.Text,
        };
    }

}