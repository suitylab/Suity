using Suity.Editor.AIGC.Assistants;
using Suity.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.RAG;

/// <summary>
/// Extension methods for RAG (Retrieval-Augmented Generation) operations
/// </summary>
public static class RAGExtensions
{
    /// <summary>
    /// Query and aggregate information from knowledge base
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="msg"></param>
    /// <param name="multiple">Only used to get topK count, not to decide whether to return multiple</param>
    /// <param name="conversation"></param>
    /// <param name="context"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    /// <exception cref="AigcException"></exception>
    public static async Task<string> QueryKnowledge(this AIRequest request, IVectorKnowledge vec, bool multiple)
    {
        // Create query keywords
        string query = await RAGService.Instance.CreateQueryKeywords(request);

        // Query knowledge base
        int topK = AIAssistantService.Config.KnowledgeConfig.GetTopK(multiple);
        var ragResults = await vec.QueryVectorDocuments(query, topK, request.Cancel);
        if (ragResults is null || ragResults.Length == 0)
        {
            //throw new LLmException("No information obtained from knowledge base.");
            request.Conversation.AddSystemMessage("No information obtained from knowledge base.");
            return null;
        }

        // Build knowledge text
        string knowledge = string.Join("\r\n", ragResults.Select(o => o.Content)).Trim();

        if (string.IsNullOrWhiteSpace(knowledge))
        {
            request.Conversation.AddSystemMessage("No information obtained from knowledge base.");
            return null;
        }

        return knowledge;
    }

    /// <summary>
    /// Get source document summary for feature
    /// </summary>
    /// <param name="featureRag"></param>
    /// <param name="featureName"></param>
    /// <param name="msg"></param>
    /// <param name="conversation"></param>
    /// <param name="context"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    public static async Task<string> QueryFeatureKnowledge(this IFeatureKnowledge featureRag, AIRequest request, string featureName)
    {
        var sourceVec = featureRag.GetBaseVectorRAG(request.Cancel)
            ?? throw new AigcException("Unable to get source vector knowledge base of feature knowledge base");

        var refIds = await featureRag.GetFeatureSourceRefIds(featureName, request.Cancel) ?? [];

        var ragResults = await sourceVec.QueryVectorDocuments(refIds, featureName, null, request.Cancel);
        if (ragResults is null || ragResults.Length == 0)
        {
            //throw new LLmException("No information obtained from knowledge base.");
            request.Conversation.AddSystemMessage("No information obtained from knowledge base.");
            return null;
        }


        // Build knowledge text
        string knowledge = string.Join("\r\n", ragResults.Select(o => o.Content)).Trim();

        if (string.IsNullOrWhiteSpace(knowledge))
        {
            request.Conversation.AddSystemMessage("No information obtained from knowledge base.");
            return null;
        }

        return knowledge;
    }
}