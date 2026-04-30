using System;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Code render object.
/// </summary>
public sealed class RenderItem
{
    private readonly IAttributeGetter _attributes;

    /// <summary>
    /// The id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The renderable object.
    /// </summary>
    public IRenderable Renderable { get; }

    /// <summary>
    /// The render type.
    /// </summary>
    public RenderType RenderType { get; }

    /// <summary>
    /// The name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The underlying object.
    /// </summary>
    public object Object { get; }

    /// <summary>
    /// Local non-UTC time.
    /// </summary>
    public DateTime LastUpdateTime { get; }

    /// <summary>
    /// Attributes.
    /// </summary>
    public IAttributeGetter Attributes => _attributes;

    /// <summary>
    /// Creates a new render item.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="renderable">The renderable object.</param>
    /// <param name="renderType">The render type.</param>
    /// <param name="name">The name.</param>
    /// <param name="obj">The object.</param>
    /// <param name="updateTime">The last update time.</param>
    /// <param name="attributes">The attributes.</param>
    public RenderItem(Guid id, IRenderable renderable, RenderType renderType, string name, object obj, DateTime updateTime, IAttributeGetter attributes = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Id = id;
        Renderable = renderable;
        RenderType = renderType ?? throw new ArgumentNullException(nameof(renderType));
        Name = name;
        Object = obj;
        LastUpdateTime = updateTime;
        _attributes = attributes ?? EmptyAttributeGetter.Empty;
    }

    /// <inheritdoc/>
    public override string ToString() => $"({Object} as {RenderType})";
}