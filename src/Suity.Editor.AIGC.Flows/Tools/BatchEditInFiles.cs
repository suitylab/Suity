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
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchEditInFiles", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Batch Edit In Files")]
[ToolTipsText("Perform an exact string edit in multiple files at once. Use for cross-file refactoring, or edit multiple places in one file")]
[NativeAlias("Suity.Editor.AIGC.BatchReplaceStringInFiles")]
[NativeAlias("Suity.Editor.AIGC.Tools.BatchReplaceStringInFiles")]
public class BatchEditInFiles : ToolCommand<BatchEditInFiles.Output>
{
    record FileMod(string FullPath, string RelativePath, string NewContent, int Replacements, IEnumerable<FileEditItem> Mods);
    record ErrorInfo(int Index, string FilePath, string Message);

    [NativeType("BatchEditInFiles.FileEditItem", CodeBase = "*Suity")]
    public class FileEditItem : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly TextBlockProperty _oldExactString = new("OldExactString", "Old Exact String");
        readonly TextBlockProperty _newString = new("NewString", "New String");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string OldExactString { get => _oldExactString.Text; set => _oldExactString.Text = value; }
        public string NewString { get => _newString.Text; set => _newString.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _filePath.Sync(sync);
            _oldExactString.Sync(sync);
            _newString.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _filePath.InspectorField(setup);
            _oldExactString.InspectorField(setup);
            _newString.InspectorField(setup);
        }

        public override string ToString() => $"{FilePath} -{OldExactString?.Length ?? 0} +{NewString?.Length ?? 0}";
    }

    [NativeType("BatchEditInFiles.FileResult", CodeBase = "*Suity")]
    public class FileResult : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _status = new("Status", "Status");
        readonly ValueProperty<int> _replacementsMade = new("ReplacementsMade", "Replacements Made");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Status { get => _status.Text; set => _status.Text = value; }
        public int ReplacementsMade { get => _replacementsMade.Value; set => _replacementsMade.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _filePath.Sync(sync);
            _status.Sync(sync);
            _replacementsMade.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _filePath.InspectorField(setup);
            _status.InspectorField(setup);
            _replacementsMade.InspectorField(setup);
        }

        public override string ToString() => $"{FilePath} [{Status}] Replacements: {ReplacementsMade}";
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

        public override string ToString() => $"Results: {SuccessCount} modified, {FailCount} failed ({Results.Count} items)";
    }

    readonly ListProperty<FileEditItem> _modifications = new("Modifications", "Modifications", "List of string replacements to perform.");

    public List<FileEditItem> Modifications => _modifications.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _modifications.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _modifications.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        var parentPage = context.ToolInstance.GetParentTask() as IAigcWorkflowPage;

        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        var output = new Output();
        int successCount = 0;
        var errors = new List<ErrorInfo>();
        var fileResults = new List<FileMod>();

        var fileGroups = Modifications.GroupBy(m => m.FilePath).ToList();

        foreach (var group in fileGroups)
        {
            string filePath = group.Key;
            string relativePath = filePath.TrimStart('/', '\\');
            string fullPath = relativePath;

            if (!Path.IsPathRooted(relativePath))
            {
                fullPath = Path.Combine(workspaceDir, relativePath);
            }

            if (!File.Exists(fullPath))
            {
                errors.Add(new(Modifications.IndexOf(group.First()), relativePath, "File not found"));
                continue;
            }

            string content = File.ReadAllText(fullPath);
            string newContent = content;
            int replacementsInFile = 0;
            var fileMods = group.ToList();

            foreach (var mod in fileMods)
            {
                int modIndex = Modifications.IndexOf(mod);

                if (string.IsNullOrWhiteSpace(mod.OldExactString))
                {
                    continue;
                }

                int matchCount = 0;
                int searchIndex = 0;
                StringUtility.MatchResult matchFinal = StringUtility.MatchResult.NotFound;
                StringUtility.MatchResult match = StringUtility.FuzzyMatch(newContent, mod.OldExactString, searchIndex);
                while (match.Found)
                {
                    matchCount++;
                    if (matchCount == 1)
                        matchFinal = match;
                    searchIndex = match.Index + match.Length;
                    match = StringUtility.FuzzyMatch(newContent, mod.OldExactString, searchIndex);
                }

                if (matchCount == 0)
                {
                    errors.Add(new(modIndex, relativePath, $"Old Exact String not found: {mod.OldExactString.Substring(0, Math.Min(50, mod.OldExactString.Length))}..."));
                    newContent = null;
                    continue;
                }

                if (matchCount >= 2)
                {
                    errors.Add(new(modIndex, relativePath, $"Multiple matches found, could not locate precisely: {mod.OldExactString.Substring(0, Math.Min(50, mod.OldExactString.Length))}..."));
                    newContent = null;
                    continue;
                }

                newContent = StringUtility.ReplaceContent(newContent, matchFinal.Index, matchFinal.Length, mod.NewString);
                replacementsInFile++;
            }

            if (newContent == null)
            {
                continue;
            }

            fileResults.Add(new FileMod(fullPath, relativePath, newContent, replacementsInFile, fileMods));
        }

        if (errors.Count > 0)
        {
            var errorMessages = errors.Select(e => $"[Modification index:{e.Index}] {e.FilePath}: {e.Message}");
            throw new AggregateException($"BatchEditInFiles failed with {errors.Count} error(s):\n" + string.Join("\n", errorMessages));
        }

        var fileNames = string.Join(", ", fileResults.Select(f => f.RelativePath));
        context.ToolInstance.Conversation?.AddRunningMessage($"Batch edit {fileResults.Count} file(s)", msg =>
        {
            msg.AddCode(fileNames);
        });
        context.Conversation?.AddRunningMessage($"Batch edit {fileResults.Count} file(s)", msg =>
        {
            msg.AddCode(fileNames);
        });

        foreach (var file in fileResults)
        {
            File.WriteAllText(file.FullPath, file.NewContent);

            var result = new FileResult
            {
                FilePath = file.RelativePath,
                Status = file.Replacements > 0 ? "Modified" : "No Changes",
                ReplacementsMade = file.Replacements
            };

            if (file.Replacements > 0)
            {
                var replacementSummary = string.Join("\n", file.Mods.Select(m =>
                    $"---------------- Before ----------------\n{m.OldExactString}\n---------------- After ----------------\n{m.NewString}"));
                parentPage?.SetScratchPad(ScratchPadTypes.FileEdit, file.RelativePath, replacementSummary, $"replaced {file.Replacements} place(s), use ReadFile to get full content");
                successCount++;
            }
            else
            {
                successCount++;
            }

            output.Results.Add(result);
        }

        output.SuccessCount = successCount;
        output.FailCount = 0;

        return Task.FromResult(output);
    }
}