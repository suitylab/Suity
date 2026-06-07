using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchRenameFiles", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Batch Rename File")]
[ToolTipsText("Rename multiple files at once. Each item specifies the old path and new path.")]
[NativeAlias("Suity.Editor.AIGC.BatchRenameFiles")]
public class BatchRenameFiles : ToolCommand<BatchRenameFiles.Output>
{
    [NativeType("BatchRenameFiles.RenameItem", CodeBase = "*Suity")]
    public class RenameItem : SObjectController
    {
        readonly StringProperty _sourcePath = new("SourcePath", "Source Path", string.Empty, "The current file path.");
        readonly StringProperty _targetPath = new("TargetPath", "Target Path", string.Empty, "The new file path.");

        public string SourcePath { get => _sourcePath.Text; set => _sourcePath.Text = value; }
        public string TargetPath { get => _targetPath.Text; set => _targetPath.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _sourcePath.Sync(sync);
            _targetPath.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _sourcePath.InspectorField(setup);
            _targetPath.InspectorField(setup);
        }
        public override string ToString() => $"{SourcePath} -> {TargetPath}";
    }

    [NativeType("BatchRenameFiles.RenameResult", CodeBase = "*Suity")]
    public class RenameResult : SObjectController
    {
        readonly StringProperty _sourcePath = new("SourcePath", "Source Path");
        readonly StringProperty _targetPath = new("TargetPath", "Target Path");
        readonly StringProperty _error = new("Error", "Error");

        public string SourcePath { get => _sourcePath.Text; set => _sourcePath.Text = value; }
        public string TargetPath { get => _targetPath.Text; set => _targetPath.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _sourcePath.Sync(sync);
            _targetPath.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _sourcePath.InspectorField(setup);
            _targetPath.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{SourcePath} -> {TargetPath} ({(HasError ? $"Error: {Error}" : "Success")})";
    }

    public class Output : SObjectController
    {
        readonly ListProperty<RenameResult> _results = new("Results", "Results");

        public List<RenameResult> Results => _results.List;

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _results.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

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
        var parentPage = context.ToolInstance.GetParentTask() as IAigcWorkflowPage;

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
                string relativeSourcePath = item.SourcePath.TrimStart('/', '\\');
                string relativeTargetPath = item.TargetPath.TrimStart('/', '\\');

                string fullSourcePath = relativeSourcePath;
                string fullTargetPath = relativeTargetPath;

                if (!Path.IsPathRooted(relativeSourcePath))
                {
                    fullSourcePath = Path.Combine(workspaceDir, relativeSourcePath);
                }

                if (!Path.IsPathRooted(relativeTargetPath))
                {
                    fullTargetPath = Path.Combine(workspaceDir, relativeTargetPath);
                }

                result.SourcePath = relativeSourcePath;
                result.TargetPath = relativeTargetPath;

                if (!File.Exists(fullSourcePath))
                {
                    result.Error = "Source file not found";
                }
                else if (File.Exists(fullTargetPath))
                {
                    result.Error = "Target file already exists";
                }
                else
                {
                    string targetDir = Path.GetDirectoryName(fullTargetPath);
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    File.Move(fullSourcePath, fullTargetPath);
                    parentPage?.RemoveScratchPad(relativeSourcePath);
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