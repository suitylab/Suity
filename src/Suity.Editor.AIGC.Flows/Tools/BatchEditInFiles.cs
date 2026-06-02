using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Flows.SubFlows.Running;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("BatchEditInFiles", CodeBase = "*Suity", Category = "WorkSpace")]
[DisplayText("Batch Edit In Files")]
[ToolTipsText("Perform an exact string edit in multiple files at once. Use for cross-file refactoring, or edit multiple places in one file")]
[NativeAlias("Suity.Editor.AIGC.BatchReplaceStringInFiles")]
[NativeAlias("Suity.Editor.AIGC.Tools.BatchReplaceStringInFiles")]
public class BatchEditInFiles : ToolCommand<BatchEditInFiles.Output>
{
    [NativeType("BatchEditInFiles.FileEditItem", CodeBase = "*Suity")]
    public class FileEditItem : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly TextBlockProperty _oldExactString = new("OldExactString", "Old Exact String");
        readonly TextBlockProperty _newString = new("NewString", "New String");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string OldExactString { get => _oldExactString.Text; set => _oldExactString.Text = value; }
        public string NewString { get => _newString.Text; set => _newString.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _oldExactString.Sync(sync);
            _newString.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _oldExactString.InspectorField(setup);
            _newString.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} -{OldExactString?.Length ?? 0} +{NewString?.Length ?? 0}";
    }

    [NativeType("BatchEditInFiles.FileResult", CodeBase = "*Suity")]
    public class FileResult : IViewObject
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _status = new("Status", "Status");
        readonly StringProperty _error = new("Error", "Error");
        readonly ValueProperty<int> _replacementsMade = new("ReplacementsMade", "Replacements Made");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Status { get => _status.Text; set => _status.Text = value; }
        public string Error { get => _error.Text; set => _error.Text = value; }
        public int ReplacementsMade { get => _replacementsMade.Value; set => _replacementsMade.Value = value; }
        public bool HasError => !string.IsNullOrWhiteSpace(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _status.Sync(sync);

            if (sync.IsSetter() || !string.IsNullOrWhiteSpace(_error.Text))
            {
                _error.Sync(sync);
            }

            _replacementsMade.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _status.InspectorField(setup);
            _error.InspectorField(setup);
            _replacementsMade.InspectorField(setup);
        }
        public override string ToString() => $"{FilePath} [{Status}] Replacements: {ReplacementsMade}" + (HasError ? $" - Error: {Error}" : "");
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
        int failCount = 0;

        var fileGroups = Modifications.GroupBy(m => m.FilePath);

        foreach (var group in fileGroups)
        {
            string filePath = group.Key;
            var result = new FileResult { FilePath = filePath };
            int replacementsInFile = 0;

            try
            {
                string relativePath = filePath.TrimStart('/', '\\');
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

                string content = File.ReadAllText(fullPath);
                string originalContent = content;

                foreach (var mod in group)
                {
                    if (string.IsNullOrWhiteSpace(mod.OldExactString))
                    {
                        continue;
                    }

                    int matchCount = 0;
                    int index = 0;
                    StringUtility.MatchResult matchFinal = StringUtility.MatchResult.NotFound;
                    StringUtility.MatchResult match = StringUtility.FuzzyMatch(content, mod.OldExactString, index);
                    while (match.Found)
                    {
                        matchCount++;
                        if (matchCount == 1)
                            matchFinal = match;
                        index = match.Index + match.Length;
                        match = StringUtility.FuzzyMatch(content, mod.OldExactString, index);
                    }

                    if (matchCount == 0)
                    {
                        result.Error = string.IsNullOrEmpty(result.Error)
                            ? $"OldExactString not found: {mod.OldExactString.Substring(0, Math.Min(50, mod.OldExactString.Length))}..."
                            : result.Error;
                        continue;
                    }

                    if (matchCount >= 2)
                    {
                        result.Error = "Multiple matches found, could not locate precisely.";
                        continue;
                    }

                    content = StringUtility.ReplaceContent(content, matchFinal.Index, matchFinal.Length, mod.NewString);
                    replacementsInFile++;
                }

                File.WriteAllText(fullPath, content);
                result.Status = replacementsInFile > 0 ? "Modified" : "No Changes";
                result.ReplacementsMade = replacementsInFile;

                if (replacementsInFile > 0)
                {
                    var replacementSummary = string.Join("\n", group.Select(m =>
                        $"---------------- Before ----------------\n{m.OldExactString}\n---------------- After ----------------\n{m.NewString}"));
                    parentPage?.SetScratchPad(ScratchPadTypes.FileEdit, relativePath, replacementSummary, $"replaced {replacementsInFile} place(s), use ReadFile to get full content");
                    successCount++;
                }
                else if (!string.IsNullOrEmpty(result.Error))
                {
                    failCount++;
                }
                else
                {
                    successCount++;
                }
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
}