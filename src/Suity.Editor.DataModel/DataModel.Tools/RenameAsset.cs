using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.DataModel.Tools;

[NativeType("RenameAsset", CodeBase = "*Suity", Category = "Asset Tools", Icon = "*CoreIcon|Class")]
[DisplayText("Rename Asset")]
[ToolTipsText("Rename or move an asset file to a new path.")]
[NativeAlias("Suity.Editor.DataModel.RenameAsset")]
public class RenameAsset : ToolCommand<RenameAsset.MoveResult>
{
    /// <summary>
    /// Result of a move/rename operation.
    /// </summary>
    [NativeType("RenameAsset.MoveResult", CodeBase = "*Suity")]
    public class MoveResult : SObjectController
    {
        readonly StringProperty _oldPath = new("OldPath", "Old Path");
        readonly StringProperty _newPath = new("NewPath", "New Path");

        public string OldPath { get => _oldPath.Text; set => _oldPath.Text = value; }
        public string NewPath { get => _newPath.Text; set => _newPath.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _oldPath.Sync(sync);
            _newPath.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _oldPath.InspectorField(setup);
            _newPath.InspectorField(setup);
        }

        public override string ToString() => $"{OldPath} -> {NewPath}";
    }

    readonly StringProperty _oldPath = new("OldPath", "Old Path", string.Empty, "The current path of the asset.");
    readonly StringProperty _newPath = new("NewPath", "New Path", string.Empty, "The new path for the asset.");

    public string OldPath { get => _oldPath.Text; set => _oldPath.Text = value; }
    public string NewPath { get => _newPath.Text; set => _newPath.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _oldPath.Sync(sync);
        _newPath.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _oldPath.InspectorField(setup);
        _newPath.InspectorField(setup);
    }

    public override Task<MoveResult> Run(ToolCallContext context)
    {
        if (string.IsNullOrWhiteSpace(OldPath))
            throw new ArgumentException("OldPath is not set");

        if (string.IsNullOrWhiteSpace(NewPath))
            throw new ArgumentException("NewPath is not set");

        string assetSpaceDir = Project.Current?.AssetDirectory;
        if (string.IsNullOrWhiteSpace(assetSpaceDir))
            throw new NullReferenceException("Asset directory is not set");

        string fullOldPath = Path.IsPathRooted(OldPath)
            ? OldPath
            : Path.Combine(assetSpaceDir, OldPath.TrimStart('/', '\\'));

        string fullNewPath = Path.IsPathRooted(NewPath)
            ? NewPath
            : Path.Combine(assetSpaceDir, NewPath.TrimStart('/', '\\'));

        if (!File.Exists(fullOldPath))
            throw new FileNotFoundException($"Asset not found at '{OldPath}'");

        string newDir = Path.GetDirectoryName(fullNewPath);
        if (!string.IsNullOrEmpty(newDir) && !Directory.Exists(newDir))
            Directory.CreateDirectory(newDir);

        context.ToolInstance.Conversation?.AddRunningMessage("Rename asset", msg =>
        {
            msg.AddCode(OldPath);
            msg.AddText(" -> ");
            msg.AddCode(NewPath);
        });
        context.Conversation?.AddRunningMessage("Rename asset", msg =>
        {
            msg.AddCode(OldPath);
            msg.AddText(" -> ");
            msg.AddCode(NewPath);
        });

        File.Move(fullOldPath, fullNewPath);

        return Task.FromResult(new MoveResult
        {
            OldPath = OldPath,
            NewPath = NewPath,
        });
    }
}
