using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor;

/// <summary>
/// Provides capability to perform cross-package or cross-project refactoring moves.
/// </summary>
public interface ICrossMove
{
    /// <summary>
    /// Prepares the move operation.
    /// </summary>
    void ReadyMove();

    /// <summary>
    /// Executes the move operation with the specified refactor helper.
    /// </summary>
    /// <param name="refactor">The local refactor helper for renaming references.</param>
    void DoMove(ILocalRefactor refactor);
}

/// <summary>
/// Provides capability to rename element references during refactoring.
/// </summary>
public interface ILocalRefactor
{
    /// <summary>
    /// Renames an element from the old ID to the new ID.
    /// </summary>
    /// <param name="oldId">The old unique identifier.</param>
    /// <param name="newId">The new unique identifier.</param>
    void Rename(Guid oldId, Guid newId);
}

/// <summary>
/// Provides access to data objects in the editor system.
/// </summary>
public interface IDataSource
{
    /// <summary>
    /// Gets a value indicating whether data is available.
    /// </summary>
    bool DataAvailable { get; }

    /// <summary>
    /// Gets the data value for the specified id and field.
    /// </summary>
    /// <param name="id">The unique identifier of the data object.</param>
    /// <param name="field">The field name.</param>
    /// <param name="type">The data structure type (optional).</param>
    /// <returns>The data value, or null if not found.</returns>
    object GetData(string id, string field, DStruct type = null);

    /// <summary>
    /// Fills the specified object with data from the data source.
    /// </summary>
    /// <param name="id">The unique identifier of the data object.</param>
    /// <param name="obj">The object to fill with data.</param>
    void FillObject(string id, SObject obj);

    /// <summary>
    /// Checks whether the data source contains the specified id.
    /// </summary>
    /// <param name="id">The unique identifier to check.</param>
    /// <returns>True if the id exists; otherwise, false.</returns>
    bool ContainsId(string id);

    /// <summary>
    /// Gets all available ids in the data source.
    /// </summary>
    IEnumerable<string> Ids { get; }
}

/// <summary>
/// Provides access to a group of editor fields.
/// </summary>
public interface IFieldGroup
{
    /// <summary>
    /// Gets the field object with the specified name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The editor object, or null if not found.</returns>
    EditorObject GetFieldObject(string name);

    /// <summary>
    /// Gets all field objects in this group.
    /// </summary>
    IEnumerable<EditorObject> FieldObjects { get; }
}

/// <summary>
/// Provides strongly-typed access to a group of editor fields.
/// </summary>
public interface IFieldGroup<TField> : IFieldGroup
    where TField : EditorObject
{
    /// <summary>
    /// Gets the unique identifier of the field with the specified name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The unique identifier of the field.</returns>
    Guid GetFieldId(string name);

    /// <summary>
    /// Gets the field with the specified name.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The field object, or null if not found.</returns>
    TField GetField(string name);

    /// <summary>
    /// Gets the field with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the field.</param>
    /// <returns>The field object, or null if not found.</returns>
    TField GetField(Guid id);

    /// <summary>
    /// Gets all fields in this group.
    /// </summary>
    IEnumerable<TField> Fields { get; }

    /// <summary>
    /// Gets the number of fields in this group.
    /// </summary>
    int FieldCount { get; }
}

/// <summary>
/// Provides access to a unique identifier.
/// </summary>
public interface IHasId
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Provides access to a collection of unique identifiers.
/// </summary>
public interface IIdCluster
{
    /// <summary>
    /// Gets all unique identifiers in this cluster.
    /// </summary>
    IEnumerable<Guid> Ids { get; }
}

/// <summary>
/// Native type interface
/// </summary>
public interface INativeType
{
    /// <summary>
    /// Gets the native System.Type associated with this object.
    /// </summary>
    Type NativeType { get; }
}

/// <summary>
/// Provides capability to compare values for equality.
/// </summary>
public interface IValueEqual
{
    /// <summary>
    /// Determines whether the specified object has the same value as this instance.
    /// </summary>
    /// <param name="other">The object to compare.</param>
    /// <returns>True if the values are equal; otherwise, false.</returns>
    bool ValueEquals(object other);
}

/// <summary>
/// Provides utility methods for comparing values using IValueEqual.
/// </summary>
public static class ValueEqualUtility
{
    /// <summary>
    /// Compares two objects for value equality.
    /// </summary>
    /// <param name="a">The first object to compare.</param>
    /// <param name="b">The second object to compare.</param>
    /// <returns>True if the values are equal; otherwise, false.</returns>
    public static bool ValueEquals(object a, object b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a is IValueEqual va)
        {
            return (va).ValueEquals(b);
        }

        if (b is IValueEqual vb)
        {
            return (vb).ValueEquals(a);
        }

        return Equals(a, b);
    }
}

/// <summary>
/// Provides access to a design-time value object.
/// </summary>
public interface IDesignValue
{
    /// <summary>
    /// Gets the design value object.
    /// </summary>
    SObject Value { get; }
}

/// <summary>
/// Provides capability to generate a brief description of an object.
/// </summary>
public interface IBrief
{
    /// <summary>
    /// Generates a brief description of the specified object.
    /// </summary>
    /// <param name="obj">The object to describe.</param>
    /// <param name="depth">The current depth of the description.</param>
    /// <param name="baseBrief">A function that provides the base brief description.</param>
    /// <param name="originBrief">A function that provides the original brief description.</param>
    /// <returns>A brief description string.</returns>
    string GetBrief(object obj, int depth, Func<string> baseBrief, Func<string> originBrief);
}

/// <summary>
/// Provides capability to check conditions associated with an object.
/// </summary>
public interface ICondition
{
    /// <summary>
    /// Determines whether the specified condition exists.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <returns>True if the condition exists; otherwise, false.</returns>
    bool HasCondition(string condition);

    /// <summary>
    /// Gets all available conditions.
    /// </summary>
    IEnumerable<string> Conditions { get; }
}

/// <summary>
/// Provides capability to select and manage conditions.
/// </summary>
public interface IConditionSelection
{
    /// <summary>
    /// Gets all available conditions.
    /// </summary>
    IEnumerable<string> Conditions { get; }

    /// <summary>
    /// Gets or sets the currently selected condition.
    /// </summary>
    string SelectedCondition { get; set; }
}

/// <summary>
/// Provides capability to commit changes with a marker.
/// </summary>
public interface ICommit
{
    /// <summary>
    /// Commits the changes with the specified marker.
    /// </summary>
    /// <param name="marker">An object that marks the commit operation.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task Commit(object marker);
}


