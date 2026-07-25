using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.DataModel.Tools;

[NativeType("DeleteAsset", CodeBase = "*Suity", Category = "Asset Tools", Icon = "*CoreIcon|Asset")]
[DisplayText("Delete Asset")]
[ToolTipsText("Delete an asset file.")]
[NativeAlias("Suity.Editor.DataModel.DeleteAsset")]
public class DeleteAsset : ToolCommand<DeleteAsset.DeleteResult>
{
    /// <summary>
    /// Result of a delete operation.
    /// </summary>
    [NativeType("DeleteAsset.DeleteResult", CodeBase = "*Suity")]
    public class DeleteResult : SObjectController
    {
        readonly StringProperty _deletedPath = new("DeletedPath", "Deleted Path");

        public string DeletedPath { get => _deletedPath.Text; set => _deletedPath.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _deletedPath.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _deletedPath.InspectorField(setup);
        }

        public override string ToString() => DeletedPath;
    }

    readonly StringProperty _path = new("Path", "Path", string.Empty, "The path of the asset to delete.");

    public string Path { get => _path.Text; set => _path.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _path.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _path.InspectorField(setup);
    }

    public override Task<DeleteResult> Run(ToolCallContext context)
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new ArgumentException("Path is not set");

        string assetSpaceDir = Project.Current?.AssetDirectory;
        if (string.IsNullOrWhiteSpace(assetSpaceDir))
            throw new NullReferenceException("Asset directory is not set");

        string fullPath = System.IO.Path.IsPathRooted(Path)
            ? Path
            : System.IO.Path.Combine(assetSpaceDir, Path.TrimStart('/', '\\'));

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Asset not found at '{Path}'");

        context.ToolInstance.Conversation?.AddRunningMessage("Delete asset", msg =>
        {
            msg.AddCode(Path);
        });
        context.Conversation?.AddRunningMessage("Delete asset", msg =>
        {
            msg.AddCode(Path);
        });

        File.Delete(fullPath);

        return Task.FromResult(new DeleteResult
        {
            DeletedPath = Path,
        });
    }
}
