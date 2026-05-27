using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchRenameFiles", CodeBase = "*Suity")]
[DisplayText("Batch Rename File")]
[ToolTipsText("Rename multiple files at once. Each item specifies the old path and new path.")]
[NativeAlias("Suity.Editor.AIGC.BatchRenameFiles")]
public class BatchRenameFiles : ToolCommand<BatchRenameFiles.Output>
{
    [NativeType("BatchRenameFiles.RenameItem", CodeBase = "*Suity")]
    public class RenameItem : IViewObject
    {
        readonly StringProperty _sourcePath = new("SourcePath", "Source Path", string.Empty, "The current file path.");
        readonly StringProperty _targetPath = new("TargetPath", "Target Path", string.Empty, "The new file path.");

        public string SourcePath { get => _sourcePath.Text; set => _sourcePath.Text = value; }
        public string TargetPath { get => _targetPath.Text; set => _targetPath.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _sourcePath.Sync(sync);
            _targetPath.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _sourcePath.InspectorField(setup);
            _targetPath.InspectorField(setup);
        }
        public override string ToString() => $"{SourcePath} -> {TargetPath}";
    }

    [NativeType("BatchRenameFiles.RenameResult", CodeBase = "*Suity")]
    public class RenameResult : IViewObject
    {
        readonly StringProperty _sourcePath = new("SourcePath", "Source Path");
        readonly StringProperty _targetPath = new("TargetPath", "Target Path");
        readonly StringProperty _error = new("Error", "Error");

        public string SourcePath { get => _sourcePath.Text; set => _sourcePath.Text = value; }
        public string TargetPath { get => _targetPath.Text; set => _targetPath.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _sourcePath.Sync(sync);
            _targetPath.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _sourcePath.InspectorField(setup);
            _targetPath.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{SourcePath} -> {TargetPath} ({(HasError ? $"Error: {Error}" : "Success")})";
    }

    public class Output : IViewObject
    {
        readonly ListProperty<RenameResult> _results = new("Results", "Results");

        public List<RenameResult> Results => _results.List;

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _results.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _results.InspectorField(setup);
        }
        public override string ToString() => $"Batch Rename {Results.Count} files";
    }

    readonly ListProperty<RenameItem> _renameItems = new("RenameItems", "Rename Items", "List of files to rename.");

    public List<RenameItem> RenameItems => _renameItems.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _renameItems.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _renameItems.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();

        foreach (var item in RenameItems)
        {
            var result = new RenameResult { SourcePath = item.SourcePath, TargetPath = item.TargetPath };

            try
            {
                string sourcePath = item.SourcePath.TrimStart('/', '\\');

                if (!Path.IsPathRooted(sourcePath))
                {
                    sourcePath = Path.Combine(workspaceDir, sourcePath);
                }

                string targetPath = item.TargetPath.TrimStart('/', '\\');

                if (!Path.IsPathRooted(targetPath))
                {
                    targetPath = Path.Combine(workspaceDir, targetPath);
                }

                if (!File.Exists(sourcePath))
                {
                    result.Error = "Source file not found";
                }
                else if (File.Exists(targetPath))
                {
                    result.Error = "Target file already exists";
                }
                else
                {
                    string targetDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    File.Move(sourcePath, targetPath);
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            output.Results.Add(result);
        }

        return Task.FromResult(output);
    }
}