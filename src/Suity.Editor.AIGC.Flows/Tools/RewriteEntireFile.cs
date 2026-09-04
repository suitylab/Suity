using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Editor.WorkSpaces;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("RewriteEntireFile", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Rewrite Entire File")]
[ToolTipsText("Completely overwrite a file with new content. Creates the file and its directory if they don't exist. Use as fallback when Diff or Replace operations fail.")]
[NativeAlias("Suity.Editor.AIGC.RewriteEntireFile")]
public class RewriteEntireFile : ToolCommand<RewriteEntireFile.Output>
{
    public class Output : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _message = new("Message", "Message");

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
        var parentPage = context.ToolInstance.GetParentTask() as IAigcWorkflowPage;
        var workSpace = context.WorkSpace;

        if (workSpace == null)
        {
            throw new NullReferenceException("Workspace is not set");
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
        }

        string relativePath = FilePath.TrimStart('/', '\\');

        context?.AddToolMessage("Rewrite file", msg =>
        {
            msg.AddCode(relativePath);
        });

        workSpace.WriteAllText(relativePath, NewFullContent);
        parentPage?.SetScratchPad(ScratchPadTypes.FileFullContent, relativePath, null, "rewritten");

        return Task.FromResult(new Output
        {
            FilePath = relativePath,
            Message = $"Successfully rewrote file: {relativePath}",
        });
    }
}