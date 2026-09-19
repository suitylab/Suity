namespace Suity.Editor.AIGC;

public interface IModelProviderService
{
    /// <summary>
    /// Gets an LLM model based on the specified level and type.
    /// </summary>
    /// <param name="level">The model performance level.</param>
    /// <param name="type">The type of LLM model required.</param>
    /// <returns>The configured LLM model instance.</returns>
    ILLmModel GetLLmModel(AigcModelLevel level, LLmModelType type);

    /// <summary>
    /// Gets the LLM model parameter based on the specified level and type.
    /// </summary>
    /// <param name="level">The model performance level.</param>
    /// <param name="type">The type of LLM model required.</param>
    /// <returns>The configured LLM model parameter instance.</returns>
    LLmModelParameter GetLLmModelParameter(AigcModelLevel level, LLmModelType type);


    /// <summary>
    /// Gets an image generation model based on the specified level.
    /// </summary>
    /// <param name="level">The model performance level.</param>
    /// <returns>The configured image generation model instance.</returns>
    IImageGenModel GetImageGenModel(AigcModelLevel level);

    /// <summary>
    /// Gets the embedding model for vector representations.
    /// </summary>
    /// <returns>The configured embedding model instance.</returns>
    IEmbeddingModel GetEmbedding();

    /// <summary>
    /// Gets the localized language setting for the model provider service.
    /// </summary>
    /// <returns>The localized language setting.</returns>
    string GetLocalizedLanguage();
}
