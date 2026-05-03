using System;

namespace Suity.Drawing;

/// <summary>
/// Represents a font family data structure. Contains font family properties without actual rendering functionality.
/// </summary>
public sealed class FontFamilyDef
{
    /// <summary>
    /// Gets the name of this <see cref="FontFamilyDef"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontFamilyDef"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the font family.</param>
    public FontFamilyDef(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"FontFamily [Name={Name}]";
    }

    /// <summary>
    /// Gets a generic sans-serif <see cref="FontFamilyDef"/>.
    /// </summary>
    public static FontFamilyDef GenericSansSerif => new FontFamilyDef("Generic Sans Serif");

    /// <summary>
    /// Gets a generic serif <see cref="FontFamilyDef"/>.
    /// </summary>
    public static FontFamilyDef GenericSerif => new FontFamilyDef("Generic Serif");

    /// <summary>
    /// Gets a generic monospace <see cref="FontFamilyDef"/>.
    /// </summary>
    public static FontFamilyDef GenericMonospace => new FontFamilyDef("Generic Monospace");
}
