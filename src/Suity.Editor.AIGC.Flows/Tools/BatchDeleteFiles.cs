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

[NativeType("BatchDeleteFiles", CodeBase = "*Suity", Category = "WorkSpace")]
[DisplayText("Batch Delete File")]
[ToolTipsText("Delete multiple files at once.")]
[NativeAlias("Suity.Editor.AIGC.BatchDeleteFiles")]
public class BatchDeleteFiles : ToolCommand<BatchDeleteFiles.Output>
{
    [NativeType("BatchDeleteFiles.DeleteItem", CodeBase = "*Suity")]
    public class DeleteItem : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path", string.Empty, "The file path to delete.");
        readonly ValueProperty<bool> _verifyExists = new("VerifyExists", "Verify Exists", true, "Check if file exists before deleting.");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public bool VerifyExists { get => _verifyExists.Value; set => _verifyExists.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _verifyExists.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _verifyExists.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath}";
    }

    [NativeType("BatchDeleteFiles.DeleteResult", CodeBase = "*Suity")]
    public class DeleteResult : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _error = new("Error", "Error");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} ({(HasError ? $"Error: {Error}" : "Deleted")})";
    }

    public class Output : IViewObject
    {
        readonly ListProperty<DeleteResult> _results = new("Results", "Results");

        public List<DeleteResult> Results => _results.List;

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _results.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _results.InspectorField(setup);
        }
        public override string ToString() => $"Batch Delete {Results.Count} files";
    }

    readonly ListProperty<DeleteItem> _deleteItems = new("DeleteItems", "Delete Items", "List of files to delete.");

    public List<DeleteItem> DeleteItems => _deleteItems.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _deleteItems.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _deleteItems.InspectorField(setup);
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

        foreach (var item in DeleteItems)
        {
            var result = new DeleteResult { FilePath = item.FilePath };

            try
            {
                string relativePath = item.FilePath.TrimStart('/', '\\');
                string fullPath = relativePath;

                if (!Path.IsPathRooted(relativePath))
                {
                    fullPath = Path.Combine(workspaceDir, relativePath);
                }

                result.FilePath = relativePath;

                if (item.VerifyExists && !File.Exists(fullPath))
                {
                    result.Error = "File not found";
                }
                else
                {
                    File.Delete(fullPath);
                    parentPage?.RemoveScratchPad(relativePath);
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