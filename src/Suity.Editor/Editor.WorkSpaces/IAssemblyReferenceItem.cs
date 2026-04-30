using System;
using System.Drawing;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Assembly reference item
/// </summary>
public interface IAssemblyReferenceItem : IHasId, IComparable<IAssemblyReferenceItem>
{
    /// <summary>
    /// Gets the reference key
    /// </summary>
    string Key { get; }
    /// <summary>
    /// Gets the assembly name
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Gets the hint path
    /// </summary>
    string HintPath { get; }
    /// <summary>
    /// Gets the icon
    /// </summary>
    Image Icon { get; }
    /// <summary>
    /// Gets whether the reference is valid
    /// </summary>
    bool IsValid { get; }
    /// <summary>
    /// Gets whether the reference is disabled
    /// </summary>
    bool IsDisabled { get; }
}