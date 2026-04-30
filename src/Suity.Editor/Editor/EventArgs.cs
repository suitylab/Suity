using Suity.Editor.WorkSpaces;
using System;

namespace Suity.Editor;

/// <summary>
/// Types of entry updates.
/// </summary>
public enum EntryUpdateTypes
{
    /// <summary>Object was added.</summary>
    Add,
    /// <summary>Object was removed.</summary>
    Remove,
    /// <summary>Object was updated.</summary>
    Update,
    /// <summary>Object was renamed.</summary>
    Rename,
    /// <summary>Object was replaced.</summary>
    Replace,
    /// <summary>Element was updated.</summary>
    ElementUpdate,
}

#region EntryEventArgs

/// <summary>
/// Base class for entry event arguments.
/// </summary>
public class EntryEventArgs : EventArgs
{
    /// <summary>
    /// Gets an empty event args instance.
    /// </summary>
    public new static readonly EntryEventArgs Empty = new();

    /// <summary>
    /// Gets the type of update.
    /// </summary>
    public virtual EntryUpdateTypes UpdateType => EntryUpdateTypes.Update;
    /// <summary>
    /// Gets whether the update is delayed.
    /// </summary>
    public virtual bool Delayed => false;
    /// <summary>
    /// Gets the inner event args.
    /// </summary>
    public virtual EntryEventArgs Inner => null;
    /// <summary>
    /// Gets the stack count for nested events.
    /// </summary>
    public virtual int StackCount => 0;

    /// <summary>
    /// Creates a new EntryEventArgs instance.
    /// </summary>
    public EntryEventArgs()
    {
    }

    public override string ToString()
    {
        return $"[Normal {UpdateType} event]";
    }
}

#endregion

#region DelayedEntryEventArgs

/// <summary>
/// Entry event arguments with delayed flag.
/// </summary>
public class DelayedEntryEventArgs : EntryEventArgs
{
    /// <summary>
    /// Gets an empty delayed event args instance.
    /// </summary>
    public new static readonly DelayedEntryEventArgs Empty = new();

    public override bool Delayed => true;

    /// <summary>
    /// Creates a new DelayedEntryEventArgs instance.
    /// </summary>
    public DelayedEntryEventArgs()
    {
    }

    public override string ToString()
    {
        return $"[Delayed {UpdateType} event]";
    }
}

#endregion

#region EntryResolvedEventArgs

/// <summary>
/// Entry event arguments for resolved ID.
/// </summary>
public class EntryResolvedEventArgs : EntryEventArgs
{
    /// <summary>
    /// Gets the singleton instance of EntryResolvedEventArgs.
    /// </summary>
    public static readonly EntryResolvedEventArgs Instance = new();

    public override bool Delayed => true;

    /// <summary>
    /// Creates a new EntryResolvedEventArgs instance.
    /// </summary>
    public EntryResolvedEventArgs()
    {
    }

    public override string ToString()
    {
        return $"[Resolved id {UpdateType} event]";
    }
}

#endregion

#region PropertyEntryEventArgs

/// <summary>
/// Entry event arguments for property changes.
/// </summary>
public class PropertyEntryEventArgs : EntryEventArgs
{
    public override bool Delayed => true;

    /// <summary>
    /// Gets the name of the field that changed.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Creates a new PropertyEntryEventArgs instance.
    /// </summary>
    /// <param name="fieldName">The name of the field that changed.</param>
    public PropertyEntryEventArgs(string fieldName)
    {
        FieldName = fieldName;
    }

    public override string ToString()
    {
        return $"[Property:{FieldName} changed event]";
    }
}

#endregion

#region ReplaceEntryEventArgs

/// <summary>
/// Entry event arguments for replacement operations.
/// </summary>
public class ReplaceEntryEventArgs : EntryEventArgs
{
    public override EntryUpdateTypes UpdateType => EntryUpdateTypes.Replace;

    public override bool Delayed => false;

    /// <summary>
    /// Gets the old object that was replaced.
    /// </summary>
    public EditorObject OldObject { get; }
    /// <summary>
    /// Gets the new object that replaced the old one.
    /// </summary>
    public EditorObject NewObject { get; }

