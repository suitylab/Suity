using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Suity.Editor.Values;

internal abstract class SItemExternal
{
    internal static SItemExternal _external;

    public abstract SObjectExternal CreateSObjectEx(SObject obj);

    public abstract SObjectExternal CreateSObjectEx(SObject obj, SObjectController controller);

    public abstract SObjectExternal CreateSObjectEx(SObject obj, TypeDefinition type);

    public abstract SObjectExternal CreateSObjectEx(SObject obj, TypeDefinition type, SObjectController controller);

    public abstract SArrayExternal CreateSArrayEx(SArray ary);

    public abstract SArrayExternal CreateSArrayEx(SArray ary, IEnumerable<object> values);

    public abstract string GetPath(SItem item);

    public abstract Type ResolveSType(Type type);

    public abstract SItem ResolveSItem(object value);

    public abstract object ResolveValue(SItem item, ICondition context = null);

    public abstract object ResolveValue(object value, ICondition context = null);

    public abstract object ResolveOriginValue(object value, ICondition context = null);

    public abstract bool IsMeOrParent(SItem item, SItem parent);
}

internal abstract class SObjectExternal
{
    public abstract TypeDefinition ObjectType { get; set; }

    public abstract bool ObjectTypeLocked { get; }

    #region Properties

    public abstract string ResolvePropertyName(Guid id);

    public abstract Guid ResolvePropertyId(string name);

    public abstract DStructField GetField(Guid id);

    public abstract DStructField GetField(string name);

    public abstract object this[string name] { get; set; }

    public abstract IEnumerable<Guid> GetPropertyIds();

    public abstract IEnumerable<Guid> GetStoredPropertyIds();

    public abstract IEnumerable<Guid> GetLegacyPropertyIds();

    public abstract string[] GetPropertyNames();

    public abstract bool ContainsProperty(Guid id);

    public abstract bool ContainsProperty(string name);

    public abstract object GetPropertyFormatted(Guid id, ICondition context = null);

    public abstract object GetPropertyFormatted(string name, ICondition context = null);

    public abstract object GetProperty(Guid id, ICondition context = null);

    public abstract object GetProperty(string name, ICondition context = null);

    public abstract T GetProperty<T>(Guid id, T defaultValue = default, ICondition context = null);

    public abstract T GetProperty<T>(string name, T defaultValue = default, ICondition context = null);

    public abstract void SetProperty(Guid id, object value);

    public abstract void SetProperty(string name, object value);

    public abstract bool RemoveProperty(Guid id);

    public abstract bool RemoveProperty(string name);

    public abstract void RemoveItem(SItem item);

    public abstract SItem GetItem(Guid id);

    public abstract SItem GetItem(string name);

    public abstract SItem GetItemFormatted(Guid id);

    public abstract SItem GetItemFormatted(string name);

    public abstract void InternalSetProperty(Guid id, object value);

    public abstract void InternalSetProperty(string name, object value);

    public abstract bool Clear();

    public abstract IEnumerable<object> GetValues(ICondition context = null);

    public abstract IEnumerable<SItem> Items { get; }

    public abstract bool MergeTo(SObject target, bool skipDynamic);

    #endregion

    #region Attachments

    public abstract bool HasAttachments { get; }

    public abstract object GetAttachment(string name);

    public abstract void SetAttachment(string name, object value);

    public abstract void ClearAttachments();

    public abstract IEnumerable<KeyValuePair<string, object>> GetAttachments();

    #endregion

    #region Controller

    public abstract SObjectController Controller { get; set; }

    #endregion

    #region Judgment

    public abstract bool ValueEquals(object other);

    public abstract bool TypeEquals(SItem other);

    #endregion

    #region ISyncObject

    public abstract void Sync(IPropertySync sync, ISyncContext context);

    #endregion

    #region ISupportAnalysis

    public abstract void CollectProblem(AnalysisProblem problems, AnalysisIntents intent);

    #endregion

    #region Repair

    public abstract void RepairIds();

    #endregion

    #region Misc

    public abstract void AutoConvertValue();

    public abstract void ReferenceSync(SyncPath path, IReferenceSync sync);

    public abstract FieldObject GetField(SItem item);

    public abstract string GetBrief(int depth = 10);

    #endregion
}

internal abstract class SArrayExternal
{
    public abstract void OnInputTypeChanged();

    #region Members

    /// <summary>
    /// Gets or sets child item
    /// </summary>
    /// <param name="index">Index of the child item</param>
    /// <returns>Returns the child item at the index</returns>
    public abstract object this[int index] { get; set; }

