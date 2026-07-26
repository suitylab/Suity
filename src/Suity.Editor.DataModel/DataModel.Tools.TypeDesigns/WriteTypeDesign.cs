using Suity.Editor.DataModel.Actions;
using Suity.Editor.Documents;
using Suity.Editor.Documents.TypeEdit;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.DataModel.TypeDesigns.Tools;

[NativeType("WriteTypeDesign", CodeBase = "*Suity", Category = "DataModel Tools", Icon = "*CoreIcon|Class")]
[DisplayText("Write Type Design")]
[ToolTipsText("Parse XML content and write it to a TypeDesign document. Creates a new document if it doesn't exist, or updates an existing one.")]
[NativeAlias("Suity.Editor.DataModel.WriteTypeDesign")]
public class WriteTypeDesign : ToolCommand<WriteTypeDesign.Output>
{
    public class Output : SObjectController
    {
        readonly StringProperty _filePath = new("FilePath", "File Path");
        readonly StringProperty _message = new("Message", "Message");

        public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
        public string Message { get => _message.Text; set => _message.Text = value; }

        protected override void OnSync(IPropertySync sync, ISyncContext context)
        {
            base.OnSync(sync, context);

            _filePath.Sync(sync);
            _message.Sync(sync);
        }

        protected override void OnSetupView(IViewObjectSetup setup)
        {
            base.OnSetupView(setup);

            _filePath.InspectorField(setup);
            _message.InspectorField(setup);
        }

        public override string ToString() => $"{FilePath} '{Message}'";
    }

    readonly StringProperty _filePath = new("FilePath", "FilePath", string.Empty, "The relative path for the TypeDesign document (e.g., 'Data/Models/mymodel.sasset').");
    readonly TextBlockProperty _xmlContent = new("XmlContent", "XML Content", string.Empty, "The XML content to parse and apply to the TypeDesign document.");

    public string FilePath { get => _filePath.Text; set => _filePath.Text = value; }
    public string XmlContent { get => _xmlContent.Text; set => _xmlContent.Text = value; }

    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        _filePath.Sync(sync);
        _xmlContent.Sync(sync);
    }

    public override void SetupView(IViewObjectSetup setup)
    {
        _filePath.InspectorField(setup);
        _xmlContent.InspectorField(setup);
    }

    public override Task<Output> Run(ToolCallContext context)
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            throw new ArgumentException("FilePath is not set");
        }

        if (string.IsNullOrWhiteSpace(XmlContent))
        {
            throw new ArgumentException("XmlContent is not set");
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
        if (!Path.IsPathRooted(relativePath))
        {
            fullPath = Path.Combine(assetSpaceDir, relativePath);
        }

        context.ToolInstance.Conversation?.AddRunningMessage("Write TypeDesign", msg =>
        {
            msg.AddCode(relativePath);
        });
        context.Conversation?.AddRunningMessage("Write TypeDesign", msg =>
        {
            msg.AddCode(relativePath);
        });

        // Parse XML content
        DataModelSpec spec;
        try
        {
            spec = DataModelParser.Deserialize(XmlContent);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse XML content: {ex.Message}", ex);
        }

        // Get or create TypeDesign document
        var docEntry = DocumentManager.Instance.OpenDocument(fullPath);
        TypeDesignDocument doc;

        if (docEntry != null)
        {
            // Document exists, use it
            doc = docEntry.Content as TypeDesignDocument;
            if (doc == null)
            {
                throw new InvalidOperationException($"Document at '{relativePath}' is not a TypeDesign document");
            }
        }
        else
        {
            // Create new document
            var format = DocumentManager.Instance.GetDocumentFormat("TypeDesign");
            if (format == null)
            {
                throw new InvalidOperationException("TypeDesign format not found");
            }

            // Ensure directory exists
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            docEntry = DocumentManager.Instance.NewDocument(fullPath, format);
            doc = docEntry?.Content as TypeDesignDocument;
            if (doc == null)
            {
                throw new InvalidOperationException($"Failed to create TypeDesign document at '{relativePath}'");
            }
        }

        // Get or create document view
        var view = doc.ShowView();

        // Create and execute apply action
        var action = new DataModelApplyAction(doc, view);
        action.AddSpec(spec);

        // Execute the action
        if (view?.GetService<UndoRedoManager>() is { } undoRedo)
        {
            undoRedo.Do(action);
        }
        else
        {
            action.Do();
        }

        string message = $"Successfully wrote {spec.Structures.Count} structures to '{relativePath}'";

        return Task.FromResult(new Output
        {
            FilePath = relativePath,
            Message = message,
        });
    }
}
