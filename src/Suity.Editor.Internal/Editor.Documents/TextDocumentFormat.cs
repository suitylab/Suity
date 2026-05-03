using Suity.Drawing;
using Suity.Editor.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.Documents;

/// <summary>
/// Default document format for text-based files including source code, markup, configuration, and plain text.
/// </summary>
[BaseTextDocumentFormat]
[DefaultDocumentFormat]
public class TextDocumentFormat : DocumentFormat
{
    /// <summary>
    /// Supported file extensions for text-based documents.
    /// </summary>
    public static readonly string[] Exts =
    [
        "txt", "md",
        "asp","aspx","asax","asmx",
        "bat",
        "boo",
        "atg",
        "c", "h", "cc", "cpp","hpp",
        "cs", "csx",
        "htm", "html", "xhtml", //"html" (HtmlDocument occupied),
        "css", "scss",
        "java",
        "js", "ts", "tsx",
        "patch","diff",
        "php",
        "py","pyw",
        "tex",
        "vb",
        "xml","xsl","xslt","xsd","manifest","config","addin","xshd","wxs","wxi","wxl","proj","csproj","vbproj","ilproj","booproj","build","xfrm","targets","xaml","xpt","xft","map","wsdl","disco","resx","settings",
        "ddproject", "ddsource", "ddproto",
        "json",
        "sln",
        "gitignore"
    ];

    /// <inheritdoc/>
    public override string FormatName => "Text";

    /// <inheritdoc/>
    public override string Extension => null;

    /// <inheritdoc/>
    public override string[] GetAdditionalExtensions()
    {
        return Exts;
    }

    /// <inheritdoc/>
    public override string DisplayText => "Text Document";

    /// <inheritdoc/>
    public override ImageDef Icon => CoreIconCache.Text;

    /// <inheritdoc/>
    public override bool CanCreate => true;

    /// <inheritdoc/>
    public override bool CanShowView => true;

    /// <inheritdoc/>
    public override bool CanShowAsProperty => false;

    /// <inheritdoc/>
    public override Task<string> OpenCreationUI(string basePath)
    {
        return EditorServices.FileNameService.ShowCreateDocumentDialogAsync(basePath, "Text", "txt");
    }

    /// <inheritdoc/>
    public override Type DocumentType => typeof(TextDocument);
}