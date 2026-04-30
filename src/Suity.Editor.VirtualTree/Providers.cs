using Suity.Editor.VirtualTree.Adapters;
using Suity.Editor.VirtualTree.Nodes;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Provides context information for node provider operations.
/// </summary>
public class ProviderContext
{
    private readonly VirtualTreeModel _model;
    private readonly VirtualNode _parentNode;

    /// <summary>
    /// Gets the tree model associated with this context.
    /// </summary>
    public VirtualTreeModel Model => this._model;

    /// <summary>
    /// Gets the parent node in this context, or null if at root level.
    /// </summary>
    public VirtualNode ParentNode => this._parentNode;

    /// <summary>
    /// Initializes a new instance with the specified model and optional parent node.
    /// </summary>
    /// <param name="model">The tree model.</param>
    /// <param name="node">The optional parent node.</param>
    public ProviderContext(VirtualTreeModel model, VirtualNode node = null)
    {
        this._parentNode = node;
        this._model = node != null ? node.FindModel() : model;
    }
}

/// <summary>
/// Interface for providers that create virtual nodes for specific types.
/// </summary>
public interface IVirtualNodeProvider
{
    /// <summary>
    /// Determines the responsibility level for creating a node of the specified type.
    /// </summary>
    /// <param name="baseType">The type to create a node for.</param>
    /// <param name="context">The provider context.</param>
    /// <returns>A priority score indicating responsibility level.</returns>
    int IsResponsibleFor(Type baseType, ProviderContext context);

    /// <summary>
    /// Creates a virtual node for the specified type.
    /// </summary>
    /// <param name="baseType">The type to create a node for.</param>
    /// <param name="context">The provider context.</param>
    /// <returns>The created virtual node, or null if not applicable.</returns>
    VirtualNode CreateNode(Type baseType, ProviderContext context);
}

internal class UserVirtualNodeProvider : IVirtualNodeProvider
{
    public UserVirtualNodeProvider()
    {
        //if (!AppService.Instance.ContainsAssembly(this.GetType().Assembly))
        //{
        //    throw new InvalidOperationException();
        //}
    }

    public int IsResponsibleFor(Type baseType, ProviderContext context)
    {
        if (
            typeof(IViewList).IsAssignableFrom(baseType) ||
            typeof(IViewNode).IsAssignableFrom(baseType) ||
            typeof(IViewObject).IsAssignableFrom(baseType) ||
            typeof(ISyncList).IsAssignableFrom(baseType))
        {
            return VirtualTreeModel.EditorPriority_General;
        }

        //First, look for user-defined editors
        EditorTypeHelper.GetBestEditorType(typeof(BaseObjectNode), baseType, context, out int bestScore, out Type bestEditorType);
        if (bestScore > VirtualTreeModel.EditorPriority_None)
        {
            return bestScore;
        }

        EditorTypeHelper.GetBestEditorType(typeof(ListAdapter), baseType, context, out bestScore, out bestEditorType);
        if (bestScore > VirtualTreeModel.EditorPriority_None)
        {
            return bestScore;
        }

        EditorTypeHelper.GetBestEditorType(typeof(IListAdapter), baseType, context, out bestScore, out bestEditorType);
        if (bestScore > VirtualTreeModel.EditorPriority_None)
        {
            return bestScore;
        }

        //Then, look for underlying editors
        EditorTypeHelper.GetBestEditorType(typeof(VirtualNode), baseType, context, out bestScore, out bestEditorType);
        if (bestScore > VirtualTreeModel.EditorPriority_None)
        {
            return bestScore;
        }

        return VirtualTreeModel.EditorPriority_None;
    }

    public VirtualNode CreateNode(Type baseType, ProviderContext context)
    {
        int bestScore;
        Type bestEditorType;

        //string typeName = baseType.Name;

        //First, look for user-defined editors
        EditorTypeHelper.GetBestEditorType(typeof(BaseObjectNode), baseType, context, out bestScore, out bestEditorType);
        if (bestEditorType != null)
        {
            return (VirtualNode)bestEditorType.CreateInstanceOf();
        }

        EditorTypeHelper.GetBestEditorType(typeof(ListAdapter), baseType, context, out bestScore, out bestEditorType);
        if (bestEditorType != null)
        {
            return new ListVirtualNode((ListAdapter)bestEditorType.CreateInstanceOf());
        }

        EditorTypeHelper.GetBestEditorType(typeof(IListAdapter), baseType, context, out bestScore, out bestEditorType);
        if (bestEditorType != null)
        {
            return new IListVirtualNode((IListAdapter)bestEditorType.CreateInstanceOf());
        }

        //Then, look for underlying editors
        EditorTypeHelper.GetBestEditorType(typeof(VirtualNode), baseType, context, out bestScore, out bestEditorType);
        if (bestEditorType != null)
        {
            return (VirtualNode)bestEditorType.CreateInstanceOf();
        }

        //Moved to the end, because special editors like SArray need to be determined and created early, otherwise they will be replaced by ListSyncAdapter
        //Look for basic editors
        if (typeof(IViewNode).IsAssignableFrom(baseType))
        {
            return new ListVirtualNode(new ViewNodeAdapter());
        }

        if (typeof(IViewList).IsAssignableFrom(baseType))
        {
            return new ListVirtualNode(new ViewListAdapter());
        }

        if (typeof(ISyncList).IsAssignableFrom(baseType))
        {
            return new ListVirtualNode(new ViewListAdapter());
        }

        if (typeof(IViewObject).IsAssignableFrom(baseType))
        {
            return new SyncObjectNode();
        }

        return null;
    }
}

internal static class EditorTypeHelper
{
    /// <summary>
    /// Get the best editor type
    /// </summary>
    /// <param name="editorBaseType">Editor base type</param>
    /// <param name="editedType">Target edit type</param>
    /// <param name="context">Context</param>
    /// <param name="bestScore">Return best score</param>
    /// <param name="bestType">Return best type</param>
    public static void GetBestEditorType(Type editorBaseType, Type editedType, ProviderContext context, out int bestScore, out Type bestType)
    {
        bestScore = VirtualTreeModel.EditorPriority_None;
        bestType = null;

        foreach (Type editorType in editorBaseType.GetDerivedTypes())
        {
            var assignment = editorType.GetAttributesCached<VirtualNodeUsageAttribute>().FirstOrDefault();
            if (assignment is null) continue;

            int score = assignment.MatchToProperty(editedType, context);
            if (score > bestScore)
            {
                bestScore = score;
                bestType = editorType;
            }
        }
    }
}