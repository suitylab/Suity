using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchReadFile", CodeBase = "*Suity")]
[DisplayText("Batch Read File")]
[ToolTipsText("Read multiple files at once. More efficient than reading files one by one when Agent needs to compare multiple files or process search results.")]
[NativeAlias("Suity.Editor.AIGC.BatchReadFiles")]
public class BatchReadFiles : ToolCommand<BatchReadFiles.Output>
{
    [NativeType("BatchReadFiles.FileReadItem", CodeBase = "*Suity")]
    public class FileReadItem : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path", string.Empty, "The absolute or relative path to the file to read.");
        readonly ValueProperty<int> _startLine = new("StartLine", "Start Line", 0, "The starting line number (1-based). 0 means read from the beginning.");
        readonly ValueProperty<int> _lineCount = new("LineCount", "Line Count", 0, "Number of lines to read. 0 means read to the end.");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public int StartLine { get => _startLine.Value; set => _startLine.Value = value; }
        public int LineCount { get => _lineCount.Value; set => _lineCount.Value = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _startLine.Sync(sync);
            _lineCount.Sync(sync);
        }
public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _startLine.InspectorField(setup);
            _lineCount.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} (StartLine: {StartLine}, LineCount: {LineCount})";
    }

    [NativeType("BatchReadFiles.FileResult", CodeBase = "*Suity")]
    public class FileResult : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly TextBlockProperty _content = new("Content");
        readonly StringProperty _error = new("Error", "Error");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Content { get => _content.Text; set => _content.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _content.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _content.InspectorField(setup);
            _error.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} ({(HasError ? $"Error: {Error}" : $"{Content?.Length ?? 0} chars")})";
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
        public override string ToString() => $"Batch Read {Results.Count} files";
    }

    readonly ListProperty<FileReadItem> _fileItems = new("FileItems", "File Items", "List of file items to read with optional line range.");

    public List<FileReadItem> FileItems => _fileItems.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _fileItems.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _fileItems.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();

        foreach (var item in FileItems)
        {
            var result = new FileResult { FilePath = item.FilePath };

            try
            {
string targetPath = item.FilePath.TrimStart('/', '\\');

                if (!Path.IsPathRooted(targetPath))
                {
                    targetPath = Path.Combine(workspaceDir, targetPath);
                }

                if (!File.Exists(targetPath))
                {
                    result.Error = "File not found";
                }
                else
                {
                    var lines = File.ReadAllLines(targetPath);
                    int totalLines = lines.Length;

                    string content;
                    int startLine = item.StartLine;
                    int lineCount = item.LineCount;

                    if (startLine <= 0 && lineCount <= 0)
                    {
                        content = string.Join(Environment.NewLine, lines);
                    }
                    else
                    {
                        int start = startLine <= 0 ? 0 : Math.Min(startLine - 1, totalLines - 1);
                        int count = lineCount <= 0 ? totalLines - start : Math.Min(lineCount, totalLines - start);
                        content = string.Join(Environment.NewLine, lines, start, count);
                    }

                    result.Content = content;
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