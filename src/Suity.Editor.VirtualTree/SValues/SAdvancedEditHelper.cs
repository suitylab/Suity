using Suity.Editor.Values;
using System;

namespace Suity.Editor.VirtualTree.SValues;

/// <summary>
/// Helper extension methods for advanced editing operations on <see cref="SItem"/>-backed virtual nodes.
/// </summary>
internal static class SAdvancedEditHelper
{
    /// <summary>
    /// Retrieves field navigation information from the value returned by the given getter.
    /// If the value is an <see cref="SItem"/> with a non-empty <see cref="SItem.FieldId"/>,
    /// returns that <see cref="Guid"/>; otherwise returns the item's <see cref="SItem.InputType"/>.
    /// </summary>
    /// <param name="editor">The virtual node performing the edit.</param>
    /// <param name="getter">A function that retrieves the current value from the node.</param>
    /// <returns>A <see cref="Guid"/> field identifier, a <see cref="DTypeCode"/>, or <c>null</c>.</returns>
    public static object GetFieldInfomation(this VirtualNode editor, Func<object> getter)
    {
        SItem firstObj = getter() as SItem;
        if (firstObj is null)
        {
            return null;
        }

        if (firstObj.FieldId != Guid.Empty)
        {
            return firstObj.FieldId;
        }
        else
        {
            return firstObj.InputType;
        }
    }
}
