using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender.Replacing;

/// <summary>
/// Manages a parsed code document with replaceable segment tags.
/// </summary>
public class SegmentDocument
{
    private readonly CodeSegmentConfig _segConfig;

    // Full key dictionary
    private readonly Dictionary<string, SegmentTagNode> _nodes = [];

    // Primary key dictionary
    private readonly UniqueMultiDictionary<string, SegmentTagNode> _nodesByExt = new();

    private SegmentRootNode _rootNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentDocument"/> class.
    /// </summary>
    /// <param name="segmentConfig">The segment configuration to use for parsing and generating.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="segmentConfig"/> is null.</exception>
    public SegmentDocument(CodeSegmentConfig segmentConfig)
    {
        _segConfig = segmentConfig ?? throw new ArgumentNullException(nameof(segmentConfig));
    }

    /// <summary>
    /// Parses the specified code string and builds the segment tree.
    /// </summary>
    /// <param name="code">The source code string to parse.</param>
    /// <param name="defaultKeyString">Default key string used for tags with incomplete key parts.</param>
    public void Parse(string code, string defaultKeyString)
    {
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        OriginCode = code;

        _nodes.Clear();
        _nodesByExt.Clear();
        _rootNode = null;

        SegmentParser parser = SegmentParser.GetParser(_segConfig);
        _rootNode = parser.Parse(code, defaultKeyString);
        if (_rootNode is null)
        {
            return;
        }

        _rootNode.ClearDirty();

        RebuildKeys();
    }

    /// <summary>
    /// Rebuilds the internal key dictionaries from the current segment tree.
    /// </summary>
    public void RebuildKeys()
    {
        _nodes.Clear();
        _nodesByExt.Clear();

        foreach (var node in _rootNode.GetAllNodes<SegmentTagNode>())
        {
            _nodes[node.Key] = node;
            string extKey = $"{node.TagType}:{node.ItemKey}:{node.Extension}";
            _nodesByExt.Add(extKey, node);
        }
    }

    /// <summary>
    /// Gets the segment configuration used by this document.
    /// </summary>
    public CodeSegmentConfig SegmentConfig => _segConfig;

    /// <summary>
    /// Source code without replacement executed
    /// </summary>
    public string OriginCode { get; private set; }

    /// <summary>
    /// Gets a value indicating whether any segment in the document has been modified.
    /// </summary>
    /// <returns><c>true</c> if the document is dirty; otherwise, <c>false</c>.</returns>
    public bool GetIsDirty() => _rootNode?.GetIsDirty() ?? false;

    /// <summary>
    /// Generate final code
    /// </summary>
    /// <returns>The generated code string, including all segment tags.</returns>
    public string GenerateCode() => GetIsDirty() ? _rootNode?.GetCode() ?? OriginCode : OriginCode;

    /// <summary>
    /// Generate code without tags
    /// </summary>
    /// <returns>The inner code string, excluding segment tag wrappers.</returns>
    public string GenerateInnerCode() => _rootNode?.GetInnerCode() ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the generated code is identical to the original source code.
    /// </summary>
    /// <returns><c>true</c> if the source is unchanged; otherwise, <c>false</c>.</returns>
    public bool GetIsSourceSame()
    {
        if (_rootNode is null)
        {
            return false;
        }

        return string.Equals(OriginCode, _rootNode.GetCode());
    }

    /// <summary>
    /// Gets a value indicating whether all segments in the document are empty.
    /// </summary>
    /// <returns><c>true</c> if all segments are empty; otherwise, <c>false</c>.</returns>
    public bool GetIsAllEmpty() => _nodes.Values.All(o => o.GetIsCodeEmpty());

    /// <summary>
    /// Checks whether the document contains a segment with the specified full key.
    /// </summary>
    /// <param name="key">The full segment key to look for.</param>
    /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
    public bool Contains(string key) => _nodes.ContainsKey(key);

    /// <summary>
    /// Gets all segment tag nodes in the document.
    /// </summary>
    public IEnumerable<SegmentTagNode> Segments => _nodes.Values;

    /// <summary>
    /// Gets all segment tag nodes that match the specified tag type.
    /// </summary>
    /// <param name="tagType">The tag type to filter by (e.g., UserCode, GenCode).</param>
    /// <returns>An enumerable of matching <see cref="SegmentTagNode"/> instances.</returns>
    public IEnumerable<SegmentTagNode> GetSegments(string tagType)
    {
        if (_rootNode is null)
        {
            return [];
        }

        return _rootNode.GetAllNodes<SegmentTagNode>().Where(o => o.TagType == tagType); // _nodes.Values.Where(o => o.TagType == tagType);
    }

    /// <summary>
    /// Gets all segment tag nodes that match the specified tag type, item key, and extension.
    /// </summary>
    /// <param name="tagType">The tag type to filter by.</param>
    /// <param name="keyString">The item key to filter by.</param>
    /// <param name="extension">The extension to filter by.</param>
    /// <returns>An enumerable of matching <see cref="SegmentTagNode"/> instances.</returns>
    public IEnumerable<SegmentTagNode> GetSegments(string tagType, string keyString, string extension) 
        => _nodesByExt[$"{tagType}:{keyString}:{extension}"];

