using Suity.Editor.Documents;
using Suity.Editor.Documents.Linked;
using Suity.Editor.Flows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Design;

/// <summary>
/// Provides extension methods for canvas documents.
/// </summary>
public static class CanvasDocumentExtensions
{
    #region Node
    /// <summary>
    /// Gets a canvas flow node by name.
    /// </summary>
    public static CanvasFlowNode GetNode(this ICanvasDocument canvas, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return canvas.Diagram.GetNode(name) as CanvasFlowNode;
    }

    /// <summary>
    /// Gets a canvas flow node by name as the specified type.
    /// </summary>
    public static T GetNode<T>(this ICanvasDocument canvas, string name)
        where T : CanvasFlowNode
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return canvas.Diagram.GetNode(name) as T;
    }

    /// <summary>
    /// Gets all nodes that contain documents of the specified type.
    /// </summary>
    public static IEnumerable<CanvasAssetNode> GetNodesOfDocumentType<T>(this ICanvasDocument canvas)
        where T : class
    {
        return canvas.Diagram.Nodes
            .OfType<CanvasAssetNode>()
            .Where(o => o.GetTargetDocument<T>() != null);
    }


    /// <summary>
    /// Gets all type design items in the canvas.
    /// </summary>
    public static IEnumerable<TypeDesignItem> GetAllTypeItems(this ICanvasDocument canvas)
    {
        foreach (var node in canvas.Diagram.Nodes.OfType<CanvasAssetNode>())
        {
            if (node.TargetAsset is DTypeFamily typeFamily && 
                typeFamily.GetStorageObject(true) is ITypeDesignDocument typeDoc)
            {
                foreach (var item in typeDoc.TypeItems)
                {
                    yield return item;
                }
            }
        }
    }

    #endregion

    #region Asset Node

    /// <summary>
    /// Finds all asset nodes for the specified asset.
    /// </summary>
    public static IEnumerable<CanvasAssetNode> FindAssetNode(this ICanvasDocument canvas, Asset asset)
    {
        if (asset is null)
        {
            return [];
        }

        return canvas.Diagram.Nodes
            .OfType<CanvasAssetNode>()
            .Where(o => o.TargetAsset == asset);
    }

    /// <summary>
    /// Finds all asset nodes for the specified document.
    /// </summary>
    public static IEnumerable<CanvasAssetNode> FindAssetNode(this ICanvasDocument canvas, Document document)
    {
        var asset = (document as AssetDocument)?.TargetAsset;

        if (asset is null)
        {
            return [];
        }

        return canvas.Diagram.Nodes
            .OfType<CanvasAssetNode>()
            .Where(o => o.TargetAsset == asset);
    }


    #endregion

    #region Document

    /// <summary>
    /// Finds a document by its usage attribute.
    /// </summary>
    public static T FindDocumentByUsage<T>(this ICanvasDocument canvas, string usage) where T : class
    {
        var docs = canvas.GetDocuments<T>();
        return docs.FirstOrDefault(
            o => (o as IAttributeGetter)?.GetAttribute<UsageAttribute>()?.Usage == usage
            );
    }

    /// <summary>
    /// Finds a document node by its usage attribute.
    /// </summary>
    public static CanvasAssetNode FindDocumentNodeByUsage<T>(this ICanvasDocument canvas, string usage, out T document) where T : class
    {
        var docs = canvas.FindDocumentNodes<T>();
        var node = docs
            .Where(o => o.TargetAsset?.GetStorageObject(true) is T t && (t as IAttributeGetter)?.GetAttribute<UsageAttribute>()?.Usage is { } docUsage && docUsage == usage)
            .FirstOrDefault();

        if (node != null)
        {
            document = node.TargetAsset?.GetStorageObject(true) as T;
            return node;
        }
        else
        {
            document = null;
            return null;
        }
    }


    /// <summary>
    /// Gets a document by node name.
    /// </summary>
    public static T GetDocument<T>(this ICanvasDocument canvas, string nodeName)
        where T : class
    {
        var node = canvas.Diagram.GetNode(nodeName) as CanvasAssetNode;

        return node?.GetTargetDocument<T>();
    }

    /// <summary>
    /// Creates a document with the specified document type, file path, and suffix.
    /// </summary>
    public static CanvasAssetNode CreateDocument<T>(this ICanvasDocument canvas, string docType, string rFilePath, string suffix, out T document)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(rFilePath))
        {
            throw new ArgumentException(L($"\"{rFilePath}\" cannot be null or whitespace."), nameof(rFilePath));
        }

        rFilePath = rFilePath.Trim();
        suffix = suffix?.Trim();

        if (!string.IsNullOrWhiteSpace(suffix) && !rFilePath.EndsWith(suffix))
        {
            rFilePath = rFilePath + suffix;
        }

        var docFormat = DocumentManager.Instance.GetDocumentFormat(docType)
            ?? throw new NullReferenceException("Failed to get document format: " + docType);

        return CreateDocument(canvas, docFormat, rFilePath, out document);
    }

    /// <summary>
    /// Creates a document with the specified document format and file path.
    /// </summary>
    public static CanvasAssetNode CreateDocument<T>(this ICanvasDocument canvas, DocumentFormat docFormat, string rFilePath, out T document)
        where T : class
    {
        if (canvas is null)
        {
            throw new ArgumentNullException(nameof(canvas));
        }

        if (docFormat is null)
        {
            throw new ArgumentNullException(nameof(docFormat));
        }

        if (string.IsNullOrWhiteSpace(rFilePath))
        {
            throw new ArgumentException(L($"\"{rFilePath}\" cannot be null or whitespace."), nameof(rFilePath));
        }

        var node = canvas.CreateDocument(docFormat, rFilePath)
            ?? throw new NullReferenceException(L("Failed to create canvas node."));

        document = node.GetTargetDocument<T>()
            ?? throw new NullReferenceException(L("Failed to get node document."));

        (canvas as Document)?.SaveDelayed();

        return node;
    }

    #endregion

    #region Article
    /// <summary>
    /// Gets an article by usage.
    /// </summary>
    public static CanvasAssetNode GetArticle(this ICanvasDocument canvas, string usage, out IArticleDocument document)
    {
        var node = canvas.FindDocumentNodeByUsage<IArticleDocument>(usage, out document);

        return node;
    }

    /// <summary>
    /// Creates a new article document.
    /// </summary>
    public static CanvasAssetNode CreateArticle(this ICanvasDocument canvas, string rFilePath, out IArticleDocument document, string suffix = "Article")
    {
        var node = CreateDocument<IArticleDocument>(canvas, "ArticleEdit", rFilePath, suffix, out document);

        if (document is AssetDocument assetDoc)
        {
            assetDoc.NameSpace = $"{rFilePath}.{suffix}";
        }

        (document as Document)?.SaveDelayed();

        return node;
    }
    /// <summary>
    /// Gets or creates an article document.
    /// </summary>
    public static CanvasAssetNode GetOrCreateArticle(this ICanvasDocument canvas, string rFilePath, string name, out IArticleDocument document)
    {
        var node = canvas.FindDocumentNodeByUsage<IArticleDocument>(name, out document);
        if (node is null)
        {
            string filePath = $"{rFilePath}/{name}";
            node = canvas.CreateArticle(filePath, out document);

            (document as IHasAttributeDesign)?.SetAttribute<UsageAttribute>(o => o.Usage = name);

            var assetDoc = (AssetDocument)document;
            assetDoc.MarkDirty(canvas);
            assetDoc.SaveDelayed();
        }

        return node;
    }

    #endregion

    #region Type Edit

    /// <summary>
    /// Gets a type edit document by name.
    /// </summary>
    public static CanvasAssetNode GetTypeEdit(this ICanvasDocument canvas, string name, out ITypeDesignDocument document)
    {
        var node = canvas.FindDocumentNodeByUsage<ITypeDesignDocument>(name, out document);

        return node;
    }

    /// <summary>
    /// Creates a new type edit document.
    /// </summary>
    public static CanvasAssetNode CreateTypeEdit(this ICanvasDocument canvas, string rFilePath, out ITypeDesignDocument document, string suffix = "Model")
    {
        var node = CreateDocument<ITypeDesignDocument>(canvas, "TypeEdit", rFilePath, suffix, out document);

        if (document is AssetDocument assetDoc)
        {
            assetDoc.NameSpace = $"{rFilePath}.{suffix}";
        }

    (document as Document)?.SaveDelayed();

        return node;
    }
    /// <summary>
    /// Gets or creates a type edit document.
    /// </summary>
    public static CanvasAssetNode GetOrCreateTypeEdit(this ICanvasDocument canvas, string rFilePath, string name, out ITypeDesignDocument document)
    {
        var node = canvas.FindDocumentNodeByUsage<ITypeDesignDocument>(name, out document);
        if (document is null)
        {
            string filePath = $"{rFilePath}/{name}";
            node = canvas.CreateTypeEdit(filePath, out document);

            (document as IHasAttributeDesign)?.SetAttribute<UsageAttribute>(o => o.Usage = name);

            var assetDoc = (AssetDocument)document;
            assetDoc.MarkDirty(canvas);
            assetDoc.SaveDelayed();
        }

        return node;
    }

    #endregion

    #region Data Table

    /// <summary>
    /// Gets all data grid types in the canvas.
    /// </summary>
    public static IEnumerable<DCompond> GetAllDataGridTypes(this ICanvasDocument canvas)
    {
        // Get all TypeDesignItem in Canvas
        var items = canvas.GetAllTypeItems();

        foreach (var item in items)
        {
            if (item.TargetAsset is DCompond dCompond && 
                dCompond.GetAttribute<DataUsageAttribute>()?.Usage == DataUsageMode.DataGrid)
            {
                yield return dCompond;
            }
        }
    }

    /// <summary>
    /// Gets all DCompond types in the canvas.
    /// </summary>
    public static IEnumerable<DCompond> GetAllDComponds(this ICanvasDocument canvas)
    {
        // Get all TypeDesignItem in Canvas
        var items = canvas.GetAllTypeItems();

        return items.Select(o => o.TargetAsset).OfType<DCompond>();
    }

    /// <summary>
    /// Gets all data types in the canvas.
    /// </summary>
    public static IEnumerable<DCompond> GetAllDataTypes(this ICanvasDocument canvas)
    {
        // Get all TypeDesignItem in Canvas
        var items = canvas.GetAllTypeItems();

        foreach (var item in items)
        {
            if (item.TargetAsset is DCompond dCompond &&
                dCompond.GetAttribute<DataUsageAttribute>()?.Usage.GetIsDataFilling() == true)
            {
                yield return dCompond;
            }
        }
    }

    /// <summary>
    /// Gets all entity data types in the canvas.
    /// </summary>
    public static IEnumerable<DCompond> GetAllEntityDatas(this ICanvasDocument canvas)
    {
        // Get all TypeDesignItem in Canvas
        var items = canvas.GetAllTypeItems();

        foreach (var item in items)
        {
            if (item.TargetAsset is DCompond dCompond &&
                dCompond.GetAttribute<DataUsageAttribute>()?.Usage == DataUsageMode.EntityData)
            {
                yield return dCompond;
            }
        }
    }

    /// <summary>
    /// Gets all entity types in the canvas.
    /// </summary>
    public static IEnumerable<DCompond> GetAllEntities(this ICanvasDocument canvas)
    {
        // Get all TypeDesignItem in Canvas
        var items = canvas.GetAllTypeItems();

        foreach (var item in items)
        {
            if (item.TargetAsset is DCompond dCompond &&
                dCompond.GetAttribute<DataUsageAttribute>()?.Usage == DataUsageMode.Entity)
            {
                yield return dCompond;
            }
        }
    }

    /// <summary>
    /// Get all data table documents in canvas
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static IEnumerable<IDataDocument> GetDataDocuments(this ICanvasDocument canvas)
        => canvas.GetDocuments<IDataDocument>();

    /// <summary>
    /// Get data table documents in canvas containing specified type
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<IDataDocument> GetDataDataDocuments(this ICanvasDocument canvas, TypeDefinition type)
    {
        if (TypeDefinition.IsNullOrEmpty(type))
        {
            return [];
        }

        if (type.Target is DCompond dCompond && dCompond.GetDataUsageMode() is { } usage)
        {
            switch (usage)
            {
                case DataUsageMode.DataGrid:
                    return GetDataGrids(canvas, dCompond);

                case DataUsageMode.FlowGraph:
                    {
                        if (dCompond is DStruct ds)
                        {
                            return GetFlowGraphs(canvas, ds);
                        }
                        else if (dCompond is DAbstract abs)
                        {
                            return GetFlowGraphs(canvas, abs);
                        }
                        else
                        {
                            return [];
                        }
                    }
                    
                case DataUsageMode.TreeGraph:
                    {
                        DAbstract baseType = dCompond as DAbstract;
                        baseType ??= (dCompond as DStruct)?.BaseType as DAbstract;
                        if (baseType is not null)
                        {
                            return GetTreeGraphs(canvas, baseType);
                        }
                        else
                        {
                            return [];
                        }
                    }
            }
        }


        // Get data tables in canvas that match the type
        // Use IsAssignableFrom to support abstract types
        // This approach is very slow for getting tables.
        var dataTables = canvas.GetDocuments<IDataDocument>()
            .Where(o => o.Datas.Any(r => r.Components.Any(c => type.IsAssignableFrom(c.ObjectType))));

        return dataTables;
    }

    /// <summary>
    /// Gets all data rows for the specified type.
    /// </summary>
    public static IEnumerable<IDataItem> GetDataRows(this ICanvasDocument canvas, TypeDefinition type)
    {
        //TODO: This approach cannot get non-shared type data, needs optimization.

        var dataDocs = canvas.GetDataDataDocuments(type)?.ToArray() ?? [];
        if (dataDocs.Length == 0)
        {
            return [];
        }

        return dataDocs.SelectMany(o => o.Datas);
    }

    #endregion

    #region Data Grid
    /// <summary>
    /// Gets all data grid documents containing the specified shared type.
    /// </summary>
    public static IEnumerable<IDataGridDocument> GetDataGrids(this ICanvasDocument canvas, DCompond sharedType)
        => canvas.GetDocuments<IDataGridDocument>().Where(o => o.ContainsSharedType(sharedType));

    /// <summary>
    /// Creates a new data grid document.
    /// </summary>
    public static CanvasAssetNode CreateDataGrid(this ICanvasDocument canvas, string rFilePath, DCompond dataType, out IDataGridDocument document, string docSuffix = "Grid", string ns = null, string nsSuffix = "Data")
    {
        var node = CreateDocument<IDataGridDocument>(canvas, "DataEdit", rFilePath, docSuffix, out document);

        var (nameSpace, tableId) = ResolveNS_TableId(ns, dataType, nsSuffix);
        
        document.TableId = tableId;
        document.AddSharedType(dataType);

        if (document is AssetDocument assetDoc)
        {
            assetDoc.NameSpace = nameSpace;
        }
        
        (document as Document)?.SaveDelayed();

        return node;
    }
    
    /// <summary>
    /// Gets or creates a data grid document.
    /// </summary>
    public static CanvasAssetNode GetOrCreateDataGrid(this ICanvasDocument canvas, string rFilePath, DCompond dataType, out IDataGridDocument document, string docSuffix = "Grid", string ns = null, string nsSuffix = "Data")
    {
        document = GetDataGrids(canvas, dataType).FirstOrDefault();
        if (document is AssetDocument { TargetAsset: { } asset })
        {
            var node = FindAssetNode(canvas, asset).FirstOrDefault();
            if (node != null)
            {
                return node;
            }
        }

        return CreateDataGrid(canvas, rFilePath, dataType, out document, docSuffix, ns, nsSuffix);
    }

    #endregion

    #region Data Graph

    /// <summary>
    /// Gets all flow graph documents containing the specified struct type.
    /// </summary>
    public static IEnumerable<IDataFlowDocument> GetFlowGraphs(this ICanvasDocument canvas, DStruct dataType)
        => canvas.GetDocuments<IDataFlowDocument>().Where(o => o.ContainsNodeType(dataType));

    /// <summary>
    /// Creates a new flow graph document.
    /// </summary>
    public static CanvasAssetNode CreateFlowGraph(this ICanvasDocument canvas, string rFilePath, DStruct dataType, out IDataFlowDocument document, string docSuffix = "Flow", string ns = null, string nsSuffix = "Data")
    {
        var node = CreateDocument<IDataFlowDocument>(canvas, "DataFlow", rFilePath, docSuffix, out document);

        var (nameSpace, tableId) = ResolveNS_TableId(ns, dataType, nsSuffix);

        document.TableId = tableId;
        document.AddNodeType(dataType);

        if (document is AssetDocument assetDoc)
        {
            assetDoc.NameSpace = nameSpace;
        }

        (document as Document)?.SaveDelayed();

        return node;
    }
    /// <summary>
    /// Gets or creates a flow graph document.
    /// </summary>
    public static CanvasAssetNode GetOrCreateFlowGraph(this ICanvasDocument canvas, string rFilePath, DStruct dataType, out IDataFlowDocument document, string docSuffix = "Flow", string ns = null, string nsSuffix = "Data")
    {
        document = GetFlowGraphs(canvas, dataType).FirstOrDefault();
        if (document is AssetDocument { TargetAsset: { } asset })
        {
            var node = FindAssetNode(canvas, asset).FirstOrDefault();
            if (node != null)
            {
                return node;
            }
        }

        return CreateFlowGraph(canvas, rFilePath, dataType, out document, docSuffix, ns, nsSuffix);
    }

    /// <summary>
    /// Gets all flow graph documents containing types derived from the specified base type.
    /// </summary>
    public static IEnumerable<IDataFlowDocument> GetFlowGraphs(this ICanvasDocument canvas, DAbstract baseType)
    {
        var types = DTypeManager.Instance.GetStructsByBaseType(baseType);
        if (types is null || types.Count == 0)
        {
            return [];
        }

        return canvas.GetDocuments<IDataFlowDocument>().Where(o => types.Assets.Any(x => o.ContainsNodeType(x)));
    }
    /// <summary>
    /// Creates a new flow graph document with all types derived from the base type.
    /// </summary>
    public static CanvasAssetNode CreateFlowGraph(this ICanvasDocument canvas, string rFilePath, DAbstract baseType, out IDataFlowDocument document, string docSuffix = "Flow", string ns = null, string nsSuffix = "Data")
    {
        var types = DTypeManager.Instance.GetStructsByBaseType(baseType);
        if (types is null || types.Count == 0)
        {
            document = null;
            return null;
        }

        var node = CreateDocument<IDataFlowDocument>(canvas, "DataFlow", rFilePath, docSuffix, out document);

        var (nameSpace, tableId) = ResolveNS_TableId(ns, baseType, nsSuffix);

        document.TableId = tableId;
        foreach (var nodeType in types.Assets)
        {
            document.AddNodeType(nodeType);
        }

        if (document is AssetDocument assetDoc)
        {
            assetDoc.NameSpace = nameSpace;
        }

        (document as Document)?.SaveDelayed();

        return node;
    }
    /// <summary>
    /// Gets or creates a flow graph document with all types derived from the base type.
    /// </summary>
    public static CanvasAssetNode GetOrCreateFlowGraph(this ICanvasDocument canvas, string rFilePath, DAbstract baseType, out IDataFlowDocument document, string docSuffix = "Flow", string ns = null, string nsSuffix = "Data")
    {
        document = GetFlowGraphs(canvas, baseType).FirstOrDefault();
        if (document is AssetDocument { TargetAsset: { } asset })
        {
            var node = FindAssetNode(canvas, asset).FirstOrDefault();
            if (node != null)
            {
                // Ensure all derived types are included
                var types = DTypeManager.Instance.GetStructsByBaseType(baseType);
                if (types?.Count > 0) 
                {
                    foreach (var nodeType in types.Assets)
                    {
                        document.AddNodeType(nodeType);
                    }
                }

                if (document is Document doc && doc.IsDirty)
                {
                    doc.SaveDelayed();
                }

                return node;
            }
        }

        return CreateFlowGraph(canvas, rFilePath, baseType, out document, docSuffix, ns, nsSuffix);
    }

    #endregion

    #region Data Tree
    /// <summary>
    /// Gets all tree graph documents with the specified base type.
    /// </summary>
    public static IEnumerable<IDataTreeDocument> GetTreeGraphs(this ICanvasDocument canvas, DAbstract baseType)
        => canvas.GetDocuments<IDataTreeDocument>().Where(o => o.BaseType == baseType);

    /// <summary>
    /// Creates a new tree graph document.
    /// </summary>
    public static CanvasAssetNode CreateTreeGraph(this ICanvasDocument canvas, string rFilePath, DAbstract baseType, out IDataTreeDocument document, string docSuffix = "Tree", string ns = null, string nsSuffix = "Data")
    {
        var node = CreateDocument<IDataTreeDocument>(canvas, "BehaviourTree", rFilePath, docSuffix, out document);

        var (nameSpace, tableId) = ResolveNS_TableId(ns, baseType, nsSuffix);

        document.TableId = tableId;
        document.BaseType = baseType;

        if (document is AssetDocument assetDoc)
        {
            assetDoc.NameSpace = nameSpace;
        }

        (document as Document)?.SaveDelayed();

        return node;
    }
    /// <summary>
    /// Gets or creates a tree graph document.
    /// </summary>
    public static CanvasAssetNode GetOrCreateTreeGraph(this ICanvasDocument canvas, string rFilePath, DAbstract baseType, out IDataTreeDocument document, string docSuffix = "Tree", string ns = null, string nsSuffix = "Data")
    {
        document = GetTreeGraphs(canvas, baseType).FirstOrDefault();
        if (document is AssetDocument { TargetAsset: { } asset })
        {
            var node = FindAssetNode(canvas, asset).FirstOrDefault();
            if (node != null)
            {
                return node;
            }
        }

        return CreateTreeGraph(canvas, rFilePath, baseType, out document, docSuffix, ns, nsSuffix);
    }

    #endregion

    /// <summary>
    /// Parses a file path into a namespace string.
    /// </summary>
    public static string ParseNameSpace(string rFilePath)
    {
        if (string.IsNullOrWhiteSpace(rFilePath))
        {
            return string.Empty;
        }

        string ns = rFilePath.Trim().Trim('.', '\\', '/').Trim();

        if (string.IsNullOrWhiteSpace(rFilePath))
        {
            return string.Empty;
        }

        ns = ns.Replace("/", ".");
        ns = ns.Replace("\\", ".");

        return ns;
    }

    private static (string NameSpace, string TableId) ResolveNS_TableId(string ns, DType type, string nsSuffix = null)
    {
        do
        {
            if (!string.IsNullOrWhiteSpace(ns))
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(type.NameSpace))
            {
                ns = type.NameSpace;
                break;
            }

            ns = EditorServices.CurrentProject.ProjectName;
        } while (false);

        string tableId = ns + "." + type.Name;

        if (!string.IsNullOrWhiteSpace(nsSuffix))
        {
            ns = ns + "." + nsSuffix;
        }
        
        return (ns, tableId);
    }
}