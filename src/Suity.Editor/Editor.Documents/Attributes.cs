using System;

namespace Suity.Editor.Documents;

/// <summary>
/// Specifies document properties
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class DocumentFormatAttribute : Attribute
{
    private string[] _ext;

    /// <summary>
    /// Specifies a single file extension
    /// </summary>
    public string Extension
    {
        get
        {
            if (_ext?.Length > 0)
            {
                return _ext[0];
            }
            else
            {
                return null;
            }
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _ext = [value];
            }
            else
            {
                _ext = null;
            }
        }
    }

    /// <summary>
    /// Specifies multiple file extensions
    /// </summary>
    public string[] Extensions
    {
        get
        {
            return _ext != null ? [.. _ext] : [];
        }
        set
        {
            if (value != null)
            {
                _ext = [.. value];
            }
            else
            {
                _ext = null;
            }
        }
    }

    /// <summary>
    /// The format name of the document
    /// </summary>
    public string FormatName { get; set; }

    /// <summary>
    /// Specifies multiple format names
    /// </summary>
    public string[] FormatNames { get; set; }

    /// <summary>
    /// The display name of the document
    /// </summary>
    public string DisplayText { get; set; }

    /// <summary>
    /// The icon of the document
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Whether it can be created
    /// </summary>
    public bool CanCreate { get; set; } = true;

    /// <summary>
    /// Whether it can be opened as a view
    /// </summary>
    public bool CanShowView { get; set; } = true;

    /// <summary>
    /// Whether it can be displayed in the property editor
    /// </summary>
    public bool CanShowAsProperty { get; set; }

    /// <summary>
    /// The category of the document format.
    /// </summary>
    public string Categoty { get; set; }

    /// <summary>
    /// The display order of the document format.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The loading iteration for this format.
    /// </summary>
    public LoadingIterations Iteration { get; set; } = LoadingIterations.Iteration1;

    /// <summary>
    /// Whether this format is attached to another document.
    /// </summary>
    public bool IsAttached { get; set; }
}


/// <summary>
/// Specifies unique default document property
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class BaseTextDocumentFormatAttribute : Attribute
{
}

/// <summary>
/// Specifies the default document format for a file type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class DefaultDocumentFormatAttribute : Attribute
{
}

/// <summary>
/// Document view usage
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class DocumentViewUsageAttribute : Attribute
{
    /// <summary>
    /// Gets the document type this view usage applies to.
    /// </summary>
    public Type DocumentType { get; }

    /// <summary>
    /// Gets or sets the format name.
    /// </summary>
    public string FormatName { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of DocumentViewUsageAttribute with a format name.
    /// </summary>
    /// <param name="formatName">The format name.</param>
    public DocumentViewUsageAttribute(string formatName)
    {
        FormatName = formatName;
    }

    /// <summary>
    /// Initializes a new instance of DocumentViewUsageAttribute with a document type.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    public DocumentViewUsageAttribute(Type documentType)
    {
        DocumentType = documentType;
    }
}

/// <summary>
/// [Obsolete] Specifies that a document uses a split view layout.
/// </summary>
[System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
[Obsolete]
public sealed class DocumentSplittedViewAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of DocumentSplittedViewAttribute.
    /// </summary>
    public DocumentSplittedViewAttribute()
    {
    }
}

/// <summary>
/// Knowledge base configuration
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class KnowledgeGenerateAttribute : Attribute
{
    /// <summary>
    /// Enable vector knowledge base
    /// </summary>
    public bool VectorEnabled { get; set; }

    /// <summary>
    /// Enable feature knowledge base
    /// </summary>
    public bool FeatureEnabled { get; set; }

    /// <summary>
    /// Initializes a new instance of KnowledgeGenerateAttribute.
    /// </summary>
    public KnowledgeGenerateAttribute()
    {
    }
}