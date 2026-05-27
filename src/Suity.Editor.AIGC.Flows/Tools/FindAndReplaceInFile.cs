using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("FindAndReplaceInFile", CodeBase = "*Suity")]
[DisplayText("Find And Replace In File")]
[ToolTipsText("Find and replace multiple string pairs in a single file. Can match multiple occurrences of the same string. Suitable for batch text replacement across the file.")]
[NativeAlias("Suity.Editor.AIGC.FindAndReplaceInFile")]
public class FindAndReplaceInFile : ToolCommand<FindAndReplaceInFile.Output>
{
    [NativeType("FindAndReplaceInFile.ReplaceItem", CodeBase = "*Suity")]
    public class ReplaceItem : IViewObject
    {
        readonly TextBlockProperty _oldString = new("OldString", "Old String", "The string to find.");
        readonly TextBlockProperty _newString = new("NewString", "New String", "The string to replace with.");

        public string OldString { get => _oldString.Text; set => _oldString.Text = value; }
        public string NewString { get => _newString.Text; set => _newString.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _oldString.Sync(sync);
            _newString.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _oldString.InspectorField(setup);
            _newString.InspectorField(setup);
        }
        public override string ToString() => $"'{OldString}' -> '{NewString}'";
    }

    [NativeType("FindAndReplaceInFile.ReplaceResult", CodeBase = "*Suity")]
    public class ReplaceResult : IViewObject
    {
        readonly StringProperty _oldString = new("OldString", "Old String");
        readonly StringProperty _newString = new("NewString", "New String");
        readonly ValueProperty<int> _matchCount = new("MatchCount", "Match Count");

        public string OldString { get => _oldString.Text; set => _oldString.Text = value; }
        public string NewString { get => _newString.Text; set => _newString.Text = value; }
        public int MatchCount { get => _matchCount.Value; set => _matchCount.Value = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _oldString.Sync(sync);
            _newString.Sync(sync);
            _matchCount.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _oldString.InspectorField(setup);
            _newString.InspectorField(setup);
            _matchCount.InspectorField(setup);
        }
        public override string ToString() => $"'{OldString}' -> '{NewString}' ({MatchCount} matches)";
    }

    public class Output : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly ListProperty<ReplaceResult> _results = new("Results", "Results");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public List<ReplaceResult> Results => _results.List;

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _results.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _results.InspectorField(setup);
        }
        public override string ToString() => $"Find And Replace in {FilePath}: {Results.Count} replacements";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path to the target file.");
    readonly ListProperty<ReplaceItem> _replaceItems = new("ReplaceItems", "Replace Items", "List of string pairs to find and replace. Each item can match multiple occurrences.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public List<ReplaceItem> ReplaceItems => _replaceItems.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _replaceItems.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _replaceItems.InspectorField(setup);
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

        if (ReplaceItems.Count == 0)
        {
            throw new ArgumentException("ReplaceItems is empty");
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

        string content = File.ReadAllText(fullPath);

        var output = new Output { FilePath = relativePath };

        foreach (var item in ReplaceItems)
        {
            if (string.IsNullOrWhiteSpace(item.OldString))
            {
                continue;
            }

            int matchCount = 0;
            int index = 0;
            while ((index = content.IndexOf(item.OldString, index, StringComparison.Ordinal)) != -1)
            {
                matchCount++;
                index += item.OldString.Length;
            }

            var result = new ReplaceResult
            {
                OldString = item.OldString,
                NewString = item.NewString,
                MatchCount = matchCount
            };

            if (matchCount > 0)
            {
                content = content.Replace(item.OldString, item.NewString ?? string.Empty);
            }

            output.Results.Add(result);
        }

        File.WriteAllText(fullPath, content);

        return Task.FromResult(output);
    }
}