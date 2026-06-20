using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Tools;

[NativeType("SearchFile", CodeBase = "*Suity", Category = "WorkSpace Tools")]
[DisplayText("Search File")]
[ToolTipsText("Search for keywords in the workspace.")]
[NativeAlias("Suity.Editor.AIGC.SearchFile")]
public class SearchFile : ToolCommand<SearchFile.Output>
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
    readonly StringProperty _query = new("Query", "Query", string.Empty, "The search keyword.");
    readonly ValueProperty<bool> _caseSensitive = new("CaseSensitive", "Case Sensitive", false, "Whether the search is case sensitive.");
    readonly ValueProperty<bool> _matchWholeWord = new("MatchWholeWord", "Match Whole Word", false, "Whether to match whole words only.");
    readonly StringProperty _fileExtension = new("FileExtension", "File Extension", string.Empty, "Filter by file extension, e.g. .cs, .json. Multiple extensions separated by comma.");

    public string DirPath { get => _dirPath.Text; set => _dirPath.Text = value; }
    public string Query { get => _query.Text; set => _query.Text = value; }
    public bool CaseSensitive { get => _caseSensitive.Value; set => _caseSensitive.Value = value; }
    public bool MatchWholeWord { get => _matchWholeWord.Value; set => _matchWholeWord.Value = value; }
    public string FileExtension { get => _fileExtension.Text; set => _fileExtension.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _dirPath.Sync(sync);
        _query.Sync(sync);
        _caseSensitive.Sync(sync);
        _matchWholeWord.Sync(sync);
        _fileExtension.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _dirPath.InspectorField(setup);
        _query.InspectorField(setup);
        _caseSensitive.InspectorField(setup);
        _matchWholeWord.InspectorField(setup);
        _fileExtension.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.RootDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        if (string.IsNullOrWhiteSpace(Query))
        {
            throw new ArgumentException("Query is not set");
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

        string searchQuery = CaseSensitive ? Query : Query.ToLowerInvariant();

        string searchInfo = string.IsNullOrWhiteSpace(DirPath) ? $"keyword: {Query}" : $"keyword: {Query}, dir: {relativePath}";
        context.ToolInstance.Conversation?.AddRunningMessage("Search file", msg =>
        {
            msg.AddCode(searchInfo);
        });
        context.Conversation?.AddRunningMessage("Search file", msg =>
        {
            msg.AddCode(searchInfo);
        });

        var results = new List<string>();
        int totalMatches = 0;

        SearchDirectory(fullPath, outputBasePath, useRelativeOutput, searchQuery, extensions, results, ref totalMatches);

        return Task.FromResult(new Output
        {
            Results = string.Join(Environment.NewLine, results),
            MatchCount = totalMatches,
        });
    }

    private void SearchDirectory(string dirPath, string outputBasePath, bool useRelativeOutput, string searchQuery, HashSet<string> extensions, List<string> results, ref int totalMatches)
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
                        string line = CaseSensitive ? lines[i] : lines[i].ToLowerInvariant();
                        bool found = MatchWholeWord ? ContainsWholeWord(line, searchQuery) : line.Contains(searchQuery);

                        if (found)
                        {
                            totalMatches++;
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
                SearchDirectory(subDir, outputBasePath, useRelativeOutput, searchQuery, extensions, results, ref totalMatches);
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (Exception)
        {
        }
    }

    private bool ContainsWholeWord(string line, string word)
    {
        int index = 0;
        while ((index = line.IndexOf(word, index, StringComparison.Ordinal)) != -1)
        {
            bool validBefore = index == 0 || !char.IsLetterOrDigit(line[index - 1]);
            bool validAfter = index + word.Length >= line.Length || !char.IsLetterOrDigit(line[index + word.Length]);

            if (validBefore && validAfter)
                return true;

            index++;
        }
        return false;
    }
}