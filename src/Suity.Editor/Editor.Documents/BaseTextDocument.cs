using Suity.Editor.Documents.Linked;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System.Drawing;
using System.Text;

namespace Suity.Editor.Documents;

/// <summary>
/// Base class for text-based documents that support encoding and text content management.
/// </summary>
public abstract class BaseTextDocument : AssetDocument
{
    private string _text = string.Empty;

    /// <summary>
    /// Initializes a new instance of the BaseTextDocument class.
    /// </summary>
    public BaseTextDocument()
        : base(new TextDocumentAssetBuilder())
    {
    }

    /// <summary>
    /// Initializes a new instance of the BaseTextDocument class with a custom asset builder.
    /// </summary>
    /// <param name="assetBuilder">The asset builder to use for this document.</param>
    protected BaseTextDocument(AssetBuilder assetBuilder)
        : base(assetBuilder)
    {
    }

    /// <summary>
    /// Gets or sets the code page encoding for the document text.
    /// </summary>
    public int CodePage { get; set; } = Encoding.UTF8.CodePage;

    /// <summary>
    /// Gets or sets the text content of the document.
    /// </summary>
    public string TextContent
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            OnContentChanged();
        }
    }

    /// <summary>
    /// Gets the icon for this document type.
    /// </summary>
    public override Image Icon => CoreIconCache.Text;

    /// <summary>
    /// Loads the document from storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <param name="loaderObject">The loader object.</param>
    /// <returns>True if load was successful.</returns>
    protected internal override bool LoadDocument(IStorageItem op, object loaderObject, DocumentLoadingIntent intent)
    {
        EncodingFileInfo fileInfo = TextFileHelper.GetEncodingFileInfo(op.GetInputStream().ToBytes());
        CodePage = fileInfo.CodePage;
        TextContent = fileInfo.Contents ?? string.Empty;
        return true;
    }

    /// <summary>
    /// Saves the document to storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <returns>True if save was successful.</returns>
    protected internal override bool SaveDocument(IStorageItem op)
    {
        TextFileHelper.WriteFile(op.GetOutputStream(), TextContent, Encoding.GetEncoding(CodePage), true);
        return true;
    }

    /// <summary>
    /// Exports the document to storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <returns>True if export was successful.</returns>
    protected internal override bool ExportDocument(IStorageItem op)
    {
        TextFileHelper.WriteFile(op.GetOutputStream(), TextContent, Encoding.GetEncoding(CodePage), true);
        return true;
    }

    /// <summary>
    /// Called when the document content is reset.
    /// </summary>
    protected internal override void OnReset()
    {
        base.OnReset();
        TextContent = string.Empty;
    }

    /// <summary>
    /// Called when the content changes.
    /// </summary>
    protected virtual void OnContentChanged()
    {
        AssetBuilder?.UpdateAsset();
    }

    /// <summary>
    /// Searches for text within the document.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="findStr">The search string.</param>
    /// <param name="findOption">The search option.</param>
    public override void Find(ValidationContext context, string findStr, SearchOption findOption)
    {
        base.Find(context, findStr, findOption);

        //TODO: Will cause - Call from invalid thread.
        /*if (this.View is { } view)
        {
            view.SetDataToDocument();
        }*/

        if (TextHelper.SearchText(_text, findStr, findOption) is { } results)
        {
            foreach (var result in results)
            {
                context.Report(result.lineString, result);
            }
        }
    }
}

/// <summary>
/// Generic base class for text-based documents with a specific asset builder type.
/// </summary>
/// <typeparam name="TAssetBuilder">The type of asset builder to use.</typeparam>
public abstract class BaseTextDocument<TAssetBuilder> : BaseTextDocument
    where TAssetBuilder : AssetBuilder, new()
{
    /// <summary>
    /// Initializes a new instance of the BaseTextDocument class.
    /// </summary>
    public BaseTextDocument()
        : base(new TAssetBuilder())
    {
    }
}

/// <summary>
/// Represents a simple text document with no special formatting.
/// </summary>
[DisplayText("Text Document", "*CoreIcon|Text")]
public class TextDocument : BaseTextDocument
{
    /// <summary>
    /// Initializes a new instance of the TextDocument class.
    /// </summary>
    public TextDocument()
    {
    }
}

/// <summary>
/// Asset type representing a text document that can be embedded in the system.
/// </summary>
public class TextDocumentAsset : TextAsset
{
    /// <summary>
    /// Initializes a new instance of the TextDocumentAsset class.
    /// </summary>
    public TextDocumentAsset()
    {
        this.ValueType = NativeTypes.TextBlockType;
    }

    /// <summary>
    /// Gets the text content from the associated document.
    /// </summary>
    /// <returns>The text content of the document.</returns>
    public override string GetText()
    {
        return this.GetDocument<BaseTextDocument>(true)?.TextContent;
    }

    /// <summary>
    /// Gets the text content as the value of this asset.
    /// </summary>
    public override object Value 
    {
        get => this.GetText(); 
        protected internal set { }
    }

    /// <summary>
    /// Gets the text content for a given caller context.
    /// </summary>
    /// <param name="caller">The caller object.</param>
    /// <param name="resolveContext">The condition resolve context.</param>
    /// <returns>The text content.</returns>
    public override object GetValue(object caller, ICondition resolveContext)
    {
        return this.GetText();
    }
}

/// <summary>
/// Builder for creating TextDocumentAsset instances.
/// </summary>
public class TextDocumentAssetBuilder : AssetBuilder<TextDocumentAsset>
{
    /// <summary>
    /// Initializes a new instance of the TextDocumentAssetBuilder class.
    /// </summary>
    public TextDocumentAssetBuilder()
    {
    }
}