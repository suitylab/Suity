using Suity.Collections;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

#region LLmMessageRole

/// <summary>
/// Represents the role of a message in a language model conversation.
/// </summary>
public enum LLmMessageRole
{
    /// <summary>
    /// System message that sets the behavior of the assistant.
    /// </summary>
    System,
    /// <summary>
    /// User message representing input from the user.
    /// </summary>
    User,
    /// <summary>
    /// Assistant message representing the model's response.
    /// </summary>
    Assistant,
}

#endregion

#region LLmMessage

/// <summary>
/// Represents a single message in a language model conversation with a role and content.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Language Model Info", Color = AigcColors.Task)]
public class LLmMessage : IViewObject
{
    /// <summary>
    /// Gets or sets the role of the message (System, User, or Assistant).
    /// </summary>
    public LLmMessageRole Role { get; set; }
    /// <summary>
    /// Gets or sets the text content of the message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Returns a string representation of the message with its role.
    /// </summary>
    /// <returns>A formatted string containing the role and message content.</returns>
    public override string ToString() => $"[{Role}] {Message}";


    /// <summary>
    /// Combines multiple messages into a single formatted text string.
    /// </summary>
    /// <param name="messages">The collection of messages to combine.</param>
    /// <returns>A formatted string containing all messages with role headers.</returns>
    public static string CombineText(IEnumerable<LLmMessage> messages)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        if (messages.SkipNull().CountOne())
        {
            return messages.SkipNull().FirstOrDefault()?.Message ?? string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var msg in messages.SkipNull())
        {
            builder.AppendLine($"------------------ {msg.Role} ------------------");
            builder.AppendLine(msg.Message);
            builder.AppendLine();
        }

        string text = builder.ToString();

        return text;
    }

    /// <summary>
    /// Combines multiple messages into a single user message.
    /// </summary>
    /// <param name="messages">The collection of messages to combine.</param>
    /// <returns>A new LLmMessage with the combined content as a user message.</returns>
    public static LLmMessage Combine(IEnumerable<LLmMessage> messages)
    {
        string content = CombineText(messages);

        return new LLmMessage
        {
            Role = LLmMessageRole.User,
            Message = content,
        };
    }

    #region IViewObject
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        sync.Sync(nameof(Role), Role, SyncFlag.GetOnly);
        sync.Sync(nameof(Message), new TextBlock(Message), SyncFlag.GetOnly);
    }

    public void SetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(Role, new ViewProperty(nameof(Role), "Role").WithReadOnly());
        setup.InspectorFieldOf<TextBlock>(new ViewProperty(nameof(Message), "Message").WithReadOnly());
    }
    #endregion
}

/// <summary>
/// Converts an LLmMessage to its text representation.
/// </summary>
public class LLmMessageToTextConverter : TypeToTextConverter<LLmMessage>
{
    /// <summary>
    /// Converts the LLmMessage to a formatted string.
    /// </summary>
    /// <param name="objFrom">The message to convert.</param>
    /// <returns>A string representation of the message.</returns>
    public override string Convert(LLmMessage objFrom)
    {
        return $"<{objFrom.Role}>\r\n{objFrom.Message}";
    }
}

/// <summary>
/// Converts text input to an LLmMessage.
/// </summary>
public class TextToLLmMessageConverter : TextToTypeConverter<LLmMessage>
{
    /// <summary>
    /// Converts the input text to an LLmMessage with user role.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A new LLmMessage with the text as content.</returns>
    public override LLmMessage Convert(string text)
    {
        return new LLmMessage
        {
            Role = LLmMessageRole.User,
            Message = text,
        };
    }
}

#endregion

#region LLmModelParameter

/// <summary>
/// Represents the configuration parameters for a language model, including temperature, top-p, penalties, and token limits.
/// </summary>
[DisplayText("LLM Parameter")]
[NativeType(CodeBase = "AIGC", Description = "Language Model Parameter")]
public class LLmModelParameter : IViewObject
{
    private readonly ValueProperty<double> _temperature
        = new(nameof(Temperature), "Temperature", 0.5, "TOOLTIPS_TEMPERATURE");

