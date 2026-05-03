using Suity.Drawing;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Types;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Suity.Editor;

/// <summary>
/// Interface for text assets.
/// </summary>
[NativeType(Name = "TextAsset", Description = "Text asset", CodeBase = "*Core", Icon = "*CoreIcon|Text")]
public interface ITextAsset
{
    string GetText();
}

/// <summary>
/// Interface for paged text assets.
/// </summary>
[NativeType(Name = "PagedTextAsset", Description = "Paged text asset", CodeBase = "*Core", Icon = "*CoreIcon|Text")]
public interface IPagedTextAsset
{
    int PageCount { get; }

    string GetText(int pageIndex);
}

public abstract class TextAsset : ValueAsset, 
    ITextAsset, 
    IPagedTextAsset, 
    IRenderable, 
    IArticleAsset
{
    public TextAsset()
    {
        UpdateAssetTypes(typeof(IPagedTextAsset), typeof(ITextAsset), typeof(IRenderable), typeof(IArticleAsset));
    }

    public override ImageDef DefaultIcon => CoreIconCache.Text;

    public override ImageDef GetIcon() => EditorUtility.GetIconForFile(FileName?.FullPath);

    public override bool CanExportToLibrary => true;

    #region ITextAsset

    public abstract string GetText();

    #endregion

    #region IPagedTextAsset

    public virtual int PageCount => 1;

    public virtual string GetText(int pageIndex)
    {
        if (pageIndex == 0)
        {
            return GetText();
        }
        else 
        {
            return null;
        }
    } 

    #endregion

    #region IRenderable

    public virtual bool RenderEnabled => true;

    public virtual IMaterial DefaultMaterial => MaterialUtility.DefaultTextMaterial;

    public virtual IEnumerable<RenderItem> GetRenderItems()
    {
        string typeName = ShortTypeName;

        yield return new RenderItem(Id, this, RenderType.Text, typeName, this, this.LastUpdateTime);
    }

    public virtual IEnumerable<RenderTarget> GetRenderTargets(IMaterial material, RenderFileName basePath)
    {
        var path = basePath.WithNameSpace(NameSpace);

        return GetRenderItems().SelectMany(o => material.GetRenderTargets(o, path));
    }

    public virtual ICodeLibrary GetCodeLibrary()
    {
        return this.GetAttachedUserLibrary();
    }



    #endregion

    #region IArticleAsset

    public virtual IAttributeDesign Attributes => EmptyAttributeDesign.Empty;

    public virtual bool ReadingMaterial => true;

    public string GetTitle(bool inHierarchy)
    {
        if (this.Description is { } desc && !string.IsNullOrWhiteSpace(desc))
        {
            return desc;
        }
        else
        {
            return this.LocalName;
        }
    }

    public string GetOverview() => string.Empty;

    public virtual string GetContentText()
    {
        if (PageCount <= 1)
        {
            return GetText();
        }
        else
        {
            return string.Empty;
        }
    }

    public virtual string GetFullText(int markdownTitle = 1)
    {
        if (PageCount <= 1)
        {
            return GetText();
        }
        else
        {
            var builder = new StringBuilder();
            for (int i = 0; i < PageCount; i++)
            {
                builder.AppendLine($"# Page {i + 1}");
                builder.AppendLine(GetText(i));
            }

            return builder.ToString();
        }
    }

    public virtual IArticle GetArticle(bool tryLoadStorage = true) => null;

    #endregion
}
