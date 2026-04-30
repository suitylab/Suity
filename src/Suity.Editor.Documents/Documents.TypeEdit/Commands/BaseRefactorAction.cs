using Suity.Editor.Values;
using Suity.Synchonizing.Core;
using Suity.UndoRedos;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents.TypeEdit.Commands;

/// <summary>
/// Base class for refactoring undo/redo actions, providing document management and value migration utilities.
/// </summary>
internal abstract class BaseRefactorAction : UndoRedoAction
{
    readonly HashSet<Document> _docs = [];

    /// <summary>
    /// Opens all documents that reference the specified struct to prevent data loss during refactoring.
    /// </summary>
    /// <param name="structId">The ID of the struct to find references for.</param>
    protected void PreopenDocument(Guid structId)
    {
        foreach (var docHost in ReferenceManager.Current.FindReferenceHosts(structId).OfType<DocumentReferenceHost>())
        {
            docHost.OpenDocument();
        }
    }

    /// <summary>
    /// Enumerates all SObject instances that reference the specified struct type.
    /// </summary>
    /// <param name="structId">The ID of the struct to find references for.</param>
    /// <returns>An enumerable of SObject instances referencing the struct.</returns>
    protected IEnumerable<SObject> EnumerateSObject(Guid structId)
    {
        foreach (var refItem in ReferenceManager.Current.FindReference(structId))
        {
            if (refItem.Owner is not DocumentReferenceHost docHost)
            {
                continue;
            }

            var doc = docHost.OpenDocument();
            if (doc is null)
            {
                continue;
            }

            if (!Visitor.TryGetValueDeep(doc, refItem.Path, out var result))
            {
                continue;
            }

            if (result is not SObject obj)
            {
                continue;
            }

            if (obj.ObjectType is null || obj.ObjectType.TargetId != structId)
            {
                continue;
            }

            _docs.Add(doc);

            yield return obj;
        }
    }

    /// <summary>
    /// Saves all documents that were modified during the refactoring operation.
    /// </summary>
    protected void SaveDocuments()
    {
        foreach (var doc in _docs)
        {
            var logItem = new ObjectLogItem(L($"Applying external document modification:{doc.Entry}..."), doc);
            Logs.LogInfo(logItem);
            doc.Entry.ForceSave();
        }

        _docs.Clear();
    }
}
