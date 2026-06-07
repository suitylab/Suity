using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("SearchFileRegex", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Search File Regex")]
[ToolTipsText("Search for regex patterns in the workspace.")]
[NativeAlias("Suity.Editor.AIGC.SearchFileRegex")]
public class SearchFileRegex : ToolCommand<SearchFileRegex.Output>
{
    public class Output : SObjectController
    {
        readonly TextBlockProperty _results = new("Results");
        readonly ValueProperty<int> _matchCount = new("MatchCount", 0);

        public string Results { get => _results.Text; set => _results.Text = value; }
        public int MatchCount { get => _matchCount.Value; set => _matchCount.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _results.Sync(sync);
            _matchCount.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _results.InspectorField(setup);
            _matchCount.InspectorField(setup);
        }

        public override string ToString() => $"{Results} ({MatchCount})";
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

        string relativePath = string.IsNullOrWhiteSpace(DirPath) ? "" : DirPath.TrimStart('/', '\\');
        string fullPath = string.IsNullOrWhiteSpace(relativePath) ? workspaceDir : relativePath;
        string outputBasePath = workspaceDir;
        bool useRelativeOutput = !string.IsNullOrWhiteSpace(DirPath) && !Path.IsPathRooted(DirPath);

        if (!string.IsNullOrWhiteSpace(relativePath) && !Path.IsPathRooted(relativePath))
        {
            fullPath = Path.Combine(workspaceDir, relativePath);
        }

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {relativePath}");
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

        SearchDirectory(fullPath, outputBasePath, useRelativeOutput, regex, extensions, results, ref totalMatches);

        return Task.FromResult(new Output
        {
            Results = string.Join(Environment.NewLine, results),
            MatchCount = totalMatches,
        });
    }

    private void SearchDirectory(string dirPath, string outputBasePath, bool useRelativeOutput, Regex regex, HashSet<string> extensions, List<string> results, ref int totalMatches)
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
                    string outputPath = useRelativeOutput && file.StartsWith(outputBasePath, StringComparison.OrdinalIgnoreCase)
                        ? file.Substring(outputBasePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        : file;

                    var lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var matches = regex.Matches(lines[i]);
                        if (matches.Count > 0)
                        {
                            totalMatches += matches.Count;
                            results.Add($"{outputPath}({i + 1}): {lines[i]}");
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            foreach (var subDir in Directory.GetDirectories(dirPath))
            {
                SearchDirectory(subDir, outputBasePath, useRelativeOutput, regex, extensions, results, ref totalMatches);
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