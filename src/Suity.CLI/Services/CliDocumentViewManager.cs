using Suity.Editor.Documents;

namespace Suity.Editor.Services;

internal class CliDocumentViewManager : DocumentViewManager
{
    public static CliDocumentViewManager Instance { get; } = new();

    public override IEnumerable<DocumentEntry> OpenedDocuments => [];

    public override DocumentEntry ActiveDocument => null;

    public override bool CloseDocument(DocumentEntry entry) => false;

    public override bool FocusDocument(DocumentEntry entry) => false;

    public override IDocumentView GetDocumentView(DocumentEntry entry) => null;

    public override IDocumentView ShowDocumentView(DocumentEntry entry) => null;
}
