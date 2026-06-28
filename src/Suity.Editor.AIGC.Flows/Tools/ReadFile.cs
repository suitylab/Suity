using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("ReadFile", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Read File")]
[ToolTipsText("Read file content, optionally starting from a line and reading a specified number of lines.")]
[NativeAlias("Suity.Editor.AIGC.ReadFile")]
public class ReadFile : ToolCommand<ReadFile.Output>
{
    public class Output : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The file path that was read.");
        readonly StringProperty _message = new("Message");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Message { get => _message.Text; set => _message.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _filePath.Sync(sync);
            _message.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _filePath.InspectorField(setup);
            _message.InspectorField(setup);
        }

        public override string ToString() => Message;
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path to the file to read.");
    readonly ValueProperty<int> _startLine = new("StartLine", "Start Line", 0, "The starting line number (1-based). 0 means read from the beginning.");
    readonly ValueProperty<int> _lineCount = new("LineCount", "Line Count", 0, "Number of lines to read. 0 means read to the end.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public int StartLine { get => _startLine.Value; set => _startLine.Value = value; }
    public int LineCount { get => _lineCount.Value; set => _lineCount.Value = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _startLine.Sync(sync);
        _lineCount.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _startLine.InspectorField(setup);
        _lineCount.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        var parentPage = context.ToolInstance.GetParentTask() as IAigcWorkflowPage;

        string workspaceDir = context.RootDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
        }

        string relativePath = FilePath.TrimStart('/', '\\');
        var lastScratchPad = parentPage?.GetHistoryScratchPad(relativePath);

        var output = new Output();

        if (lastScratchPad?.Type == ScratchPadTypes.FileFullContent)
        {
            output.FilePath = relativePath;
            output.Message = "read successful. See ScratchPad for full content.";
            return Task.FromResult(output);
        }

        string fullPath = relativePath;

        if (!Path.IsPathRooted(relativePath))
        {
            fullPath = Path.Combine(workspaceDir, relativePath);
        }

        string readInfo = relativePath;
        if (StartLine > 0 || LineCount > 0)
        {
            readInfo += $" (startLine: {StartLine}, lineCount: {LineCount})";
        }

        context.ToolInstance.Conversation?.AddRunningMessage("Read file", msg =>
        {
            msg.AddCode(readInfo);
        });
        context.Conversation?.AddRunningMessage("Read file", msg =>
        {
            msg.AddCode(readInfo);
        });

        int startLine = StartLine;
        int lineCount = LineCount;
        if (startLine == 1 && lineCount <= 0)
        {
            startLine = 0;
            lineCount = 0;
        }

        if (!File.Exists(fullPath))
        {
            parentPage?.RemoveScratchPad(relativePath);
            output.FilePath = fullPath;
            output.Message = "File not found";
        }
        else
        {
            if (startLine <= 0 && lineCount <= 0)
            {
                //content = string.Join(Environment.NewLine, lines);
                output.FilePath = fullPath;
                output.Message = "read successful, see ScratchPad for detail.";
                parentPage?.SetScratchPad(ScratchPadTypes.FileFullContent, relativePath);
            }
            else
            {
                var lines = File.ReadAllLines(fullPath);
                int totalLines = lines.Length;
                string content;

                int start = startLine <= 0 ? 0 : Math.Min(startLine - 1, totalLines - 1);
                int count = lineCount <= 0 ? totalLines - start : Math.Min(lineCount, totalLines - start);
                
                var linesWithNumbers = new string[count];
                for (int i = 0; i < count; i++)
                {
                    linesWithNumbers[i] = $"{start + i + 1}: {lines[start + i]}";
                }
                content = string.Join(Environment.NewLine, linesWithNumbers);
                string msg = $"start line: {startLine}, line count: {lineCount}";

                if (lastScratchPad?.Type == ScratchPadTypes.FileSegment)
                {
                    string previousContent = lastScratchPad.Content;
                    if (!string.IsNullOrEmpty(previousContent))
                    {
                        string header = $"========== FILE SECTION: start line: {startLine}, line count: {count} ==========";
                        if (!string.IsNullOrEmpty(lastScratchPad.Note))
                        {
                            content = $"========== FILE SECTION: {lastScratchPad.Note} =========={Environment.NewLine}{previousContent}{Environment.NewLine}{Environment.NewLine}{header}{Environment.NewLine}{content}";
                        }
                        else
                        {
                            content = previousContent + Environment.NewLine + Environment.NewLine + header + Environment.NewLine + content;
                        }
                    }
                }

                output.FilePath = fullPath;
                output.Message = $"read successful. {msg}, see ScratchPad for detail.";
                bool merged = lastScratchPad?.Type == ScratchPadTypes.FileSegment && !string.IsNullOrEmpty(lastScratchPad.Content);
                parentPage?.SetScratchPad(ScratchPadTypes.FileSegment, relativePath, content, merged ? "Contains multiple sections (sections start with: ========== FILE SECTION: ...)" : msg);
            }
        }

        return Task.FromResult(output);
    }
}
