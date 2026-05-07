using Suity.Views;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// AI assistant config
/// </summary>
public interface IAIAssistantConfig
{
    // Model
    LLmModelParameter GetPresetModelLLmConfig(LLmModelPreset presetModel);
    ILLmModel GetPresetModel(LLmModelPreset presetModel);


    // Parameters
    ValueProperty<int> RetryCount { get; }
    ValueProperty<int> MaxQuery { get; }
    ValueProperty<int> MaxGenerateDepth { get; }
    ValueProperty<int> LinkedDataMemorySize { get; }
    ValueProperty<bool> ShowPromptInConverasation { get; }
    ValueProperty<bool> AutoAddNewEnumValue { get; }


    // Modules
    public AIClissifierConfig ClassifyConfig { get; }
    public AISubdivideConfig SubdivideConfig { get; }
    public AIExtractorConfig ExtractorConfig { get; }
    public AISupportConfig SupportConfig { get; }
    public AIKnowledgeConfig KnowledgeConfig { get; }
    public AIDataGenerationConfig DataGenerationConfig { get; }
}