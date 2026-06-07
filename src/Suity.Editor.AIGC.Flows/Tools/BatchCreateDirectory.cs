using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchCreateDirectory", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Batch Create Directory")]
[ToolTipsText("Create multiple directories at once. Use when initializing new module structures or creating multi-layer architecture folders.")]
[NativeAlias("Suity.Editor.AIGC.BatchCreateDirectory")]
public class BatchCreateDirectory : ToolCommand<BatchCreateDirectory.Output>
{
    [NativeType("BatchCreateDirectory.DirectoryItem", CodeBase = "*Suity")]
    public class DirectoryItem : SObjectController
    {
        readonly StringProperty _directoryPath = new("DirectoryPath", "Directory Path");

        public string DirectoryPath { get => _directoryPath.Text; set => _directoryPath.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _directoryPath.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _directoryPath.InspectorField(setup);
        }
        public override string ToString() => DirectoryPath;
    }

    [NativeType("BatchCreateDirectory.DirectoryResult", CodeBase = "*Suity")]
    public class DirectoryResult : SObjectController
    {
        readonly StringProperty _directoryPath = new("DirectoryPath", "Directory Path");
        readonly StringProperty _status = new("Status", "Status");
        readonly StringProperty _error = new("Error", "Error");

        public string DirectoryPath { get => _directoryPath.Text; set => _directoryPath.Text = value; }
        public string Status { get => _status.Text; set => _status.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _directoryPath.Sync(sync);
            _status.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _directoryPath.InspectorField(setup);
            _status.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{DirectoryPath} [{Status}]" + (HasError ? $" - Error: {Error}" : "");
    }

    public class Output : SObjectController
    {
        readonly ListProperty<DirectoryResult> _results = new("Results", "Results");
        readonly ValueProperty<int> _successCount = new("SuccessCount", "Success Count");
        readonly ValueProperty<int> _failCount = new("FailCount", "Fail Count");

        public List<DirectoryResult> Results => _results.List;
        public int SuccessCount { get => _successCount.Value; set => _successCount.Value = value; }
        public int FailCount { get => _failCount.Value; set => _failCount.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _results.Sync(sync);
            _successCount.Sync(sync);
            _failCount.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _results.InspectorField(setup);
            _successCount.InspectorField(setup);
            _failCount.InspectorField(setup);
        }

        public override string ToString() => $"Results: {SuccessCount} created, {FailCount} failed ({Results.Count} items)";
    }

    readonly ListProperty<DirectoryItem> _directories = new("Directories", "Directories", "List of directories to create.");

    public List<DirectoryItem> Directories => _directories.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _directories.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _directories.InspectorField(setup);
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

        var dirNames = string.Join(", ", Directories.Select(d => d.DirectoryPath));
        context.ToolInstance.Conversation?.AddRunningMessage($"Create {Directories.Count} directory(ies)", msg =>
        {
            msg.AddCode(dirNames);
        });
        context.Conversation?.AddRunningMessage($"Create {Directories.Count} directory(ies)", msg =>
        {
            msg.AddCode(dirNames);
        });

        foreach (var item in Directories)
        {
            var result = new DirectoryResult { DirectoryPath = item.DirectoryPath };

            try
            {
                string relativePath = item.DirectoryPath.TrimStart('/', '\\');
                string fullPath = relativePath;

                if (!Path.IsPathRooted(relativePath))
                {
                    fullPath = Path.Combine(workspaceDir, relativePath);
                }

                result.DirectoryPath = relativePath;

                if (Directory.Exists(fullPath))
                {
                    result.Status = "Failed";
                    result.Error = "Directory already exists";
                }
                else
                {
                    Directory.CreateDirectory(fullPath);
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