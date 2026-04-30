using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Documents;

/// <summary>
/// Interface for resolving document formats based on file content or extension.
/// </summary>
public interface IDocumentFormatResolver
{
    /// <summary>
    /// Gets the file extension this resolver handles.
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Resolves the document format by examining the stream content.
    /// </summary>
    /// <param name="stream">The stream to examine.</param>
    /// <returns>The resolve result containing the format information.</returns>
    IDocumentResolveResult ResolveDocumentFormat(Stream stream);

    /// <summary>
    /// Gets all supported formats by this resolver.
    /// </summary>
    IEnumerable<DocumentFormat> Formats { get; }
}

/// <summary>
/// Interface representing the result of a document format resolution.
/// </summary>
public interface IDocumentResolveResult : IDisposable
{
    /// <summary>
    /// Gets the resolved document format.
    /// </summary>
    DocumentFormat Format { get; }

    /// <summary>
    /// Gets the loader object for the document.
    /// </summary>
    object LoaderObject { get; }
}

/// <summary>
/// Implementation of IDocumentResolveResult for document format resolution.
/// </summary>
public class DocumentResolveResult : IDocumentResolveResult
{
    /// <summary>
    /// Gets or sets the resolved document format.
    /// </summary>
    public DocumentFormat Format { get; set; }

    /// <summary>
    /// Gets the loader object (always null for this implementation).
    /// </summary>
    public object LoaderObject => null;

    /// <summary>
    /// Disposes the resolve result (no resources to release).
    /// </summary>
    public void Dispose()
    {
    }
}