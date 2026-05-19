using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("BatchApplyDiffPatches", CodeBase = "*Suity")]
[DisplayText("Batch Apply Diff Patches")]
[ToolTipsText("Apply multiple unified diff patches at once. Use for complex feature development involving multiple file changes with atomicity.")]
public class BatchApplyDiffPatches : ToolCommand<BatchApplyDiffPatches.Output>
{
    public class FilePatchItem : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly TextBlockProperty _diffContent = new("DiffContent", "Diff Content");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string DiffContent { get => _diffContent.Text; set => _diffContent.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _diffContent.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _diffContent.InspectorField(setup);
        }
    }

    public class FileResult : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _status = new("Status", "Status");
        readonly StringProperty _error = new("Error", "Error");
        readonly ValueProperty<int> _hunksApplied = new("HunksApplied", "Hunks Applied");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Status { get => _status.Text; set => _status.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public int HunksApplied { get => _hunksApplied.Value; set => _hunksApplied.Value = value; }
        public bool HasError => !string.IsNullOrEmpty(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _status.Sync(sync);
            _error.Sync(sync);
            _hunksApplied.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _status.InspectorField(setup);
            _error.InspectorField(setup);
            _hunksApplied.InspectorField(setup);
        }
    }

    public class Output : IViewObject
    {
        readonly ListProperty<FileResult> _results = new("Results", "Results");
        readonly ValueProperty<int> _successCount = new("SuccessCount", "Success Count");
        readonly ValueProperty<int> _failCount = new("FailCount", "Fail Count");

        public List<FileResult> Results => _results.List;
        public int SuccessCount { get => _successCount.Value; set => _successCount.Value = value; }
        public int FailCount { get => _failCount.Value; set => _failCount.Value = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _results.Sync(sync);
            _successCount.Sync(sync);
            _failCount.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _results.InspectorField(setup);
            _successCount.InspectorField(setup);
            _failCount.InspectorField(setup);
        }
    }

    readonly ListProperty<FilePatchItem> _patches = new("Patches", "Patches", "List of diff patches to apply.");

    public List<FilePatchItem> Patches => _patches.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _patches.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _patches.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();
        int successCount = 0;
        int failCount = 0;

        foreach (var patchItem in Patches)
        {
            var result = new FileResult { FilePath = patchItem.FilePath };
            int hunksApplied = 0;

            try
            {
                string targetPath = patchItem.FilePath;

                if (!Path.IsPathRooted(targetPath))
                {
                    targetPath = Path.Combine(workspaceDir, targetPath);
                }

                if (!File.Exists(targetPath))
                {
                    result.Status = "Failed";
                    result.Error = "File not found";
                    failCount++;
                    output.Results.Add(result);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(patchItem.DiffContent))
                {
                    result.Status = "Failed";
                    result.Error = "DiffContent is empty";
                    failCount++;
                    output.Results.Add(result);
                    continue;
                }

                var lines = File.ReadAllLines(targetPath).ToList();
                var diffLines = patchItem.DiffContent.Split('\n').Select(l => l.TrimEnd('\r')).ToList();

                var patches = ParseUnifiedDiff(diffLines);
                hunksApplied = ApplyPatchesToLines(lines, patches);

                File.WriteAllLines(targetPath, lines);
                result.Status = "Applied";
                result.HunksApplied = hunksApplied;
                successCount++;
            }
            catch (Exception ex)
            {
                result.Status = "Failed";
                result.Error = ex.Message;
                failCount++;
            }

            output.Results.Add(result);
        }

        output.SuccessCount = successCount;
        output.FailCount = failCount;

        return Task.FromResult(output);
    }

    private List<DiffPatch> ParseUnifiedDiff(List<string> diffLines)
    {
        var patches = new List<DiffPatch>();
        int i = 0;

        while (i < diffLines.Count)
        {
            var line = diffLines[i];

            if (line.StartsWith("@@"))
            {
                var match = Regex.Match(line, @"@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@");
                if (match.Success)
                {
                    int oldStart = int.Parse(match.Groups[1].Value);
                    int oldCount = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;

                    var patch = new DiffPatch
                    {
                        StartLine = oldStart,
                        OldLines = oldCount,
                    };

                    i++;

                    while (i < diffLines.Count && !diffLines[i].StartsWith("@@") && !diffLines[i].StartsWith("diff ") && !diffLines[i].StartsWith("--- ") && !diffLines[i].StartsWith("+++ "))
                    {
                        var diffLine = diffLines[i];
                        if (diffLine.Length > 0)
                        {
                            char prefix = diffLine[0];
                            string content = diffLine.Length > 1 ? diffLine.Substring(1) : "";

                            switch (prefix)
                            {
                                case ' ':
                                case '+':
                                    patch.NewContent.Add(content);
                                    break;
                                case '-':
                                    patch.OldContent.Add(content);
                                    break;
                            }
                        }
                        i++;
                    }

                    patches.Add(patch);
                    continue;
                }
            }
            i++;
        }

        return patches;
    }

    private int ApplyPatchesToLines(List<string> lines, List<DiffPatch> patches)
    {
        int hunksApplied = 0;
        int offset = 0;

        foreach (var patch in patches)
        {
            int adjustedStart = patch.StartLine - 1 + offset;

            if (adjustedStart < 0 || adjustedStart >= lines.Count)
            {
                continue;
            }

            int removeCount = patch.OldLines;
            int insertCount = patch.NewContent.Count;

            lines.RemoveRange(adjustedStart, removeCount);
            lines.InsertRange(adjustedStart, patch.NewContent);

            offset += insertCount - removeCount;
            hunksApplied++;
        }

        return hunksApplied;
    }

    private class DiffPatch
    {
        public int StartLine { get; set; }
        public int OldLines { get; set; }
        public List<string> OldContent { get; set; } = new List<string>();
        public List<string> NewContent { get; set; } = new List<string>();
    }
}