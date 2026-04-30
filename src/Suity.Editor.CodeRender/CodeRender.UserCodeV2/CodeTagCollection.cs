using Suity.Editor.CodeRender.Replacing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender.UserCodeV2;

/// <summary>
/// Represents a collection of <see cref="CodeTag"/> instances associated with a specific file or location.
/// Provides methods for querying, adding, updating, and managing code tags.
/// </summary>
public class CodeTagCollection
{
    /// <summary>
    /// Gets the name of the file that this collection of code tags belongs to.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the location identifier within the file that this collection applies to.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Gets the list of <see cref="CodeTag"/> instances contained in this collection.
    /// </summary>
    public List<CodeTag> Tags { get; } = [];

    /// <summary>
    /// Initializes a new empty instance of the <see cref="CodeTagCollection"/> class.
    /// </summary>
    public CodeTagCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeTagCollection"/> class with a collection of code tags.
    /// </summary>
    /// <param name="fragments">The collection of <see cref="CodeTag"/> instances to add to this collection.</param>
    public CodeTagCollection(IEnumerable<CodeTag> fragments)
    {
        Tags.AddRange(fragments);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeTagCollection"/> class with specified file name, location, and optional tags.
    /// </summary>
    /// <param name="fileName">The name of the file this collection belongs to.</param>
    /// <param name="location">The location identifier within the file.</param>
    /// <param name="fragments">An optional collection of <see cref="CodeTag"/> instances to add.</param>
    public CodeTagCollection(string fileName, string location, IEnumerable<CodeTag> fragments = null)
    {
        FileName = fileName;
        Location = location;

        if (fragments != null)
        {
            Tags.AddRange(fragments);
        }
    }

    /// <summary>
    /// Adds a new code tag to the collection, or updates an existing tag if one with the same key already exists.
    /// Tags are considered equal if their material, render type, key string, and extension match.
    /// </summary>
    /// <param name="tag">The <see cref="CodeTag"/> to add or update. Must not be null and must have a non-empty <see cref="CodeTag.KeyString"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tag"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> has an empty <see cref="CodeTag.KeyString"/>.</exception>
    public void AddOrUpdateTag(CodeTag tag)
    {
        if (tag == null)
        {
            throw new ArgumentNullException();
        }
        if (string.IsNullOrEmpty(tag.KeyString))
        {
            throw new ArgumentException();
        }

        for (int i = 0; i < Tags.Count; i++)
        {
            var item = Tags[i];
            if (item.KeyEquals(tag))
            {
                Tags[i] = tag;
                return;
            }
        }
        Tags.Add(tag);
    }

    /// <summary>
    /// Gets a code tag from the collection that matches the specified key components.
    /// </summary>
    /// <param name="material">The material to search for.</param>
    /// <param name="type">The render type to search for.</param>
    /// <param name="name">The key string to search for.</param>
    /// <param name="ext">The extension to search for.</param>
    /// <returns>The matching <see cref="CodeTag"/>, or null if no match is found.</returns>
    public CodeTag GetTag(string material, string type, string name, string ext)
    {
        return Tags.Find(item => item.KeyEquals(material, type, name, ext));
    }

    /// <summary>
    /// Gets an existing code tag that matches the specified key, or creates and adds a new one if not found.
    /// </summary>
    /// <param name="material">The material for the tag.</param>
    /// <param name="type">The render type for the tag.</param>
    /// <param name="name">The key string for the tag.</param>
    /// <param name="ext">The extension for the tag.</param>
    /// <returns>The existing or newly created <see cref="CodeTag"/>.</returns>
    public CodeTag GetOrCreateTag(string material, string type, string name, string ext)
    {
        CodeTag tag = GetTag(material, type, name, ext);
        if (tag == null)
        {
            tag = new CodeTag { FileName = this.FileName, Location = this.Location, Material = material, RenderType = type, KeyString = name, Extension = ext, Code = string.Empty };
            Tags.Add(tag);
        }
        return tag;
    }

    /// <summary>
    /// Gets an existing code tag that matches the key of another tag, or creates and adds a new one if not found.
    /// </summary>
    /// <param name="other">The <see cref="CodeTag"/> whose key to match against.</param>
    /// <returns>The existing or newly created <see cref="CodeTag"/>.</returns>
    public CodeTag GetOrCreateTag(CodeTag other)
    {
        return GetOrCreateTag(other.Material, other.RenderType, other.KeyString, other.Extension);
    }

    /// <summary>
    /// Gets an existing code tag that matches the key of a <see cref="SegmentTagNode"/>, or creates and adds a new one if not found.
    /// </summary>
    /// <param name="r">The <see cref="SegmentTagNode"/> whose key to match against.</param>
    /// <returns>The existing or newly created <see cref="CodeTag"/>.</returns>
    public CodeTag GetOrCreateTag(SegmentTagNode r)
    {
        return GetOrCreateTag(r.Material, r.RenderType, r.ItemKey, r.Extension);
    }

    /// <summary>
    /// Removes all code tags from the collection that match the specified key components.
    /// </summary>
    /// <param name="material">The material to match.</param>
    /// <param name="type">The render type to match.</param>
    /// <param name="name">The key string to match.</param>
    /// <param name="ext">The extension to match.</param>
    /// <returns><c>true</c> if any tags were removed; otherwise, <c>false</c>.</returns>
    public bool Remove(string material, string type, string name, string ext)
    {
        return Tags.RemoveAll(o => o.KeyEquals(material, type, name, ext)) > 0;
    }

    /// <summary>
    /// Determines whether the collection contains a code tag with the specified key components.
    /// </summary>
    /// <param name="material">The material to search for.</param>
    /// <param name="type">The render type to search for.</param>
    /// <param name="name">The key string to search for.</param>
    /// <param name="ext">The extension to search for.</param>
    /// <returns><c>true</c> if a matching tag exists; otherwise, <c>false</c>.</returns>
    public bool Contains(string material, string type, string name, string ext)
    {
        return Tags.Any(item => item.KeyEquals(material, type, name, ext));
    }

    /// <summary>
    /// Determines whether all code tags in this collection match the corresponding tags in another collection by key and code content.
    /// </summary>
    /// <param name="other">The other <see cref="CodeTagCollection"/> to compare against.</param>
    /// <returns><c>true</c> if all tags match; otherwise, <c>false</c>.</returns>
    public bool Match(CodeTagCollection other)
    {
        foreach (CodeTag item in Tags)
        {
            CodeTag otherItem = other.GetTag(item.Material, item.RenderType, item.KeyString, item.Extension);
            if (otherItem == null || otherItem.Code != item.Code)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determines whether this collection contains any tags that are no longer present in the specified segment document (legacy tags).
    /// </summary>
    /// <param name="collection">The <see cref="SegmentDocument"/> to check against.</param>
    /// <returns><c>true</c> if any legacy tags are found; otherwise, <c>false</c>.</returns>
    public bool HasLagacyTags(SegmentDocument collection)
    {
        return Tags.Any(item => item != null && !collection.Contains(item.GetFullKey()));
    }

    /// <summary>
    /// Determines whether this collection contains any tags that are no longer present in the specified segment document and match a given predicate.
    /// </summary>
    /// <param name="collection">The <see cref="SegmentDocument"/> to check against.</param>
    /// <param name="predicate">A predicate function to filter which tags to check for legacy status.</param>
    /// <returns><c>true</c> if any legacy tags matching the predicate are found; otherwise, <c>false</c>.</returns>
    public bool HasLagacyTags(SegmentDocument collection, Predicate<CodeTag> predicate)
    {
        return Tags.Any(item => item != null && predicate(item) && !collection.Contains(item.GetFullKey()));
    }

    /// <summary>
    /// Determines whether the user code in the specified segment document has changed compared to the tags in this collection.
    /// </summary>
    /// <param name="segments">The <see cref="SegmentDocument"/> containing the current user code segments to compare against.</param>
    /// <returns><c>true</c> if any user code has changed; otherwise, <c>false</c>.</returns>
    public bool IsUserCodeChanged(SegmentDocument segments)
    {
        foreach (SegmentTagNode rep in segments.GetSegments(CodeSegmentConfig.UserCode))
        {
            CodeTag userCode = GetTag(rep.Material, rep.RenderType, rep.ItemKey, rep.Extension);
            if (userCode == null)
            {
                // Since empty code is not written to the database, we need to consider the empty code case when encountering empty userCode
                if (!rep.GetIsCodeEmpty())
                {
                    return true;
                }
            }
            else
            {
                if (userCode.IsCodeEmpty && rep.GetIsCodeEmpty())
                {
                    return false;
                }
                if (userCode.Code != rep.GetInnerCode())
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Renames a code tag by removing it with its old key and re-adding it with a new key, then updating its properties.
    /// </summary>
    /// <param name="tag">The <see cref="CodeTag"/> to rename. Must exist in this collection.</param>
    /// <param name="material">The new material value.</param>
    /// <param name="location">The new location value.</param>
    /// <param name="type">The new render type value.</param>
    /// <param name="name">The new key string value. Must not be null or empty.</param>
    /// <param name="ext">The new extension value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tag"/> is not contained in this collection.</exception>
    public void RenameAndUpdate(CodeTag tag, string material, string location, string type, string name, string ext)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
        if (!Tags.Contains(tag)) throw new ArgumentException();

        Remove(tag.Material, tag.RenderType, tag.KeyString, tag.Extension);

        tag.Material = material;
        tag.Location = location;
        tag.RenderType = type;
        tag.KeyString = name;
        tag.Extension = ext;

        AddOrUpdateTag(tag);
    }

    /// <summary>
    /// Replaces all tags in this collection with copies of the tags from another collection.
    /// </summary>
    /// <param name="other">The <see cref="CodeTagCollection"/> to copy tags from.</param>
    public void CopyFrom(CodeTagCollection other)
    {
        Tags.Clear();
        foreach (var item in other.Tags)
        {
            var itemNew = new CodeTag();
            itemNew.CopyFrom(item);
            AddOrUpdateTag(itemNew);
        }
    }

    /// <summary>
    /// Adds or updates tags in this collection from another collection, skipping suspended tags.
    /// Tags with empty key strings will cause an exception.
    /// </summary>
    /// <param name="other">The <see cref="CodeTagCollection"/> to add tags from.</param>
    /// <exception cref="ArgumentException">Thrown when any tag in <paramref name="other"/> has an empty <see cref="CodeTag.KeyString"/>.</exception>
    public void AddFrom(CodeTagCollection other)
    {
        foreach (var item in other.Tags)
        {
            if (string.IsNullOrEmpty(item.KeyString))
            {
                throw new ArgumentException();
            }

            CodeTag tag = GetOrCreateTag(item);
            if (!tag.IsSuspended)
            {
                tag.CopyFrom(item);
            }
        }
    }

    /// <summary>
    /// Replaces all tags in this collection with tags created from the user code segments in a <see cref="SegmentDocument"/>.
    /// </summary>
    /// <param name="collection">The <see cref="SegmentDocument"/> to extract user code segments from.</param>
    /// <param name="skipEmpty">If <c>true</c>, skips creating tags for segments with empty code content.</param>
    /// <exception cref="ArgumentException">Thrown when any segment has an empty key.</exception>
    public void CopyFrom(SegmentDocument collection, bool skipEmpty)
    {
        Tags.Clear();

        foreach (var node in collection.GetSegments(CodeSegmentConfig.UserCode))
        {
            if (string.IsNullOrEmpty(node.Key))
            {
                throw new ArgumentException();
            }

            if (!skipEmpty || !node.GetIsCodeEmpty())
            {
                var tag = new CodeTag
                {
                    FileName = this.FileName,
                    Location = this.Location,
                    Material = node.Material,
                    RenderType = node.RenderType,
                    KeyString = node.ItemKey,
                    Extension = node.Extension,
                    Code = node.GetInnerCode()
                };

                Tags.Add(tag);
            }
        }
    }

    /// <summary>
    /// Adds or updates tags in this collection from the user code segments in a <see cref="SegmentDocument"/>, skipping suspended tags.
    /// </summary>
    /// <param name="collection">The <see cref="SegmentDocument"/> to extract user code segments from.</param>
    /// <param name="skipEmpty">If <c>true</c>, skips creating tags for segments with empty code content.</param>
    /// <exception cref="ArgumentException">Thrown when any segment has an empty key.</exception>
    public void AddFrom(SegmentDocument collection, bool skipEmpty)
    {
        foreach (SegmentTagNode r in collection.GetSegments(CodeSegmentConfig.UserCode))
        {
            if (string.IsNullOrEmpty(r.Key)) throw new ArgumentException();

            if (!skipEmpty || !r.GetIsCodeEmpty())
            {
                CodeTag tag = GetOrCreateTag(r);
                if (!tag.IsSuspended)
                {
                    tag.CopyFrom(r);
                }
            }
        }
    }

    /// <summary>
    /// Validates that none of the tags in this collection are null.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when any tag in the collection is null.</exception>
    public void CheckTagsNull()
    {
        if (Tags.Any(item => item == null))
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Finds all code tags in this collection whose extension matches the specified string.
    /// </summary>
    /// <param name="find">The string to search for in the extension.</param>
    /// <param name="exactMatch">If <c>true</c>, performs an exact case-sensitive match; if <c>false</c>, performs a case-insensitive substring search.</param>
    /// <returns>An enumerable of <see cref="CodeTag"/> instances with matching extensions.</returns>
    public IEnumerable<CodeTag> FindExtension(string find, bool exactMatch)
    {
        if (string.IsNullOrEmpty(find)) yield break;

        foreach (CodeTag item in Tags)
        {
            if (item == null)
            {
                continue;
            }

            if (exactMatch)
            {
                if (item.Extension == find)
                {
                    yield return item;
                }
            }
            else
            {
                if (item.Extension?.IndexOf(find, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    yield return item;
                }
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="CodeTagCollection"/> populated with tags extracted from the user code segments in a <see cref="SegmentDocument"/>.
    /// </summary>
    /// <param name="fileName">The file name to assign to each created tag.</param>
    /// <param name="location">The location to assign to each created tag.</param>
    /// <param name="segments">The <see cref="SegmentDocument"/> containing the user code segments.</param>
    /// <param name="ignoreEmpty">If <c>true</c>, skips creating tags for segments with empty code content.</param>
    /// <returns>A new <see cref="CodeTagCollection"/> populated with tags from the segments.</returns>
    /// <exception cref="ArgumentException">Thrown when any segment has an empty key.</exception>
    public static CodeTagCollection CreateBySegments(string fileName, string location, SegmentDocument segments, bool ignoreEmpty)
    {
        var tagFile = new CodeTagCollection(fileName, location);

        foreach (var rep in segments.GetSegments(CodeSegmentConfig.UserCode))
        {
            if (string.IsNullOrEmpty(rep.Key))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrEmpty(rep.ItemKey))
            {
                continue;
            }

            if (!ignoreEmpty || !rep.GetIsCodeEmpty())
            {
                var tag = new CodeTag
                {
                    FileName = fileName,
                    Location = location,
                    Material = rep.Material,
                    RenderType = rep.RenderType,
                    KeyString = rep.ItemKey,
                    Extension = rep.Extension,
                    Code = rep.GetInnerCode()
                };

                tagFile.AddOrUpdateTag(tag);
            }
        }

        return tagFile;
    }
}
