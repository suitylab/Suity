using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("TestTool", CodeBase = "*Suity")]
[DisplayText("Test Tool")]
public class TestTool : ToolCommand<TestTool.Output>
{
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


    readonly TextBlockProperty _text = new("Text");
    readonly ValueProperty<bool> _throwError = new("ThrowError", "Throw Error");

    public string Text { get => _text.Text; set => _text.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _text.Sync(sync);
        _throwError.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _text.InspectorField(setup);
        _throwError.InspectorField(setup);
    }

    public override async Task<Output> Run(ToolCallContext context)
    {
        if (_throwError.Value)
        {
            throw new System.Exception("TestTool error");
        }

        context.Conversation.AddMessage("Handle");

        return new Output
        {
            Text = "Handle: " + Text,
        };
    }

}