using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("ReadFile", CodeBase = "*Suity")]
[DisplayText("Read File")]
[ToolTipsText("Read file content, optionally limited to specific line range.")]
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
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path to the file to read.");
    readonly ValueProperty<int> _startLine = new("StartLine", "Start Line", 0, "The starting line number (1-based). 0 means read from the beginning.");
    readonly ValueProperty<int> _endLine = new("EndLine", "End Line", 0, "The ending line number (1-based). 0 means read to the end.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public int StartLine { get => _startLine.Value; set => _startLine.Value = value; }
    public int EndLine { get => _endLine.Value; set => _endLine.Value = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _startLine.Sync(sync);
        _endLine.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _startLine.InspectorField(setup);
        _endLine.InspectorField(setup);
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
        if (StartLine <= 0 && EndLine <= 0)
        {
            content = string.Join(Environment.NewLine, lines);
        }
        else
        {
            int start = StartLine <= 0 ? 0 : Math.Min(StartLine - 1, totalLines - 1);
            int end = EndLine <= 0 ? totalLines : Math.Min(EndLine, totalLines);
            content = string.Join(Environment.NewLine, lines, start, Math.Max(0, end - start));
        }

        return Task.FromResult(new Output
        {
            Content = content,
        });
    }
}