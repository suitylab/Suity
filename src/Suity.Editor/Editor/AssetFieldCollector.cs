using System;

namespace Suity.Editor;

public interface IAssetFieldCollector<TField>
    where TField : FieldObject, new()
{
    void AddOrUpdatedField(string name, Action<TField> action, IdResolveType resolveType = IdResolveType.Auto, Guid? recordedId = null);

    bool UpdateField(string name, Action<TField> action, Guid? recordedId = null);

    void RemoveField(string name);

    void RenameField(string oldName, string newName);
}

public delegate void FieldRenameAction<TAsset>(TAsset asset, string oldName, string newName);

public delegate void FieldAddOrUpdateAction<TAsset, TField>(TAsset asset, TField field, IdResolveType resolveType);

public delegate void FieldUpdateAction<TAsset, TField>(TAsset asset, TField field);