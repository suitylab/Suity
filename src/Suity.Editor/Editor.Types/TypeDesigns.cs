using Suity.Selecting;
using System;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a type design in the editor.
/// </summary>
public interface ITypeDesign
{
    /// <summary>
    /// Event raised when the field type changes.
    /// </summary>
    event EventHandler FieldTypeChanged;

    /// <summary>
    /// Gets or sets the field type.
    /// </summary>
    TypeDefinition FieldType { get; set; }

    /// <summary>
    /// Gets the base type selection.
    /// </summary>
    ITypeDesignSelection BaseType { get; }

    /// <summary>
    /// Gets or sets whether this is an array.
    /// </summary>
    bool IsArray { get; set; }

    /// <summary>
    /// Gets the display text.
    /// </summary>
    string DisplayText { get; }

    /// <summary>
    /// Gets the icon.
    /// </summary>
    Image Icon { get; }

    /// <summary>
    /// Synchronizes the default value.
    /// </summary>
    object SyncDefaultValue(object value, IAssetFilter filter);
}

/// <summary>
/// Represents a type design selection.
/// </summary>
public interface ITypeDesignSelection : ISelection
{
    /// <summary>
    /// Gets or sets whether the selection is optional.
    /// </summary>
    bool Optional { get; set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Gets the DType.
    /// </summary>
    DType GetDType();

    /// <summary>
    /// Gets the type definition.
    /// </summary>
    TypeDefinition GetTypeDefinition();
}