using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suity.Editor.CodeRender.Replacing;

#region SegmentNode

/// <summary>
/// Abstract base class representing a node in a code segment tree.
/// </summary>
public abstract class SegmentNode
{
    /// <summary>
    /// Gets or sets the parent node of this node.
    /// </summary>
    public SegmentNode Parent { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this node itself has been modified.
    /// </summary>
    public bool IsDirtySelf { get; private set; }

    /// <summary>
    /// Marks this node as dirty (modified).
    /// </summary>
    public void MarkDirty()
    {
        IsDirtySelf = true;
    }

    /// <summary>
    /// Clears the dirty flag on this node. Derived classes may override to clear child nodes as well.
    /// </summary>
    public virtual void ClearDirty()
    {
        IsDirtySelf = false;
    }

    /// <summary>
    /// Gets a value indicating whether this node or any of its children are dirty.
    /// </summary>
    /// <returns><c>true</c> if this node is dirty; otherwise, <c>false</c>.</returns>
    public virtual bool GetIsDirty() => IsDirtySelf;

    /// <summary>
    /// Gets the full code representation of this node, including any wrapper tags.
    /// </summary>
    /// <returns>The full code string.</returns>
    public abstract string GetCode();

    /// <summary>
    /// Gets the inner code representation of this node, excluding wrapper tags.
    /// </summary>
    /// <returns>The inner code string.</returns>
    public abstract string GetInnerCode();

    /// <summary>
    /// Creates a clone of this node with an optional extension prefix applied.
    /// </summary>
    /// <param name="extPrefix">Optional extension prefix to append to the node's extension.</param>
    /// <returns>A cloned <see cref="SegmentNode"/>.</returns>
    public abstract SegmentNode Clone(string extPrefix = null);

    /// <summary>
    /// Gets a value indicating whether the code content of this node is empty or whitespace.
    /// </summary>
    /// <returns><c>true</c> if the code is empty; otherwise, <c>false</c>.</returns>
    public virtual bool GetIsCodeEmpty() => true;

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetCode();
    }
}

#endregion

#region SegmentTextNode

/// <summary>
/// Represents a text node containing raw code content.
/// </summary>
public class SegmentTextNode : SegmentNode
{
    private string _text;

    /// <summary>
    /// Gets or sets the text content of this node.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            MarkDirty();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentTextNode"/> class with empty text.
    /// </summary>
    public SegmentTextNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentTextNode"/> class with the specified text.
    /// </summary>
    /// <param name="text">The text content for this node.</param>
    public SegmentTextNode(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <inheritdoc/>
    public override string GetCode() => Text;

    /// <inheritdoc/>
    public override string GetInnerCode() => Text;

    /// <inheritdoc/>
    public override bool GetIsCodeEmpty() => string.IsNullOrWhiteSpace(Text);

    /// <inheritdoc/>
    public override SegmentNode Clone(string extPrefix = null)
    {
        return new SegmentTextNode(Text);
    }
}

#endregion

#region SegmentElementNode

/// <summary>
/// Abstract base class for element nodes that can contain child nodes.
/// </summary>
public abstract class SegmentElementNode : SegmentNode
{
    private readonly List<SegmentNode> _childNodes = [];

    /// <summary>
    /// Gets the collection of child nodes.
    /// </summary>
    public IEnumerable<SegmentNode> ChildNodes => _childNodes;

    /// <summary>
    /// Gets the number of child nodes.
    /// </summary>
    public int Count => _childNodes.Count;

    /// <summary>
    /// Adds a child node to the end of this element.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is this node or already has a parent.</exception>
    public void Add(SegmentNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        if (node == this)
        {
            throw new ArgumentException(nameof(node));
        }
        if (node.Parent != null)
        {
            throw new ArgumentException(nameof(node));
        }

        _childNodes.Add(node);
        node.Parent = this;
        MarkDirty();
    }

    /// <summary>
    /// Inserts a child node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the node.</param>
    /// <param name="node">The node to insert.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is this node or already has a parent.</exception>
    public void Insert(int index, SegmentNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        if (node == this)
        {
            throw new ArgumentException(nameof(node));
        }
        if (node.Parent != null)
        {
            throw new ArgumentException(nameof(node));
        }

        _childNodes.Insert(index, node);
        node.Parent = this;
        MarkDirty();
    }

    /// <summary>
    /// Adds a text node with the specified text content.
    /// </summary>
    /// <param name="text">The text content to add. If null or empty, no node is added.</param>
    public void AddText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        Add(new SegmentTextNode(text));
    }

