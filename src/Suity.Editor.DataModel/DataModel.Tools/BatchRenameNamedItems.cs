using Suity.Editor.DataModel.Actions;
using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.DataModel.Tools;

[NativeType("BatchRenameNamedItems", CodeBase = "*Suity", Category = "Asset Tools", Icon = "*CoreIcon|Asset")]
[DisplayText("Batch Rename Named Items")]
[ToolTipsText("Rename multiple named items in a SNamedDocument.")]
[NativeAlias("Suity.Editor.DataModel.BatchRenameNamedItems")]
public class BatchRenameNamedItems : ToolCommand<BatchRenameNamedItems.RenameResult>
{
    /// <summary>
    /// Represents a single item rename operation.
    /// </summary>
    [NativeType("BatchRenameNamedItems.RenameItem", CodeBase = "*Suity")]
    public class RenameItem : SObjectController
    {
        readonly StringProperty _oldName = new("OldName", "Old Name");
        readonly StringProperty _newName = new("NewName", "New Name");

        public string OldName { get => _oldName.Text; set => _oldName.Text = value; }
        public string NewName { get => _newName.Text; set => _newName.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _oldName.Sync(sync);
            _newName.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _oldName.InspectorField(setup);
            _newName.InspectorField(setup);
        }

        public override string ToString() => $"{OldName} -> {NewName}";
    }

    /// <summary>
    /// Result of a batch rename operation.
    /// </summary>
    [NativeType("BatchRenameNamedItems.RenameResult", CodeBase = "*Suity")]
    public class RenameResult : SObjectController
    {
        readonly ListProperty<RenameItem> _renamedItems = new("RenamedItems", "Renamed Items");
        readonly ListProperty<string> _itemsNotFound = new("ItemsNotFound", "Items Not Found");

        public List<RenameItem> RenamedItems => _renamedItems.List;
        public List<string> ItemsNotFound => _itemsNotFound.List;

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _renamedItems.Sync(sync);
            _itemsNotFound.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _renamedItems.InspectorField(setup);
            _itemsNotFound.InspectorField(setup);
        }

        public override string ToString() => $"Renamed: {RenamedItems.Count}, Not Found: {ItemsNotFound.Count}";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The relative path to the SNamedDocument.");
    readonly ListProperty<RenameItem> _renames = new("Renames", "Renames", "List of rename operations (OldName -> NewName).");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public List<RenameItem> Renames => _renames.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _renames.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _renames.InspectorField(setup);
    }

    public override Task<RenameResult> Run(ToolCallContext context)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
        }

        if (Renames == null || Renames.Count == 0)
        {
            throw new ArgumentException("Renames list is empty");
        }

        string relativePath = FilePath.TrimStart('/', '\\');

        string assetSpaceDir = Project.Current?.AssetDirectory;
        if (string.IsNullOrWhiteSpace(assetSpaceDir))
        {
            throw new NullReferenceException("Asset directory is not set");
        }

        string fullPath = relativePath;
        if (!System.IO.Path.IsPathRooted(relativePath))
        {
            fullPath = System.IO.Path.Combine(assetSpaceDir, relativePath);
        }

        context.ToolInstance.Conversation?.AddRunningMessage("Batch rename named items", msg =>
        {
            msg.AddCode(relativePath);
        });
        context.Conversation?.AddRunningMessage("Batch rename named items", msg =>
        {
            msg.AddCode(relativePath);
        });

        // Get or open document
        var docEntry = DocumentManager.Instance.GetDocument(fullPath);
        if (docEntry == null)
        {
            docEntry = DocumentManager.Instance.OpenDocument(fullPath);
        }

        if (docEntry == null)
        {
            throw new InvalidOperationException($"Document not found at '{relativePath}'");
        }

        var doc = docEntry.Content as SNamedDocument;
        if (doc == null)
        {
            throw new InvalidOperationException($"Document at '{relativePath}' is not a SNamedDocument");
        }

        // Find items to rename
        var itemsToRename = new List<(NamedItem Item, string NewName)>();
        var renamedItems = new List<RenameItem>();
        var itemsNotFound = new List<string>();

        foreach (var rename in Renames)
        {
            if (rename == null || string.IsNullOrWhiteSpace(rename.OldName) || string.IsNullOrWhiteSpace(rename.NewName))
            {
                continue;
            }

            var item = doc.ItemCollection.GetItemAll(rename.OldName);
            if (item != null)
            {
                itemsToRename.Add((item, rename.NewName));
                renamedItems.Add(rename);
            }
            else
            {
                itemsNotFound.Add(rename.OldName);
            }
        }

        // Execute rename action
        if (itemsToRename.Count > 0)
        {
            var action = new NamedItemsRenameAction(itemsToRename);

            if (doc.ShowView()?.GetService<UndoRedoManager>() is { } undoRedo)
            {
                undoRedo.Do(action);
            }
            else
            {
                action.Do();
            }
        }

        var result = new RenameResult();
        result.RenamedItems.AddRange(renamedItems);
        result.ItemsNotFound.AddRange(itemsNotFound);

        return Task.FromResult(result);
    }
}
