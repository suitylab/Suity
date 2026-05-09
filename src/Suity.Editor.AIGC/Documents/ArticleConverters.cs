using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System.Text;

namespace Suity.Editor.Documents;

/// <summary>
/// Converts an <see cref="IArticleAsset"/> to its text content representation.
/// </summary>
public class IArticleAssetToTextConverter : TypeToTextConverter<IArticleAsset>
{
    /// <summary>
    /// Converts the specified article asset to its text content.
    /// </summary>
    /// <param name="objFrom">The article asset to convert.</param>
    /// <returns>The text content of the article, or an empty string if null.</returns>
    public override string Convert(IArticleAsset objFrom)
    {
        return objFrom.GetContentText() ?? string.Empty;
    }
}

/// <summary>
/// Converts <see cref="IArticleAsset"/> or arrays of article assets to <see cref="HistoryText"/> format.
/// </summary>
public class IArticleAssetToChatHistoryTextConverter : ITypeDefinitionConverter
{
    /// <summary>
    /// Gets the source types that this converter can handle.
    /// </summary>
    public TypeDefinition[] TypesFrom => [
        TypeDefinition.FromNative<IArticleAsset>(),
        TypeDefinition.FromNative<IArticleAsset>().MakeArrayType(),
        TypeDefinition.FromAssetLink<IArticleAsset>(),
        TypeDefinition.FromAssetLink<IArticleAsset>().MakeArrayType(),
        ];

    /// <summary>
    /// Gets the target types this converter produces.
    /// </summary>
    public TypeDefinition[] TypesTo => [TypeDefinition.FromNative<HistoryText>()];

