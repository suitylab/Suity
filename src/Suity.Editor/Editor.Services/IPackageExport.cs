using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>Content kinds selected for a headless package export.</summary>
[Flags]
public enum PackageExportSelection
{
    None = 0,
    Asset = 1,
    WorkSpace = 2,
    System = 4,
    All = Asset | WorkSpace | System,
}

/// <summary>
/// Service interface for exporting packages.
/// </summary>
public interface IPackageExport
{
    /// <summary>
    /// Shows the export package dialog.
    /// </summary>
    /// <param name="files">The files to export.</param>
    /// <param name="workSpaces">The workspaces to export.</param>
    /// <param name="onComplete">Optional callback when export completes.</param>
    void ShowExportPackage(IEnumerable<string> files, IEnumerable<string> workSpaces, Action onComplete = null);

    /// <summary>
    /// Exports the current project to a <c>.suitypackage</c> without any UI (used by the
    /// in-editor "publish scaffold" flow). Returns the package path, or null when nothing
    /// was written.
    /// </summary>
    /// <param name="packageFileName">Destination path for the package archive.</param>
    /// <param name="selection">Which content kinds to include.</param>
    /// <param name="token">Cancellation token.</param>
    Task<string> ExportPackage(string packageFileName, PackageExportSelection selection, CancellationToken token = default);
}