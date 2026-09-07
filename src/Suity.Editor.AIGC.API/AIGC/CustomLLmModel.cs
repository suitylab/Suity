using System;

namespace Suity.Editor.AIGC;

public class CustomLLmModel : ILLmModel
{
    private readonly ILLmManufacturer _manufacturer;
    private readonly string _modelId;

    public CustomLLmModel(ILLmManufacturer manufacturer, string modelId)
    {
        _manufacturer = manufacturer ?? throw new ArgumentNullException(nameof(manufacturer));
        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    public CustomLLmModel(string providerId, string apiBaseUrl, string apiKey, string modelId)
    {
        _manufacturer = new CustomModelProvider()
        {
            ProviderId = providerId,
            ApiUrl = apiBaseUrl,
            ApiKey = apiKey,
        };

        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    public string ModelId => _modelId;

    public bool IsManual => false;

    public bool ApiKeyValid => !string.IsNullOrWhiteSpace(_manufacturer.ApiKey);

    public bool SupportToolCalling => false;

    public bool SupportReasoning => false;

    public bool SupportStreaming => true;

    public ILLmCall CreateCall(LLmModelParameter? parameter = null, FunctionContext? context = null)
    {
        return new CustomLLmCall(_manufacturer, this, parameter, context);
    }

    public ILLmChat? CreateConversation(LLmModelParameter? parameter = null, FunctionContext? context = null) => null;
}

public class CustomModelProvider : ILLmManufacturer
{
    public string ProviderId { get; init; } = string.Empty;

    public string ApiUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;
}

public class CustomLLmCall : BaseOpenAICall
{
    public CustomLLmCall(ILLmManufacturer manufacturer, ILLmModel model, LLmModelParameter? parameter = null, FunctionContext? context = null) 
        : base(manufacturer, model, parameter, context)
    {
    }
}