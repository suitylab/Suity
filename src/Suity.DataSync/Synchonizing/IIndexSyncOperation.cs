using System;

namespace Suity.Synchonizing;

/// <summary>
/// Interface for index-based list operations
/// </summary>
public interface IIndexSyncOperation
{
    Type GetElementType(IIndexSync sync);

    int Count { get; }

    object GetItem(IIndexSync sync, int index);

    void SetItem(IIndexSync sync, int index, object value);

    void Insert(IIndexSync sync, int index, object value);

    void RemoveAt(IIndexSync sync, int index);

    void Clear(IIndexSync sync);

    object CreateNew(IIndexSync sync);
}