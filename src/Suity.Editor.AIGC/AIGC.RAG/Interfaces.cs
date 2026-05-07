using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.RAG;


/// <summary>
/// Enumeration of knowledge base types
/// </summary>
public enum KnowledgeTypes
{
    /// <summary>
    /// Vector-based knowledge base using embeddings
    /// </summary>
    Vector,

    /// <summary>
    /// Feature-based knowledge base for structured features
    /// </summary>
    Feature,

    /// <summary>
    /// Graph-based knowledge base using entity relationships
    /// </summary>
    Graph,
}

/// <summary>
/// Base interface for all knowledge base implementations
/// </summary>
[NativeType(Name = "KnowledgeBase", Description = "Knowledge Base", CodeBase = "*AIGC", Icon = "*CoreIcon|Knowledge", Color = "#9900FF")]
public interface IKnowledgeBase
{
    /// <summary>
    /// Gets the database information for this knowledge base
    /// </summary>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of database info strings</returns>
    Task<string[]> GetDBInfos(CancellationToken cancel = default);

    /// <summary>
    /// Clears knowledge data of the specified type
    /// </summary>
    /// <param name="type">The type of knowledge to clear</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ClearKnowledge(KnowledgeTypes type, CancellationToken cancel = default);
}

/// <summary>
/// Interface for vector-based knowledge base operations
/// </summary>
[NativeType(Name = "VectorKnowledge", Description = "Vector Knowledge Base", CodeBase = "*AIGC", Icon = "*CoreIcon|Knowledge", Color = "#9900FF")]
public interface IVectorKnowledge : IKnowledgeBase
{
    /// <summary>
    /// Get all vector documents
    /// </summary>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of all vector documents</returns>
    Task<RagQueryResult[]> GetAllVectorDocuments(CancellationToken cancel = default);

    /// <summary>
    /// Get vector documents
    /// </summary>
    /// <param name="query">Query words filtered from user prompt</param>
    /// <param name="topk">Number of documents to retrieve</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of matching vector documents</returns>
    Task<RagQueryResult[]> QueryVectorDocuments(string query, int? topk = null, CancellationToken cancel = default);

    /// <summary>
    /// Get vector documents within provided Id range
    /// </summary>
    /// <param name="ids">Collection of document Ids to search within</param>
    /// <param name="query">Query words filtered from user prompt</param>
    /// <param name="topk">Number of documents to retrieve</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of matching vector documents</returns>
    Task<RagQueryResult[]> QueryVectorDocuments(IEnumerable<string> ids, string query, int? topk = null, CancellationToken cancel = default);
}

/// <summary>
/// Interface for feature-based knowledge base operations
/// </summary>
[NativeType(Name = "FeatureKnowledge", Description = "Feature Knowledge Base", CodeBase = "*AIGC", Icon = "*CoreIcon|Knowledge", Color = "#9900FF")]
public interface IFeatureKnowledge : IKnowledgeBase
{
    /// <summary>
    /// Gets the underlying vector knowledge base used by this feature knowledge base
    /// </summary>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>The base vector knowledge instance, or null if not available</returns>
    IVectorKnowledge GetBaseVectorRAG(CancellationToken cancel = default);

    /// <summary>
    /// Gets a single feature by its name
    /// </summary>
    /// <param name="name">The feature name</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>The feature query result, or null if not found</returns>
    Task<FeatureQueryResult> GetFeature(string name, CancellationToken cancel = default);

    /// <summary>
    /// Enumerate all features related to the query word
    /// </summary>
    /// <param name="query">Query words filtered from user prompt</param>
    /// <param name="topk">Number of features to retrieve, including topK feature names and topK type names</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of matching feature query results</returns>
    Task<FeatureQueryResult[]> EnumerateFeature(string query, int? topk = null, CancellationToken cancel = default);

    /// <summary>
    /// Gets all features associated with the specified tag
    /// </summary>
    /// <param name="tag">The tag to search by</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of feature query results matching the tag</returns>
    Task<FeatureQueryResult[]> GetFeatureByTag(string tag, CancellationToken cancel = default);

    /// <summary>
    /// Get source vector knowledge base knowledge Id for the specified tag
    /// </summary>
    /// <param name="name">The feature name</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of source reference Ids</returns>
    Task<string[]> GetFeatureSourceRefIds(string name, CancellationToken cancel = default);


    /// <summary>
    /// Gets all entity types available in the knowledge base
    /// </summary>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of entity type names</returns>
    Task<string[]> GetAllEntityTypes(CancellationToken cancel = default);

    /// <summary>
    /// Gets all entity names of the specified type
    /// </summary>
    /// <param name="type">The entity type to filter by</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>Array of entity names</returns>
    Task<string[]> GetEntitiesByType(string type, CancellationToken cancel = default);

    /// <summary>
    /// Gets the count of entities for the specified type
    /// </summary>
    /// <param name="type">The entity type to count</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>The number of entities of the specified type</returns>
    Task<int> GetEntityCountByType(string type, CancellationToken cancel = default);

