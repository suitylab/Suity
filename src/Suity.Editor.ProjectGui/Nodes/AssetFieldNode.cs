using Suity.Helpers;
using Suity.Views;
using Suity.Views.PathTree;
using System;
using System.Drawing;

namespace Suity.Editor.ProjectGui.Nodes;

/// <summary>
/// Represents a field node within an asset element in the project tree view.
/// </summary>
public class AssetFieldNode : PathNode, IHasId
{
    private FieldObject _cachedFieldObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetFieldNode"/> class.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    public AssetFieldNode(string fieldName)
    {
        FieldName = fieldName;
    }

    /// <summary>
    /// Gets the name of this field.
    /// </summary>
    public string FieldName { get; }

    /// <inheritdoc/>
    public override bool Reusable => true;

    /// <inheritdoc/>
    public override string TypeName => GetFieldObject()?.Name ?? string.Empty;

    /// <inheritdoc/>
    public override Image Image
    {
        get
        {
            if (GetFieldObject() is FieldObject f)
            {
                return f.GetIcon()?.ToIconSmall() ?? CoreIconCache.Field.ToIconSmall();
            }
            else
            {
                return CoreIconCache.Field.ToIconSmall();
            }
        }
    }

    /// <inheritdoc/>
    public override Color? Color
    {
        get
        {
            if (TextColorStatus == TextStatus.Normal && GetFieldObject() is IViewColor c)
            {
                return c.ViewColor;
            }

            return base.Color;
        }
    }

    /// <summary>
    /// Gets the field object associated with this node.
    /// </summary>
    /// <returns>The field object, or null if not found.</returns>
    public FieldObject GetFieldObject()
    {
        if (_cachedFieldObject != null && _cachedFieldObject.Id != Guid.Empty)
        {
            return _cachedFieldObject;
        }

        if (this.Parent is AssetElementNode fileNode)
        {
            if (fileNode.GetAsset() is IFieldGroup asset)
            {
                _cachedFieldObject = asset.GetFieldObject(FieldName) as FieldObject;

                return _cachedFieldObject;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public override bool CanUserDrag => false;

    /// <inheritdoc/>
    public override object DisplayedValue => GetFieldObject();

    /// <inheritdoc/>
    Guid IHasId.Id => GetFieldObject()?.Id ?? Guid.Empty;
}