using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Documents;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.RAG;

/// <summary>
/// Vector knowledge base service
/// </summary>
public abstract class RAGService
{
    /// <summary>
    /// File extension for vector RAG files
    /// </summary>
    public const string VectorRagExtension = ".svrag";

    /// <summary>
    /// File extension for graph RAG files
    /// </summary>
    public const string GraphRagExtension = ".sgrag";

    /// <summary>
    /// Gets or sets the singleton instance of the RAG service
    /// </summary>
    public static RAGService Instance { get; internal set; }

    #region Embedding

    /// <summary>
    /// Gets the embedding model, throws if not available
    /// </summary>
    /// <returns>The embedding model instance</returns>
    public abstract IEmbeddingModel GetEmbedding();

    /// <summary>
    /// Tries to get the embedding model without throwing
    /// </summary>
    /// <returns>The embedding model instance, or null if not available</returns>
    public abstract IEmbeddingModel TryGetEmbedding();

    #endregion

    #region KnowledgeBase

    /// <summary>
    /// Tries to get the knowledge base for the specified document
    /// </summary>
    /// <param name="document">The document to get knowledge base for</param>
    /// <returns>The knowledge base instance, or null if not available</returns>
    public abstract IKnowledgeBase TryGetKnowledgeBase(Document document);

    /// <summary>
    /// Ensures the knowledge base exists for the specified document, creating it if necessary
    /// </summary>
    /// <param name="request">The AI request context</param>
    /// <param name="document">The document to ensure knowledge base for</param>
    /// <returns>The knowledge base instance</returns>
    public abstract Task<IKnowledgeBase> EnsureKnowledgeBase(AIRequest request, Document document);



    #endregion

    #region Query

    /// <summary>
    /// Creates query keywords from the AI request context
    /// </summary>
    /// <param name="request">The AI request context</param>
    /// <returns>The generated query keywords string</returns>
    public abstract Task<string> CreateQueryKeywords(AIRequest request);

    /// <summary>
    /// Queries text from the knowledge base with the specified scope
    /// </summary>
    /// <param name="request">The AI request context</param>
    /// <param name="document">The document to query</param>
    /// <param name="scope">The query scope type</param>
    /// <param name="predicate">Optional predicate to filter results</param>
    /// <returns>The queried text content</returns>
    public abstract Task<string> QueryText(AIRequest request, Document document, QueryScopeTypes scope, Predicate<string> predicate = null);

    /// <summary>
    /// Queries all knowledge items from the document
    /// </summary>
    /// <param name="request">The AI request context</param>
    /// <param name="document">The document to query</param>
    /// <param name="predicate">Optional predicate to filter results</param>
    /// <returns>Array of matching RAG items</returns>
    public abstract Task<RAGItem[]> QueryAll(AIRequest request, Document document, Predicate<string> predicate = null);

    /// <summary>
    /// Queries all typed knowledge items from the document
    /// </summary>
    /// <typeparam name="T">The type of the target object</typeparam>
    /// <param name="request">The AI request context</param>
    /// <param name="document">The document to query</param>
    /// <param name="predicate">Optional predicate to filter results</param>
    /// <returns>Array of matching typed RAG items</returns>
    public abstract Task<RAGItem<T>[]> QueryAll<T>(AIRequest request, Document document, Predicate<T> predicate = null)
        where T : class;

    /// <summary>
    /// Queries knowledge items using vector similarity for a single document
    /// </summary>
    /// <param name="request">The AI request context</param>
    /// <param name="document">The document to query</param>
    /// <param name="query">The query string for vector search</param>
    /// <param name="predicate">Optional predicate to filter results</param>
    /// <returns>Array of matching RAG items</returns>
    public abstract Task<RAGItem[]> QueryVector(AIRequest request, Document document, string query, Predicate<string> predicate = null);

    /// <summary>
    /// Queries typed knowledge items using vector similarity for a single document
    /// </summary>
    /// <typeparam name="T">The type of the target object</typeparam>
    /// <param name="request">The AI request context</param>
    /// <param name="document">The document to query</param>
    /// <param name="query">The query string for vector search</param>
    /// <param name="predicate">Optional predicate to filter results</param>
    /// <returns>Array of matching typed RAG items</returns>
    public abstract Task<RAGItem<T>[]> QueryVector<T>(AIRequest request, Document document, string query, Predicate<T> predicate = null)
        where T : class;

