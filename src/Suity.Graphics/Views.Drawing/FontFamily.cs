using System;

namespace Suity.Views.Drawing;

/// <summary>
/// Represents a font family data structure. Contains font family properties without actual rendering functionality.
/// </summary>
public sealed class FontFamily
{
    /// <summary>
    /// Gets the name of this <see cref="FontFamily"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontFamily"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the font family.</param>
    public FontFamily(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"FontFamily [Name={Name}]";
    }

    /// <summary>
    /// Gets a generic sans-serif <see cref="FontFamily"/>.
    /// </summary>
    public static FontFamily GenericSansSerif => new FontFamily("Generic Sans Serif");

    /// <summary>
    /// Gets a generic serif <see cref="FontFamily"/>.
    /// </summary>
    public static FontFamily GenericSerif => new FontFamily("Generic Serif");

    /// <summary>
    /// Gets a generic monospace <see cref="FontFamily"/>.
    /// </summary>
    public static FontFamily GenericMonospace => new FontFamily("Generic Monospace");
}
