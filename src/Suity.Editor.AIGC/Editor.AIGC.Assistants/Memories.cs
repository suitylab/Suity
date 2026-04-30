using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Flows;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC.Assistants;

#region CanvasNodeSelectionMemory
/// <summary>
/// Represents memory for tracking selected canvas flow nodes.
/// </summary>
public class CanvasNodeSelectionMemory
{
    /// <summary>
    /// Node
    /// </summary>
    public List<CanvasFlowNode> SelectedNodes { get; } = [];
} 
#endregion

#region LinkedDataSelectionMemory
/// <summary>
/// Represents memory for tracking selected linked data items organized by type.
/// </summary>
public class LinkedDataSelectionMemory
{
    /// <summary>
    /// Gets the dictionary of selection lists keyed by type definition.
    /// </summary>
    public Dictionary<TypeDefinition, LinkedDataSelectionList> ListDictionary { get; } = [];

    /// <summary>
    /// Gets the selection list for the specified type.
    /// </summary>
    /// <param name="type">The type definition to look up.</param>
    /// <returns>The selection list for the type, or null if not found.</returns>
    public LinkedDataSelectionList GetList(TypeDefinition type)
    {
        type = type?.OriginType ?? TypeDefinition.Empty;
        return ListDictionary.GetValueSafe(type);
    }

    /// <summary>
    /// Gets the selection list for the specified data type.
    /// </summary>
    /// <param name="type">The data type to look up.</param>
    /// <returns>The selection list for the type, or null if not found.</returns>
    public LinkedDataSelectionList GetList(DType type) => GetList(type.Definition);

    /// <summary>
    /// Gets or creates a selection list for the specified type.
    /// </summary>
    /// <param name="type">The type definition to look up or add.</param>
    /// <returns>The existing or newly created selection list.</returns>
    public LinkedDataSelectionList GetOrAddList(TypeDefinition type)
    {
        type = type?.OriginType ?? TypeDefinition.Empty;
        return ListDictionary.GetOrAdd(type, t => new(t));
    }

    /// <summary>
    /// Gets or creates a selection list for the specified data type.
    /// </summary>
    /// <param name="type">The data type to look up or add.</param>
    /// <returns>The existing or newly created selection list.</returns>
    public LinkedDataSelectionList GetOrAddList(DType type) => GetOrAddList(type.Definition);
}

/// <summary>
/// Represents a list of selected linked data items for a specific data type.
/// </summary>
public class LinkedDataSelectionList
{
    /// <summary>
    /// Gets the data type associated with this selection list.
    /// </summary>
    public TypeDefinition DataType { get; }

    /// <summary>
    /// Gets the collection of selection records in this list.
    /// </summary>
    public List<LinkedDataSelectionRecord> Selections { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedDataSelectionList"/> class.
    /// </summary>
    /// <param name="dataType">The data type for this selection list.</param>
    public LinkedDataSelectionList(TypeDefinition dataType)
    {
        DataType = dataType;
    }

    /// <summary>
    /// Determines whether a selection with the specified ID exists.
    /// </summary>
    /// <param name="id">The GUID to check.</param>
    /// <returns>True if a selection with the ID exists; otherwise, false.</returns>
    public bool Contains(Guid id)
    {
        return Selections.Any(o => o.Id == id);
    }

    /// <summary>
    /// Determines whether a selection with the specified data ID exists.
    /// </summary>
    /// <param name="dataId">The data ID to check.</param>
    /// <returns>True if a selection with the data ID exists; otherwise, false.</returns>
    public bool Contains(string dataId)
    {
        return Selections.Any(o => o.DataId == dataId);
    }

    /// <summary>
    /// Adds a data item to the selection list.
    /// </summary>
    /// <param name="row">The data item to add.</param>
    public void Add(IDataItem row) => Add(row?.Id ?? Guid.Empty);

    /// <summary>
    /// Adds a selection by GUID to the list, maintaining the maximum size limit.
    /// </summary>
    /// <param name="id">The GUID of the item to add.</param>
    public void Add(Guid id)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        if (!Contains(id))
        {
            Selections.Add(new LinkedDataSelectionRecord { Id = id });
        }

        int maxSize = AIAssistantService.Config.LinkedDataMemorySize;
        while (Selections.Count > maxSize)
        {
            Selections.RemoveAt(0);
        }
    }