    /// <summary>
    /// Creates a new ReplaceEntryEventArgs instance.
    /// </summary>
    /// <param name="oldObject">The old object that was replaced.</param>
    /// <param name="newObject">The new object that replaced the old one.</param>
    public ReplaceEntryEventArgs(EditorObject oldObject, EditorObject newObject)
    {
        OldObject = oldObject;
        NewObject = newObject;
    }

    public override string ToString()
    {
        return $"[Replace event {OldObject} to {NewObject}]";
    }
}

#endregion

#region RenameAssetEventArgs

/// <summary>
/// Entry event arguments for asset rename operations.
/// </summary>
public class RenameAssetEventArgs : EntryEventArgs
{
    public override EntryUpdateTypes UpdateType => EntryUpdateTypes.Rename;

    public override bool Delayed => false;

    /// <summary>
    /// Gets the old name of the asset.
    /// </summary>
    public string OldName { get; }
    /// <summary>
    /// Gets the new name of the asset.
    /// </summary>
    public string NewName { get; }

    /// <summary>
    /// Creates a new RenameAssetEventArgs instance.
    /// </summary>
    /// <param name="oldName">The old name of the asset.</param>
    /// <param name="newName">The new name of the asset.</param>
    public RenameAssetEventArgs(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }

    public override string ToString()
    {
        return $"[Rename event {OldName} to {NewName}]";
    }
}

#endregion

#region GroupAssetEventArgs

/// <summary>
/// Entry event arguments for group asset operations.
/// </summary>
public class GroupAssetEventArgs : EntryEventArgs
{
    /// <summary>
    /// Gets the child asset involved in the operation.
    /// </summary>
    public Asset ChildAsset { get; }
    public override EntryUpdateTypes UpdateType { get; }
    public override EntryEventArgs Inner { get; }
    public override int StackCount { get; }

    public override bool Delayed { get; }

    /// <summary>
    /// Creates a new GroupAssetEventArgs instance.
    /// </summary>
    /// <param name="childAsset">The child asset involved in the operation.</param>
    /// <param name="updateType">The type of update operation.</param>
    public GroupAssetEventArgs(Asset childAsset, EntryUpdateTypes updateType)
    {
        ChildAsset = childAsset;
        UpdateType = updateType;
        Delayed = true;
    }

    /// <summary>
    /// Creates a new GroupAssetEventArgs instance with inner event args.
    /// </summary>
    /// <param name="childAsset">The child asset involved in the operation.</param>
    /// <param name="updateType">The type of update operation.</param>
    /// <param name="inner">The inner event arguments.</param>
    public GroupAssetEventArgs(Asset childAsset, EntryUpdateTypes updateType, EntryEventArgs inner)
        : this(childAsset, updateType)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        Delayed = false;
        StackCount = inner.StackCount + 1;
    }

    public override string ToString()
    {
        if (Inner != null)
        {
            return $"[Group {UpdateType} event : {ChildAsset}, inner : {Inner}]";
        }
        else
        {
            return $"[Group {UpdateType} event : {ChildAsset}]";
        }
    }
}

#endregion

#region FieldEntryEventArgs

/// <summary>
/// Entry event arguments for field operations.
/// </summary>
public class FieldEntryEventArgs : EntryEventArgs
{
    public override EntryUpdateTypes UpdateType { get; }
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the old name of the field.
    /// </summary>
    public string OldName { get; }

    public override EntryEventArgs Inner { get; }
    public override int StackCount { get; }
    public override bool Delayed { get; }

    /// <summary>
    /// Creates a new FieldEntryEventArgs instance.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="updateType">The type of update operation.</param>
    public FieldEntryEventArgs(string name, EntryUpdateTypes updateType)
    {
        Name = name;
        UpdateType = updateType;

        Delayed = true;
    }

