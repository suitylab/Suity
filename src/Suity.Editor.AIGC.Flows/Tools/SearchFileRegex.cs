using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

[NativeType("SearchFileRegex", CodeBase = "*Suity")]
[DisplayText("Search File Regex")]
[ToolTipsText("Search for regex patterns in the workspace.")]
public class SearchFileRegex : ToolCommand<SearchFileRegex.Output>
{
    public class Output : IViewObject
    {
        readonly TextBlockProperty _results = new("Results");
        readonly ValueProperty<int> _matchCount = new("MatchCount", 0);

        public string Results { get => _results.Text; set => _results.Text = value; }
        public int MatchCount { get => _matchCount.Value; set => _matchCount.Value = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _results.Sync(sync);
            _matchCount.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _results.InspectorField(setup);
            _matchCount.InspectorField(setup);
        }
    }

    readonly StringProperty _dirPath = new("DirPath", "DirPath", string.Empty, "The relative path within workspace to search. If empty, searches entire workspace.");
    readonly StringProperty _queryRegex = new("QueryRegex", "Query Regex", string.Empty, "The regex pattern to search for.");
    readonly StringProperty _fileExtension = new("FileExtension", "File Extension", string.Empty, "Filter by file extension, e.g. .cs, .json. Multiple extensions separated by comma.");

    public string DirPath { get => _dirPath.Text; set => _dirPath.Text = value; }
    public string QueryRegex { get => _queryRegex.Text; set => _queryRegex.Text = value; }
    public string FileExtension { get => _fileExtension.Text; set => _fileExtension.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _dirPath.Sync(sync);
        _queryRegex.Sync(sync);
        _fileExtension.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _dirPath.InspectorField(setup);
        _queryRegex.InspectorField(setup);
        _fileExtension.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        if (string.IsNullOrWhiteSpace(QueryRegex))
        {
            throw new ArgumentException("QueryRegex is not set");
        }

        string targetPath = string.IsNullOrWhiteSpace(DirPath) ? workspaceDir : DirPath;

        if (!Path.IsPathRooted(targetPath))
        {
            targetPath = Path.Combine(workspaceDir, targetPath);
        }

        if (!Directory.Exists(targetPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {targetPath}");
        }

        HashSet<string> extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(FileExtension))
        {
            var exts = FileExtension.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in exts)
            {
                string trimmed = ext.Trim();
                if (!trimmed.StartsWith("."))
                    trimmed = "." + trimmed;
                extensions.Add(trimmed);
            }
        }

        Regex regex;
        try
        {
            regex = new Regex(QueryRegex);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {ex.Message}");
        }

        var results = new List<string>();
        int totalMatches = 0;

        SearchDirectory(targetPath, regex, extensions, results, ref totalMatches);

        return Task.FromResult(new Output
        {
            Results = string.Join(Environment.NewLine, results),
            MatchCount = totalMatches,
        });
    }

    private void SearchDirectory(string dirPath, Regex regex, HashSet<string> extensions, List<string> results, ref int totalMatches)
    {
        try
        {
            var files = Directory.GetFiles(dirPath);

            foreach (var file in files)
            {
                if (extensions.Count > 0)
                {
                    string ext = Path.GetExtension(file);
                    if (!extensions.Contains(ext))
                        continue;
                }

                try
                {
                    var lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var matches = regex.Matches(lines[i]);
                        if (matches.Count > 0)
                        {
                            totalMatches += matches.Count;
                            results.Add($"{file}({i + 1}): {lines[i]}");
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            foreach (var subDir in Directory.GetDirectories(dirPath))
            {
                SearchDirectory(subDir, regex, extensions, results, ref totalMatches);
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (Exception)
        {
        }
    }
}