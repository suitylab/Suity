using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("ReplaceStringInFile", CodeBase = "*Suity")]
[DisplayText("Replace String In File")]
[ToolTipsText("Replace an exact string in a file with new content. Suitable for small-scale refactoring.")]
[NativeAlias("Suity.Editor.AIGC.ReplaceStringInFile")]
public class ReplaceStringInFile : ToolCommand<ReplaceStringInFile.Output>
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
        public override string ToString() => $"{FilePath}: '{Message}'";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path to the target file.");
    readonly TextBlockProperty _oldExactString = new("OldExactString", "Old Exact String", "The exact string to find and replace (including indentation and newlines).");
    readonly TextBlockProperty _newString = new("NewString", "New String", "The new string to replace with.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public string OldExactString { get => _oldExactString.Text; set => _oldExactString.Text = value; }
    public string NewString { get => _newString.Text; set => _newString.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _oldExactString.Sync(sync);
        _newString.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _oldExactString.InspectorField(setup);
        _newString.InspectorField(setup);
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

        if (string.IsNullOrWhiteSpace(OldExactString))
        {
            throw new ArgumentException("OldExactString is not set");
        }

        string targetPath = FilePath.TrimStart('/', '\\');

        if (!Path.IsPathRooted(targetPath))
        {
            targetPath = Path.Combine(workspaceDir, targetPath);
        }

        if (!File.Exists(targetPath))
        {
            throw new FileNotFoundException($"File not found: {targetPath}");
        }

        string content = File.ReadAllText(targetPath);

        int matchCount = 0;
        int index = 0;
        while ((index = content.IndexOf(OldExactString, index, StringComparison.Ordinal)) != -1)
        {
            matchCount++;
            index += OldExactString.Length;
        }

        if (matchCount == 0)
        {
            throw new InvalidOperationException($"The specified OldExactString was not found in the file: {targetPath}");
        }

        if (matchCount >= 2)
        {
            throw new InvalidOperationException("Multiple matches found, could not locate precisely.");
        }

        content = content.Replace(OldExactString, NewString);
        File.WriteAllText(targetPath, content);

        return Task.FromResult(new Output
        {
            FilePath = targetPath,
            Message = $"Successfully replaced string in file: {targetPath}",
        });
    }
}