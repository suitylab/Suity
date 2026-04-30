using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Views;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a field in a type definition.
/// </summary>
public abstract class DField : FieldObject,
    IViewColor,
    IAttributeGetter,
    ICodeRenderElement,
    IHasToolTips
{
    /// <summary>
    /// Gets the parent type.
    /// </summary>
    public virtual DType ParentType => Parent as DType;

    /// <summary>
    /// Gets the field index.
    /// </summary>
    public virtual int Index { get; }

    /// <summary>
    /// Gets the field description.
    /// </summary>
    public virtual string Description { get; }

    /// <summary>
    /// Gets the binding info.
    /// </summary>
    public virtual object BindingInfo { get; }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public virtual IAttributeGetter Attributes { get; }

    /// <summary>
    /// Gets whether the field is disabled.
    /// </summary>
    public virtual bool IsDisabled => Attributes?.GetIsDisabled() == true;

    /// <summary>
    /// Gets whether the field is hidden.
    /// </summary>
    public virtual bool IsHidden => Attributes?.GetIsHidden() == true;

    /// <summary>
    /// Gets whether the field is in preview mode.
    /// </summary>
    public virtual bool IsPreview => Attributes?.GetIsPreview() == true;

    /// <summary>
    /// Gets whether the field is hidden or disabled.
    /// </summary>
    public bool IsHiddenOrDisabled => Attributes?.GetIsHiddenOrDisabled() == true;

    /// <summary>
    /// Gets the access mode.
    /// </summary>
    public virtual AssetAccessMode AccessMode { get; } = AssetAccessMode.Public;

    /// <summary>
    /// Gets the view color.
    /// </summary>
    public virtual Color? ViewColor => null;

    public override object GetStorageObject(bool tryLoadStorage = true)
    {
        return (ParentType?.GetStorageObject(tryLoadStorage) as IMemberContainer)?.GetMember(Name);
    }

    public override string DisplayText
    {
        get
        {
            //string parentText = null;
            string text;

            if (EditorUtility.ShowAsDescription.Value)
            {
                text = !string.IsNullOrEmpty(Description) ? Description : Name;

                return text;
            }
            else
            {
                //parentText = Parent?.Name;
                // text = Name;
                return Name;

                //if (!string.IsNullOrEmpty(parentText))
                //{
                //    return $"{parentText}.{text}";
                //}
                //else
                //{
                //    return text;
                //}
            }
        }
    }

    #region ICompileNode

    public virtual RenderType RenderType => null;

    public virtual object GetProperty(CodeRenderProperty property, object argument) => property.PropertyName switch
    {
        CodeRenderProperty.Id => Id,
        CodeRenderProperty.Name => Name,
        CodeRenderProperty.FullName => FullName,
        CodeRenderProperty.FullTypeName => $"{ParentType.FullTypeName}.{this.Name}",
        CodeRenderProperty.Description => Description,
        CodeRenderProperty.Parent => Parent,
        _ => null,
    };

    #endregion

    #region IHasToolTips
    public string ToolTips => Attributes.GetAttribute<ToolTipsAttribute>()?.ToolTips;

    #endregion

    #region IHasAttribute

    public IEnumerable<object> GetAttributes() => Attributes.GetAttributes();

    public IEnumerable<object> GetAttributes(string typeName) => Attributes.GetAttributes(typeName);

    public IEnumerable<T> GetAttributes<T>() where T : class => Attributes.GetAttributes<T>();

    #endregion
}