    /// <summary>
    /// Queries knowledge items using vector similarity across multiple documents
    /// </summary>
    /// <param name="request">The AI request context</param>
    /// <param name="documents">The collection of documents to query</param>
    /// <param name="query">The query string for vector search</param>
    /// <param name="predicate">Optional predicate to filter results</param>
    /// <returns>Array of matching RAG items</returns>
    public abstract Task<RAGItem[]> QueryVector(AIRequest request, IEnumerable<Document> documents, string query, Predicate<string> predicate = null);

    /// <summary>
    /// Queries typed knowledge items using vector similarity across multiple documents
    /// </summary>
    /// <typeparam name="T">The type of the target object</typeparam>
    /// <param name="request">The AI request context</param>
    /// <param name="documents">The collection of documents to query</param>
    /// <param name="query">The query string for vector search</param>
    /// <param name="predicate">Optional predicate to filter results</param>
    /// <returns>Array of matching typed RAG items</returns>
    public abstract Task<RAGItem<T>[]> QueryVector<T>(AIRequest request, IEnumerable<Document> documents, string query, Predicate<T> predicate = null)
        where T : class;

    #endregion

    #region Get

    /// <summary>
    /// Gets the content of a knowledge item by its GUID identifier
    /// </summary>
    /// <param name="document">The document containing the knowledge</param>
    /// <param name="id">The GUID of the knowledge item</param>
    /// <returns>The content string of the knowledge item</returns>
    public string GetContent(Document document, Guid id)
    {
        return GetContent(document, id.ToString());
    }

    /// <summary>
    /// Gets the content of a knowledge item by its string identifier
    /// </summary>
    /// <param name="document">The document containing the knowledge</param>
    /// <param name="id">The string identifier of the knowledge item</param>
    /// <returns>The content string of the knowledge item</returns>
    public abstract string GetContent(Document document, string id);

    /// <summary>
    /// Tries to get the source tag and hash for a knowledge item by GUID
    /// </summary>
    /// <param name="document">The document containing the knowledge</param>
    /// <param name="id">The GUID of the knowledge item</param>
    /// <param name="sourceTag">The source tag of the knowledge item</param>
    /// <param name="sourceHash">The hash of the source content</param>
    /// <returns>True if the source tag was found, false otherwise</returns>
    public bool TryGetSourceTag(Document document, Guid id, out string sourceTag, out string sourceHash)
    {
        return TryGetSourceTag(document, id.ToString(), out sourceTag, out sourceHash);
    }

    /// <summary>
    /// Tries to get the source tag and hash for a knowledge item by string identifier
    /// </summary>
    /// <param name="document">The document containing the knowledge</param>
    /// <param name="id">The string identifier of the knowledge item</param>
    /// <param name="sourceTag">The source tag of the knowledge item</param>
    /// <param name="sourceHash">The hash of the source content</param>
    /// <returns>True if the source tag was found, false otherwise</returns>
    public abstract bool TryGetSourceTag(Document document, string id, out string sourceTag, out string sourceHash);

    #endregion

    #region Index
    /// <summary>
    /// Record a knowledge item
    /// </summary>
    /// <param name="request">The AI request context</param>
    /// <param name="document">Document that the knowledge base belongs to</param>
    /// <param name="item">Selected object</param>
    /// <param name="content">Knowledge content</param>
    /// <param name="sourceTag">Used to record source knowledge base tag, fill null if not using source knowledge base</param>
    /// <param name="sourceHash">Hash of the source content</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public abstract Task IndexItem(AIRequest request, Document document, object item, string content, string sourceTag = null, string sourceHash = null);

    /// <summary>
    /// Gets the item associated with the specified source tag and hash
    /// </summary>
    /// <param name="document">The document containing the knowledge</param>
    /// <param name="sourceTag">The source tag to look up</param>
    /// <param name="sourceHash">The source hash to look up</param>
    /// <param name="hashChanged">Indicates whether the source hash has changed</param>
    /// <param name="conversation">Optional conversation handler</param>
    /// <returns>The matched object, or null if not found</returns>
    public abstract object GetItemBySourceTag(Document document, string sourceTag, string sourceHash, out bool hashChanged,
        IConversationHandler conversation = null);

    #endregion
}