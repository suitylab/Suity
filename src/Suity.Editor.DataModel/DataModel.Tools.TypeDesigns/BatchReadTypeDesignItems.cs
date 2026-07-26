using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Documents.TypeEdit;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.DataModel.TypeDesigns.Tools;

[NativeType("BatchReadTypeDesignItems", CodeBase = "*Suity", Category = "DataModel Tools", Icon = "*CoreIcon|Class")]
[DisplayText("Batch Read TypeDesign Items")]
[ToolTipsText("Read multiple type items from a TypeDesign document and convert them to XML format.")]
[NativeAlias("Suity.Editor.DataModel.BatchReadTypeDesignItems")]
public class BatchReadTypeDesignItems : ToolCommand<BatchReadTypeDesignItems.ReadResult>
{
    [NativeType("BatchReadTypeDesignItems.ReadResult", CodeBase = "*Suity")]
    public class ReadResult : SObjectController
    {
        readonly TextBlockProperty _xmlContent = new("XmlContent", "XML Content");
        readonly ListProperty<string> _itemsNotFound = new("ItemsNotFound", "Items Not Found");

        public string XmlContent { get => _xmlContent.Text; set => _xmlContent.Text = value; }
        public List<string> ItemsNotFound => _itemsNotFound.List;

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _xmlContent.Sync(sync);
            _itemsNotFound.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _xmlContent.InspectorField(setup);
            _itemsNotFound.InspectorField(setup);
        }

        public override string ToString() => $"XML: {XmlContent?.Length ?? 0} chars, Not Found: {ItemsNotFound.Count}";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The relative path to the TypeDesign document (e.g., 'Data/Models/mymodel.sasset').");
    readonly ListProperty<string> _names = new("Names", "Names", "List of type item names to read.");

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

    public override Task<ReadResult> Run(ToolCallContext context)
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
        if (!relativePath.EndsWith(".sasset", StringComparison.OrdinalIgnoreCase))
        {
            relativePath += ".sasset";
        }

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

        context.ToolInstance.Conversation?.AddRunningMessage("Batch read TypeDesign items", msg =>
        {
            msg.AddCode(relativePath);
        });
        context.Conversation?.AddRunningMessage("Batch read TypeDesign items", msg =>
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
            throw new InvalidOperationException($"TypeDesign document not found at '{relativePath}'");
        }

        var doc = docEntry.Content as TypeDesignDocument;
        if (doc == null)
        {
            throw new InvalidOperationException($"Document at '{relativePath}' is not a TypeDesign document");
        }

        // Build DataModelSpec from requested items
        var spec = new DataModelSpec();
        var itemsNotFound = new List<string>();

        foreach (var name in Names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var item = doc.TypeItems.FirstOrDefault(i => i.Name == name);
            if (item == null)
            {
                itemsNotFound.Add(name);
                continue;
            }

            var typeSpec = item.ToSpec();
            if (typeSpec != null)
            {
                spec.Structures.Add(typeSpec);
            }
        }

        // Serialize to XML
        string xml = DataModelParser.Serialize(spec);

        var result = new ReadResult
        {
            XmlContent = xml,
        };
        result.ItemsNotFound.AddRange(itemsNotFound);

        return Task.FromResult(result);
    }

}
