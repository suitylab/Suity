using Suity.Editor.Documents;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using System;

namespace Suity.Editor;

/// <summary>
/// Internal singleton that handles export-specific synchronization operations including
/// name conversion, ID sync, asset reference sync, object reference sync, type definition sync,
/// and field ID resolution.
/// </summary>
internal class SyncExportExternalBK : SyncExportExternal
{
    /// <summary>
    /// Gets the singleton instance of this export external handler.
    /// </summary>
    public static readonly SyncExportExternalBK Instance = new();

    /// <summary>
    /// Initializes this instance as the active export external handler by registering it
    /// with <see cref="SyncExportExtensions"/>.
    /// </summary>
    public void Initialize()
    {
        SyncExportExtensions._external = this;
    }

    /// <inheritdoc/>
    public override string GetExportedName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        name = NativeTypes.GetFullName(name);

        Asset asset = AssetManager.Instance.GetAsset(name);
        if (asset != null)
        {
            return asset.AssetKey;
        }

        if (name.StartsWith("*"))
        {
            // System resource name
            return name;
        }
        else if (name.StartsWith(":"))
        {
            // Non-path resource name
            return name.TrimStart(':');
        }

        var keyCode = new KeyCode(name);
        // Convert \ to /
        string mainKey = keyCode.MainKey.GetPathId();

