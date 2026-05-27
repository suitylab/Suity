using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchWriteFile", CodeBase = "*Suity")]
[DisplayText("Batch Write File")]
[ToolTipsText("Write multiple files at once. Use when initializing new modules, scaffolding, or creating multi-layer architecture code.")]
[NativeAlias("Suity.Editor.AIGC.BatchCreateNewFiles")]
[NativeAlias("Suity.Editor.AIGC.Tools.BatchCreateNewFiles")]
public class BatchWriteFile : ToolCommand<BatchWriteFile.Output>
{
    [NativeType("BatchWriteFile.FileWriteItem", CodeBase = "*Suity")]
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
        public override string ToString() => $"{FilePath} ({Content?.Length ?? 0} chars)";
    }

    [NativeType("BatchWriteFile.FileResult", CodeBase = "*Suity")]
    public class FileResult : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _status = new("Status", "Status");
        readonly StringProperty _error = new("Error", "Error");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Status { get => _status.Text; set => _status.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _status.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _status.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} [{Status}]" + (HasError ? $" - Error: {Error}" : "");
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
        public override string ToString() => $"Results: {SuccessCount} created, {FailCount} failed ({Results.Count} items)";
    }

    readonly ListProperty<FileWriteItem> _files = new("Files", "Files", "List of files to create.");
    readonly ValueProperty<bool> _overwriteFile = new("OverwriteFile", "Overwrite File", true, "Overwrite files if they already exist, default is true.");

    public List<FileWriteItem> Files => _files.List;
    public bool OverwriteFile { get => _overwriteFile.Value; set => _overwriteFile.Value = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _files.Sync(sync);
        _overwriteFile.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _files.InspectorField(setup);
        _overwriteFile.InspectorField(setup);
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
                string relativePath = item.FilePath.TrimStart('/', '\\');
                string fullPath = relativePath;

                if (!Path.IsPathRooted(relativePath))
                {
                    fullPath = Path.Combine(workspaceDir, relativePath);
                }

                result.FilePath = relativePath;

                if (File.Exists(fullPath))
                {
                    if (OverwriteFile)
                    {
                        string dir = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        File.WriteAllText(fullPath, item.Content);
                        result.Status = "Overwritten";
                        successCount++;
                    }
                    else
                    {
                        result.Status = "Failed";
                        result.Error = "File already exists";
                        failCount++;
                    }
                }
                else
                {
                    string dir = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(fullPath, item.Content);
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