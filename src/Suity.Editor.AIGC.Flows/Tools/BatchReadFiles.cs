using Suity.Collections;
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

[NativeType("BatchReadFile", CodeBase = "*Suity", Category = "WorkSpace")]
[DisplayText("Batch Read File")]
[ToolTipsText("Read multiple files at once. More efficient than reading files one by one when Agent needs to compare multiple files or process search results.")]
[NativeAlias("Suity.Editor.AIGC.BatchReadFiles")]
public class BatchReadFiles : ToolCommand<BatchReadFiles.Output>
{
[NativeType("BatchReadFiles.FileReadItem", CodeBase = "*Suity")]
    public class FileReadItem : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path", string.Empty, "The absolute or relative path to the file to read.");
        readonly ValueProperty<int> _startLine = new("StartLine", "Start Line", 0, "The starting line number (1-based). 0 means read from the beginning.");
        readonly ValueProperty<int> _lineCount = new("LineCount", "Line Count", 0, "Number of lines to read. 0 means read to the end.");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public int StartLine { get => _startLine.Value; set => _startLine.Value = value; }
        public int LineCount { get => _lineCount.Value; set => _lineCount.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _startLine.Sync(sync);
            _lineCount.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _startLine.InspectorField(setup);
            _lineCount.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} (StartLine: {StartLine}, LineCount: {LineCount})";
    }

    [NativeType("BatchReadFiles.FileResult", CodeBase = "*Suity")]
    public class FileResult : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _message = new("Message");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Message { get => _message.Text; set => _message.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _message.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _message.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} ({Message})";
    }

    public class Output : SObjectController
    {
        readonly ListProperty<FileResult> _results = new("Results", "Results");

        public List<FileResult> Results => _results.List;

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
        var parentPage = context.ToolInstance.GetParentTask() as IAigcWorkflowPage;

        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();

        foreach (var item in FileItems.SkipNull())
        {
            string relativePath = item.FilePath?.TrimStart('/', '\\');
            var result = new FileResult { FilePath = relativePath };

            var lastScratchPad = parentPage?.GetHistoryScratchPad(relativePath);
            if (lastScratchPad != null)
            {
                // The lastest full content of the file is already in the scratch pad,
                // we can skip reading the file again, and just return the result with a message to indicate that the content is in the scratch pad.
                if (lastScratchPad.Type == ScratchPadTypes.FileFullContent)
                {
                    result.Message = $"read successful. See ScratchPad for full content.";
                    output.Results.Add(result);
                    continue;
                }
            }

            try
            {
                string fullPath = relativePath;

                if (!Path.IsPathRooted(relativePath))
                {
                    fullPath = Path.Combine(workspaceDir, relativePath);
                }

                if (!File.Exists(fullPath))
                {
                    result.Message = "File not found";
                    parentPage?.RemoveScratchPad(relativePath);
                }
                else
                {
                    int startLine = item.StartLine;
                    int lineCount = item.LineCount;

                    if (startLine <= 0 && lineCount <= 0)
                    {
                        //content = string.Join(Environment.NewLine, lines);
                        result.Message = "read successful, see ScratchPad for detail.";
                        parentPage?.SetScratchPad(ScratchPadTypes.FileFullContent, relativePath);
                    }
                    else
                    {
                        var lines = File.ReadAllLines(fullPath);
                        int totalLines = lines.Length;
                        string content;

                        int start = startLine <= 0 ? 0 : Math.Min(startLine - 1, totalLines - 1);
                        int count = lineCount <= 0 ? totalLines - start : Math.Min(lineCount, totalLines - start);
                        content = string.Join(Environment.NewLine, lines, start, count);
                        string msg = $"start line: {startLine}, line count: {lineCount}";
                        result.Message = $"read successful. {msg}, see ScratchPad for detail.";

                        // If there is a previous FileSegment scratch pad,
                        // it means the user has read a segment of the file before,
                        // we will append the new content to the previous content,
                        // so that user can read the file in segments and keep all the content in the scratch pad.
                        if (lastScratchPad?.Type == ScratchPadTypes.FileSegment)
                        {
                            string previousContent = lastScratchPad.Content;
                            if (!string.IsNullOrEmpty(previousContent))
                            {
                                content = previousContent + Environment.NewLine + content;
                            }
                        }

                        parentPage?.SetScratchPad(ScratchPadTypes.FileSegment, relativePath, content, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                parentPage?.RemoveScratchPad(relativePath);
                result.Message = ex.Message;
            }

            output.Results.Add(result);
        }

        return Task.FromResult(output);
    }
}