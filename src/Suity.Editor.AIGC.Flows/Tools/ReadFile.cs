using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("ReadFile", CodeBase = "*Suity")]
[DisplayText("Read File")]
[ToolTipsText("Read file content, optionally starting from a line and reading a specified number of lines.")]
[NativeAlias("Suity.Editor.AIGC.ReadFile")]
public class ReadFile : ToolCommand<ReadFile.Output>
{
    public class Output : IViewObject
    {
        readonly TextBlockProperty _content = new("Content");

        public string Content { get => _content.Text; set => _content.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _content.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _content.InspectorField(setup);
        }
        public override string ToString() => $"{Content?.Length ?? 0} chars";
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
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
        }

        string targetPath = FilePath;

        if (!Path.IsPathRooted(targetPath))
        {
            targetPath = Path.Combine(workspaceDir, targetPath);
        }

        if (!File.Exists(targetPath))
        {
            throw new FileNotFoundException($"File not found: {targetPath}");
        }

        var lines = File.ReadAllLines(targetPath);
        int totalLines = lines.Length;

        string content;
        if (StartLine <= 0 && LineCount <= 0)
        {
            content = string.Join(Environment.NewLine, lines);
        }
        else
        {
            int start = StartLine <= 0 ? 0 : Math.Min(StartLine - 1, totalLines - 1);
            int count = LineCount <= 0 ? totalLines - start : Math.Min(LineCount, totalLines - start);
            content = string.Join(Environment.NewLine, lines, start, count);
        }

        return Task.FromResult(new Output
        {
            Content = content,
        });
    }
}