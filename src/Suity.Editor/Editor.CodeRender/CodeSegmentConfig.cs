using System;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Configuration for code segments.
/// </summary>
public class CodeSegmentConfig
{
    /// <summary>
    /// Key splitter character.
    /// </summary>
    public const char KeySplitter = ':';

    /// <summary>
    /// Gen code tag.
    /// </summary>
    public const string GenCode = "GenCode";

    /// <summary>
    /// User code tag.
    /// </summary>
    public const string UserCode = "UserCode";

    /// <summary>
    /// Namespace tag.
    /// </summary>
    public const string NameSpace = "NameSpace";

    /// <summary>
    /// Default C# segment config.
    /// </summary>
    public static readonly CodeSegmentConfig CsDefault = new("//{#", "//}#", "#//");

    /// <summary>
    /// Prefix begin.
    /// </summary>
    public string PrefixBegin { get; }

    /// <summary>
    /// Prefix end.
    /// </summary>
    public string PrefixEnd { get; }

    /// <summary>
    /// Suffix.
    /// </summary>
    public string Suffix { get; }

    /// <summary>
    /// Creates a new code segment config.
    /// </summary>
    /// <param name="prefixBegin">Prefix begin.</param>
    /// <param name="prefixEnd">Prefix end.</param>
    /// <param name="suffix">Suffix.</param>
    public CodeSegmentConfig(string prefixBegin, string prefixEnd, string suffix)
    {
        PrefixBegin = prefixBegin;
        PrefixEnd = prefixEnd;
        Suffix = suffix;
    }

    /// <summary>
    /// Gets a key with all parameters.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="material">Material id.</param>
    /// <param name="renderType">Render type id.</param>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string GetKey(string tag, Guid material, Guid renderType, Guid itemId, string extension)
    {
        char s = KeySplitter;
        string key = $"{tag}{s}{material}{s}{renderType}{s}{itemId}{s}{extension}";

        return key;
    }

    /// <summary>
    /// Gets a key with all parameters (string versions).
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="material">Material id.</param>
    /// <param name="renderType">Render type id.</param>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string GetKey(string tag, string material, string renderType, string itemId, string extension)
    {
        char s = KeySplitter;
        string key = $"{tag}{s}{material}{s}{renderType}{s}{itemId}{s}{extension}";

        return key;
    }

    /// <summary>
    /// Gets a key without render type.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="material">Material id.</param>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string GetKey(string tag, Guid material, Guid itemId, string extension)
    {
        char s = KeySplitter;
        string key = $"{tag}{s}{material}{s}{itemId}{s}{extension}";

        return key;
    }

    /// <summary>
    /// Gets a key without render type (string version).
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="material">Material id.</param>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string Getey(string tag, string material, string itemId, string extension)
    {
        char s = KeySplitter;
        string key = $"{tag}{s}{material}{s}{itemId}{s}{extension}";

        return key;
    }

    /// <summary>
    /// Gets a key without material and render type.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string GetKey(string tag, Guid itemId, string extension)
    {
        char s = KeySplitter;
        string key = $"{tag}{s}{itemId}{s}{extension}";

        return key;
    }

    /// <summary>
    /// Gets a key without material and render type (string version).
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string GetKey(string tag, string itemId, string extension)
    {
        char s = KeySplitter;
        string key = $"{tag}{s}{itemId}{s}{extension}";

        return key;
    }

    /// <summary>
    /// Gets a key with only tag.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string GetKey(string tag, string extension)
    {
        char s = KeySplitter;
        string key = $"{tag}{s}{extension}";

        return key;
    }

    /// <summary>
    /// Gets a key automatically based on available parameters.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="material">Material id.</param>
    /// <param name="renderType">Render type id.</param>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The key.</returns>
    public string GetKeyAuto(string tag, string material, string renderType, string itemId, string extension)
    {
        bool mat = !string.IsNullOrEmpty(material);
        bool ren = !string.IsNullOrEmpty(renderType);
        bool itm = !string.IsNullOrEmpty(itemId);

        if (mat && ren && itm)
        {
            return GetKey(tag, material, renderType, itemId, extension);
        }
        else if (mat && itm)
        {
            return Getey(tag, material, itemId, extension);
        }
        else if (itm)
        {
            return GetKey(tag, itemId, extension);
        }
        else
        {
            return GetKey(tag, extension);
        }
    }
}