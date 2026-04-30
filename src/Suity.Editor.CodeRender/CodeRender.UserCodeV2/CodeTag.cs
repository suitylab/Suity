using LiteDB;
using Suity.Editor.CodeRender.Replacing;
using Suity.Editor.Services;

namespace Suity.Editor.CodeRender.UserCodeV2;

//TODO: Suity.Editor.CodeRender.UserCodeV2.CodeTag needs to store UserTagConfig in the database

/// <summary>
/// Represents a code tag that stores user code metadata and content in the database.
/// A code tag identifies a specific user code segment by its location, material, render type, key, and extension.
/// </summary>
public class CodeTag
{
    private string _code;

    /// <summary>
    /// Gets or sets the unique identifier for this code tag in the database.
    /// </summary>
    public ObjectId Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the file that contains this code tag.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the location identifier within the file where this code tag applies.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the material identifier associated with this code tag.
    /// </summary>
    public string Material { get; set; }

    /// <summary>
    /// Gets or sets the render type that determines how this code tag should be rendered.
    /// </summary>
    public string RenderType { get; set; }

    /// <summary>
    /// Gets or sets the key string that uniquely identifies this code tag within its category.
    /// </summary>
    public string KeyString { get; set; }

    /// <summary>
    /// Gets or sets the extension identifier for additional categorization of this code tag.
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// Gets or sets the actual code content stored in this tag.
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this code tag is suspended (temporarily disabled).
    /// </summary>
    public bool IsSuspended { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeTag"/> class.
    /// </summary>
    public CodeTag()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the code content is empty or contains only whitespace.
    /// </summary>
    public bool IsCodeEmpty => string.IsNullOrEmpty(GetTrimCode());

    /// <summary>
    /// Determines whether this code tag's key (material, render type, key string, and extension) matches the specified values.
    /// </summary>
    /// <param name="material">The material to compare against.</param>
    /// <param name="type">The render type to compare against.</param>
    /// <param name="keyString">The key string to compare against.</param>
    /// <param name="ext">The extension to compare against.</param>
    /// <returns><c>true</c> if all key components match; otherwise, <c>false</c>.</returns>
    public bool KeyEquals(string material, string type, string keyString, string ext)
    {
        return Material == material && RenderType == type && KeyString == keyString && Extension == ext;
    }

    /// <summary>
    /// Determines whether this code tag's key matches the key of another code tag.
    /// </summary>
    /// <param name="other">The other <see cref="CodeTag"/> to compare against.</param>
    /// <returns><c>true</c> if all key components match; otherwise, <c>false</c>.</returns>
    public bool KeyEquals(CodeTag other)
    {
        return KeyEquals(other.Material, other.RenderType, other.KeyString, other.Extension);
    }

    /// <summary>
    /// Gets the full key string that uniquely identifies this code tag, including the segment type prefix.
    /// </summary>
    /// <returns>A formatted string representing the complete key for this code tag.</returns>
    public string GetFullKey()
    {
        return $"{CodeSegmentConfig.UserCode}:{Material}:{RenderType}:{KeyString}:{Extension}";
    }

    /// <summary>
    /// Gets the code content with leading and trailing whitespace characters removed.
    /// Whitespace characters include newline (10), carriage return (13), space (32), and tab (9).
    /// </summary>
    /// <returns>The trimmed code content, or an empty string if the code is null.</returns>
    public string GetTrimCode()
    {
        Code ??= string.Empty;

        return Code.Trim((char)10, (char)13, (char)32, (char)9);
    }

    /// <summary>
    /// Copies all property values from another <see cref="CodeTag"/> instance to this instance.
    /// </summary>
    /// <param name="item">The source <see cref="CodeTag"/> to copy values from.</param>
    public void CopyFrom(CodeTag item)
    {
        FileName = item.FileName;
        Location = item.Location;
        Material = item.Material;
        RenderType = item.RenderType;
        KeyString = item.KeyString;
        Extension = item.Extension;
        Code = item.Code;
        IsSuspended = item.IsSuspended;
    }

    /// <summary>
    /// Copies relevant property values from a <see cref="SegmentTagNode"/> to this code tag.
    /// </summary>
    /// <param name="r">The source <see cref="SegmentTagNode"/> to copy values from.</param>
    public void CopyFrom(SegmentTagNode r)
    {
        Material = r.Material;
        RenderType = r.RenderType;
        KeyString = r.ItemKey;
        Extension = r.Extension;
        Code = r.GetCode();
    }

    /// <summary>
    /// Suspends this code tag by adopting the code from another tag and marking itself as suspended.
    /// If the other tag's code is empty, only marks this tag as suspended without changing the code.
    /// </summary>
    /// <param name="other">The other <see cref="CodeTag"/> to suspend from.</param>
    /// <returns><c>true</c> if the state of this tag was changed; otherwise, <c>false</c>.</returns>
    public bool Suspend(CodeTag other)
    {
        // If the other is empty or equal
        if (other.IsCodeEmpty)
        {
            if (IsSuspended)
            {
                return false;
            }
            else
            {
                IsSuspended = true;
                return true;
            }
        }
        else if (IsCodeEmpty)
        {
            if (!other.IsCodeEmpty || !IsSuspended)
            {
                Code = other.Code;
                IsSuspended = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (IsSuspended)
            {
                return false;
            }
            else
            {
                Code = other.Code;
                IsSuspended = true;
                return true;
            }
        }
    }

    /// <summary>
    /// Suspends this code tag by adopting the code from a <see cref="Replacement"/> and marking itself as suspended.
    /// If the replacement's code is empty, only marks this tag as suspended without changing the code.
    /// </summary>
    /// <param name="other">The <see cref="Replacement"/> to suspend from.</param>
    /// <returns><c>true</c> if the state of this tag was changed; otherwise, <c>false</c>.</returns>
    public bool Suspend(Replacement other)
    {
        // If the other is empty or equal
        if (other.GetIsCodeEmpty())
        {
            if (IsSuspended)
            {
                return false;
            }
            else
            {
                IsSuspended = true;
                return true;
            }
        }
        else if (IsCodeEmpty)
        {
            if (!other.GetIsCodeEmpty() || !IsSuspended)
            {
                Code = other.GetCode();
                IsSuspended = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (IsSuspended)
            {
                return false;
            }
            else
            {
                Code = other.GetCode();
                IsSuspended = true;
                return true;
            }
        }
    }

    /// <summary>
    /// Creates a <see cref="SegmentTagNode"/> replacement from this code tag using the specified configuration.
    /// </summary>
    /// <param name="userTag">The <see cref="CodeSegmentConfig"/> to use for creating the replacement node.</param>
    /// <param name="withCode">If <c>true</c>, includes the code content in the created node; otherwise, creates an empty node.</param>
    /// <returns>A new <see cref="SegmentTagNode"/> representing this code tag as a replacement.</returns>
    public SegmentTagNode MakeReplacement(CodeSegmentConfig userTag, bool withCode)
    {
        var node = new SegmentTagNode(userTag, CodeSegmentConfig.UserCode, Material, RenderType, KeyString, Extension);
        if (withCode)
        {
            node.AddText(Code);
        }
        return node;
    }

    /// <summary>
    /// Converts this code tag to a <see cref="UserCodeInfo"/> object containing all tag metadata and code content.
    /// </summary>
    /// <returns>A new <see cref="UserCodeInfo"/> instance populated with this tag's data.</returns>
    public UserCodeInfo ToUserCodeInfo()
    {
        return new UserCodeInfo
        {
            FileName = FileName,
            Location = Location,
            Material = Material,
            RenderType = RenderType,
            KeyString = KeyString,
            Extension = Extension,
            Code = Code,
        };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetFullKey();
    }
}
