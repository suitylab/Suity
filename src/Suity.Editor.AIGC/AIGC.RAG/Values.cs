using System.Collections.Generic;

namespace Suity.Editor.AIGC.RAG;

/// <summary>
/// Represents a single knowledge item returned from RAG queries
/// </summary>
public class RAGItem
{
    /// <summary>
    /// Gets the unique identifier of the knowledge item
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name of the knowledge item
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a brief overview or summary of the knowledge item
    /// </summary>
    public string Overview { get; }

    /// <summary>
    /// Gets the underlying target object associated with this knowledge item
    /// </summary>
    public object TargetObject { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RAGItem"/> class
    /// </summary>
    public RAGItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RAGItem"/> class with specified values
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="name">Display name</param>
    /// <param name="overview">Brief overview or summary</param>
    /// <param name="target">Underlying target object</param>
    public RAGItem(string id, string name, string overview, object target)
    {
        Id = id;
        Name = name;
        Overview = overview;
        TargetObject = target;
    }

    /// <summary>
    /// Returns a string representation of this item, preferring Name over Id
    /// </summary>
    /// <returns>The name, id, or base string representation</returns>
    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            return Name;
        }

        if (!string.IsNullOrWhiteSpace(Id))
        {
            return Id;
        }

        return base.ToString();
    }
}

/// <summary>
/// Represents a typed knowledge item returned from RAG queries
/// </summary>
/// <typeparam name="T">The type of the target object</typeparam>
public class RAGItem<T> : RAGItem
    where T : class
{
    /// <summary>
    /// Gets the strongly-typed target object associated with this knowledge item
    /// </summary>
    public T Target { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RAGItem{T}"/> class
    /// </summary>
    public RAGItem()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RAGItem{T}"/> class with specified values
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="name">Display name</param>
    /// <param name="overview">Brief overview or summary</param>
    /// <param name="target">Strongly-typed target object</param>
    public RAGItem(string id, string name, string overview, T target)
        : base(id, name, overview, target)
    {
        Target = target;
    }
}

/// <summary>
/// Represents a result from a vector knowledge base query
/// </summary>
public class RagQueryResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the document
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the content of the document
    /// </summary>
    public string Content { get; set; }
}

/// <summary>
/// Feature tag query result
/// </summary>
public class FeatureQueryResult
{
    /// <summary>
    /// Tag
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the array of types associated with this feature
    /// </summary>
    public string[] Types { get; set; }

    /// <summary>
    /// Gets or sets the array of tags associated with this feature
    /// </summary>
    public string[] Tags { get; set; }

    /// <summary>
    /// All content combined for the tag
    /// </summary>
    public string CombinedContent { get; set; }

    /// <summary>
    /// Hash of all content for the tag
    /// </summary>
    public string CombinedHash { get; set; }

    /// <summary>
    /// Additional knowledge provided on demand, not provided by default
    /// </summary>
    public string Knowledge { get; set; }
}


/// <summary>
/// Represents a graph entity query result with its attributes and connections
/// </summary>
public class EntityQueryResult
{
    /// <summary>
    /// Gets or sets the name of the entity
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the key-value attributes of the entity
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; }

    /// <summary>
    /// Gets or sets the list of edges (connections) from this entity
    /// </summary>
    public List<EdgeQueryResult> Edges { get; set; } = [];
}

/// <summary>
/// Feature connection query result
/// </summary>
public class EdgeQueryResult
{
    /// <summary>
    /// Gets or sets the source entity name of the edge
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the target entity name of the edge
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets the type of the relationship
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the description of the relationship
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Returns a formatted string representation of the edge
    /// </summary>
    /// <returns>String in format "Source - Type - Target :Description"</returns>
    public override string ToString()
    {
        return $"{Source} - {Type} - {Target} :{Description}";
    }
}