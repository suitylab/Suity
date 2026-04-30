using Suity.Editor.Controls;
using Suity.Editor.Documents;
using Suity.Editor.Views;
using System.Collections.Generic;

namespace Suity.Editor.Services;

internal class AvaDocumentViewManager : DocumentViewManager
{
    public static AvaDocumentViewManager Instance { get; } = new();

    public override IEnumerable<DocumentEntry> OpenedDocuments 
        => ResolveContainer()?.OpenedDocuments ?? [];

    public override DocumentEntry? ActiveDocument 
        => ResolveContainer()?.ActiveDocument;

    public override bool CloseDocument(DocumentEntry entry)
        => ResolveContainer()?.CloseDocument(entry) == true;

    public override bool FocusDocument(DocumentEntry entry) 
        => ResolveContainer()?.FocusDocument(entry) == true;

    public override IDocumentView? GetDocumentView(DocumentEntry entry)
        => ResolveContainer()?.GetDocumentControl(entry)?.DocumentView;

    public override IDocumentView? ShowDocumentView(DocumentEntry entry) 
        => ResolveContainer()?.ShowDocumentView(entry);

    public static MainWindow? ResolveWindow() => SuityApp.Instance?.Window as MainWindow;

    public static EditorDockContainer? ResolveContainer() => ResolveWindow()?.View?.DockContainer;
}
