namespace Suity.Editor.AIGC;

/// <summary>
/// Base class for internal LLM model assets within the AIGC module.
/// </summary>
public class InternalLLmModelAsset : LLmModelAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InternalLLmModelAsset"/> class.
    /// </summary>
    /// <param name="name">The name of the model asset.</param>
    /// <param name="resolveId">Whether to resolve the ID from the name.</param>
    protected InternalLLmModelAsset(string name, bool resolveId = true)
        : base(name, resolveId) { }
}