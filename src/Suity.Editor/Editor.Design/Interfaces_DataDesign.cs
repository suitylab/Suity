using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Design;

/// <summary>
/// Represents a container for data items that also implements asset and member management functionality.
/// This interface extends multiple base interfaces to provide comprehensive data handling capabilities.
/// </summary>
/// <remarks>
/// Implementors of this interface should provide functionality for:
/// - Basic identification and naming
/// - Asset management
/// - Member access
/// - Data item storage and retrieval
/// - Data cleanup operations
/// </remarks>
[NativeType(Name = "DataContainer", Description = "Data Table", CodeBase = "*Core", Icon = "*CoreIcon|Data")]
[NativeAlias("Suity.Editor.Design.DataTable")]
public interface IDataContainer : IMemberContainer, IHasAsset, IHasId
{
    /// <summary>
    /// Gets the name of the data container.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets the table identifier for the data container.
    /// </summary>
    string TableId { get; set; }

    /// <summary>
    /// Gets an enumerable collection of all data items contained within this container.
    /// </summary>
    IEnumerable<IDataItem> Datas { get; }

    /// <summary>
    /// Retrieves a specific data item by its name.
    /// </summary>
    /// <param name="name">The name of the data item to retrieve.</param>
    /// <returns>The requested data item if found; otherwise, behavior is implementation-specific.</returns>
    IDataItem GetData(string name);

    /// <summary>
    /// Cleans up and releases any resources or data held by the container.
    /// </summary>
    /// <remarks>
    /// This method should be called when the container is no longer needed to ensure proper resource management.
    /// </remarks>
    void CleanUp();
}


/// <summary>
/// Represents a data item with native type attributes.
/// This interface extends IMember and provides properties for accessing various data item properties.
/// </summary>
/// <remarks>
/// The NativeType attribute specifies metadata for this interface:
/// - Name: "DataItem"
/// - Description: "Data Item"
/// - CodeBase: "*Core"
/// - Icon: "*CoreIcon|Row"
/// </remarks>
[NativeType(Name = "DataItem", Description = "Data Item", CodeBase = "*Core", Icon = "*CoreIcon|Row")]
public interface IDataItem : IMember
{
    /// <summary>
    /// Gets the data container associated with this data item.
    /// </summary>
    IDataContainer DataContainer { get; }

    /// <summary>
    /// Gets a value indicating whether this data item is linked.
    /// </summary>
    bool IsLinked { get; }

    /// <summary>
    /// Gets the globally unique identifier (GUID) for this data item.
    /// </summary>
    Guid DataGuid { get; }

    /// <summary>
    /// Gets the local identifier for this data item.
    /// </summary>
    string DataLocalId { get; }

    /// <summary>
    /// Gets the description of this data item.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the index of this data item.
    /// </summary>
    int Index { get; }

    /// <summary>
    /// Gets the collection of components associated with this data item.
    /// </summary>
    IEnumerable<SObject> Components { get; }
}

/// <summary>
/// Represents an item in a data tree structure.
/// This interface serves as a base for tree-organized data items.
/// </summary>
public interface IDataTreeItem
{
}

/// <summary>
/// Provides context for a data source.
/// This interface allows access to the data source it represents.
/// </summary>
public interface IDataSourceContext
{
    /// <summary>
    /// Gets the data source associated with this context.
    /// </summary>
    IDataSource DataSource { get; }
}

/// <summary>
/// Represents a document that contains data.
/// This interface extends IDataContainer to provide document-level data management.
/// </summary>
public interface IDataDocument : IDataContainer
{
}

/// <summary>
/// Represents a grid-based data document.
/// This interface extends IDataDocument and provides methods for managing shared types and data in a grid format.
/// </summary>
public interface IDataGridDocument : IDataDocument
{
    /// <summary>
    /// Gets an array of shared types in this document.
    /// </summary>
    /// <returns>An array of DCompond objects representing shared types.</returns>
    DCompond[] GetSharedTypes();
    
    /// <summary>
    /// Checks if the document contains a specific shared type.
    /// </summary>
    /// <param name="type">The DCompond type to check for.</param>
    /// <returns>True if the type is contained in the document; otherwise, false.</returns>
    bool ContainsSharedType(DCompond type);

    /// <summary>
    /// Adds a shared type to the document.
    /// </summary>
    /// <param name="type">The DCompond type to add.</param>
    void AddSharedType(DCompond type);
    
    /// <summary>
    /// Removes a shared type from the document.
    /// </summary>
    /// <param name="type">The DCompond type to remove.</param>
    void RemoveSharedType(DCompond type);

    /// <summary>
    /// Adds data to the document.
    /// </summary>
    /// <param name="name">The name of the data.</param>
    /// <param name="objs">An array of SObject components.</param>
    /// <param name="description">Optional description of the data.</param>
    /// <param name="groupPath">Optional group path for organizing the data.</param>
    /// <returns>The IDataItem that was created.</returns>
    IDataItem AddData(string name, SObject[] objs, string description = null, string groupPath = null);
    
    /// <summary>
    /// Removes data from the document by name.
    /// </summary>
    /// <param name="name">The name of the data to remove.</param>
    /// <returns>True if the data was successfully removed; otherwise, false.</returns>
    bool RemoveData(string name);
}

/// <summary>
/// Represents a flow-based data document.
/// This interface extends IDataDocument and provides methods for managing node types in a data flow.
/// </summary>
public interface IDataFlowDocument : IDataDocument
{
    /// <summary>
    /// Gets an array of node types available in this document.
    /// </summary>
    /// <returns>An array of DStruct objects representing node types.</returns>
    DStruct[] GetNodeTypes();
    
    /// <summary>
    /// Checks if the document contains a specific node type.
    /// </summary>
    /// <param name="type">The DStruct type to check for.</param>
    /// <returns>True if the type is contained in the document; otherwise, false.</returns>
    public bool ContainsNodeType(DStruct type);

    /// <summary>
    /// Adds a node type to the document.
    /// </summary>
    /// <param name="type">The DStruct type to add.</param>
    public void AddNodeType(DStruct type);
    
    /// <summary>
    /// Removes a node type from the document.
    /// </summary>
    /// <param name="type">The DStruct type to remove.</param>
    public void RemoveNodeType(DStruct type);
}

/// <summary>
/// Represents a tree-based data document.
/// This interface extends IDataDocument and provides functionality for managing a hierarchical data structure.
/// </summary>
public interface IDataTreeDocument : IDataDocument
{
    /// <summary>
    /// Gets or sets the base type for this tree document.
    /// </summary>
    DAbstract BaseType { get; set; }
}
