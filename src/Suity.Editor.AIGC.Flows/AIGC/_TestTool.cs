using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("TestTool", CodeBase = "*Suity")]
[AssetAutoCreate]
public class TestTool : ToolAsset<TestTool.TestInput, TestTool.TestOutput>
{
    public class TestInput : IViewObject
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

    public class TestOutput : IViewObject
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

    protected override async Task<TestOutput> RunTask(TestInput input, CancellationToken cancellation)
    {
        return new TestOutput
        {
            Text = "Handle: " + input?.Text,
        };
    }

}