    public abstract object GetValue(int index, ICondition context = null);

    public abstract object EnsureValue(int index, ICondition context = null);

    public abstract void SetValue(int index, object value);

    public abstract IEnumerable<object> GetValues(ICondition context = null);

    public abstract SItem GetItem(int index);

    public abstract SItem GetItemFormatted(int index);

    public abstract void SetItem(int index, SItem item);

    public abstract IEnumerable<SItem> Items { get; }

    public abstract void Add(object value);

    public abstract void Insert(int index, object value);

    public abstract void RemoveItem(SItem item);

    public abstract void RemoveAt(int index);

    public abstract bool Clear();

    public abstract int Count { get; }

    public abstract bool MergeTo(SArray target, bool skipDynamic);

    public abstract Array ToArray();

    public abstract Array ToArray(Type type);

    public abstract T[] ToArray<T>(T defaultValue = default);

    #endregion

    #region Judgment

    public abstract bool ValueEquals(object other);

    #endregion

    #region ISyncList

    public abstract void Sync(IIndexSync sync, ISyncContext context);

    #endregion

    public abstract void AutoConvertValue();

    public abstract void Validate(ValidationContext context);

    public abstract FieldObject GetField(SItem item);

    public abstract void AssertIndex();
}

internal abstract class SValueExternal
{
    internal static SValueExternal _external;

    #region Resolve

    public abstract Type GetNativeType(TypeDefinition typeInfo);

    public abstract Type GetFieldEditedType(IEnumerable<SObject> objs, DStructField field);

    public abstract Type GetArrayEditedType(IEnumerable<SArray> arys, int elementIndex);

    public abstract DStructField GetParentField(SItem item, IAssetFilter filter);

    public abstract TypeDefinition GetTypeFromParent(SItem item, IAssetFilter filter);

    #endregion

    #region Create

    public abstract SItem CreateValue(TypeDefinition definition);

    public abstract SItem CreateValue(DType type);

    public abstract SObject CreateObject(DCompond objectType);

    public abstract SObject CreateObject(DCompond objectType, TypeDefinition inputType, string inputText = null);

    public abstract SObject CreateObject(TypeDefinition objectType, TypeDefinition inputType, IAssetFilter filter, string inputText = null);

    public abstract SObject CreateDefaultObject(TypeDefinition inputType, IAssetFilter filter, string inputText = null);

    public abstract SArray CreateArray(TypeDefinition typeInfo);

    public abstract SObject CreateEmptyObject(TypeDefinition inputType);

    #endregion

    #region Repair

    public abstract void Repair(SObject obj, TypeDefinition type = null);

    public abstract void RepairDeep(SContainer container, TypeDefinition type = null, int limit = 100);

    public abstract void RepairObject(DCompond objType, SObject obj);

    public abstract void RemoveReadonlyDeep(SContainer container);

    public abstract void UnsetReadonlyDeep(SItem item);

    public abstract Type GetEditedType(TypeDefinition typeInfo);

    public abstract bool SupportValue(TypeDefinition typeInfo, object value, bool nullable);

    public abstract object ConvertValue(TypeDefinition typeInfo, object value, bool nullable);

    public abstract bool TryConvertValue(TypeDefinition typeInfo, object value, bool nullable, out object result);

    public abstract object CreateDefaultValue(TypeDefinition typeInfo, IAssetFilter filter = null);

    public abstract object CreateOrRepairValue(TypeDefinition typeInfo, object value, bool nullable);

    public abstract object CreateOrRepairValue(DStructField field, object value);

    public abstract object CreateDefaultValue(DStructField field, IAssetFilter filter = null);

    #endregion

    #region Update

    public abstract SObject UpdateOrCreateSObject(TypeDefinition type, SObject obj, IAssetFilter filter);

    #endregion

    #region Preview & Icon

    public abstract string GetBrief(DCompond type, SObject obj, int depth = 10);

    #endregion

    #region CreateObject GUI

    public abstract Task<SObject> GuiCreateObject(TypeDefinition type, SObject parent, string title);

    public abstract Task<SObject> GuiCreateObject(TypeDefinition type, SArray parent, string title);

    public abstract Task<SObject> GuiCreateObject(TypeDefinition type, IAssetFilter filter, string title);

    public abstract Task GuiConfigObject(SObject obj, string title);

    #endregion

    #region Setup

    public abstract void SetupObjects(SObject[] objs, IViewObjectSetup setup, bool preview, out DCompond sharedType);

    #endregion
}