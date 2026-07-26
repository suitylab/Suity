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
using System.Threading.Tasks;

namespace Suity.Editor.DataModel.TypeDesigns.Tools;

[NativeType("GetTypeDesignItemList", CodeBase = "*Suity", Category = "DataModel Tools", Icon = "*CoreIcon|Class")]
[DisplayText("Get TypeDesign Item List")]
[ToolTipsText("Get the list of type items (Enum, Struct, Abstract) from a TypeDesign document.")]
[NativeAlias("Suity.Editor.DataModel.GetTypeDesignItemList")]
public class GetTypeDesignItemList : ToolCommand<GetTypeDesignItemList.Output>
{
    [NativeType("GetTypeDesignItemList.TypeDesignItemInfo", CodeBase = "*Suity")]
    public class TypeDesignItemInfo : SObjectController
    {
        readonly StringProperty _name = new("Name", "Name");
        readonly StringProperty _description = new("Description", "Description");
        readonly ValueProperty<DataStructureType> _type = new("Type", "Type", DataStructureType.Struct);

        public string Name { get => _name.Text; set => _name.Text = value; }
        public string Description { get => _description.Text; set => _description.Text = value; }
        public DataStructureType Type { get => _type.Value; set => _type.Value = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _name.Sync(sync);
            _description.Sync(sync);
            _type.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _name.InspectorField(setup);
            _description.InspectorField(setup);
            _type.InspectorField(setup);
        }

        public override string ToString() => $"{Type} {Name}";
    }

    public class Output : SObjectController
    {
        readonly ListProperty<TypeDesignItemInfo> _items = new("Items", "Items");

        public List<TypeDesignItemInfo> Items => _items.List;

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _items.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _items.InspectorField(setup);
        }

        public override string ToString() => $"Found {Items.Count} type items";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The relative path to the TypeDesign document (e.g., 'Data/Models/mymodel.sasset').");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
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

        context.ToolInstance.Conversation?.AddRunningMessage("Get TypeDesign items", msg =>
        {
            msg.AddCode(relativePath);
        });
        context.Conversation?.AddRunningMessage("Get TypeDesign items", msg =>
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

        // Collect items
        var output = new Output();

        foreach (var item in doc.TypeItems)
        {
            DataStructureType? type = item switch
            {
                EnumType => DataStructureType.Enum,
                AbstractType => DataStructureType.Abstract,
                StructType => DataStructureType.Struct,
                _ => null,
            };

            if (type is null)
            {
                continue;
            }

            var info = new TypeDesignItemInfo
            {
                Name = item.Name,
                Description = item.Description,
                Type = type.Value,
            };

            output.Items.Add(info);
        }

        return Task.FromResult(output);
    }
}
