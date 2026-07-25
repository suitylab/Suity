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

[NativeType("BatchDeleteNamedItems", CodeBase = "*Suity", Category = "Asset Tools", Icon = "*CoreIcon|Asset")]
[DisplayText("Batch Delete Named Items")]
[ToolTipsText("Delete multiple named items from a SNamedDocument by name.")]
[NativeAlias("Suity.Editor.DataModel.BatchDeleteNamedItems")]
public class BatchDeleteNamedItems : ToolCommand<BatchDeleteNamedItems.DeleteResult>
{
    [NativeType("BatchDeleteNamedItems.DeleteResult", CodeBase = "*Suity")]
    public class DeleteResult : SObjectController
    {
        readonly ListProperty<string> _deletedItems = new("DeletedItems", "Deleted Items");
        readonly ListProperty<string> _itemsNotFound = new("ItemsNotFound", "Items Not Found");

        public List<string> DeletedItems => _deletedItems.List;
        public List<string> ItemsNotFound => _itemsNotFound.List;

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _deletedItems.Sync(sync);
            _itemsNotFound.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _deletedItems.InspectorField(setup);
            _itemsNotFound.InspectorField(setup);
        }

        public override string ToString() => $"Deleted: {DeletedItems.Count}, Not Found: {ItemsNotFound.Count}";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The relative path to the SNamedDocument.");
    readonly ListProperty<string> _names = new("Names", "Names", "List of item names to delete.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public List<string> Names => _names.List;

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _names.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _names.InspectorField(setup);
    }

    public override Task<DeleteResult> Run(ToolCallContext context)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
        }

        if (Names == null || Names.Count == 0)
        {
            throw new ArgumentException("Names list is empty");
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

        context.ToolInstance.Conversation?.AddRunningMessage("Batch delete named items", msg =>
        {
            msg.AddCode(relativePath);
        });
        context.Conversation?.AddRunningMessage("Batch delete named items", msg =>
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

        // Find items to delete
        var itemsToDelete = new List<NamedItem>();
        var deletedItems = new List<string>();
        var itemsNotFound = new List<string>();

        foreach (var name in Names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var item = doc.ItemCollection.GetItemAll(name);
            if (item != null)
            {
                itemsToDelete.Add(item);
                deletedItems.Add(name);
            }
            else
            {
                itemsNotFound.Add(name);
            }
        }

        // Execute delete action
        if (itemsToDelete.Count > 0)
        {
            var action = new NamedItemsDeleteAction(itemsToDelete);

            if (doc.ShowView()?.GetService<UndoRedoManager>() is { } undoRedo)
            {
                undoRedo.Do(action);
            }
            else
            {
                action.Do();
            }
        }

        var result = new DeleteResult();
        result.DeletedItems.AddRange(deletedItems);
        result.ItemsNotFound.AddRange(itemsNotFound);

        return Task.FromResult(result);
    }
}
