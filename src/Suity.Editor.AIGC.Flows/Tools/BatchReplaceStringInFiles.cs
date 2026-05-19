using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("BatchReplaceStringInFiles", CodeBase = "*Suity")]
[DisplayText("Batch Replace String In Files")]
[ToolTipsText("Replace exact strings in multiple files at once. Use for cross-file refactoring like updating global constants or base class method calls.")]
public class BatchReplaceStringInFiles : ToolCommand<BatchReplaceStringInFiles.Output>
{
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
    }

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
        public bool HasError => !string.IsNullOrEmpty(Error);

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _filePath.Sync(sync);
            _status.Sync(sync);
            _error.Sync(sync);
            _replacementsMade.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _filePath.InspectorField(setup);
            _status.InspectorField(setup);
            _error.InspectorField(setup);
            _replacementsMade.InspectorField(setup);
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
                string targetPath = filePath;

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

                string content = File.ReadAllText(targetPath);

                foreach (var mod in group)
                {
                    if (string.IsNullOrWhiteSpace(mod.OldExactString))
                    {
                        continue;
                    }

                    if (!content.Contains(mod.OldExactString))
                    {
                        result.Error = string.IsNullOrEmpty(result.Error)
                            ? $"OldExactString not found: {mod.OldExactString.Substring(0, Math.Min(50, mod.OldExactString.Length))}..."
                            : result.Error;
                        continue;
                    }

                    content = content.Replace(mod.OldExactString, mod.NewString);
                    replacementsInFile++;
                }

                File.WriteAllText(targetPath, content);
                result.Status = replacementsInFile > 0 ? "Modified" : "No Changes";
                result.ReplacementsMade = replacementsInFile;

                if (replacementsInFile > 0)
                {
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