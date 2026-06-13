using Suity.Editor.Documents;
using Suity.UndoRedos;
using System;

namespace Suity.Editor.Controls;

public class DocumentViewHost : IDocumentViewHost
{
    private UndoRedoManager? _undoRedoManager;

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(UndoRedoManager))
        {
            _undoRedoManager ??= new UndoRedoManager();

            return _undoRedoManager;
        }

        return null;
    }
}