    /// <summary>
    /// Adds a selection by data ID to the list, maintaining the maximum size limit.
    /// </summary>
    /// <param name="dataId">The data ID of the item to add.</param>
    public void Add(string dataId)
    {
        if (string.IsNullOrWhiteSpace(dataId))
        {
            return;
        }

        if (!Contains(dataId))
        {
            Selections.Add(new LinkedDataSelectionRecord { DataId = dataId });
        }

        int maxSize = AIAssistantService.Config.LinkedDataMemorySize;
        while (Selections.Count > maxSize)
        {
            Selections.RemoveAt(0);
        }
    }

    /// <summary>
    /// Retrieves the data items corresponding to the current selections.
    /// </summary>
    /// <returns>An enumerable of data items for the selected records.</returns>
    public IEnumerable<IDataItem> GetDataRows()
    {
        return Selections
            .Select(o => AssetManager.Instance.GetAsset(o.Id))
            .SkipNull()
            .Select(o => o.GetStorageObject(true))
            .OfType<IDataItem>();
    }
}

/// <summary>
/// Represents a record of a selected linked data item.
/// </summary>
public readonly record struct LinkedDataSelectionRecord
{
    /// <summary>
    /// Gets or initializes the unique identifier of the selected item.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or initializes the data identifier of the selected item.
    /// </summary>
    public string DataId { get; init; }
}
#endregion

#region ConsistencyValueMemory

/// <summary>
/// Represents memory for tracking consistency values across fields.
/// </summary>
public class ConsistencyValueMemory
{
    /// <summary>
    /// Gets the dictionary of field records keyed by field ID.
    /// </summary>
    public Dictionary<Guid, ConsistencyValueFieldRecord> FieldRecords { get; } = [];

    /// <summary>
    /// Gets the field record for the specified field ID.
    /// </summary>
    /// <param name="id">The field ID to look up.</param>
    /// <returns>The field record, or null if not found.</returns>
    public ConsistencyValueFieldRecord GetFieldRecord(Guid id) => FieldRecords.GetValueSafe(id);

    /// <summary>
    /// Gets the field record for the specified field.
    /// </summary>
    /// <param name="field">The field to look up.</param>
    /// <returns>The field record, or null if not found.</returns>
    public ConsistencyValueFieldRecord GetFieldRecord(DField field) => GetFieldRecord(field?.Id ?? Guid.Empty);

    /// <summary>
    /// Gets or creates a field record for the specified field ID.
    /// </summary>
    /// <param name="id">The field ID to look up or add.</param>
    /// <returns>The existing or newly created field record.</returns>
    public ConsistencyValueFieldRecord GetOrAddFieldRecord(Guid id) => FieldRecords.GetOrAdd(id, newId => new ConsistencyValueFieldRecord(newId));

    /// <summary>
    /// Gets or creates a field record for the specified field.
    /// </summary>
    /// <param name="field">The field to look up or add.</param>
    /// <returns>The existing or newly created field record.</returns>
    public ConsistencyValueFieldRecord GetOrAddFieldRecord(DField field) => GetOrAddFieldRecord(field?.Id ?? Guid.Empty);
}

/// <summary>
/// Represents a record of consistency values for a specific field.
/// </summary>
public class ConsistencyValueFieldRecord
{
    /// <summary>
    /// Gets the unique identifier of the field.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the dictionary of values keyed by their string keys.
    /// </summary>
    public Dictionary<string, SItem> Values { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsistencyValueFieldRecord"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this field record.</param>
    public ConsistencyValueFieldRecord(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Sets a value for the specified key, cloning the value before storage.
    /// </summary>
    /// <param name="key">The key to set the value for.</param>
    /// <param name="value">The value to store.</param>
    public void SetValue(string key, SItem value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        Values[key.Trim()] = Cloner.Clone(value);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The stored value, or null if the key is empty or not found.</returns>
    public SItem GetValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return Values.GetValueSafe(key.Trim());
    }
}

#endregion
