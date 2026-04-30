using System;

namespace Suity.Editor.CodeRender.Replacing;

/// <summary>
/// Represents header information extracted from a replacement tag, containing mode, material, family, and type names.
/// </summary>
public class ReplacementHeaderInfo
{
    /// <summary>
    /// Gets or sets the mode identifier.
    /// </summary>
    public string Mode { get; set; }
    /// <summary>
    /// Gets or sets the material name.
    /// </summary>
    public string MaterialName { get; set; }
    /// <summary>
    /// Gets or sets the family name.
    /// </summary>
    public string FamilyName { get; set; }
    /// <summary>
    /// Gets or sets the type name.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplacementHeaderInfo"/> class with default values.
    /// </summary>
    public ReplacementHeaderInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplacementHeaderInfo"/> class with the specified values.
    /// </summary>
    /// <param name="mode">The mode identifier.</param>
    /// <param name="materialName">The material name.</param>
    /// <param name="familyName">The family name.</param>
    /// <param name="typeName">The type name.</param>
    public ReplacementHeaderInfo(string mode, string materialName, string familyName, string typeName)
    {
        Mode = mode;
        MaterialName = materialName;
        FamilyName = familyName;
        TypeName = typeName;
    }

    /// <summary>
    /// Generates a header string formatted with the specified prefix and suffix.
    /// </summary>
    /// <param name="prefix">The prefix to prepend to the header string.</param>
    /// <param name="suffix">The suffix to append to the header string.</param>
    /// <returns>The formatted header string in the format: {prefix}{Mode}/{MaterialName}/{FamilyName}/{TypeName}{suffix}.</returns>
    public string GetHeaderString(string prefix, string suffix)
    {
        return string.Format("{0}{1}/{2}/{3}/{4}{5}", prefix, Mode, MaterialName, FamilyName, TypeName, suffix);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format("[{0}][{1}][{2}][{3}]", Mode, MaterialName, FamilyName, TypeName);
    }

    /// <summary>
    /// Compares this header info with another for equality of all fields.
    /// </summary>
    /// <param name="other">The other <see cref="ReplacementHeaderInfo"/> to compare against.</param>
    /// <returns><c>true</c> if all fields match; otherwise, <c>false</c>.</returns>
    public bool IsEqual(ReplacementHeaderInfo other)
    {
        return Mode == other.Mode && MaterialName == other.MaterialName && FamilyName == other.FamilyName && TypeName == other.TypeName;
    }

    /// <summary>
    /// Attempts to collect and parse a <see cref="ReplacementHeaderInfo"/> from the specified source code.
    /// </summary>
    /// <param name="sourceCode">The source code string to search for the header.</param>
    /// <param name="prefix">The prefix marker that begins the header tag.</param>
    /// <param name="suffix">The suffix marker that ends the header tag.</param>
    /// <param name="info">When this method returns, contains the parsed <see cref="ReplacementHeaderInfo"/> if successful; otherwise, null.</param>
    /// <returns><c>true</c> if the header was successfully parsed; otherwise, <c>false</c>.</returns>
    public static bool Collect(string sourceCode, string prefix, string suffix, out ReplacementHeaderInfo info)
    {
        info = null;

        if (string.IsNullOrEmpty(sourceCode)) return false;

        int index = 0;

        // Get BeginTag header
        int ichBegin = sourceCode.IndexOf(prefix, index, StringComparison.Ordinal);
        if (ichBegin < 0) return false;
        index = ichBegin + prefix.Length;

        // Get BeginTag footer
        int icFooterBegin = sourceCode.IndexOf(suffix, index, StringComparison.Ordinal);
        if (icFooterBegin < 0) return false;
        index = icFooterBegin + suffix.Length;

        // Get BeginTag content
        string infoStr = sourceCode.Substring(ichBegin + prefix.Length, icFooterBegin - ichBegin - prefix.Length);
        string[] infoStrSplit = infoStr.Split('/');
        if (infoStrSplit.Length != 4) return false;

        info = new ReplacementHeaderInfo(infoStrSplit[0], infoStrSplit[1], infoStrSplit[2], infoStrSplit[3]);
        return true;
    }
}