    /// <summary>
    /// Adds a range of child nodes to the end of this element.
    /// </summary>
    /// <param name="nodes">The collection of nodes to add.</param>
    public void AddRange(IEnumerable<SegmentNode> nodes)
    {
        foreach (var node in nodes)
        {
            Add(node);
        }
    }

    /// <summary>
    /// Removes the specified child node from this element.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <returns><c>true</c> if the node was successfully removed; otherwise, <c>false</c>.</returns>
    public bool Remove(SegmentNode node)
    {
        bool removed = _childNodes.Remove(node);
        MarkDirty();
        return removed;
    }

    /// <summary>
    /// Removes all child nodes from this element.
    /// </summary>
    public void Clear()
    {
        _childNodes.Clear();
        MarkDirty();
    }

    /// <summary>
    /// Gets the child node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the child node to get.</param>
    /// <returns>The child node at the specified index.</returns>
    public SegmentNode this[int index] => _childNodes[index];

    /// <inheritdoc/>
    public override string GetCode()
    {
        StringBuilder builder = new();
        foreach (var node in _childNodes)
        {
            builder.Append(node.GetCode());
        }

        return builder.ToString();
    }

    /// <inheritdoc/>
    public override string GetInnerCode()
    {
        StringBuilder builder = new();
        foreach (var node in _childNodes)
        {
            builder.Append(node.GetInnerCode());
        }

        return builder.ToString();
    }

    /// <inheritdoc/>
    public override bool GetIsCodeEmpty() => _childNodes.Count == 0 || _childNodes.All(o => o.GetIsCodeEmpty());

    /// <inheritdoc/>
    public override void ClearDirty()
    {
        base.ClearDirty();

        foreach (var node in _childNodes)
        {
            node.ClearDirty();
        }
    }

    /// <inheritdoc/>
    public override bool GetIsDirty()
    {
        if (IsDirtySelf)
        {
            return true;
        }

        return _childNodes.Any(o => o.GetIsDirty());
    }
}

#endregion

#region SegmentTagNode

/// <summary>
/// Represents a tag node that wraps code segments with begin/end markers.
/// Implements <see cref="IRenderSegment"/> for rendering operations.
/// </summary>
public class SegmentTagNode : SegmentElementNode, IRenderSegment
{
    /// <summary>
    /// Gets the configuration used for this tag's segment markers.
    /// </summary>
    public CodeSegmentConfig Config { get; }

    /// <summary>
    /// Gets the full key string for this tag.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentTagNode"/> class with the specified configuration.
    /// </summary>
    /// <param name="config">The segment configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public SegmentTagNode(CodeSegmentConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentTagNode"/> class by parsing a key string.
    /// </summary>
    /// <param name="config">The segment configuration.</param>
    /// <param name="key">The key string to parse, split by <see cref="CodeSegmentConfig.KeySplitter"/>.</param>
    /// <param name="defaultKeyString">Default item key used when the parsed key has fewer parts.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public SegmentTagNode(CodeSegmentConfig config, string key, string defaultKeyString)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        Key = key;

        string[] infoStrSplit = key.Split(CodeSegmentConfig.KeySplitter);
        TagType = infoStrSplit.Length > 0 ? infoStrSplit[0] : string.Empty;

        if (infoStrSplit.Length == 5)
        {
            // Material : RenderType : Model : Extension
            Material = infoStrSplit[1];
            RenderType = infoStrSplit[2];
            ItemKey = infoStrSplit[3];
            Extension = infoStrSplit[4];
        }
        else if (infoStrSplit.Length == 4)
        {
            // RenderType : Model : Extension
            Material = infoStrSplit[1];
            RenderType = string.Empty;
            ItemKey = infoStrSplit[2];
            Extension = infoStrSplit[3];
        }
        else if (infoStrSplit.Length == 3)
        {
            // Model : Extension
            Material = string.Empty;
            RenderType = string.Empty;
            ItemKey = infoStrSplit[1];
            Extension = infoStrSplit[2];
        }
        else if (infoStrSplit.Length == 2)
        {
            // DefaultModel : Extension
            Material = string.Empty;
            RenderType = string.Empty;
            ItemKey = defaultKeyString;
            Extension = infoStrSplit[1];
        }
        else if (infoStrSplit.Length == 1)
        {
            // DefaultModel
            Material = string.Empty;
            RenderType = string.Empty;
            ItemKey = defaultKeyString;
            Extension = string.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentTagNode"/> class with explicit parameters.
    /// </summary>
    /// <param name="config">The segment configuration.</param>
    /// <param name="tagType">Main tag type (e.g., GenCode, UserCode, NameSpace).</param>
    /// <param name="material">Material identifier.</param>
    /// <param name="renderType">Render type identifier.</param>
    /// <param name="itemKey">Item key identifier.</param>
    /// <param name="extension">File extension.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public SegmentTagNode(CodeSegmentConfig config, string tagType, string material, string renderType, string itemKey, string extension)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        TagType = tagType;
        Material = material;
        RenderType = renderType;
        ItemKey = itemKey;
        Extension = extension;

        Key = config.GetKeyAuto(TagType, Material, RenderType, ItemKey, Extension);
    }

