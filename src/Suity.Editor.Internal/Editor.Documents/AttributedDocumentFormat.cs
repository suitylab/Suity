using Suity.Drawing;
using Suity.Editor.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// A document format derived from attributes applied to a Document type.
/// </summary>
internal class AttributedDocumentFormat : DocumentFormat
{
    private readonly Type _documentType;
    private readonly DocumentFormatAttribute _attribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributedDocumentFormat"/> class.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="attribute">The document format attribute.</param>
    public AttributedDocumentFormat(Type documentType, DocumentFormatAttribute attribute)
    {
        _documentType = documentType;
        _attribute = attribute;
    }

    /// <inheritdoc/>
    public override string FormatName => _attribute.FormatName;

    /// <inheritdoc/>
    public override string[] FormatNames => _attribute.FormatNames?.ToArray() ?? [];

    /// <inheritdoc/>
    public override string Extension => null;

    /// <inheritdoc/>
    public override string[] GetAdditionalExtensions() => _attribute.Extensions;

    /// <inheritdoc/>
    public override string DisplayText => _attribute.DisplayText;

    /// <inheritdoc/>
    public override ImageDef Icon => EditorUtility.GetIconByAssetKey(_attribute.Icon);

    /// <inheritdoc/>
    public override bool CanCreate => _attribute.CanCreate;

    /// <inheritdoc/>
    public override bool CanShowView => _attribute.CanShowView;

    /// <inheritdoc/>
    public override bool CanShowAsProperty => _attribute.CanShowAsProperty;

    /// <inheritdoc/>
    public override Task<string> OpenCreationUI(string basePath)
    {
        return EditorServices.FileNameService.ShowCreateDocumentDialogAsync(basePath, _attribute.FormatName, _attribute.Extension);
    }

    /// <inheritdoc/>
    public override Type DocumentType => _documentType;

    /// <inheritdoc/>
    public override string Category => _attribute.Categoty;

    /// <inheritdoc/>
    public override int Order => _attribute.Order;

    /// <inheritdoc/>
    public override LoadingIterations Iteration => _attribute.Iteration;

    /// <inheritdoc/>
    public override bool IsAttached => _attribute.IsAttached;

    /// <inheritdoc/>
    public override string ToString() => _attribute.DisplayText;
}