using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("CreateNewFile", CodeBase = "*Suity", Category = "WorkSpace")]
[DisplayText("Create New File")]
[ToolTipsText("Create a new file and write content to it. Fails if file already exists to prevent accidental overwriting.")]
[NativeAlias("Suity.Editor.AIGC.CreateNewFile")]
public class CreateNewFile : ToolCommand<CreateNewFile.Output>
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

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path for the new file to create.");
    readonly TextBlockProperty _content = new("Content", "Content", "The full content to write to the file.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public string Content { get => _content.Text; set => _content.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _content.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _content.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        var parentPage = context.ToolInstance.GetParentTask() as IAigcWorkflowPage;

        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
        }

        string relativePath = FilePath.TrimStart('/', '\\');
        string fullPath = relativePath;

        if (!Path.IsPathRooted(relativePath))
        {
            fullPath = Path.Combine(workspaceDir, relativePath);
        }

        if (File.Exists(fullPath))
        {
            throw new InvalidOperationException($"File already exists: {relativePath}. Use a modify tool instead to update existing files.");
        }

        string dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(fullPath, Content);
        parentPage?.SetScratchPad(ScratchPadTypes.FileFullContent, relativePath, null, "created");

        return Task.FromResult(new Output
        {
            FilePath = relativePath,
            Message = $"Successfully created file: {relativePath}",
        });
    }
}