using System.Diagnostics;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Abstract base class for code segment replacements in rendered output.
/// </summary>
public abstract class Replacement
{
    private readonly CodeSegmentConfig _config;

    /// <summary>
    /// Gets the code segment configuration.
    /// </summary>
    public CodeSegmentConfig Config => _config;

    /// <summary>
    /// Represents the complete string key after removing tag markers
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Main tag type, usually (UserCode)
    /// </summary>
    public string TagType { get; set; }

    /// <summary>
    /// 1st parameter in key: Material
    /// </summary>
    public string Material { get; private set; }

    /// <summary>
    /// 2nd parameter in key: Render Type
    /// </summary>
    public string RenderType { get; private set; }

    /// <summary>
    /// 3rd parameter in key: Render Object Key
    /// </summary>
    public string ItemKey { get; private set; }

    /// <summary>
    /// 4th parameter in key: Extension
    /// </summary>
    public string Extension { get; private set; }

    /// <summary>
    /// Flag indicating whether matched; when false means the flag is obsolete
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Creates a new replacement with the specified parameters.
    /// </summary>
    /// <param name="config">The code segment configuration.</param>
    /// <param name="tagType">The tag type.</param>
    /// <param name="material">The material name.</param>
    /// <param name="renderType">The render type.</param>
    /// <param name="keyString">The key string.</param>
    /// <param name="ext">The extension.</param>
    public Replacement(CodeSegmentConfig config, string tagType, string material, string renderType, string keyString, string ext)
    {
        Debug.Assert(config != null);

        _config = config;

        TagType = tagType ?? string.Empty;
        Material = material ?? string.Empty;
        RenderType = renderType ?? string.Empty;
        ItemKey = keyString ?? string.Empty;
        Extension = ext ?? string.Empty;

        char s = CodeSegmentConfig.KeySplitter;

        Key = $"{TagType}{s}{Material}{s}{RenderType}{s}{ItemKey}{s}{Extension}";
    }

    /// <summary>
    /// Creates a new replacement by cloning an existing one.
    /// </summary>
    /// <param name="repClone">The replacement to clone from.</param>
    public Replacement(Replacement repClone)
        : this(repClone.Config, repClone.TagType, repClone.Material, repClone.RenderType, repClone.ItemKey, repClone.Extension)
    {
    }

    /// <summary>
    /// Gets the complete code string for this replacement.
    /// </summary>
    /// <returns>The code string.</returns>
    public abstract string GetCode();

    /// <summary>
    /// Gets the inner code string (excluding outer tags).
    /// </summary>
    /// <returns>The inner code string.</returns>
    public abstract string GetInnerCode();

    /// <summary>
    /// Checks if the code content is empty or whitespace.
    /// </summary>
    /// <returns>True if the code is empty or whitespace.</returns>
    public bool GetIsCodeEmpty() => string.IsNullOrWhiteSpace(GetCode());

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetCode();
    }
}