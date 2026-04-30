using Suity.Editor.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// Document format
/// </summary>
public abstract class DocumentFormat
{
    /// <summary>
    /// Document format name
    /// </summary>
    public abstract string FormatName { get; }

    /// <summary>
    /// Gets additional format names this format is known by.
    /// </summary>
    public virtual string[] FormatNames => [];

    /// <summary>
    /// Gets the primary file extension for this format.
    /// </summary>
    public abstract string Extension { get; }

    /// <summary>
    /// Get file extension
    /// </summary>
    /// <returns>Returns file extension</returns>
    public abstract string[] GetAdditionalExtensions();

    /// <summary>
    /// Get type display name
    /// </summary>
    public abstract string DisplayText { get; }

    /// <summary>
    /// Get data source object icon
    /// </summary>
    /// <returns></returns>
    public virtual Image Icon => null;

    /// <summary>
    /// Gets whether new documents of this format can be created.
    /// </summary>
    [DefaultValue(true)]
    public virtual bool CanCreate => true;

    /// <summary>
    /// Gets whether documents of this format can show a view.
    /// </summary>
    [DefaultValue(true)]
    public virtual bool CanShowView => true;

    /// <summary>
    /// Gets whether documents of this format can show in property editor.
    /// </summary>
    [DefaultValue(false)]
    public virtual bool CanShowAsProperty => false;

    /// <summary>
    /// Handle UI creation action
    /// </summary>
    /// <param name="context">Resource context</param>
    /// <param name="basePath">Folder where file is created</param>
    /// <returns>Returns file name to create, not full path</returns>
    public virtual Task<string> OpenCreationUI(string basePath)
    {
        string ext = Extension;

        return EditorServices.FileNameService.ShowCreateDocumentDialogAsync(basePath, FormatName, ext);
    }

    /// <summary>
    /// Creates a new document silently without showing UI.
    /// </summary>
    /// <param name="basePath">The folder where the document will be created.</param>
    /// <returns>The created document entry, or null if creation failed.</returns>
    public virtual DocumentEntry SilentCreate(string basePath)
    {
        string ext = Extension;

        string fileName = EditorServices.FileNameService.GetIncrementalFileName(basePath, FormatName, ext);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        string fullPath = Path.Combine(basePath, fileName);
        if (File.Exists(fullPath))
        {
            return null;
        }

        return DocumentManager.Instance.NewDocument(fullPath, this);
    }

    /// <summary>
    /// Create document object
    /// </summary>
    /// <returns>Returns created document object</returns>
    public abstract Type DocumentType { get; }

    /// <summary>
    /// Gets the category this format belongs to.
    /// </summary>
    public virtual string Category { get; }

    /// <summary>
    /// Gets the display order of this format.
    /// </summary>
    public virtual int Order { get; }

    /// <summary>
    /// Gets the loading iteration for this format.
    /// </summary>
    public virtual LoadingIterations Iteration { get; } = LoadingIterations.Iteration1;

    /// <summary>
    /// Gets whether this format is attached to another document.
    /// </summary>
    public virtual bool IsAttached => false;
}