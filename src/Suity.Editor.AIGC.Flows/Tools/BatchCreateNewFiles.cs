using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("BatchCreateNewFiles", CodeBase = "*Suity")]
[DisplayText("Batch Create New Files")]
[ToolTipsText("Create multiple new files at once. Use when initializing new modules, scaffolding, or creating multi-layer architecture code.")]
public class BatchCreateNewFiles : ToolCommand<BatchCreateNewFiles.Output>
{
    [NativeType("BatchCreateNewFiles.FileWriteItem", CodeBase = "*Suity")]
    public class FileWriteItem : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly TextBlockProperty _content = new("Content", "Content");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Content { get => _content.Text; set => _content.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _content.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _content.InspectorField(setup);
        }
    }

    public class FileResult : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _status = new("Status", "Status");
        readonly StringProperty _error = new("Error", "Error");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Status { get => _status.Text; set => _status.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrEmpty(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _status.Sync(sync);
            _error.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _status.InspectorField(setup);
            _error.InspectorField(setup);
        }
    }

    public class Output : IViewObject
    {
        readonly ListProperty<FileResult> _results = new("Results", "Results");
        readonly ValueProperty<int> _successCount = new("SuccessCount", "Success Count");
        readonly ValueProperty<int> _failCount = new("FailCount", "Fail Count");

        public List<FileResult> Results => _results.List;
        public int SuccessCount { get => _successCount.Value; set => _successCount.Value = value; }
        public int FailCount { get => _failCount.Value; set => _failCount.Value = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _results.Sync(sync);
            _successCount.Sync(sync);
            _failCount.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _results.InspectorField(setup);
            _successCount.InspectorField(setup);
            _failCount.InspectorField(setup);
        }
    }

    readonly ListProperty<FileWriteItem> _files = new("Files", "Files", "List of files to create.");

    public List<FileWriteItem> Files => _files.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _files.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _files.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();
        int successCount = 0;
        int failCount = 0;

        foreach (var item in Files)
        {
            var result = new FileResult { FilePath = item.FilePath };

            try
            {
                string targetPath = item.FilePath;

                if (!Path.IsPathRooted(targetPath))
                {
                    targetPath = Path.Combine(workspaceDir, targetPath);
                }

                if (File.Exists(targetPath))
                {
                    result.Status = "Failed";
                    result.Error = "File already exists";
                    failCount++;
                }
                else
                {
                    string dir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(targetPath, item.Content);
                    result.Status = "Created";
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                result.Status = "Failed";
                result.Error = ex.Message;
                failCount++;
            }

            output.Results.Add(result);
        }

        output.SuccessCount = successCount;
        output.FailCount = failCount;

        return Task.FromResult(output);
    }
}