    /// <summary>
    /// Gets a single entity by its name with full details including attributes and edges
    /// </summary>
    /// <param name="name">The entity name</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns>The entity query result, or null if not found</returns>
    Task<EntityQueryResult> GetEntity(string name, CancellationToken cancel = default);
}


#region Legacy Keyword

/// <summary>
/// Legacy interface for keyword-based knowledge base operations (obsolete)
/// </summary>
[NativeType(Name = "KeywordKnowledge", Description = "Keyword Knowledge Base", CodeBase = "*AIGC", Icon = "*CoreIcon|Knowledge", Color = "#9900FF")]
[Obsolete]
public interface IKeywordKnowledge : IKnowledgeBase
{
    /// <summary>
    /// Queries concepts matching the given input
    /// </summary>
    /// <param name="any">The input string to match</param>
    /// <returns>Array of matching concept strings</returns>
    string[] QueryConcept(string any);

    /// <summary>
    /// Queries keywords for the specified feature
    /// </summary>
    /// <param name="feature">The feature name</param>
    /// <returns>Array of keyword strings</returns>
    string[] QueryKeyword(string feature);

    /// <summary>
    /// Queries synonyms for the specified feature
    /// </summary>
    /// <param name="feature">The feature name</param>
    /// <returns>Array of synonym strings</returns>
    string[] QuerySynonyms(string feature);
}

/// <summary>
/// Legacy interface for keyword knowledge operations (obsolete)
/// </summary>
[Obsolete]
public interface IKeywordKnowkegeOp
{
    /// <summary>
    /// Gets the total number of concepts stored
    /// </summary>
    public int ConceptCount { get; }

    /// <summary>
    /// Gets the total number of features stored
    /// </summary>
    public int FeatureCount { get; }

    /// <summary>
    /// Gets the total number of synonyms stored
    /// </summary>
    public int SynonymCount { get; }

    /// <summary>
    /// Gets the total number of writing segments stored
    /// </summary>
    public int WritingSegmentCount { get; }

    /// <summary>
    /// Gets the total number of writing tasks stored
    /// </summary>
    public int WritingTaskCount { get; }

    /// <summary>
    /// Gets all feature names in the knowledge base
    /// </summary>
    /// <returns>Enumerable of feature name strings</returns>
    public IEnumerable<string> GetAllFeatures();

    /// <summary>
    /// Adds a set of features under the specified concept
    /// </summary>
    /// <param name="concept">The concept name</param>
    /// <param name="features">Collection of feature strings to add</param>
    void AddFeature(string concept, IEnumerable<string> features);

    /// <summary>
    /// Adds a new concept with associated keywords and features
    /// </summary>
    /// <param name="concept">The concept name</param>
    /// <param name="keywords">Collection of keyword strings</param>
    /// <param name="features">Collection of feature strings</param>
    void AddConcept(string concept, IEnumerable<string> keywords, IEnumerable<string> features);

    /// <summary>
    /// Adds synonyms for the specified feature
    /// </summary>
    /// <param name="feature">The feature name</param>
    /// <param name="synonyms">Collection of synonym strings to add</param>
    void AddSynonyms(string feature, IEnumerable<string> synonyms);

    /// <summary>
    /// Creates a new writing task from the given paragraphs
    /// </summary>
    /// <param name="paragraphs">Collection of paragraph strings</param>
    /// <returns>The task Id string</returns>
    string CreateWritingTask(IEnumerable<string> paragraphs);

    /// <summary>
    /// Gets all writing task Ids
    /// </summary>
    /// <returns>Array of task Id strings</returns>
    string[] QueryWritingTaskIds();

    /// <summary>
    /// Queries the writing segment for the specified task
    /// </summary>
    /// <param name="taskId">The task Id</param>
    /// <returns>The writing segment instance</returns>
    IKeywordKnowkegeWritingSegment QueryWritingSegment(string taskId);

    /// <summary>
    /// Moves to the next writing segment for the specified task
    /// </summary>
    /// <param name="taskId">The task Id</param>
    /// <returns>The next writing segment, or null if at the end</returns>
    IKeywordKnowkegeWritingSegment MoveToNextWritingSegment(string taskId);

    /// <summary>
    /// Deletes the specified writing task
    /// </summary>
    /// <param name="taskId">The task Id to delete</param>
    void DeleteWritingTask(string taskId);
}

/// <summary>
/// Legacy interface representing a single writing segment (obsolete)
/// </summary>
[Obsolete]
public interface IKeywordKnowkegeWritingSegment
{
    /// <summary>
    /// Gets the total number of segments in this writing task
    /// </summary>
    public int SegmentCount { get; }

    /// <summary>
    /// Gets the current segment index
    /// </summary>
    public int CurrentIndex { get; }

    /// <summary>
    /// Gets the text content of the current segment
    /// </summary>
    public string Text { get; }
}

#endregion