    /// <summary>
    /// Converts an object to the specified target type.
    /// </summary>
    /// <param name="objFrom">The source object to convert.</param>
    /// <param name="typeTo">The target type definition.</param>
    /// <returns>The converted object as <see cref="HistoryText"/>.</returns>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        if (objFrom is not string && objFrom is System.Collections.IEnumerable ary)
        {
            return ConvertArray(ary);
        }
        else
        {
            return ConvertObject(objFrom);
        }

    }

    /// <summary>
    /// Converts an enumerable collection of article assets to chat history text.
    /// </summary>
    /// <param name="ary">The collection of article assets to convert.</param>
    /// <returns>The combined chat history text.</returns>
    public static HistoryText ConvertArray(System.Collections.IEnumerable ary)
    {
        var builder = new StringBuilder();
        foreach (var item in ary)
        {
            string text = ConvertObject(item);
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts a single object to chat history text.
    /// </summary>
    /// <param name="objFrom">The object to convert (can be <see cref="IArticleAsset"/> or <see cref="SAssetKey"/>).</param>
    /// <returns>The chat history text representation, or <see cref="HistoryText.Empty"/> if conversion fails.</returns>
    public static HistoryText ConvertObject(object objFrom)
    {
        if (objFrom is IArticleAsset articleAsset)
        {
            return GetArticleAsset(articleAsset);
        }
        else if (objFrom is SAssetKey key && key.TargetAsset is IArticleAsset inner)
        {
            return GetArticleAsset(inner);
        }
        else
        {
            return HistoryText.Empty;
        }
    }

    /// <summary>
    /// Gets the formatted article text with title and content wrapped in article tags.
    /// </summary>
    /// <param name="articleAsset">The article asset to format.</param>
    /// <returns>The formatted article text, or <see cref="HistoryText.Empty"/> if the asset is null.</returns>
    public static HistoryText GetArticleAsset(IArticleAsset articleAsset)
    {
        if (articleAsset is null)
        {
            return HistoryText.Empty;
        }

        string title = articleAsset.GetTitle(true);
        string text = articleAsset.GetContentText();

        return $"<article title='{title}'>\r\n{text}\r\n</article>";
    }
}

/// <summary>
/// Converts an <see cref="IArticle"/> to its text content representation.
/// </summary>
public class IArtlceToTextConverter : TypeToTextConverter<IArticle>
{
    /// <summary>
    /// Converts the specified article to its text content.
    /// </summary>
    /// <param name="objFrom">The article to convert.</param>
    /// <returns>The content text of the article, or an empty string if null.</returns>
    public override string Convert(IArticle objFrom)
    {
        return objFrom.Content ?? string.Empty;
    }
}

/// <summary>
/// Converts an <see cref="IArticle"/> to its corresponding <see cref="IArticleAsset"/>.
/// </summary>
public class IArticleToIArticleAssetConverter : TypeConverter<IArticle, IArticleAsset>
{
    /// <summary>
    /// Converts the article to its target asset.
    /// </summary>
    /// <param name="objFrom">The article to convert.</param>
    /// <returns>The target article asset.</returns>
    public override IArticleAsset Convert(IArticle objFrom)
    {
        return objFrom.TargetAsset as IArticleAsset;
    }
}

/// <summary>
/// Converts an <see cref="IArticleAsset"/> to its corresponding <see cref="IArticle"/>.
/// </summary>
public class IArticleAssetToIArticleConverter : TypeConverter<IArticleAsset, IArticle>
{
    /// <summary>
    /// Converts the article asset to its article representation.
    /// </summary>
    /// <param name="objFrom">The article asset to convert.</param>
    /// <returns>The article representation.</returns>
    public override IArticle Convert(IArticleAsset objFrom)
    {
        return objFrom.GetArticle();
    }
}

/// <summary>
/// Converts an <see cref="IArticle"/> or <see cref="IArticleAsset"/> to an asset link reference.
/// </summary>
public class IArticleToArticleAssetLinkConverter : ITypeDefinitionConverter
{
    /// <summary>
    /// Gets the source types that this converter can handle.
    /// </summary>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromNative<IArticle>(), TypeDefinition.FromNative<IArticleAsset>()];

    /// <summary>
    /// Gets the target types this converter produces.
    /// </summary>
    public TypeDefinition[] TypesTo => [TypeDefinition.FromAssetLink<IArticleAsset>()];

    /// <summary>
    /// Converts an article or article asset to an asset link key.
    /// </summary>
    /// <param name="objFrom">The source object to convert.</param>
    /// <param name="typeTo">The target type definition.</param>
    /// <returns>An <see cref="SAssetKey"/> representing the asset link, or null if conversion fails.</returns>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        var type = TypeDefinition.FromAssetLink<IArticleAsset>();

        if (objFrom is IArticle article)
        {
            return new SAssetKey(type, article.Id);
        }
        else if (objFrom is IArticleAsset asset)
        {
            return new SAssetKey(type, asset.Id);
        }
        else
        {
            return null;
        }
    }
}

/// <summary>
/// Converts an article asset link to <see cref="IArticle"/> or <see cref="IArticleAsset"/>.
/// </summary>
public class ArticleAssetLinkToIArticleConverter : ITypeDefinitionConverter
{
    /// <summary>
    /// Gets the source types that this converter can handle.
    /// </summary>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromAssetLink<IArticleAsset>()];

    /// <summary>
    /// Gets the target types this converter produces.
    /// </summary>
    public TypeDefinition[] TypesTo => [TypeDefinition.FromNative<IArticle>(), TypeDefinition.FromNative<IArticleAsset>()];

    /// <summary>
    /// Converts an asset link key to an article or article asset.
    /// </summary>
    /// <param name="objFrom">The source object to convert.</param>
    /// <param name="typeTo">The target type definition.</param>
    /// <returns>The resolved article or article asset, or null if conversion fails.</returns>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        var articleType = TypeDefinition.FromNative<IArticle>();
        var articleAssetType = TypeDefinition.FromNative<IArticleAsset>();

        if (objFrom is not SAssetKey key)
        {
            return null;
        }

        var asset = key.TargetAsset as IArticleAsset;
        if (asset is null)
        {
            return null;
        }

        if (typeTo == articleType)
        {
            return asset.GetArticle();
        }
        else if (typeTo == articleAssetType)
        {
            return asset;
        }
        else
        {
            return null;
        }
    }
}