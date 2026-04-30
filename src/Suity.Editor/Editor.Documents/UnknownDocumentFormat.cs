using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// Represents a document format for unknown file types.
/// </summary>
public class UnknownDocumentFormat : DocumentFormat
{
    /// <summary>
    /// Gets the singleton instance of UnknownDocumentFormat.
    /// </summary>
    public static readonly UnknownDocumentFormat Instance = new();

    /// <summary>
    /// Gets the format name.
    /// </summary>
    public override string FormatName => "Unknown";

    /// <summary>
    /// Gets the file extension (none for unknown format).
    /// </summary>
    public override string Extension => null;

    /// <summary>
    /// Gets additional file extensions (none for unknown format).
    /// </summary>
    /// <returns>Null.</returns>
    public override string[] GetAdditionalExtensions() => null;

    /// <summary>
    /// Gets the display text for the format.
    /// </summary>
    public override string DisplayText => "Unknown";

    /// <summary>
    /// Gets the icon for this format (none for unknown format).
    /// </summary>
    public override Image Icon => null;

    /// <summary>
    /// Gets whether new documents of this format can be created.
    /// </summary>
    public override bool CanCreate => false;

    /// <summary>
    /// Gets whether documents of this format can show a view.
    /// </summary>
    public override bool CanShowView => false;

    /// <summary>
    /// Gets whether documents of this format can show in property editor.
    /// </summary>
    public override bool CanShowAsProperty => false;

    /// <summary>
    /// Opens the creation UI for this format (not supported).
    /// </summary>
    /// <param name="basePath">The base path for creating the document.</param>
    /// <returns>Null task result.</returns>
    public override Task<string> OpenCreationUI(string basePath) => Task.FromResult<string>(null);

    /// <summary>
    /// Gets the document type (none for unknown format).
    /// </summary>
    public override Type DocumentType => null;
}