using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("ApplyDiffPatch", CodeBase = "*Suity", Category = "WorkSpace")]
[DisplayText("Apply Diff Patch")]
[ToolTipsText("Apply a Unified Diff format patch to a file. Supports multiple non-contiguous edits in a single file.")]
[NativeAlias("Suity.Editor.AIGC.ApplyDiffPatch")]
public class ApplyDiffPatch : ToolCommand<ApplyDiffPatch.Output>
{
    public class Output : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _message = new("Message", "Message");
        readonly ValueProperty<int> _hunksApplied = new("HunksApplied", "Hunks Applied");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Message { get => _message.Text; set => _message.Text = value; }
        public int HunksApplied { get => _hunksApplied.Value; set => _hunksApplied.Value = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _message.Sync(sync);
            _hunksApplied.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _message.InspectorField(setup);
            _hunksApplied.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath}: {HunksApplied} hunks applied - {Message}";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The absolute or relative path to the target file.");
    readonly TextBlockProperty _diffContent = new("DiffContent", "Diff Content", "The unified diff patch string to apply.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public string DiffContent { get => _diffContent.Text; set => _diffContent.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _diffContent.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _diffContent.InspectorField(setup);
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

        if (string.IsNullOrWhiteSpace(DiffContent))
        {
            throw new ArgumentException("DiffContent is not set");
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

        var lines = File.ReadAllLines(fullPath).ToList();
        var originalContent = string.Join(Environment.NewLine, lines);
        var diffLines = DiffContent.Split('\n').Select(l => l.TrimEnd('\r')).ToList();

        var patches = ParseUnifiedDiff(diffLines);
        int hunksApplied = 0;

        foreach (var patch in patches)
        {
            if (patch.StartLine <= 0 || patch.StartLine > lines.Count)
            {
                throw new InvalidOperationException($"Invalid patch: start line {patch.StartLine} is out of range (file has {lines.Count} lines)");
            }

            int removeCount = patch.OldLines;
            int insertCount = patch.NewLines;

            if (patch.StartLine - 1 + removeCount > lines.Count)
            {
                throw new InvalidOperationException($"Invalid patch: patch tries to remove {removeCount} lines starting at {patch.StartLine}, but file only has {lines.Count} lines");
            }

            List<string> actualContent = lines.GetRange(patch.StartLine - 1, removeCount);
            if (!actualContent.SequenceEqual(patch.OldContent))
            {
                throw new InvalidOperationException(
                    $"Patch content mismatch at line {patch.StartLine}: " +
                    $"expected lines do not match file content. " +
                    $"Patch may be outdated or incorrectly generated.");
            }

            lines.RemoveRange(patch.StartLine - 1, removeCount);
            lines.InsertRange(patch.StartLine - 1, patch.NewContent);
            hunksApplied++;
        }

        File.WriteAllLines(fullPath, lines);

        string diffSummary = $"---------------- Before ----------------\n{DiffContent}\n---------------- After ----------------\n(Applied {hunksApplied} hunk(s))";
        parentPage?.SetScratchPad(ScratchPadTypes.FileEdit, relativePath, diffSummary, $"applied {hunksApplied} hunk(s), use ReadFile to get full content");

        return Task.FromResult(new Output
        {
            FilePath = relativePath,
            Message = $"Successfully applied {hunksApplied} hunk(s) to file: {relativePath}. Use ReadFile to get full content.",
            HunksApplied = hunksApplied,
        });
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
                    int newStart = int.Parse(match.Groups[3].Value);
                    int newCount = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 1;

                    var patch = new DiffPatch
                    {
                        StartLine = oldStart,
                        OldLines = oldCount,
                        NewLines = newCount,
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

    private class DiffPatch
    {
        public int StartLine { get; set; }
        public int OldLines { get; set; }
        public int NewLines { get; set; }
        public List<string> OldContent { get; set; } = new List<string>();
        public List<string> NewContent { get; set; } = new List<string>();
    }
}