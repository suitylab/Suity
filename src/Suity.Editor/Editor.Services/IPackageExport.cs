using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

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
}