    private readonly ValueProperty<double> _topP
        = new(nameof(TopP), "TopP", 0.8, "TOOLTIPS_TOPP");

    private readonly ValueProperty<double> _presencePenalty
        = new(nameof(PresencePenalty), "Presence penalty", 0, "");

    private readonly ValueProperty<double> _frequencyPenalty
        = new(nameof(FrequencyPenalty), "Repetition penalty", 0, "");

    private readonly ValueProperty<int> _maxTokens
        = new(nameof(MaxTokens), "Token number limit", 0, "TOOLTIPS_MAXTOKEN");

    public LLmModelParameter()
    {
        _temperature.Property.WithRange(0, 1, 0.01m);
        _topP.Property.WithRange(0, 1, 0.01m);
    }

    /// <summary>
    /// Gets or sets the sampling temperature. Higher values produce more random output, lower values make output more deterministic.
    /// </summary>
    public double Temperature { get => _temperature.Value; set => _temperature.Value = value; }
    /// <summary>
    /// Gets or sets the nucleus sampling probability threshold. Controls diversity by limiting to tokens with cumulative probability up to this value.
    /// </summary>
    public double TopP { get => _topP.Value; set => _topP.Value = value; }
    /// <summary>
    /// Gets or sets the presence penalty. Higher values increase the likelihood of generating new topics.
    /// </summary>
    public double PresencePenalty { get => _presencePenalty.Value; set => _presencePenalty.Value = value; }
    /// <summary>
    /// Gets or sets the frequency penalty. Higher values reduce repetition of previously generated text.
    /// </summary>
    public double FrequencyPenalty { get => _frequencyPenalty.Value; set => _frequencyPenalty.Value = value; }
    /// <summary>
    /// Gets or sets the maximum number of tokens to generate. Zero means no limit.
    /// </summary>
    public int MaxTokens { get => _maxTokens.Value; set => _maxTokens.Value = value; }

    /// <summary>
    /// Synchronizes all parameter properties with the given sync provider.
    /// </summary>
    /// <param name="sync">The property synchronization provider.</param>
    /// <param name="context">The synchronization context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        _temperature.Sync(sync);
        _topP.Sync(sync);
        _presencePenalty.Sync(sync);
        _frequencyPenalty.Sync(sync);
        _maxTokens.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all parameter properties.
    /// </summary>
    /// <param name="setup">The view object setup interface.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        _temperature.InspectorField(setup);
        _topP.InspectorField(setup);
        _presencePenalty.InspectorField(setup);
        _frequencyPenalty.InspectorField(setup);
        _maxTokens.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for this parameter configuration.
    /// </summary>
    /// <returns>A localized string representing the parameter configuration.</returns>
    public override string ToString()
    {
        return L(this.GetType().ToDisplayText());
    }
}

#endregion

#region LLmCallOption

/// <summary>
/// Represents optional configuration settings for a language model call, including seed, search, and thinking options.
/// </summary>
public class LLmCallOption
{
    /// <summary>
    /// Gets or sets the random seed for reproducible output generation.
    /// </summary>
    public ulong? Seed { get; set; }
    /// <summary>
    /// Gets or sets whether web search capability is enabled during the model call.
    /// </summary>
    public bool? EnableSearch { get; set; }
    /// <summary>
    /// Gets or sets whether extended thinking/reasoning mode is enabled for the model.
    /// </summary>
    public bool? EnableThinking { get; set; }
}

#endregion

#region LLmModelPresetConfig

/// <summary>
/// Represents the preset configuration for multiple language model types, including default, planning, tool calling, web search, coding, and lightweight models.
/// </summary>
public class LLmModelPresetConfig : IViewObject
{
    /// <summary>
    /// Gets the default language model used when no specific model type is specified.
    /// </summary>
    public AssetProperty<ILLmModel> Default { get; }
        = new(nameof(Default), "Default language model", toolTips: "TOOLTIPS_DEFAULT_MODEL");

    /// <summary>
    /// Gets the language model configured for planning tasks.
    /// </summary>
    public AssetProperty<ILLmModel> Planning { get; }
        = new(nameof(Planning), "Planning language model", toolTips: "TOOLTIPS_PLANNING_MODEL");

