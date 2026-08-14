using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        string workspaceDir = workspace.MasterDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        string backupDir = workspaceDir.PathAppend("Backup");

        if (!string.IsNullOrWhiteSpace(BackupName))
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            if (BackupName.Any(c => invalidChars.Contains(c))
                || BackupName.EndsWith(".") || BackupName.EndsWith(" ") || BackupName.EndsWith("\t"))
            {
                throw new ArgumentException($"Invalid backup name: {BackupName}");
            }
        }

        string id = IdGenerator.GenerateId(12);
        string backupFileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}_{id}";
        string backupName = BackupName?.Trim();
        if (!string.IsNullOrWhiteSpace(backupName))
        {
            backupFileName = backupFileName + "_" + backupName;
        }
        string backupPath = backupDir.PathAppend(backupFileName + ".zip");

        var ignoreSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Backup",
        };
        foreach (var pattern in IgnorePatterns.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
        {
            ignoreSet.Add(pattern.Trim());
        }

        Directory.CreateDirectory(backupDir);

        int fileCount;
        using (var fileStream = File.Create(backupPath))
        {
            using var zipStream = new ZipOutputStream(fileStream);
            zipStream.SetLevel(6);
            fileCount = AddDirectoryToZip(zipStream, workspaceDir, string.Empty, ignoreSet);
        }

        return Task.FromResult(new Output
        {
            Message = $"Backup completed: {backupFileName}.zip ({fileCount} files).",
        });
    }

    private int AddDirectoryToZip(ZipOutputStream zipStream, string sourceDir, string relativePath, HashSet<string> ignoreSet)
    {
        int fileCount = 0;

        var directories = new DirectoryInfo(sourceDir).GetDirectories()
            .Where(d => !ignoreSet.Contains(d.Name))
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var dir in directories)
        {
            string dirEntryName = relativePath + dir.Name + "/";
            zipStream.PutNextEntry(new ZipEntry(dirEntryName) { DateTime = dir.LastWriteTime });
            zipStream.CloseEntry();
            fileCount += AddDirectoryToZip(zipStream, dir.FullName, dirEntryName, ignoreSet);
        }

        var files = new DirectoryInfo(sourceDir).GetFiles()
            .Where(f => !ignoreSet.Contains(f.Name))
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            var entry = new ZipEntry(relativePath + file.Name)
            {
                DateTime = file.LastWriteTime,
                Size = file.Length,
            };
            zipStream.PutNextEntry(entry);
            using (var input = file.OpenRead())
            {
                input.CopyTo(zipStream);
            }
            zipStream.CloseEntry();
            fileCount++;
        }

        return fileCount;
    }
}
