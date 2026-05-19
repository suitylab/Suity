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

[NativeType("BatchReadFile", CodeBase = "*Suity")]
[DisplayText("Batch Read File")]
[ToolTipsText("Read multiple files at once. More efficient than reading files one by one when Agent needs to compare multiple files or process search results.")]
public class BatchReadFiles : ToolCommand<BatchReadFiles.Output>
{
    public class FileResult : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly TextBlockProperty _content = new("Content");
        readonly StringProperty _error = new("Error", "Error");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Content { get => _content.Text; set => _content.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrEmpty(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _content.Sync(sync);
            _error.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _content.InspectorField(setup);
            _error.InspectorField(setup);
        }
    }

    public class Output : IViewObject
    {
        readonly ListProperty<FileResult> _results = new("Results", "Results");

        public List<FileResult> Results => _results.List;

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _results.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _results.InspectorField(setup);
        }
    }

    readonly ListProperty<string> _filePaths = new("FilePaths", "File Paths", "List of file paths to read.");

    public List<string> FilePaths => _filePaths.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePaths.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePaths.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();

        foreach (var filePath in FilePaths)
        {
            var result = new FileResult { FilePath = filePath };

            try
            {
                string targetPath = filePath;

                if (!Path.IsPathRooted(targetPath))
                {
                    targetPath = Path.Combine(workspaceDir, targetPath);
                }

                if (!File.Exists(targetPath))
                {
                    result.Error = $"File not found";
                }
                else
                {
                    result.Content = File.ReadAllText(targetPath);
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