using Suity.Editor.AIGC.StreamUpdaters;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.Flows.Nodes;

[NativeType("RunShellCommand", CodeBase = "*Suity")]
[DisplayText("Run shell command")]
[ToolTipsText("Run a single shell command. Do not run multiple commands and a same time.")]
public class RunShellCommand : ToolCommand<RunShellCommand.Output>
{
    public class Output : IViewObject
    {
        readonly TextBlockProperty _result = new("Result");

        public string Result { get => _result.Text; set => _result.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _result.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _result.InspectorField(setup);
        }
    }

    readonly TextBlockProperty _command = new("Command");

    public string Command { get => _command.Text; set => _command.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _command.Sync(sync);
    }
    public override void SetupView(IViewObjectSetup setup)
    {
        _command.InspectorField(setup);
    }

    public override async Task<Output> Run(ToolCallContext context)
    {
        string workingDirectory = context.WorkingDirectory;
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new NullReferenceException("Working directory is not set");
        }

        string command = this.Command;
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new NullReferenceException("Command is not set");
        }

        SimpleStreamUpdater? updater = null;
        Action<string>? onOutput = null;

        if (context.ToolInstance.Conversation != null)
        {
            updater = new SimpleStreamUpdater { Conversation = context.ToolInstance.Conversation };
            onOutput = updater.Append;
        }

        try
        {
            context.ToolInstance.Conversation?.AddRunningMessage("Run command", msg => 
            {
                msg.AddCode(command);
            });

            string output = await RunShellCommandNode.ExecuteCommandAsync(command, workingDirectory, onOutput, context.Cancellation);
            return new Output
            {
                Result = output,
            };
        }
        finally
        {
            updater?.Dispose();
        }
    }
}