    /// <summary>
    /// Gets a single segment tag node by its full key.
    /// </summary>
    /// <param name="key">The full segment key.</param>
    /// <returns>The <see cref="SegmentTagNode"/> if found; otherwise, null.</returns>
    public SegmentTagNode GetSegment(string key)
    {
        return _nodes.GetValueSafe(key);
    }

    /// <summary>
    /// Compares this document's segments with another document to check if they match.
    /// </summary>
    /// <param name="other">The other document to compare against.</param>
    /// <returns><c>true</c> if all segments match in both key and code content; otherwise, <c>false</c>.</returns>
    public bool Match(SegmentDocument other)
    {
        foreach (var item in _nodes.Values)
        {
            if (!other._nodes.TryGetValue(item.Key, out SegmentTagNode itemOther))
            {
                return false;
            }
            if (!string.Equals(item.GetCode(), itemOther.GetCode()))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Execute replacement
    /// </summary>
    /// <param name="other">Document to replace in</param>
    /// <param name="tagType">The tag type to filter replacements by, or null to replace all types.</param>
    /// <param name="option">The replacement option controlling behavior.</param>
    public void Replace(SegmentDocument other, string tagType, ReplaceOption option)
    {
        if (option == ReplaceOption.None)
        {
            return;
        }

        if (other._nodes.Count == 0)
        {
            return;
        }

        foreach (var item in _nodes.Values)
        {
            if (!other._nodes.TryGetValue(item.Key, out SegmentTagNode itemOther))
            {
                continue;
            }
            if (tagType != null && item.TagType != tagType)
            {
                continue;
            }
            if (option == ReplaceOption.SkipEmpty && itemOther.GetIsCodeEmpty())
            {
                continue;
            }

            item.Clear();
            item.AddRange(itemOther.ChildNodes.Select(o => o.Clone(item.Extension)));
        }
    }

    /// <summary>
    /// Execute single replacement
    /// </summary>
    /// <param name="other">Item to be replaced</param>
    /// <returns><c>true</c> if the replacement was performed; otherwise, <c>false</c>.</returns>
    public bool Replace(SegmentTagNode other)
    {
        if (other is null)
        {
            return false;
        }

        if (!_nodes.TryGetValue(other.Key, out SegmentTagNode item))
        {
            return false;
        }

        item.Clear();
        item.AddRange(other.ChildNodes.Select(o => o.Clone(item.Extension)));

        return true;
    }

    /// <summary>
    /// Execute content replacement
    /// </summary>
    /// <param name="key">The full segment key to replace.</param>
    /// <param name="code">The new code content to set.</param>
    /// <returns><c>true</c> if the replacement was performed; otherwise, <c>false</c>.</returns>
    public bool Replace(string key, string code)
    {
        if (!_nodes.TryGetValue(key, out SegmentTagNode item))
        {
            return false;
        }

        item.Clear();
        item.Add(new SegmentTextNode(code));

        return true;
    }

    /// <summary>
    /// Execute single insertion
    /// </summary>
    /// <param name="other">Item to be inserted</param>
    /// <returns><c>true</c> if the insertion was performed; otherwise, <c>false</c>.</returns>
    public bool MatchInsert(SegmentTagNode other)
    {
        if (other is null)
        {
            return false;
        }

        if (!_nodes.TryGetValue(other.Key, out SegmentTagNode item))
        {
            return false;
        }

        item.AddRange(other.ChildNodes.Select(o => o.Clone(item.Extension)));

        return true;
    }

    /// <summary>
    /// Execute content insertion
    /// </summary>
    /// <param name="key">The full segment key to insert into.</param>
    /// <param name="code">The code content to insert.</param>
    /// <returns><c>true</c> if the insertion was performed; otherwise, <c>false</c>.</returns>
    public bool MatchInsert(string key, string code)
    {
        if (!_nodes.TryGetValue(key, out SegmentTagNode item))
        {
            return false;
        }

        item.Add(new SegmentTextNode(code));

        return true;
    }

    /// <summary>
    /// Adds a new segment tag node to the root of the document and rebuilds the key dictionaries.
    /// </summary>
    /// <param name="node">The segment tag node to add.</param>
    public void Add(SegmentTagNode node)
    {
        _rootNode.Add(node);

        RebuildKeys();
    }

    /// <summary>
    /// Clear all unchanged content
    /// </summary>
    /// <param name="tagType">The tag type to filter by, or null to clear all types.</param>
    public void SetEmptyUnmarked(string tagType)
    {
        foreach (var item in _nodes.Values)
        {
            if (item.IsDirtySelf)
            {
                continue;
            }
            if (tagType != null && item.TagType != tagType)
            {
                continue;
            }

            item.Clear();
        }
    }

    /// <summary>
    /// Replaces keys in all segment tag nodes using the provided resolver function and rebuilds the key dictionaries.
    /// </summary>
    /// <param name="keyResolve">A function that resolves a key component string to a new value.</param>
    public void ReplaceKeys(Func<string, string> keyResolve)
    {
        foreach (var node in _rootNode.GetAllNodes<SegmentTagNode>())
        {
            node.ReplaceKey(keyResolve);
        }

        RebuildKeys();
    }
}