using System;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

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
    Task ImportPackage(string fileName, string packageFullName = null);
}