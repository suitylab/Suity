using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("GetFileMetadata", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Get File Metadata")]
[ToolTipsText("Get basic file metadata (size, modified time) to help Agent estimate token consumption before reading.")]
[NativeAlias("Suity.Editor.AIGC.GetFileMetadata")]
public class GetFileMetadata : ToolCommand<GetFileMetadata.Output>
{
    public class Output : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _fileName = new("FileName", "File Name");
        readonly StringProperty _extension = new("Extension", "Extension");
        readonly ValueProperty<int> _sizeBytes = new("SizeBytes", "Size (Bytes)");
        readonly ValueProperty<int> _totalLines = new("TotalLines", "Total Lines");
        readonly StringProperty _modifiedTime = new("ModifiedTime", "Modified Time");
        readonly StringProperty _createdTime = new("CreatedTime", "Created Time");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string FileName { get => _fileName.Text; set => _fileName.Text = value; }
        public string Extension { get => _extension.Text; set => _extension.Text = value; }
        public int SizeBytes { get => _sizeBytes.Value; set => _sizeBytes.Value = value; }
        public int TotalLines { get => _totalLines.Value; set => _totalLines.Value = value; }
        public string ModifiedTime { get => _modifiedTime.Text; set => _modifiedTime.Text = value; }
        public string CreatedTime { get => _createdTime.Text; set => _createdTime.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _filePath.Sync(sync);
            _fileName.Sync(sync);
            _extension.Sync(sync);
            _sizeBytes.Sync(sync);
            _totalLines.Sync(sync);
            _modifiedTime.Sync(sync);
            _createdTime.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _filePath.InspectorField(setup);
            _fileName.InspectorField(setup);
            _extension.InspectorField(setup);
            _sizeBytes.InspectorField(setup);
            _totalLines.InspectorField(setup);
            _modifiedTime.InspectorField(setup);
            _createdTime.InspectorField(setup);
        }

        public override string ToString() => $"{FileName} ({Extension}, {SizeBytes} bytes, {TotalLines} lines)";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path to the target file.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
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

        string relativePath = FilePath.TrimStart('/', '\\');
        string fullPath = relativePath;

        if (!Path.IsPathRooted(relativePath))
        {
            fullPath = Path.Combine(workspaceDir, relativePath);
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {relativePath}");
        }

        var fileInfo = new FileInfo(fullPath);
        var lines = File.ReadAllLines(fullPath);

        return Task.FromResult(new Output
        {
            FilePath = relativePath,
            FileName = fileInfo.Name,
            Extension = fileInfo.Extension,
            SizeBytes = (int)fileInfo.Length,
            TotalLines = lines.Length,
            ModifiedTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
            CreatedTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
        });
    }
}