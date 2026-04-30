using Suity.Editor.Types;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Values;

/// <summary>
/// Resolves native field names and IDs for types that use native field mapping.
/// Provides caching to improve lookup performance.
/// </summary>
class NativeFieldResolver
{
    /// <summary>
    /// Stores information about a native field mapping.
    /// </summary>
    class NativeFieldInfo
    {
        /// <summary>
        /// Gets or sets the type that owns this field.
        /// </summary>
        public DType Type { get; set; }

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the field ID.
        /// </summary>
        public Guid FieldId { get; set; }
    }

    /// <summary>
    /// Gets the singleton instance of <see cref="NativeFieldResolver"/>.
    /// </summary>
    public static NativeFieldResolver Instance { get; } = new();

    readonly object _lock = new();
    readonly Dictionary<string, NativeFieldInfo> _fieldsByFullName = [];
    readonly Dictionary<Guid, NativeFieldInfo> _fieldsById = [];

    private NativeFieldResolver()
    {
    }

    /// <summary>
    /// Resolves the field name for the given type and field ID.
    /// </summary>
    /// <param name="type">The type that owns the field.</param>
    /// <param name="id">The field ID to resolve.</param>
    /// <returns>The field name, or null if not found.</returns>
    public string ResolveFieldName(DType type, Guid id)
    {
        lock (_lock)
        {
            if (type is null || id == Guid.Empty)
            {
                return null;
            }

            if (_fieldsById.TryGetValue(id, out var info))
            {
                return info.FieldName;
            }

            // Get full name
            string fullFieldName = GlobalIdResolver.RevertResolve(id);
            string fieldName = null;
            if (!string.IsNullOrWhiteSpace(fullFieldName) && fullFieldName.StartsWith("$NF:"))
            {
                var lastDotIndex = fullFieldName.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    fieldName = fullFieldName[(lastDotIndex + 1)..];
                }
                else
                {
                    fieldName = fullFieldName[4..];
                }
            }

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            info = new NativeFieldInfo
            {
                Type = type,
                FieldName = fieldName,
                FieldId = id,
            };

            string fieldFullName = $"$NF:{type.FullName}.{fieldName}";

            _fieldsByFullName[fieldFullName] = info;
            _fieldsById[id] = info;

            return fieldName; 
        }
    }

    /// <summary>
    /// Resolves the field ID for the given type and field name.
    /// </summary>
    /// <param name="type">The type that owns the field.</param>
    /// <param name="fieldName">The field name to resolve.</param>
    /// <returns>The field ID, or <see cref="Guid.Empty"/> if not found.</returns>
    public Guid ResolveFieldId(DType type, string fieldName)
    {
        lock (_lock)
        {
            if (type is null || string.IsNullOrWhiteSpace(fieldName))
            {
                return Guid.Empty;
            }

            string fieldFullName = $"$NF:{type.FullName}.{fieldName}";
            if (_fieldsByFullName.TryGetValue(fieldFullName, out var info))
            {
                return info.FieldId;
            }

            Guid id = GlobalIdResolver.Resolve(fieldFullName);
            if (id == Guid.Empty)
            {
                return Guid.Empty;
            }

            info = new NativeFieldInfo
            {
                Type = type,
                FieldName = fieldName,
                FieldId = id,
            };

            _fieldsByFullName[fieldFullName] = info;
            _fieldsById[id] = info;

            return id; 
        }
    }
}
