using Suity.Editor.Types;
using Suity.Synchonizing;
using System;

namespace Suity.Editor;

internal abstract class SyncExportExternal
{
    /// <summary>
    /// Converts standard key names to actual queryable keys
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public abstract string GetExportedName(string name);

    public abstract void SyncId(IPropertySync sync, ref Guid id, ISyncContext context = null);

    public abstract void SyncAssetRef(IPropertySync sync, EditorAssetRef assetRef, ISyncContext context = null);

    public abstract void SyncAssetRef(IPropertySync sync, EditorAssetRef assetRef, ref string prefix, ref string originKey, ISyncContext context = null);

    public abstract void SyncAssetRef<T>(IPropertySync sync, EditorAssetRef<T> assetRef, ISyncContext context = null) where T : class;

    public abstract void SyncObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef, ISyncContext context = null) where T : EditorObject;

    public abstract void SyncObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef, ref string key, Func<string, Guid> idResolve, Func<Guid, string> keyResolve, ISyncContext context = null) where T : EditorObject;

    public abstract Guid SyncSetId(IPropertySync sync, string attr, out string newTypeId);

    public abstract Guid SyncSetId(IIndexSync sync, string attr, out string newTypeId);

    public abstract bool SyncSetTypeDefinition(IPropertySync sync, string attr, TypeDefinition current, out TypeDefinition newInputType, out string newTypeId);

    public abstract bool SyncSetTypeDefinition(IIndexSync sync, string attr, TypeDefinition current, out TypeDefinition newInputType, out string newTypeId);

    public abstract void SyncSetObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef, string newTypeId) where T : EditorObject;

    public abstract void SyncGetTypeDefinition(IPropertySync sync, string attr, TypeDefinition inputType);

    public abstract void SyncGetTypeDefinition(IIndexSync sync, string attr, TypeDefinition inputType);

    public abstract void SyncGetEditorObjectRef<T>(IPropertySync sync, EditorObjectRef<T> objRef) where T : EditorObject;

    public abstract Guid ResolveFieldId(string typeId, string name, ISyncContext context = null);

    public abstract string GetDataAccessFieldName(Guid fieldId);
}

public static class SyncExportExtensions
{
    internal static SyncExportExternal _external;

    /// <summary>
    /// Converts standard key names to actual queryable keys
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetExportedName(string name)
        => _external.GetExportedName(name);

    public static void SyncId(this IPropertySync sync, ref Guid id, ISyncContext context = null)
        => _external.SyncId(sync, ref id, context);

    public static void SyncAssetRef(this IPropertySync sync, EditorAssetRef assetRef, ISyncContext context = null)
        => _external.SyncAssetRef(sync, assetRef, context);

    public static void SyncAssetRef(this IPropertySync sync, EditorAssetRef assetRef, ref string prefix, ref string originKey, ISyncContext context = null)
        => _external.SyncAssetRef(sync, assetRef, ref prefix, ref originKey, context);

    public static void SyncAssetRef<T>(this IPropertySync sync, EditorAssetRef<T> assetRef, ISyncContext context = null) where T : class
        => _external.SyncAssetRef<T>(sync, assetRef, context);

    public static void SyncObjectRef<T>(this IPropertySync sync, EditorObjectRef<T> objRef, ISyncContext context = null) where T : EditorObject
        => _external.SyncObjectRef<T>(sync, objRef, context);

    public static void SyncObjectRef<T>(this IPropertySync sync, EditorObjectRef<T> objRef, ref string key, Func<string, Guid> idResolve, Func<Guid, string> keyResolve, ISyncContext context = null) where T : EditorObject
        => _external.SyncObjectRef(sync, objRef, ref key, idResolve, keyResolve, context);

    public static Guid SyncSetId(this IPropertySync sync, string attr, out string newTypeId)
        => _external.SyncSetId(sync, attr, out newTypeId);

    public static Guid SyncSetId(this IIndexSync sync, string attr, out string newTypeId)
        => _external.SyncSetId(sync, attr, out newTypeId);

    public static bool SyncSetTypeDefinition(this IPropertySync sync, string attr, TypeDefinition current, out TypeDefinition newInputType, out string newTypeId)
        => _external.SyncSetTypeDefinition(sync, attr, current, out newInputType, out newTypeId);

    public static bool SyncSetTypeDefinition(this IIndexSync sync, string attr, TypeDefinition current, out TypeDefinition newInputType, out string newTypeId)
        => _external.SyncSetTypeDefinition(sync, attr, current, out newInputType, out newTypeId);

    public static void SyncSetObjectRef<T>(this IPropertySync sync, EditorObjectRef<T> objRef, string newTypeId) where T : EditorObject
        => _external.SyncSetObjectRef<T>(sync, objRef, newTypeId);

    public static void SyncGetTypeDefinition(this IPropertySync sync, string attr, TypeDefinition inputType)
        => _external.SyncGetTypeDefinition(sync, attr, inputType);

    public static void SyncGetTypeDefinition(this IIndexSync sync, string attr, TypeDefinition inputType)
        => _external.SyncGetTypeDefinition(sync, attr, inputType);

    public static void SyncGetEditorObjectRef<T>(this IPropertySync sync, EditorObjectRef<T> objRef) where T : EditorObject
        => _external.SyncGetEditorObjectRef<T>(sync, objRef);

    public static Guid ResolveFieldId(string typeId, string name, ISyncContext context = null)
        => _external.ResolveFieldId(typeId, name, context);

    public static string GetDataAccessFieldName(this Guid fieldId)
        => _external.GetDataAccessFieldName(fieldId);
}