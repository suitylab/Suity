using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("ListDirectory", CodeBase = "*Suity", Category = "WorkSpace")]
[DisplayText("List Directory")]
[ToolTipsText("List directory contents, showing files and folders in the specified directory.")]
[NativeAlias("Suity.Editor.AIGC.ListDirectory")]
public class ListDirectory : ToolCommand<ListDirectory.Output>
{
    public class Output : SObjectController
    {
        readonly TextBlockProperty _result = new("Result");

        public string Result { get => _result.Text; set => _result.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _result.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _result.InspectorField(setup);
        }

        public override string ToString() => $"Directory listing: {Result?.Split('\n').Length ?? 0} entries";
    }

    readonly StringProperty _dirPath = new("DirPath", "DirPath", string.Empty, "The relative path to the directory to list. If empty, the workspace directory is used.");

    public string DirPath { get => _dirPath.Text; set => _dirPath.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _dirPath.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _dirPath.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        string relativePath = string.IsNullOrWhiteSpace(DirPath) ? "" : DirPath.TrimStart('/', '\\');
        string fullPath = string.IsNullOrWhiteSpace(relativePath) ? workspaceDir : relativePath;

        if (!string.IsNullOrWhiteSpace(relativePath) && !Path.IsPathRooted(relativePath))
        {
            fullPath = Path.Combine(workspaceDir, relativePath);
        }

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {relativePath}");
        }

        var dirInfo = new DirectoryInfo(fullPath);
        var entries = dirInfo.GetFileSystemInfos()
            .OrderBy(f => f is DirectoryInfo ? 0 : 1)
            .ThenBy(f => f.Name);

        var lines = entries.Select(f => f is DirectoryInfo dir ? $"{dir.Name}/" : $"{f.Name} ({DisplayFormatter.GetFileSizeDisplay(((FileInfo)f).Length)})");

        return Task.FromResult(new Output
        {
            Result = string.Join(Environment.NewLine, lines),
        });
    }
}