using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("ManualTest", CodeBase = "*Suity", Icon = "*CoreIcon|System", Category = "WorkSpace Tools")]
[DisplayText("Manual test")]
[ToolTipsText("Do not run the test command in the editor. Pause and wait for manual test of the provided test content, the test command is executed in an external console window via the Run Test button, then output the reply result.")]
public class ManualTest : ToolCommand<ManualTest.Output>
{
    public class Output : SObjectController
    {
        readonly TextBlockProperty _result = new("Result", toolTips: "The reply result of the manual test.");

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

    readonly TextBlockProperty _testContent = new("TestContent", toolTips: "The test content that needs to be manually tested.");

    public string TestContent { get => _testContent.Text; set => _testContent.Text = value; }

    readonly TextBlockProperty _shellCommand = new("ShellCommand", toolTips: "The shell command to execute in an external console window for testing.");

    public string ShellCommand { get => _shellCommand.Text; set => _shellCommand.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _testContent.Sync(sync);
        _shellCommand.Sync(sync);
    }
    public override void SetupView(IViewObjectSetup setup)
    {
        _testContent.InspectorField(setup);
        _shellCommand.InspectorField(setup);
    }

    public override async Task<Output> Run(ToolCallContext context)
    {
        string testContent = this.TestContent;
        if (string.IsNullOrWhiteSpace(testContent))
        {
            throw new NullReferenceException("TestContent is not set");
        }

        string shellCommand = this.ShellCommand;
        if (string.IsNullOrWhiteSpace(shellCommand))
        {
            throw new NullReferenceException("ShellCommand is not set");
        }

        context?.AddToolMessage("Manual test", msg =>
        {
            msg.AddCode(testContent);
            msg.AddText("Test command:");
            msg.AddCode(shellCommand);
            msg.AddButton("RunTest", "Run Test");
        });

        IConversation conversation = context?.Conversation;
        if (conversation is null)
        {
            conversation = context?.ToolInstance?.Conversation;
        }
        if (conversation is null)
        {
            throw new NullReferenceException("Conversation is not found");
        }

        conversation.AddInfoMessage("Please enter your test result in the input field at the bottom.");
        while (true)
        {
            await conversation.WaitForInput(context.Cancellation);
            if (conversation.InputMessage is { } result && !string.IsNullOrWhiteSpace(result))
            {
                return new Output
                {
                    Result = result,
                };
            }
            else if (conversation.InputButton == "RunTest")
            {
                RunTest(context, shellCommand);
            }
        }
    }

    void RunTest(ToolCallContext context, string command)
    {
        string directory = context?.RootDirectory;
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        string shell = isWindows ? "cmd.exe" : "/bin/bash";
        string arguments = isWindows ? $"/K {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = arguments,
            WorkingDirectory = directory,
            UseShellExecute = true,
        };

        Process.Start(startInfo);
    }
}
