using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("RestoreWorkspace", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Restore Workspace")]
[ToolTipsText("Restore workspace from a backup file. Returns the restore execution result message.")]
[NativeAlias("Suity.Editor.AIGC.RestoreWorkspace")]
public class RestoreWorkspace : ToolCommand<RestoreWorkspace.Output>
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

        public override string ToString() => $"Restore result: {Message}";
    }

    readonly StringProperty _backupName = new("BackupName", "Backup Name", string.Empty, "The backup file name to restore. Leave empty to use the latest backup file.");

    public string BackupName { get => _backupName.Text; set => _backupName.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _backupName.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _backupName.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        var workspace = context.WorkSpace;
        if (workspace is null)
        {
            throw new NullReferenceException("Workspace is null.");
        }

        if (string.IsNullOrWhiteSpace(BackupName))
        {
            var backupNames = workspace.GetBackupNames();
            if (backupNames == null || backupNames.Length == 0)
            {
                return Task.FromResult(new Output
                {
                    Message = "Restore failed. No backup file found.",
                });
            }
        }

        string backupName = BackupName?.Trim();
        try
        {
            bool success = workspace.RestoreWorkspace(string.IsNullOrWhiteSpace(backupName) ? null : backupName);

            return Task.FromResult(new Output
            {
                Message = success ? $"Restore completed: {backupName ?? "latest"}" : "Restore failed. Backup file not found.",
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new Output
            {
                Message = $"Restore failed: {ex.Message}",
            });
        }
    }
}