        return KeyCode.Combine(mainKey, keyCode.ElementKey);
    }

    /// <inheritdoc/>
    public override void SyncId(IPropertySync sync, ref Guid id, ISyncContext context = null)
    {
        if (sync.Intent == SyncIntent.DataExport && sync.IsGetter())
        {
            sync.Sync("Key", AssetManager.Instance.GetAsset(id)?.AssetKey ?? string.Empty);
        }
        else
        {
            id = sync.Sync("Id", id);

            if (sync.Mode == SyncMode.SetAll && id == Guid.Empty)
            {
                string key = sync.Sync("Key", string.Empty, SyncFlag.None, string.Empty);
                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Sync("Name", string.Empty, SyncFlag.None, string.Empty);
                }

                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Value as string;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    key = GetExportedName(key);
                    id = GlobalIdResolver.Resolve(key);
                    context?.GetService<ILegacy>()?.ReportLegacy();
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void SyncAssetRef(IPropertySync sync, EditorAssetRef assetRef, ISyncContext context = null)
    {
        if (sync.Intent == SyncIntent.DataExport && sync.IsGetter())
        {
            sync.Sync("Key", assetRef.AssetKey);
        }
        else
        {
            assetRef.Id = sync.Sync("Id", assetRef.Id);

            if (sync.Mode == SyncMode.SetAll && assetRef.Id == Guid.Empty)
            {
                string key = sync.Sync("Key", string.Empty, SyncFlag.None, string.Empty);
                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Sync("Name", string.Empty, SyncFlag.None, string.Empty);
                }

                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Value as string;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    key = GetExportedName(key);
                    assetRef.Id = GlobalIdResolver.Resolve(key);
                    context?.GetService<ILegacy>()?.ReportLegacy();
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void SyncAssetRef(IPropertySync sync, EditorAssetRef assetRef, ref string prefix, ref string originKey, ISyncContext context = null)
    {
        if (sync.Intent == SyncIntent.DataExport && sync.IsGetter())
        {
            sync.Sync("Key", $"{prefix}{assetRef.AssetKey}");
        }
        else
        {
            assetRef.Id = sync.Sync("Id", assetRef.Id);
            prefix = sync.Sync("Prefix", prefix, SyncFlag.None, string.Empty);

            if (sync.Mode == SyncMode.SetAll && assetRef.Id == Guid.Empty)
            {
                string key = sync.Sync("Key", string.Empty, SyncFlag.None, string.Empty);
                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Sync("Name", string.Empty, SyncFlag.None, string.Empty);
                }

                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Value as string;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    TypeDefinition.ResolveExportedDefinition(key, out prefix, out originKey);
                    assetRef.AssetKey = originKey;
                    context?.GetService<ILegacy>()?.ReportLegacy();
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void SyncAssetRef<T>(IPropertySync sync, EditorAssetRef<T> assetRef, ISyncContext context = null)
    {
        if (sync.Intent == SyncIntent.DataExport && sync.IsGetter())
        {
            sync.Sync("Key", assetRef.AssetKey);
        }
        else
        {
            assetRef.Id = sync.Sync("Id", assetRef.Id);

            if (sync.Mode == SyncMode.SetAll && assetRef.Id == Guid.Empty)
            {
                string key = sync.Sync("Key", string.Empty, SyncFlag.None, string.Empty);
                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Sync("Name", string.Empty, SyncFlag.None, string.Empty);
                }

                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Value as string;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    key = GetExportedName(key);
                    assetRef.Id = GlobalIdResolver.Resolve(key);
                    context?.GetService<ILegacy>()?.ReportLegacy();
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void SyncObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef, ISyncContext context = null)
    {
        if (sync.Intent == SyncIntent.DataExport && sync.IsGetter())
        {
            sync.Sync("Key", objRef.Target?.FullName ?? string.Empty);
        }
        else
        {
            objRef.Id = sync.Sync("Id", objRef.Id);

            if (sync.Mode == SyncMode.SetAll && objRef.Id == Guid.Empty)
            {
                string key = sync.Sync("Key", string.Empty, SyncFlag.None, string.Empty);
                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Sync("Name", string.Empty, SyncFlag.None, string.Empty);
                }

                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Value as string;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    key = GetExportedName(key);
                    objRef.Id = GlobalIdResolver.Resolve(key);
                    context?.GetService<ILegacy>()?.ReportLegacy();
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void SyncObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef, ref string key, Func<string, Guid> idResolve, Func<Guid, string> keyResolve, ISyncContext context = null)
    {
        if (sync.Intent == SyncIntent.DataExport && sync.IsGetter())
        {
            sync.Sync("Key", objRef.Target?.FullName ?? string.Empty);
        }
        else
        {
            objRef.Id = sync.Sync("Id", objRef.Id);

            if (sync.Mode == SyncMode.SetAll && objRef.Id == Guid.Empty)
            {
                key = sync.Sync("Key", string.Empty, SyncFlag.None, string.Empty);
                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Sync("Name", string.Empty, SyncFlag.None, string.Empty);
                }

                if (string.IsNullOrEmpty(key))
                {
                    key = sync.Value as string;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    objRef.Id = idResolve(key);
                    context?.GetService<ILegacy>()?.ReportLegacy();
                }
            }
            else if (sync.IsSetter())
            {
                key = keyResolve(objRef.Id);
            }
        }
    }

    /// <inheritdoc/>
    public override Guid SyncSetId(IPropertySync sync, string attr, out string newTypeId)
    {
        newTypeId = sync.Sync(attr, default(string), SyncFlag.AttributeMode);
        if (Guid.TryParseExact(newTypeId, "D", out Guid id))
        {
            return id;
        }
        else
        {
            // exported
            newTypeId = GetExportedName(newTypeId);
            return GlobalIdResolver.Resolve(newTypeId);
        }
    }

    /// <inheritdoc/>
    public override Guid SyncSetId(IIndexSync sync, string attr, out string newTypeId)
    {
        newTypeId = sync.SyncAttribute(attr, default);
        if (Guid.TryParseExact(newTypeId, "D", out Guid id))
        {
            return id;
        }
        else
        {
            // exported
            newTypeId = GetExportedName(newTypeId);
            return GlobalIdResolver.Resolve(newTypeId);
        }
    }

    /// <inheritdoc/>
    public override bool SyncSetTypeDefinition(IPropertySync sync, string attr, TypeDefinition current, out TypeDefinition newInputType, out string newTypeId)
    {
        newTypeId = sync.Sync(attr, current.TypeCode, SyncFlag.AttributeMode);
        if (newTypeId != current.TypeCode)
        {
            newInputType = TypeDefinition.Resolve(newTypeId);
            newTypeId = GetExportedName(newTypeId);

            return true;
        }
        else
        {
            newInputType = null;

            return false;
        }
    }

    /// <inheritdoc/>
    public override bool SyncSetTypeDefinition(IIndexSync sync, string attr, TypeDefinition current, out TypeDefinition newInputType, out string newTypeId)
    {
        newTypeId = sync.SyncAttribute(attr, current.TypeCode);
        if (newTypeId != current.TypeCode)
        {
            newInputType = TypeDefinition.Resolve(newTypeId);
            newTypeId = GetExportedName(newTypeId);

            return true;
        }
        else
        {
            newInputType = null;

            return false;
        }
    }

    /// <inheritdoc/>
    public override void SyncSetObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef, string newTypeId)
    {
        objRef.Id = sync.Sync("Id", objRef.Id);

        if (sync.Mode == SyncMode.SetAll && objRef.Id == Guid.Empty)
        {
            // exported
            string value = sync.Sync("Value", string.Empty, SyncFlag.None, string.Empty);
            newTypeId = GetExportedName(newTypeId);

            TypeDefinition.SplitPrefix(newTypeId, out string prefix, out string originName);

            if (AssetManager.Instance.GetAsset(originName) is IFieldGroup asset)
            {
                string fieldName = FieldCode.ParseFullFieldName(value);
                var field = asset.GetFieldObject(fieldName);
                if (field != null)
                {
                    objRef.Id = field.Id;

                    return;
                }
            }

            string fullName = $"{originName}.{value}";
            objRef.Id = GlobalIdResolver.Resolve(fullName);
        }
    }

    /// <inheritdoc/>
    public override void SyncGetTypeDefinition(IPropertySync sync, string attr, TypeDefinition type)
    {
        if (sync.Intent == SyncIntent.DataExport)
        {
            // exported
            sync.Sync(attr, type.ToTypeName(), SyncFlag.AttributeMode);
        }
        else
        {
            sync.Sync(attr, type.TypeCode, SyncFlag.AttributeMode);
        }
    }

    /// <inheritdoc/>
    public override void SyncGetTypeDefinition(IIndexSync sync, string attr, TypeDefinition inputType)
    {
        if (sync.Intent == SyncIntent.DataExport)
        {
            // exported
            sync.SyncAttribute(attr, inputType.ToTypeName());
        }
        else
        {
            sync.SyncAttribute(attr, inputType.TypeCode);
        }
    }

    /// <inheritdoc/>
    public override void SyncGetEditorObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef)
    {
        if (sync.Intent == SyncIntent.DataExport)
        {
            // exported
            sync.Sync("Value", objRef.Target?.FullName ?? objRef.Id.ToString());
        }
        else
        {
            sync.Sync("Id", objRef.Id);
        }
    }

    /// <inheritdoc/>
    public override Guid ResolveFieldId(string typeId, string name, ISyncContext context = null)
    {
        if (name.StartsWith("id-"))
        {
            if (Guid.TryParseExact(name.RemoveFromFirst(3), "D", out Guid id))
            {
                return id;
            }
            else
            {
                return Guid.Empty;
            }
        }
        else
        {
            // exported
            string fullName = $"{typeId}.{name}";
            Guid id = GlobalIdResolver.Resolve(fullName);
            context?.GetService<ILegacy>()?.ReportLegacy();

            return id;
        }
    }

    /// <inheritdoc/>
    public override string GetDataAccessFieldName(Guid fieldId)
    {
        return $"id-{fieldId}";
    }
}