    /// <summary>
    /// Gets the language model configured for tool calling capabilities.
    /// </summary>
    public AssetProperty<ILLmModel> ToolCalling { get; }
        = new(nameof(ToolCalling), "Tool calls language model", toolTips: "TOOLTIPS_TOOL_CALLS_MODEL");

    /// <summary>
    /// Gets the language model configured for web search operations.
    /// </summary>
    public AssetProperty<ILLmModel> WebSearch { get; }
        = new(nameof(WebSearch), "Web search language model", toolTips: "TOOLTIPS_WEB_SEARCH_MODEL");


    /// <summary>
    /// Gets the lightweight language model for simple or fast operations.
    /// </summary>
    public AssetProperty<ILLmModel> Lightweight { get; }
        = new(nameof(Lightweight), "Lightweight language model", toolTips: "TOOLTIPS_LIGHTWEIGHT_MODEL");

    /// <summary>
    /// Gets the language model configured for code-specific tasks.
    /// </summary>
    public AssetProperty<ILLmModel> Coding { get; }
        = new(nameof(Coding), "Code-specific language model", toolTips: "TOOLTS_CODE_SPECIFIC_MODEL");

    /// <summary>
    /// Gets the default parameter configuration applied to language model calls.
    /// </summary>
    public ValueProperty<LLmModelParameter> DefaultParameters { get; }
        = new(nameof(DefaultParameters), "Default parameters", new LLmModelParameter());


    public LLmModelPresetConfig()
    {
        DefaultParameters.Property.WithWriteBack().WithOptional().WithExpand();
    }

