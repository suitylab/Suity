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

[NativeType("GetWorkspaceTree", CodeBase = "*Suity")]
[DisplayText("Get Workspace Tree")]
[ToolTipsText("Get project folder/file structure tree. Helps Agent quickly establish a global perspective of routing.")]
public class GetWorkspaceTree : ToolCommand<GetWorkspaceTree.Output>
{
    public class Output : IViewObject
    {
        readonly TextBlockProperty _tree = new("Tree");

        public string Tree { get => _tree.Text; set => _tree.Text = value; }

        public void Sync(IPropertySync sync, ISyncContext context)
        {
            _tree.Sync(sync);
        }
        public void SetupView(IViewObjectSetup setup)
        {
            _tree.InspectorField(setup);
        }
    }

    readonly StringProperty _path = new("Path", "Path", string.Empty, "The relative path to the directory to scan. If empty, the workspace directory is used.");
    readonly ValueProperty<int> _maxDepth = new("MaxDepth", "Max Depth", 0, "The maximum depth to scan. 0 means unlimited.");
    readonly StringProperty _ignorePatterns = new("IgnorePatterns", "Ignore Patterns", string.Empty, "Comma or semicolon separated list of patterns to ignore.");

    public string Path { get => _path.Text; set => _path.Text = value; }
    public int MaxDepth { get => _maxDepth.Value; set => _maxDepth.Value = value; }
    public string IgnorePatterns { get => _ignorePatterns.Text; set => _ignorePatterns.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _path.Sync(sync);
        _maxDepth.Sync(sync);
        _ignorePatterns.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _path.InspectorField(setup);
        _maxDepth.InspectorField(setup);
        _ignorePatterns.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        string workspaceDir = context.WorkSpaceDirectory;
        if (string.IsNullOrWhiteSpace(workspaceDir))
        {
            throw new NullReferenceException("Workspace directory is not set");
        }

        string targetPath = string.IsNullOrWhiteSpace(Path) ? workspaceDir : Path;

        if (!System.IO.Path.IsPathRooted(targetPath))
        {
            targetPath = System.IO.Path.Combine(workspaceDir, targetPath);
        }

        if (!Directory.Exists(targetPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {targetPath}");
        }

        int depthLimit = MaxDepth;
        HashSet<string> ignoreSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(IgnorePatterns))
        {
            var patterns = IgnorePatterns.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pattern in patterns)
            {
                ignoreSet.Add(pattern.Trim());
            }
        }

        string tree = BuildDirectoryTree(targetPath, depthLimit, ignoreSet, 0);

        return Task.FromResult(new Output
        {
            Tree = tree,
        });
    }

    private string BuildDirectoryTree(string rootPath, int depthLimit, HashSet<string> ignoreSet, int currentDepth)
    {
        var lines = new List<string>();

        try
        {
            var dirInfo = new DirectoryInfo(rootPath);

            var entries = dirInfo.GetFileSystemInfos()
                .Where(f => !ignoreSet.Contains(f.Name))
                .OrderBy(f => f is DirectoryInfo ? 0 : 1)
                .ThenBy(f => f.Name);

            foreach (var entry in entries)
            {
                string prefix = currentDepth == 0 ? "" : new string(' ', currentDepth * 4) + "|-- ";

                if (entry is DirectoryInfo dir)
                {
                    lines.Add($"{prefix}{dir.Name}/");

                    if (depthLimit <= 0 || currentDepth < depthLimit)
                    {
                        var subTree = BuildDirectoryTree(dir.FullName, depthLimit, ignoreSet, currentDepth + 1);
                        if (!string.IsNullOrEmpty(subTree))
                        {
                            lines.Add(subTree);
                        }
                    }
                }
                else
                {
                    lines.Add($"{prefix}{entry.Name}");
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            lines.Add($"{new string(' ', currentDepth * 4)}[Access Denied]");
        }
        catch (Exception ex)
        {
            lines.Add($"{new string(' ', currentDepth * 4)}[Error: {ex.Message}]");
        }

        return string.Join(Environment.NewLine, lines);
    }
}