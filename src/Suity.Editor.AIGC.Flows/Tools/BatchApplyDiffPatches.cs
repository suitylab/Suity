using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchApplyDiffPatches", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Batch Apply Diff Patches")]
[ToolTipsText("Apply multiple unified diff patches at once. Use for complex feature development involving multiple file changes with atomicity.")]
[NativeAlias("Suity.Editor.AIGC.BatchApplyDiffPatches")]
public class BatchApplyDiffPatches : ToolCommand<BatchApplyDiffPatches.Output>
{
[NativeType("BatchApplyDiffPatches.FilePatchItem", CodeBase = "*Suity")]
    public class FilePatchItem : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly TextBlockProperty _diffContent = new("DiffContent", "Diff Content");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string DiffContent { get => _diffContent.Text; set => _diffContent.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _diffContent.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _diffContent.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} ({DiffContent?.Length ?? 0} chars diff)";
    }

[NativeType("BatchApplyDiffPatches.FileResult", CodeBase = "*Suity")]
    public class FileResult : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _status = new("Status", "Status");
        readonly StringProperty _error = new("Error", "Error");
        readonly ValueProperty<int> _hunksApplied = new("HunksApplied", "Hunks Applied");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Status { get => _status.Text; set => _status.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public int HunksApplied { get => _hunksApplied.Value; set => _hunksApplied.Value = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _status.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }

            _hunksApplied.Sync(sync);
        }
        protected override void OnSetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _status.InspectorField(setup);
            _error.InspectorField(setup);
            _hunksApplied.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} [{Status}] {(HasError ? $"- Error: {Error}" : $"Hunks: {HunksApplied}")}";
    }

    public class Output : SObjectController
    {
        readonly ListProperty<FileResult> _results = new("Results", "Results");
        readonly ValueProperty<int> _successCount = new("SuccessCount", "Success Count");
        readonly ValueProperty<int> _failCount = new("FailCount", "Fail Count");

        public List<FileResult> Results => _results.List;
        public int SuccessCount { get => _successCount.Value; set => _successCount.Value = value; }
        public int FailCount { get => _failCount.Value; set => _failCount.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _results.Sync(sync);
            _successCount.Sync(sync);
            _failCount.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _results.InspectorField(setup);
            _successCount.InspectorField(setup);
            _failCount.InspectorField(setup);
        }

        public override string ToString() => $"Results: {SuccessCount} success, {FailCount} failed ({Results.Count} items)";
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
        var parentPage = context.ToolInstance.GetParentTask() as IAigcWorkflowPage;

        string workspaceDir = context.RootDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();
        int successCount = 0;
        int failCount = 0;

        var patchFileNames = string.Join(", ", Patches.Select(p => p.FilePath));
        context.ToolInstance.Conversation?.AddRunningMessage($"Apply diff patches to {Patches.Count} file(s)", msg =>
        {
            msg.AddCode(patchFileNames);
        });
        context.Conversation?.AddRunningMessage($"Apply diff patches to {Patches.Count} file(s)", msg =>
        {
            msg.AddCode(patchFileNames);
        });

        foreach (var patchItem in Patches)
        {
            var result = new FileResult { FilePath = patchItem.FilePath };
            int hunksApplied = 0;

            try
            {
                string relativePath = patchItem.FilePath.TrimStart('/', '\\');
                string fullPath = relativePath;

                if (!Path.IsPathRooted(relativePath))
                {
                    fullPath = Path.Combine(workspaceDir, relativePath);
                }

                result.FilePath = relativePath;

                if (!File.Exists(fullPath))
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

                var lines = File.ReadAllLines(fullPath).ToList();
                var originalContent = string.Join(Environment.NewLine, lines);
                var diffLines = patchItem.DiffContent.Split('\n').Select(l => l.TrimEnd('\r')).ToList();

                var patches = ParseUnifiedDiff(diffLines);
                hunksApplied = ApplyPatchesToLines(lines, patches);

                File.WriteAllLines(fullPath, lines);

                string diffSummary = $"---------------- Before ----------------\n{patchItem.DiffContent}\n---------------- After ----------------\n(Applied {hunksApplied} hunk(s))";
                parentPage?.SetScratchPad(ScratchPadTypes.FileEdit, relativePath, diffSummary, $"applied {hunksApplied} hunk(s), use ReadFile to get full content");

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
                                    patch.OldContent.Add(content);
                                    patch.NewContent.Add(content);
                                    break;
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
                throw new InvalidOperationException($"Invalid patch: start line {patch.StartLine} is out of range (file has {lines.Count} lines)");
            }

            int removeCount = patch.OldLines;
            int insertCount = patch.NewContent.Count;

            if (adjustedStart + removeCount > lines.Count)
            {
                throw new InvalidOperationException($"Patch tries to remove {removeCount} lines at {adjustedStart + 1}, but file only has {lines.Count} lines");
            }

            List<string> actualContent = lines.GetRange(adjustedStart, removeCount);
            if (!actualContent.SequenceEqual(patch.OldContent))
            {
                throw new InvalidOperationException(
                    $"Patch content mismatch at line {patch.StartLine}: " +
                    $"expected lines do not match file content. " +
                    $"Patch may be outdated or incorrectly generated.");
            }

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