    /// <summary>
    /// Synchronizes all model preset properties with the given sync provider.
    /// </summary>
    /// <param name="sync">The property synchronization provider.</param>
    /// <param name="context">The synchronization context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        Default.Sync(sync);
        Planning.Sync(sync);
        ToolCalling.Sync(sync);
        WebSearch.Sync(sync);
        Coding.Sync(sync);
        Lightweight.Sync(sync);
        DefaultParameters.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all model preset properties.
    /// </summary>
    /// <param name="setup">The view object setup interface.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        Default.InspectorField(setup);
        Planning.InspectorField(setup);
        ToolCalling.InspectorField(setup);
        WebSearch.InspectorField(setup);
        Coding.InspectorField(setup);
        Lightweight.InspectorField(setup);
        DefaultParameters.InspectorField(setup);
    }

    /// <summary>
    /// Gets the appropriate language model for the specified model type, falling back to the default model if not configured.
    /// </summary>
    /// <param name="type">The type of language model to retrieve.</param>
    /// <returns>The configured language model for the specified type, or the default model.</returns>
    public ILLmModel GetModel(LLmModelType type) => type switch
    {
        LLmModelType.Planning => Planning.Target ?? Default.Target,
        LLmModelType.ToolCalls => ToolCalling.Target ?? Default.Target,
        LLmModelType.WebSearch => WebSearch.Target ?? Default.Target,
        LLmModelType.Coding => Coding.Target ?? Default.Target,
        LLmModelType.Lightweight => Lightweight.Target ?? Default.Target,
        _ => Default.Target,
    };

    /// <summary>
    /// Validates that all configured language models are properly set up.
    /// </summary>
    /// <param name="message">A list to populate with validation error messages if validation fails.</param>
    /// <returns>True if all configured models are valid; otherwise, false.</returns>
    public bool GetIsValud(ref List<string> message)
    {
        if (!GetLLmModelIsValud(Default, ref message))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Planning.Selection.SelectedKey) && !GetLLmModelIsValud(Planning, ref message))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(WebSearch.Selection.SelectedKey) && !GetLLmModelIsValud(WebSearch, ref message))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Lightweight.Selection.SelectedKey) && !GetLLmModelIsValud(Lightweight, ref message))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Coding.Selection.SelectedKey) && !GetLLmModelIsValud(Coding, ref message))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a specific language model property is properly configured.
    /// </summary>
    /// <param name="model">The asset property containing the language model to validate.</param>
    /// <param name="message">A list to populate with validation error messages if validation fails.</param>
    /// <returns>True if the model is valid; otherwise, false.</returns>
    private bool GetLLmModelIsValud(AssetProperty<ILLmModel> model, ref List<string> message)
    {
        if (model.Target is not { } target)
        {
            message ??= [];
            message.Add(L("Language model not configured correctly") + ": " + model.Property.DisplayName);

            return false;
        }

        if (!target.ApiKeyValid)
        {
            message ??= [];
            message.Add(L($"{L(model.Property.DisplayName)}({target.ModelId}) Language model API key not configured correctly."));

            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a string representation indicating the current model configuration status.
    /// </summary>
    /// <returns>The display text of the default model if valid, or an error message if misconfigured.</returns>
    public override string ToString()
    {
        List<string> message = null;

        if (GetIsValud(ref message))
        {
            return Default?.Target?.ToDisplayText() ?? string.Empty;
        }
        else
        {
            return "(" + L("Language model not configured correctly") + ")";
        }
    }
}

#endregion

#region LLmCallRequest

/// <summary>
/// Represents a request to call a language model, containing messages, parameters, options, and metadata.
/// </summary>
public class LLmCallRequest
{
    /// <summary>
    /// Gets the list of messages in the conversation to send to the language model.
    /// </summary>
    public List<LLmMessage> Messages { get; } = [];

    /// <summary>
    /// Gets or sets the conversation handler for managing ongoing dialogue state.
    /// </summary>
    public IConversationHandler Conversation { get; set; }
    /// <summary>
    /// Gets or sets the cancellation token used to cancel the language model call.
    /// </summary>
    public CancellationToken Cancel { get; set; }
    /// <summary>
    /// Gets or sets the parameter configuration for the language model call.
    /// </summary>
    public LLmModelParameter Parameter { get; set; }
    /// <summary>
    /// Gets or sets the optional settings for the language model call.
    /// </summary>
    public LLmCallOption Option { get; set; }
    /// <summary>
    /// Gets or sets the title or identifier for this request.
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Gets or sets the number of retry attempts for failed calls.
    /// </summary>
    public int? RetryCount { get; set; }

    /// <summary>
    /// Gets or sets a custom tag object for associating arbitrary data with this request.
    /// </summary>
    public object Tag { get; set; } 

    /// <summary>
    /// Initializes a new empty instance of the LLmCallRequest class.
    /// </summary>
    public LLmCallRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the LLmCallRequest class with a single user prompt.
    /// </summary>
    /// <param name="userPrompt">The user prompt message to add.</param>
    public LLmCallRequest(string userPrompt)
    {
        if (!string.IsNullOrWhiteSpace(userPrompt))
        {
            Messages.Add(new() { Role = LLmMessageRole.User, Message = userPrompt });
        }
    }

    /// <summary>
    /// Initializes a new instance of the LLmCallRequest class with a system prompt and one or more user prompts.
    /// </summary>
    /// <param name="systemPrompt">The system prompt to set the assistant's behavior.</param>
    /// <param name="userPrompts">One or more user prompt messages.</param>
    public LLmCallRequest(string systemPrompt, params string[] userPrompts)
    {
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            Messages.Add(new() { Role = LLmMessageRole.System, Message = systemPrompt });
        }

        if (userPrompts?.Length > 0)
        {
            foreach (var msg in userPrompts.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                Messages.Add(new() { Role = LLmMessageRole.User, Message = msg });
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the LLmCallRequest class with multiple system and user prompts.
    /// </summary>
    /// <param name="systemPrompts">A collection of system prompt messages.</param>
    /// <param name="userPrompts">A collection of user prompt messages.</param>
    public LLmCallRequest(IEnumerable<string> systemPrompts, IEnumerable<string> userPrompts)
    {
        if (systemPrompts?.Any() == true)
        {
            foreach (var msg in systemPrompts.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                Messages.Add(new() { Role = LLmMessageRole.System, Message = msg });
            }
        }

        if (userPrompts?.Any() == true)
        {
            foreach (var msg in userPrompts.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                Messages.Add(new() { Role = LLmMessageRole.User, Message = msg });
            }
        }
    }


    /// <summary>
    /// Appends a system message to the message list.
    /// </summary>
    /// <param name="systemPrompt">The system prompt text to add.</param>
    public void AppendSystemMessage(string systemPrompt)
    {
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            Messages.Add(new() { Role = LLmMessageRole.System, Message = systemPrompt });
        }
    }

    /// <summary>
    /// Appends a user message to the message list.
    /// </summary>
    /// <param name="userPrompt">The user prompt text to add.</param>
    public void AppendUserMessage(string userPrompt)
    {
        if (!string.IsNullOrWhiteSpace(userPrompt))
        {
            Messages.Add(new() { Role = LLmMessageRole.User, Message = userPrompt });
        }
    }

    /// <summary>
    /// Appends an assistant message to the message list.
    /// </summary>
    /// <param name="assistantPrompt">The assistant response text to add.</param>
    public void AppendAssistantMessage(string assistantPrompt)
    {
        if (!string.IsNullOrWhiteSpace(assistantPrompt))
        {
            Messages.Add(new() { Role = LLmMessageRole.Assistant, Message = assistantPrompt });
        }
    }
}

#endregion

#region CustomLLmModelSetting

/// <summary>
/// Represents a custom language model setting that allows specifying either a particular model or a model type with parameters.
/// </summary>
public class CustomLLmModelSetting : IViewObject, IViewOptional
{
    /// <summary>
    /// Gets or sets whether a specific model is explicitly chosen instead of using a model type.
    /// </summary>
    public ValueProperty<bool> SpecifiedModel { get; } = new(nameof(SpecifiedModel), "Specified Model");

    /// <summary>
    /// Gets or sets the type of model to use when not specifying a particular model.
    /// </summary>
    public ValueProperty<LLmModelType> ModelType { get; } = new(nameof(ModelType), "Model Type");

    /// <summary>
    /// Gets or sets the specific model asset to use when SpecifiedModel is enabled.
    /// </summary>
    public AssetProperty<LLmModelAsset> Model { get; } = new(nameof(Model), "Model");

    /// <summary>
    /// Gets or sets the parameter configuration for the selected model.
    /// </summary>
    public ValueProperty<LLmModelParameter> Parameters { get; } = new(nameof(Parameters), "Parameters");
    
    /// <summary>
    /// Gets or sets whether this setting is optional (can be disabled).
    /// </summary>
    public bool IsOptional { get; set; }

    public CustomLLmModelSetting()
    {
        Parameters.Property.WithOptional();
    }

    /// <summary>
    /// Gets the resolved language model based on the current configuration settings.
    /// </summary>
    /// <returns>The configured language model instance, or null if not optional or not configured.</returns>
    public ILLmModel GetModel()
    {
        if (!IsOptional)
        {
            return null;
        }

        if (SpecifiedModel.Value)
        {
            return Model.Target;
        }
        else
        {
            return LLmService.Instance.GetLLmModel(AigcModelLevel.Default, ModelType.Value);
        }
    }

    /// <summary>
    /// Synchronizes all custom model setting properties with the given sync provider.
    /// </summary>
    /// <param name="sync">The property synchronization provider.</param>
    /// <param name="context">The synchronization context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        IsOptional = sync.Sync(nameof(IsOptional), IsOptional);

        SpecifiedModel.Sync(sync);
        ModelType.Sync(sync);
        Model.Sync(sync);
        Parameters.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for the custom model setting properties, showing either the model or model type field based on SpecifiedModel.
    /// </summary>
    /// <param name="setup">The view object setup interface.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        SpecifiedModel.InspectorField(setup);
        if (SpecifiedModel.Value)
        {
            Model.InspectorField(setup);
        }
        else
        {
            ModelType.InspectorField(setup);
        }
        Parameters.InspectorField(setup);
    }

    /// <summary>
    /// Returns a string representation of the current model selection.
    /// </summary>
    /// <returns>The display text of the selected model or model type, or empty if not optional.</returns>
    public override string ToString()
    {
        if (!IsOptional)
        {
            return string.Empty;
        }

        if (SpecifiedModel.Value)
        {
            return $"[{Model.TargetAsset?.ToDisplayTextL()}]";
        }
        else
        {
            return ModelType.Value.ToDisplayTextL();
        }
    }
}

#endregion