    /// <summary>
    /// Creates a new FieldEntryEventArgs instance with inner event args.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="updateType">The type of update operation.</param>
    /// <param name="inner">The inner event arguments.</param>
    public FieldEntryEventArgs(string name, EntryUpdateTypes updateType, EntryEventArgs inner)
    {
        Name = name;
        UpdateType = updateType;
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));

        StackCount = inner.StackCount + 1;
        Delayed = false;
    }

    /// <summary>
    /// Creates a new FieldEntryEventArgs instance for rename operations.
    /// </summary>
    /// <param name="oldName">The old name of the field.</param>
    /// <param name="newName">The new name of the field.</param>
    /// <param name="updateType">The type of update operation.</param>
    public FieldEntryEventArgs(string oldName, string newName, EntryUpdateTypes updateType)
    {
        Name = newName;
        OldName = oldName;
        UpdateType = updateType;

        Delayed = true;
    }

    public override string ToString()
    {
        if (Inner != null)
        {
            return $"[Field:{Name} {UpdateType} event, inner : {Inner}]";
        }
        else
        {
            return $"[Field:{Name} {UpdateType} event]";
        }
    }
}

#endregion

#region RefEntryEventArgs

/// <summary>
/// Entry event arguments for reference operations.
/// </summary>
public class RefEntryEventArgs : EntryEventArgs
{
    /// <summary>
    /// Gets the target object of the reference.
    /// </summary>
    public EditorObject Target { get; }

    public override EntryEventArgs Inner { get; }
    public override int StackCount { get; }
    public override bool Delayed => false;

    /// <summary>
    /// Creates a new RefEntryEventArgs instance.
    /// </summary>
    /// <param name="target">The target object of the reference.</param>
    /// <param name="inner">The inner event arguments.</param>
    public RefEntryEventArgs(EditorObject target, EntryEventArgs inner)
    {
        Target = target;
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));

        StackCount = inner.StackCount + 1;
    }

    public override string ToString()
    {
        return $"[Ref {UpdateType} event : {Target}, inner : {Inner}]";
    }
}

#endregion

#region WorkSpaceEventArgs

/// <summary>
/// Event arguments for workspace events.
/// </summary>
public class WorkSpaceEventArgs : EventArgs
{
    /// <summary>
    /// Gets the workspace associated with the event.
    /// </summary>
    public WorkSpace WorkSpace { get; }

    /// <summary>
    /// Creates a new WorkSpaceEventArgs instance.
    /// </summary>
    /// <param name="workSpace">The workspace associated with the event.</param>
    public WorkSpaceEventArgs(WorkSpace workSpace)
    {
        WorkSpace = workSpace;
    }
}

#endregion

#region WorkSpaceRenameEventArgs

/// <summary>
/// Event arguments for workspace rename events.
/// </summary>
public class WorkSpaceRenameEventArgs : EventArgs
{
    /// <summary>
    /// Gets the workspace associated with the event.
    /// </summary>
    public WorkSpace WorkSpace { get; }
    /// <summary>
    /// Gets the old name of the workspace.
    /// </summary>
    public string OldName { get; }

    /// <summary>
    /// Creates a new WorkSpaceRenameEventArgs instance.
    /// </summary>
    /// <param name="workSpace">The workspace associated with the event.</param>
    /// <param name="oldName">The old name of the workspace.</param>
    public WorkSpaceRenameEventArgs(WorkSpace workSpace, string oldName)
    {
        WorkSpace = workSpace;
        OldName = oldName;
    }
}

#endregion

#region ObjectPropertyEventArgs

/// <summary>
/// Event arguments for object property change events.
/// </summary>
public class ObjectPropertyEventArgs : EventArgs
{
    /// <summary>
    /// Gets the objects affected by the property change.
    /// </summary>
    public object[] Objects { get; }
    /// <summary>
    /// Gets the name of the property that changed.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Creates a new ObjectPropertyEventArgs instance.
    /// </summary>
    /// <param name="objs">The objects affected by the property change.</param>
    /// <param name="propertyName">The name of the property that changed.</param>
    public ObjectPropertyEventArgs(object[] objs, string propertyName)
    {
        Objects = objs;
        PropertyName = propertyName;
    }
}

#endregion
