using System;
using System.Threading.Tasks;

namespace Suity.Editor.Services;


[Flags]
public enum ImportOptions
{
    None = 0,
    Asset = 1,
    Workspace = 2,
    System = 4,
    All = Asset | Workspace | System
}

/// <summary>
/// Service interface for importing packages.
/// </summary>
public interface IPackageImport
{
    /// <summary>
    /// Shows the import package GUI.
    /// </summary>
    /// <param name="fileName">The file name to import.</param>
    /// <param name="packageFullName">The full package name.</param>
    /// <param name="onComplete">Optional callback when import completes.</param>
    void ShowImportPackageGui(string fileName, string packageFullName = null, Action onComplete = null);

    /// <summary>
    /// Imports a package.
    /// </summary>
    /// <param name="fileName">The file name to import.</param>
    /// <param name="packageFullName">The full package name.</param>
    /// <param name="onComplete">Optional callback when import completes.</param>
    Task ImportPackage(string fileName, string packageFullName = null, ImportOptions options = ImportOptions.All);

    /// <summary>
    /// Removes the files that were written into the project by a previously
    /// imported package. Only asset files are removed; workspace and system
    /// files are left untouched.
    /// </summary>
    /// <param name="fileName">The path to the package file to clean up.</param>
    /// <param name="options">The package content kinds to remove.</param>
    Task CleanUpPackage(string fileName, ImportOptions options = ImportOptions.Asset);
}