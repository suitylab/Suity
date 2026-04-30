using Suity.Editor.ProjectGui.Nodes;
using Suity.Helpers;
using Suity.Views.Menu;

namespace Suity.Editor.ProjectGui.Commands;

/// <summary>
/// Command that imports a package into the project.
/// </summary>
public class ImportPackageCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportPackageCommand"/> class.
    /// </summary>
    public ImportPackageCommand()
      : base("Import", CoreIconCache.Export.ToIconSmall())
    {
        AcceptType<AssetRootNode>(false);
        AcceptType<AssetDirectoryNode>(false);
        AcceptType<WorkSpaceRootNode>(false);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        HandleImport(view);
    }

    /// <summary>
    /// Handles the import operation for the specified project view.
    /// </summary>
    /// <param name="view">The project GUI view.</param>
    public static void HandleImport(IProjectGui view)
    {
    }

    /// <summary>
    /// Handles importing a package from the specified file.
    /// </summary>
    /// <param name="fileName">The path to the package file to import.</param>
    /// <param name="packageFullName">The full name of the package, or null to infer from the file.</param>
    public static void HandleImport(string fileName, string packageFullName = null)
    {
        EditorUtility.ShowImportPackage(fileName, packageFullName);
    }
}