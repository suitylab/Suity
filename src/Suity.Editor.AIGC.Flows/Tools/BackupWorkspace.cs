using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BackupWorkspace", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Backup Workspace")]
[ToolTipsText("Backup project folder/files. Returns the backup execution result message.")]
[NativeAlias("Suity.Editor.AIGC.BackupWorkspace")]
public class BackupWorkspace : ToolCommand<BackupWorkspace.Output>
{
    public class Output : SObjectController
    {
        readonly TextBlockProperty _message = new("Message");

        public string Message { get => _message.Text; set => _message.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _message.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _message.InspectorField(setup);
        }

        public override string ToString() => $"Backup result: {Message}";
    }

    readonly StringProperty _backupName = new("BackupName", "Backup Name", string.Empty, "The backup file name. Use PascalCase format. Leave empty to use a default generated name.");
    readonly StringProperty _ignorePatterns = new("IgnorePatterns", "Ignore Patterns", string.Empty, "Comma or semicolon separated list of patterns to ignore.");

    public string BackupName { get => _backupName.Text; set => _backupName.Text = value; }
    public string IgnorePatterns { get => _ignorePatterns.Text; set => _ignorePatterns.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _backupName.Sync(sync);
        _ignorePatterns.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _backupName.InspectorField(setup);
        _ignorePatterns.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        var workspace = context.WorkSpace;
        if (workspace is null)
        {
            throw new NullReferenceException("Workspace is null.");
        }

        string backupName = BackupName?.Trim();
        workspace.BackupWorkspace(IgnorePatterns, string.IsNullOrWhiteSpace(backupName) ? null : backupName);

        return Task.FromResult(new Output
        {
            Message = "Backup completed.",
        });
    }
}
