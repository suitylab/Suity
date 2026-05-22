using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("RewriteEntireFile", CodeBase = "*Suity")]
[DisplayText("Rewrite Entire File")]
[ToolTipsText("Completely overwrite an existing file with new content. Use as fallback when Diff or Replace operations fail.")]
public class RewriteEntireFile : ToolCommand<RewriteEntireFile.Output>
{
    public class Output : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _message = new("Message", "Message");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Message { get => _message.Text; set => _message.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _message.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _message.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} '{Message}'";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path to the target file.");
    readonly TextBlockProperty _newFullContent = new("NewFullContent", "New Full Content", "The complete new content to write to the file.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public string NewFullContent { get => _newFullContent.Text; set => _newFullContent.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _newFullContent.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _newFullContent.InspectorField(setup);
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

        File.WriteAllText(targetPath, NewFullContent);

        return Task.FromResult(new Output
        {
            FilePath = targetPath,
            Message = $"Successfully rewrote file: {targetPath}",
        });
    }
}