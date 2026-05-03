using Suity.Drawing;
using Suity.Editor.Types;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

/// <summary>
/// Represents an abstract base class for AIGC embedding models that provides text-to-vector embedding capabilities.
/// </summary>
[NativeType(Name = "EmbeddingAsset", Description = "AIGC Embedding Model", CodeBase = "*AIGC", Icon = "*CoreIcon|AI")]
public abstract class LLmEmbeddingAsset : StandaloneAsset<IEmbeddingModel>, IEmbeddingModel
{
    internal LLmEmbeddingAsset(string name, bool active = true)
        : base(name, active)
    {
    }

    /// <summary>
    /// Gets the default icon displayed for this embedding asset in the editor.
    /// </summary>
    public override ImageDef DefaultIcon => CoreIconCache.AI;

    /// <summary>
    /// Gets or sets the unique identifier for the embedding model.
    /// </summary>
    public string ModelId { get; internal protected set; }

    /// <summary>
    /// Activates or deactivates this embedding model.
    /// </summary>
    /// <param name="active">True to activate the model, false to deactivate it.</param>
    public void SetModelActive(bool active)
    {
        if (active)
        {
            base.ResolveId();
        }
        else
        {
            base.DetachId();
        }
    }

    /// <summary>
    /// Generates a vector embedding for the given document text.
    /// </summary>
    /// <param name="document">The input text document to embed.</param>
    /// <returns>A task representing the asynchronous operation, returning the vector embedding as a double array.</returns>
    public virtual Task<double[]> GetVector(string document) => Task.FromResult<double[]>([]);
}

/// <summary>
/// Represents an abstract base class for third-party LLM embedding assets.
/// </summary>
public abstract class ThirdPartyLLmEmbeddingAsset : LLmEmbeddingAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdPartyLLmEmbeddingAsset"/> class.
    /// </summary>
    /// <param name="name">The name of the embedding asset.</param>
    /// <param name="active">Whether the asset should be active upon creation. Defaults to true.</param>
    protected ThirdPartyLLmEmbeddingAsset(string name, bool active = true)
        : base(name, active) { }
}

/// <summary>
/// Builder class for constructing <see cref="LLmEmbeddingAsset"/> instances with configurable options.
/// </summary>
public class LLmEmbeddingAssetBuilder<T> : AssetBuilder<T>
    where T : LLmEmbeddingAsset, new()
{
    /// <summary>
    /// Gets the unique identifier of the embedding model.
    /// </summary>
    public string ModelId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether reasoning capabilities are enabled.
    /// </summary>
    public bool Reasoner { get; private set; }

    /// <summary>
    /// Gets a value indicating whether web search capabilities are enabled.
    /// </summary>
    public bool WebSearch { get; private set; }

    /// <summary>
    /// Gets the context size in thousands of tokens (K).
    /// </summary>
    public int ContextSizeK { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LLmEmbeddingAssetBuilder{T}"/> class.
    /// </summary>
    public LLmEmbeddingAssetBuilder()
    {
        AddAutoUpdate(nameof(ModelId), o => o.ModelId = ModelId);
    }

    /// <summary>
    /// Sets the model identifier and updates the associated asset accordingly.
    /// </summary>
    /// <param name="modelId">The unique identifier for the embedding model.</param>
    public void SetModelId(string modelId)
    {
        if (ModelId == modelId) { return; }

        ModelId = modelId;

        TryUpdateNow(o => o.ModelId = ModelId);

        SetLocalName(modelId);
    }
}