    /// <summary>
    /// Gets the main tag type, usually indicating the segment category (e.g., UserCode, GenCode).
    /// </summary>
    public string TagType { get; private set; }

    /// <summary>
    /// Gets the material identifier, which is the first parameter in the key.
    /// </summary>
    public string Material { get; private set; }

    /// <summary>
    /// Gets the render type identifier, which is the second parameter in the key.
    /// </summary>
    public string RenderType { get; private set; }

    /// <summary>
    /// Gets the item key identifier, which is the third parameter in the key.
    /// </summary>
    public string ItemKey { get; private set; }

    /// <summary>
    /// Gets the file extension, which is the fourth parameter in the key.
    /// </summary>
    public string Extension { get; private set; }

    /// <inheritdoc/>
    public override string GetCode()
    {
        return $"{Config.PrefixBegin}{Key}{Config.Suffix}{base.GetCode()}{Config.PrefixEnd}{Key}{Config.Suffix}";
    }

    /// <inheritdoc/>
    public override string GetInnerCode() => base.GetCode();

    /// <inheritdoc/>
    public override SegmentNode Clone(string extPrefix = null)
    {
        string key;
        string ext;

        if (string.IsNullOrEmpty(extPrefix))
        {
            ext = Extension;
            key = Key;
        }
        else
        {
            ext = $"{extPrefix}.{Extension}";
            key = Config.GetKeyAuto(TagType, Material, RenderType, ItemKey, ext);
        }

        var node = new SegmentTagNode(Config)
        {
            Key = key,
            TagType = this.TagType,
            Material = this.Material,
            RenderType = this.RenderType,
            ItemKey = this.ItemKey,
            Extension = ext,
        };

        node.AddRange(this.ChildNodes.Select(o => o.Clone(extPrefix)));

        return node;
    }

    /// <inheritdoc/>
    void IRenderSegment.AddCode(IRenderSegment other)
    {
        if (other is SegmentTagNode node)
        {
            AddRange(node.ChildNodes.Select(o => o.Clone(Extension)));
        }
    }

    /// <summary>
    /// Replaces the key components (Material, RenderType, ItemKey) using the provided resolver function
    /// and updates the full key if any component changed.
    /// </summary>
    /// <param name="keyResolve">A function that resolves a key component string to a new value.</param>
    internal void ReplaceKey(Func<string, string> keyResolve)
    {
        Material = keyResolve(Material) ?? Material;
        RenderType = keyResolve(RenderType) ?? RenderType;
        ItemKey = keyResolve(ItemKey) ?? ItemKey;

        string key = Config.GetKeyAuto(TagType, Material, RenderType, ItemKey, Extension);
        if (key != Key)
        {
            Key = key;
            MarkDirty();
        }
    }
}

#endregion

#region SegmentRootNode

/// <summary>
/// Represents the root node of a code segment tree.
/// </summary>
public class SegmentRootNode : SegmentElementNode
{
    /// <inheritdoc/>
    public override SegmentNode Clone(string extPrefix = null)
    {
        var node = new SegmentRootNode();

        node.AddRange(this.ChildNodes.Select(o => o.Clone(extPrefix)));

        return node;
    }

    /// <summary>
    /// Gets all nodes of the specified type in this tree, including the root if it matches.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <returns>An enumerable of all matching nodes in the tree.</returns>
    public IEnumerable<T> GetAllNodes<T>() where T : SegmentNode
    {
        if (this is T t)
        {
            yield return t;
        }

        foreach (var childNode in GetAllNodes<T>(this))
        {
            yield return childNode;
        }
    }

    /// <summary>
    /// Recursively retrieves all nodes of the specified type within the given element node.
    /// </summary>
    /// <typeparam name="T">The type of nodes to retrieve.</typeparam>
    /// <param name="node">The element node to search within.</param>
    /// <returns>An enumerable of all matching nodes.</returns>
    private IEnumerable<T> GetAllNodes<T>(SegmentElementNode node) where T : SegmentNode
    {
        foreach (var childNode in node.ChildNodes)
        {
            if (childNode is T t)
            {
                yield return t;
            }

            if (childNode is SegmentElementNode ele)
            {
                foreach (var c2 in GetAllNodes<T>(ele))
                {
                    yield return c2;
                }
            }
        }
    }
}

#endregion
