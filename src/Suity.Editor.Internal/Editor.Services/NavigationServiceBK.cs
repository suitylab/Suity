using Suity.Editor.Documents;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Services;

/// <summary>
/// Provides navigation history tracking for documents, supporting backward and forward navigation.
/// </summary>
internal class NavigationServiceBK : NavigationService
{
    /// <summary>
    /// Singleton instance of the navigation service.
    /// </summary>
    public static NavigationServiceBK Instance { get; } = new();

    /// <summary>
    /// Represents a single navigation record with file path and selection state.
    /// </summary>
    private class NaviRecord
    {
        /// <summary>
        /// The storage location of the navigated document.
        /// </summary>
        public StorageLocation FilePath { get; set; }

        /// <summary>
        /// The selection state within the document.
        /// </summary>
        public ViewSelection Selection { get; set; }
    }

    /// <summary>
    /// Maximum number of navigation records to keep in history.
    /// </summary>
    public const int MaxSize = 100;

    private readonly LinkedList<NaviRecord> _actions = new();
    private readonly LinkedList<NaviRecord> _actions_b = new();

    private bool _undoRedoInAction;

    /// <inheritdoc/>
    public override void AddRecord(Document document)
    {
        if (_undoRedoInAction)
        {
            return;
        }

        if (document?.View is not IViewSelectable selectable)
        {
            return;
        }

        var selection = selectable.GetSelection();
        if (selection is null)
        {
            return;
        }

        var record = new NaviRecord
        {
            FilePath = document.FileName,
            Selection = selection,
        };

        _actions.AddLast(record);

        while (_actions.Count > MaxSize)
        {
            _actions.RemoveFirst();
        }

        _actions_b.Clear();
    }

    /// <inheritdoc/>
    public override void BackwardNavigation()
    {
        if (_undoRedoInAction)
        {
            return;
        }

        if (_actions.Count < 2)
        {
            return;
        }

        var record = _actions.Last;
        _actions.RemoveLast();

        _actions_b.AddLast(record);
        while (_actions_b.Count > MaxSize)
        {
            _actions_b.RemoveFirst();
        }

        record = _actions.Last;

        DoSelection(record.Value);
    }

    /// <inheritdoc/>
    public override void ForwardNavigation()
    {
        if (_undoRedoInAction)
        {
            return;
        }

        if (_actions_b.Count < 1)
        {
            return;
        }

        var record = _actions_b.Last;
        _actions_b.RemoveLast();

        _actions.AddLast(record);
        while (_actions.Count > MaxSize)
        {
            _actions.RemoveFirst();
        }

        DoSelection(record.Value);
    }

    /// <inheritdoc/>
    public override bool HasBackward => _actions.Count > 1;

    /// <inheritdoc/>
    public override bool HasForward => _actions_b.Count > 0;

    /// <summary>
    /// Executes navigation to a specific record by opening the document and restoring selection.
    /// </summary>
    /// <param name="record">The navigation record to navigate to.</param>
    private void DoSelection(NaviRecord record)
    {
        if (_undoRedoInAction)
        {
            return;
        }

        if (record is null)
        {
            return;
        }

        try
        {
            _undoRedoInAction = true;

            var document = DocumentManager.Instance.OpenDocument(record.FilePath);
            if (document?.ShowView() is IViewSelectable selectable)
            {
                DocumentViewManager.Current.FocusDocument(document);
                selectable.SetSelection(record.Selection);
            }
        }
        catch (Exception err)
        {
            err.LogError();
        }
        finally
        {
            _undoRedoInAction = false;
        }
    }
}
